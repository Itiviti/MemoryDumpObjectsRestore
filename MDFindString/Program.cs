using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Diagnostics.Runtime;

namespace MDFindString
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: MDFindString \"dump\" \"string\"");
                return;
            }

            string file = args[0];
            string search = args[1];
            LoadDump(file, search);
        }

        private static DataTarget _target;
        private static ClrRuntime _runtime;
        private static List<ClrObject> _strings = new List<ClrObject>();

        private static void LoadDump(string fileName, string search)
        {
            _target?.Dispose();

            _target = DataTarget.LoadCrashDump(fileName);
            ClrInfo runtimeInfo = _target.ClrVersions[0];
            _runtime = runtimeInfo.CreateRuntime();

            var heap = _runtime.Heap;

            if (!heap.CanWalkHeap)
            {
                Console.WriteLine("Cannot walk the heap!");
            }
            else
            {
                _strings.Clear();
                foreach (var item in heap.EnumerateObjects()
                    .Where(o => o.Type != null)
                    .Where(o => o.Type.IsString)
                    .Select(o => new
                    {
                        Obj = o,
                        Val = o.Type.GetValue(o.Address).ToString()
                    })
                    .Where(arg => arg.Val.StartsWith(search))
                    )

                {
                    _strings.Add(item.Obj);
                    Console.WriteLine(item.Val+ " " + item.Obj.HexAddress);
                }
            }
        }
    }
}
