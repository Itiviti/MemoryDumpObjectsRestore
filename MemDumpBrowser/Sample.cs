using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemDumpBrowser
{
    class Sample
    {
        public Sample(string v1, int v2)
        {
            this.Name = v1;
            this.Age = v2;
            _list = new List<string>() { "adi", "rus" };

        }
        
        string Name { get; set; }
        int Age { get; set; }

        private List<string> _list = new List<string>();

        private int[] _ints = new[] { 32, 33, 45 };
    }
}
