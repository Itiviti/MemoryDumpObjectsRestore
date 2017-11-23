using Microsoft.Diagnostics.Runtime;
using System.Collections.Generic;

namespace MemDumpBrowser
{
    internal class ThreadData
    {
        private ClrThread thread;

        public ThreadData(ClrThread thread)
        {
            this.thread = thread;

            foreach (ClrStackFrame frame in thread.StackTrace)
                _stack.Add(frame.ToString());
        }

        List<string> _stack = new List<string>();

        public List<string> Stack
        {
            get
            {
                return _stack;
            }

            set
            {
                _stack = value;
            }
        }

        public override string ToString()
        {
            return string.Format("Thread {0:X}", thread.OSThreadId);
        }
    }
}