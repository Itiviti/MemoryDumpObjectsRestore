using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;
using SampleProject;
using static MDExportObjects.Mixins;

namespace MDExportObjects
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: MDExportObjects \"dump\"");
                return;
            }

            string file = args[0];

            Load<SimpleArrayType>(LoadDump(file));
        }

        public static List<Pair> Load<T>(IEnumerable<ClrObject> objects)
        {
            var pairs = objects
                .Where(o => o.Type.ToString() == typeof(T).FullName).ToList();

            var export = pairs.Export(typeof(T)).ToList();
            
            
            export.SelectMany(p => p.Errors).Output("Error: ").ToList();
            export.SelectMany(p => p.Warnings).Output("Warn: ").ToList();
            return export;
        }

        internal static DataTarget _target;
        internal static ClrRuntime _runtime;

        public static IEnumerable<ClrObject> LoadDump(string fileName)
        {
            _target?.Dispose();

            _target = DataTarget.LoadCrashDump(fileName);

            ClrInfo runtimeInfo = _target.ClrVersions[0];
            _runtime = runtimeInfo.CreateRuntime();

            var heap = _runtime.Heap;

            if (!heap.CanWalkHeap)
            {
                Console.WriteLine("Cannot walk the heap!");
                return Enumerable.Empty<ClrObject>();
            }

            return heap.EnumerateObjects()
                .Where(o => o.Type != null);
        }
    }
}
