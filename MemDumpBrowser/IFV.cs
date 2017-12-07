using Microsoft.Diagnostics.Runtime;

namespace MemDumpBrowser
{
    public interface IFV
    {
        ulong Address { get; }
        ClrType Type { get; }
    }
}