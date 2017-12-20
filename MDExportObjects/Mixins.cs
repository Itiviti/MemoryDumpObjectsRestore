using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Desktop;
using MoreLinq;
using SampleProject;

namespace MDExportObjects
{
    public static class Mixins
    {
        public static IEnumerable<Pair> Export(this IEnumerable<ClrObject> source, Type type)
        {
            return source
                .Select(x => new Pair(x, GetUninitializedObject(type, x.Address)))
                .Select(ExtractPrimitiveFields)
                .Select(ExtractStructFields)
                .Select(ExtractArray)
                .Select(ExtractReferenceObjects)

                ;
        }

        private static object GetUninitializedObject(Type type, ulong address)
        {
            object result = null;
            if (Pair.Cache.TryGetValue(address, out result))
                return result;

            var uninitializedObject = FormatterServices.GetUninitializedObject(type);
            Pair.Cache.Add(address, uninitializedObject);
            return uninitializedObject;
        }

        private static Pair ExtractArray(Pair p)
        {
            var obj = p.ClrObj;
            foreach (var field in obj.Type.Fields
                .Where(field => field.Type.IsArray))
            {
                Array arr;
                if (!ExtractArray(p, obj, field, out arr)) continue;

                p.FieldInfo[field.Name].SetValue(p.RuntimeObject, arr);
            }

            return p;
        }

        private static bool ExtractArray(Pair p, ClrObject obj, ClrInstanceField field, out Array arr)
        {
            arr = null;
            var array = obj.GetObjectField(field.Name);

            if (array.Type.ComponentType.IsString || array.Type.ComponentType.IsPrimitive)
            {
                arr = CreatePrimitiveArray(array);
            }
            else
            {
                int len = field.Type.GetArrayLength(array.Address);

                Type type = null;;
                if (!GetType(p, array.Type.ComponentType, out type))
                    return false;

                arr = Array.CreateInstance(type, len);
                for (int i = 0; i < len; i++)
                {
                    ulong address = (ulong) array.Type.GetArrayElementValue(array.Address, i);
                    if (address == 0) continue;
                    ClrType otype = Program._runtime.Heap.GetObjectType(address);

                    Type type1 = null; ;
                    if (!GetType(p, otype, out type1))
                        continue;

                    var clrObject = new ClrObject(address, otype);

                    if (clrObject.IsNull) continue;

                    object value1 = null;
                    if (clrObject.Type.IsString)
                    {
                        value1 = GetStringContents(clrObject);
                    }
                    else if (clrObject.Type.IsPrimitive)
                    {
                        value1 = field.GetValue(obj.Address);
                    }
                    else
                    {
                        var value = Export(Yield(clrObject), type1).FirstOrDefault();
                        p.Warnings.AddRange(value.Warnings);
                        p.Errors.AddRange(value.Errors);
                        value1 = value.RuntimeObject;
                    }

                    arr.SetValue(value1, i);
                }
            }
            return true;
        }

        private static IEnumerable<T> Yield<T>(T clrObject)
        {
            return Enumerable.Repeat(clrObject, 1);
        }

        private static Pair ExtractReferenceObjects(Pair p)
        {
            var obj = p.ClrObj;
            foreach (var field in obj.Type.Fields
                .Where(field => field.IsObjectReference)
                .Where(field => !field.Type.IsArray)
                .Where(field => !field.Type.IsString))
            {
                object reff;
                if (!ExtractRefObject(p, field, obj, out reff)) continue;

                p.FieldInfo[field.Name].SetValue(p.RuntimeObject, reff);
            }

            return p;
        }

        private static bool ExtractRefObject(Pair p, ClrInstanceField field, ClrObject obj, out object reff)
        {
            reff = null;

            var reference = obj.GetObjectField(field.Name);
            if (reference.IsNull) return true;

            if (reference.Type.IsString )
            {
                var value = GetStringContents(reference);

                reff = value;
                return true;
            }

            if (reference.Type.IsPrimitive)
            {
                reff = field.GetValue(obj.Address);
                return true;
            }

            if (Pair.Cache.TryGetValue(reference.Address, out reff))
                return true;

            Type type = null;

            if (!GetType(p, reference.Type, out type))
                return false;

            var par = Export(Yield(reference), type).Single();
            reff = par.RuntimeObject;
            p.Warnings.AddRange(par.Warnings);
            p.Errors.AddRange(par.Errors);

            return true;
        }

        private static string GetStringContents(ClrObject reference)
        {
            // TODO: The amounts of hack is a bit much here
            var clrHeap = Program._runtime.Heap.GetType().GetMethod("GetStringContents", BindingFlags.NonPublic | BindingFlags.Instance);
            return (string)clrHeap.Invoke(Program._runtime.Heap, new object[] { reference.Address });
        }

        private static string HackGenerics(ClrType type)
        {
            return type.ToString().Replace("<","`1[").Replace(">", "]"); ;
        }

        public struct Pair
        {
            public Pair(ClrObject clrObj, object runtimeObject)
            {
                this.ClrObj = clrObj;
                this.RuntimeObject = runtimeObject;
                this.FieldInfo = GetFields(runtimeObject.GetType());
                Errors = new List<string>();
                Warnings = new List<string>();
            }

            public static Dictionary<ulong, object> Cache { get; } = new Dictionary<ulong, object>();

            public ClrObject ClrObj;
            public object RuntimeObject;
            public Dictionary<string, FieldInfo> FieldInfo;
            public List<string> Errors;
            public List<string> Warnings;
        }

        public static Pair ExtractPrimitiveFields(Pair p)
        {
            foreach (var field in p.ClrObj.Type.Fields.Where(field => field.IsPrimitive || field.Type.IsString))
            {
                var value = field.GetValue(p.ClrObj.Address);

                p.FieldInfo[field.Name].SetValue(p.RuntimeObject, value);
            }

            return p;
        }

        private static Array CreatePrimitiveArray(ClrObject array)
        {
            int len = array.Type.GetArrayLength(array.Address);
            var type = Type.GetType(array.Type.ComponentType.ToString());
            var arr = Array.CreateInstance(type, len);

            for (int i = 0; i < len; i++)
            {
                var value = array.Type.GetArrayElementValue(array.Address, i);
                arr.SetValue(value, i);
            }
            return arr;
        }

        public static Pair ExtractStructFields(Pair p)
        {
            var obj = p.ClrObj;
            foreach (var field in obj.Type.Fields.Where(field => field.IsValueClass))
            {
                ClrValueClass structClass = obj.GetValueClassField(field.Name);

                object result;
                if (!ExtractStruct(p, field, structClass, out result)) continue;

                p.FieldInfo[field.Name].SetValue(p.RuntimeObject, result);
            }

            return p;
        }

        private static bool ExtractStruct(Pair p, ClrInstanceField field, ClrValueClass structClass, out object result)
        {
            result = null;
            Type type;
            if (!GetType(p, field.Type, out type)) return false;

            var fieldInfos = GetFields(type);

            result = FormatterServices.GetUninitializedObject(type);

            foreach (var fld in structClass.Type.Fields)
            {
                object value = null;
                if (fld.Type.IsString)
                    value = structClass.GetStringField(fld.Name);
                else if (fld.Type.IsPrimitive)
                    value = fld.GetValue(structClass.Address, true);
                else if (fld.Type.IsValueClass)
                {
                    if (!ExtractStruct(p, fld, structClass.GetValueClassField(fld.Name), out value))
                        continue;
                }
                else if (fld.Type.IsArray)
                {
                    Array arr = null;
                    if (!ExtractArray(p, structClass.GetObjectField(fld.Name), fld, out arr))
                        continue;
                    value = arr;
                }
                else
                {
                    if (!ExtractRefObject(p, field, structClass.GetObjectField(fld.Name), out value))
                        continue;
                }
                fieldInfos[fld.Name].SetValue(result, value);
            }
            return true;
        }

        private static bool GetType(Pair p, ClrType clrType, out Type type)
        {
            string typeString = HackGenerics(clrType);

            var combine = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Path.GetFileName(clrType.Module.AssemblyName));

            Assembly ass = null;
            if (File.Exists(combine))
                ass = Assembly.LoadFile(combine);

            var typeName = ass != null ? Assembly.CreateQualifiedName(ass.FullName, typeString) : typeString;
            type = Type.GetType(typeName);

            if (type == null)
            {
                p.Errors.Add($"Can't get type for {typeString}");
                return false;
            }
            return true;
        }

        private static Dictionary<string, FieldInfo> GetFields(Type t)
        {
            var fieldInfos = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            while (t != typeof(object) && t.BaseType != typeof(object) && t.BaseType != null)
            {
                t = t.BaseType;
                fieldInfos.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList());
            }
            return fieldInfos.DistinctBy(info => info.Name).ToDictionary(info => info.Name);
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> t, Action<T> action)
        {
            foreach (var item in t)
            {
                action(item);
                yield return item;
            }
        }

        public static IEnumerable<T> Output<T>(this IEnumerable<T> source, string label = "")
        {
            return source.Do(x => Console.WriteLine(label + x));
        }

        public static IEnumerable<ClrObject> GetObjectField(this IEnumerable<ClrObject> source, string field)
        {
            return source.Select(o => o.GetObjectField(field));
        }

        public static IEnumerable<T> GetValueField<T>(this IEnumerable<ClrObject> source, string field) where T : struct
        {
            return source.Select(o => o.GetField<T>(field));
        }

        public static IEnumerable<ClrObject> OfClrType(this IEnumerable<ClrObject> source, string type)
        {
            return source.Where(o => o.Type.ToString() == type);
        }
    }
}
