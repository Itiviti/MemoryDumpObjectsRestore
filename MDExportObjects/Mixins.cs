using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Diagnostics.Runtime;
using SampleProject;

namespace MDExportObjects
{
    public static class Mixins
    {
        public static IEnumerable<Pair> Export(this IEnumerable<ClrObject> source, Type type)
        {
            return source
                .Select(x => new Pair(x, FormatterServices.GetUninitializedObject(type)))
                .Select(ExtractPrimitiveFields)
                .Select(ExtractStructFields)
                .Select(ExtractArray)
                .Select(ExtractReferenceObjects)

                ;
        }

        private static Pair ExtractArray(Pair p)
        {
            var obj = p.ClrObj;
            foreach (var field in obj.Type.Fields
                .Where(field => field.Type.IsArray))
            {
                Array arr;
                if (ExtractArray(p, obj, field, out arr)) continue;

                p.fieldInfo[field.Name].SetValue(p.RuntimeObject, arr);
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

                string typeString2 = HackGenerics(array.Type.ComponentType);
                var type = Type.GetType(typeString2);
                if (type == null)
                {
                    p.Errors.Add($"Can't get type for {typeString2}");
                    return true;
                }
                arr = Array.CreateInstance(type, len);
                for (int i = 0; i < len; i++)
                {
                    ulong address = (ulong) array.Type.GetArrayElementValue(array.Address, i);
                    ClrType otype = Program._runtime.Heap.GetObjectType(address);
                    string typeString = HackGenerics(otype);
                    var type1 = Type.GetType(typeString);
                    if (type1 == null)
                    {
                        p.Errors.Add($"Can't get type for {typeString}");
                        continue;
                    }
                    var clrObject = new ClrObject(address, otype);
                    var value = Export(Yield(clrObject), type1).FirstOrDefault();
                    p.Warnings.AddRange(value.Warnings);
                    p.Errors.AddRange(value.Errors);
                    arr.SetValue(value.RuntimeObject, i);
                }
            }
            return false;
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
                if (ExtractRefObject(p, field, obj, out reff)) continue;

                p.fieldInfo[field.Name].SetValue(p.RuntimeObject, reff);
            }

            return p;
        }

        private static bool ExtractRefObject(Pair p, ClrInstanceField field, ClrObject obj, out object reff)
        {
            reff = null;
            string typeString = HackGenerics(field.Type);
            var type = Type.GetType(typeString);
            if (type == null)
            {
                p.Errors.Add($"Can't get type for {typeString}");
                return true;
            }

            var reference = obj.GetObjectField(field.Name);
            reff = null;
            if (reference.IsNull)
            {
                p.Warnings.Add($"Can't get info for {obj.Type.Name} -> {field.Name}");
                reff = FormatterServices.GetUninitializedObject(type);
            }
            else
            {
                var par = Export(Yield(reference), type).Single();
                reff = par.RuntimeObject;
                p.Warnings.AddRange(par.Warnings);
                p.Errors.AddRange(par.Errors);
            }
            return false;
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
                this.fieldInfo = GetFields(runtimeObject.GetType());
                Errors = new List<string>();
                Warnings = new List<string>();
            }

            private static Dictionary<string, FieldInfo> GetFields(Type t)
            {
                var fieldInfos = t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).ToList();
                return fieldInfos.ToDictionary(info => info.Name);
            }

            public ClrObject ClrObj;
            public object RuntimeObject;
            public Dictionary<string, FieldInfo> fieldInfo;
            public List<string> Errors;
            public List<string> Warnings;
        }

        public static Pair ExtractPrimitiveFields(Pair p)
        {
            foreach (var field in p.ClrObj.Type.Fields.Where(field => field.IsPrimitive || field.Type.IsString))
            {
                var value = field.GetValue(p.ClrObj.Address);

                p.fieldInfo[field.Name].SetValue(p.RuntimeObject, value);
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
                string typeString = HackGenerics(field.Type);

                var assName = Path.ChangeExtension(Path.GetFileName(field.Type.Module.AssemblyName), null);
                var ass =
                    Assembly
                        .GetExecutingAssembly()
                        .GetReferencedAssemblies()
                        .FirstOrDefault(name => assName == name.Name);
                Type type;
                if (ass != null)
                {
                    type = Type.GetType(Assembly.CreateQualifiedName(ass.Name, typeString));
                }
                else
                {
                    type = Type.GetType(typeString);
                }

                if (type == null)
                {
                    p.Errors.Add($"Can't get type for {typeString}");
                    continue;
                }

                var fieldInfos = GetFields(type);
                ClrValueClass structClass = obj.GetValueClassField(field.Name);
                var result = FormatterServices.GetUninitializedObject(type);

                foreach (var fld in structClass.Type.Fields)
                {
                    // object res = ExtractRes(fld, structClass.Address, structClass);

                    // var value = field.GetValue(structClass.Address);
                    object value = null;
                    if (fld.Type.IsString)
                        value = structClass.GetStringField(fld.Name);
                    else if (fld.Type.IsPrimitive)
                        value = fld.GetValue(structClass.Address, true);

                    fieldInfos[fld.Name].SetValue(result, value);
                }

                p.fieldInfo[field.Name].SetValue(p.RuntimeObject, result);
            }

            return p;
        }

        private static Dictionary<string, FieldInfo> GetFields(Type t)
        {
            var fieldInfos = t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            return fieldInfos.ToDictionary(info => info.Name);
        }

        private static object ExtractRes(ClrInstanceField field, ulong address, ClrValueClass structClass)
        {
            if (field.IsPrimitive || field.Type.IsString)
                return field.GetValue(address);

            if (field.Type.IsArray)
            {
                var array = structClass.GetObjectField(field.Name);

                if (array.Type.ComponentType.IsString || array.Type.ComponentType.IsPrimitive)
                {
                    return CreatePrimitiveArray(array);
                }
                if (array.Type.ComponentType.IsValueClass)
                {
                    //TODO: create value class array;
                    return null;
                }

                return CreateReferenceArray(field, address, array);
            }
            if (field.IsObjectReference)
            {
                string typeString = HackGenerics(field.Type);
                var type = Type.GetType(typeString);
                if (type == null)
                {
                    //TODO:  p.Errors.Add($"Can't get type for {typeString}");
                    return null;
                }

                var reference = structClass.GetObjectField(field.Name);

                if (reference.IsNull)
                {
                    //TODO:  p.Warnings.Add($"Can't get info for {obj.Type.Name} -> {field.Name}");
                    return FormatterServices.GetUninitializedObject(type);
                }
                else
                {
                    var par = Export(Yield(reference), type).Single();
                    return par.RuntimeObject;
                    //TODO: p.Warnings.AddRange(par.Warnings);
                    //TODO: p.Errors.AddRange(par.Errors);
                }
            }

            return null;
        }

        private static object CreateReferenceArray(ClrInstanceField field, ulong address, ClrObject array)
        {
            int len = field.Type.GetArrayLength(array.Address);

            string typeString2 = HackGenerics(array.Type.ComponentType);
            var type = Type.GetType(typeString2);
            if (type == null)
            {
                // p.Errors.Add($"Can't get type for {typeString2}");
                return null;
            }
            var arr = Array.CreateInstance(type, len);
            for (int i = 0; i < len; i++)
            {
                ulong adr = (ulong) array.Type.GetArrayElementValue(array.Address, i);
                ClrType otype = Program._runtime.Heap.GetObjectType(adr);
                string typeString = HackGenerics(otype);
                var type1 = Type.GetType(typeString);
                if (type1 == null)
                {
                    // p.Errors.Add($"Can't get type for {typeString}");
                    return null;
                }
                var clrObject = new ClrObject(address, otype);
                var value = Export(Yield(clrObject), type1).FirstOrDefault();
                // p.Warnings.AddRange(value.Warnings);
                // p.Errors.AddRange(value.Errors);
                arr.SetValue(value.RuntimeObject, i);
            }

            return arr;
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
