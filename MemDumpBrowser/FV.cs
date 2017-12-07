using Microsoft.Diagnostics.Runtime;

namespace MemDumpBrowser
{

    public class FV //:IFV
    {
        private readonly object value;
        private readonly ulong address;
        private readonly ClrInstanceField field;
        

        public ClrInstanceField Field
        {
            get
            {
                return field;
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
                return address;
            }
        }
        public ClrType Type
        {
            get
            {
                return field.Type;
            }
        }


        public FV(ClrInstanceField field, object value, ulong address)
        {
            this.field = field;
            this.value = value;
            this.address = address;
        }

        public override string ToString()
        {
            return Field.ToString() + " " + value.ToString() + " " + address;
        }
    }
}