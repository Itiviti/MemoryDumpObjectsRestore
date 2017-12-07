using Microsoft.Diagnostics.Runtime;

namespace MemDumpBrowser
{
    internal class SFV //: IFV
    {
        private object value;
        private ClrType type;
        private ulong address;

        public SFV(ClrType type, object value, ulong address)
        {
            this.Type = type;
            this.Value = value;
            this.Address = address;
        }

        public ulong Address
        {
            get
            {
                return address;
            }

            set
            {
                address = value;
            }
        }

        public ClrType Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public object Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
            }
        }
    }
}