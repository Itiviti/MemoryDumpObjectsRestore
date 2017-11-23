using Microsoft.Diagnostics.Runtime;

namespace MemDumpBrowser
{
    internal class ObjData
    {
        private ClrObject ob;

        public ObjData(ClrObject ob)
        {
            this.ClrObject = ob;
        }

        public ClrObject ClrObject
        {
            get
            {
                return ob;
            }

            set
            {
                ob = value;
            }
        }
    }
}