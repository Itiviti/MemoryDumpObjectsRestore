using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MemDumpBrowser
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            LoadDump(@"C:\Users\adrian.rus\MemDumpBrowser.DMP");
            this.Closed += MainForm_Closed;
            treeView1.BeforeExpand += TreeView1_BeforeExpand;
        }

        Sample sample = new Sample();

        private void TreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var node = e.Node;
            if (node.Nodes[0].Tag != null) return;

            node.Nodes.RemoveAt(0);

            var obj = (ObjData)node.Tag;

            node.Nodes.AddRange(obj.ClrObject.EnumerateObjectReferences().Select(o => GetNode(o)).ToArray());
        }

        private void MainForm_Closed(object sender, EventArgs e)
        {
            _target?.Dispose();
        }

        DataTarget _target;
        ClrRuntime _runtime;
        private List<ClrObject> _objects = new List<ClrObject>();

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Multiselect = false;
            fd.Filter = "*.dmp|*.dmp|*.*|*.*";
            
            if (fd.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = fd.FileName;
                LoadDump(fileName);
            }
        }

        private async void LoadDump(string fileName)
        {
            _target?.Dispose();
            lblStatus.Text = fileName;
            _target = DataTarget.LoadCrashDump(fileName);
            ClrInfo runtimeInfo = _target.ClrVersions[0];//.ClrInfo[0];  // just using the first runtime
            _runtime = runtimeInfo.CreateRuntime();
            LoadThreads();
            btnFind.Enabled = false;
            //await Task.Run(new Action(LoadObjects));
            LoadObjects();
            btnFind.Enabled = true;
        }

        private void LoadObjects()
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

        private void openProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (_target == null) return;

            treeView1.Nodes.Clear();
            foreach (var obj in _objects
            .Where(o => o.Type.ToString().Contains(txtFind.Text)))
            {
                treeView1.Nodes.Add(GetNode(obj));
            }
        }

        private TreeNode GetNode(ClrObject o1)
        {
            var o = new ObjData(o1);
            TreeNode tn = new TreeNode(o1.Type.ToString(), new TreeNode[] { new TreeNode("Loading...") });
            tn.Tag = o;
            return tn;
        }

        private void findThreads_Click(object sender, EventArgs e)
        {
            
        }

        private void LoadThreads()
        {
            treeThreads.Nodes.Clear();
            foreach (ClrThread thread in _runtime.Threads)
            {
                if (!thread.IsAlive)
                    continue;

                var td = new ThreadData(thread);
                var tn = new TreeNode(td.ToString(), td.Stack.Select(x => new TreeNode(x)).ToArray());
                tn.Tag = td;

                treeThreads.Nodes.Add(tn);

            }
        }

        private void expandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node== null) return;
            var obj  = (ObjData)node.Tag;

            // node.

        }
    }
}
