using Microsoft.Diagnostics.Runtime;

namespace MemDumpBrowser
{
    internal class AFV// : IFV
    {
        private readonly ClrType type;
        private readonly ulong vaddress;
        private readonly object value;
        private readonly int index;

        public int Index
        {
            get
            {
                return index;
            }
        }

        public object Value
        {
            get
            {
                return value;
            }
        }

        public ulong Address
        {
            get
            {
                return vaddress;
            }
        }

        public ClrType Type
        {
            get
            {
                return type;
            }
        }

        public AFV(int index, object value, ulong vaddress)
        {
            this.index = index;
            this.value = value;
            this.vaddress = vaddress;
            
        }
    }
}