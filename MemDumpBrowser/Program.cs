using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MemDumpBrowser
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //MemoryExplorer me = new MemoryExplorer();
            //me.LoadDump(@"C:\Users\adrian.rus\AppData\Local\Temp\MemDumpBrowser (2).DMP");
            //var outp = me.Objects.Where(c => c.Type.ToString().Contains("MemDumpBrowser.Sample"));
            //var o = outp.Last();
            //var array = o.EnumerateObjectReferences().Last();
            
            //var A1 = outp.SelectMany(x => me.ShowFields(x));
            //A1.ToList().Output().ToList();

            //return;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
