using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemDumpBrowser
{
    public static class Mixins
    {

        public static IEnumerable<T> Output<T>(this IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                Console.WriteLine(item);
                yield return item;
            }
            
        }

        public static void Output(this object source)
        {
            Console.WriteLine(source);
        }
    }
}
