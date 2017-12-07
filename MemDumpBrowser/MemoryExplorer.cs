using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MemDumpBrowser
{
    internal class MemoryExplorer : IDisposable
    {
        private DataTarget _target;
        private ClrRuntime _runtime;
        private List<ClrObject> _objects = new List<ClrObject>();

        public List<ClrObject> Objects
        {
            get
            {
                return _objects;
            }
        }

        public IEnumerable<ClrThread> Threads => _runtime.Threads;

        public void LoadDump(string fileName)
        {
            _target?.Dispose();

            _target = DataTarget.LoadCrashDump(fileName);
            ClrInfo runtimeInfo = _target.ClrVersions[0];//.ClrInfo[0];  // just using the first runtime
            _runtime = runtimeInfo.CreateRuntime();

            LoadObjects();
        }

        public void LoadDump(Process fileName)
        {
            _target?.Dispose();

            _target = DataTarget.AttachToProcess(fileName.Id, 5000, AttachFlag.Invasive);
            ClrInfo runtimeInfo = _target.ClrVersions[0];//.ClrInfo[0];  // just using the first runtime
            _runtime = runtimeInfo.CreateRuntime();

            LoadObjects();
        }

        public IEnumerable<AFV> ExpandArray(ClrObject array)
        {
            var type = array.Type;

            int len = type.GetArrayLength(array.Address);
            for (int i = 0; i < len; i++)
                yield return new AFV(i, type.GetArrayElementValue(array.Address, i), type.GetArrayElementAddress(array.Address, i));
        }

        public IEnumerable<object> ShowFields(ClrObject o)
        {
            if (o.Type.IsString | o.Type.IsPrimitive)
                return new SFV[] { new SFV(o.Type, o.Type.GetValue(o.Address), o.Address) };

            if (o.IsArray)
                return ExpandArray(o);
            else
                return o.Type.Fields.Select(x => new FV(x, x.GetValue(o.Address), o.Address));//.Concat(o.EnumerateObjectReferences().Select(x => new FV(x, x.v, o.Address)));
        }

        public void LoadObjects()
        {
            var heap = _runtime.Heap;

            if (!heap.CanWalkHeap)
            {
                Console.WriteLine("Cannot walk the heap!");
            }
            else
            {
                _objects.Clear();
                foreach (var item in heap.EnumerateObjects())
                {
                    _objects.Add(item);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
