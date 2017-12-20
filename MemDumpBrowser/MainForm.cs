using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
            this.Closed += MainForm_Closed;
            treeView1.BeforeExpand += TreeView1_BeforeExpand;
            treeView1.AfterSelect += TreeView1_AfterSelect;
            dataGridView1.AutoGenerateColumns = true;

            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = "Memory Explorer!";
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var node = e.Node;

            var obj = (ObjData)node.Tag;

            dataGridView1.DataSource = _target.ShowFields(obj.ClrObject).ToList();
            dataGridView1.AutoResizeColumns();
        }

        private Sample sample = new Sample("arus", 12);
        private readonly MemoryExplorer _target = new MemoryExplorer();

        private void TreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var node = e.Node;
            if (node.Nodes[0].Tag != null) return;

            node.Nodes.RemoveAt(0);

            var obj = (ObjData)node.Tag;

            node.Nodes.AddRange(obj.ClrObject.EnumerateObjectReferences().Select(GetNode).ToArray());
        }

        private void MainForm_Closed(object sender, EventArgs e)
        {
            _target?.Dispose();
        }

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

        private void LoadDump(string fileName)
        {
            _target?.Dispose();
            _target.LoadDump(fileName);

            lblStatus.Text = fileName;

            LoadThreads();
            btnFind.Enabled = false;
            _target.LoadObjects();
            btnFind.Enabled = true;
        }

        private void LoadDump(Process fileName)
        {
            _target?.Dispose();
            _target.LoadDump(fileName);

            lblStatus.Text = fileName.ProcessName;

            LoadThreads();
            btnFind.Enabled = false;
            _target.LoadObjects();
            btnFind.Enabled = true;
        }

        private void openProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (_target == null) return;

            treeView1.SuspendLayout();

            treeView1.Nodes.Clear();
            treeView1.Nodes.AddRange(
            _target.Objects
            .Where(o => o.Type.ToString().StartsWith(txtFind.Text))
            .Select(x => GetNode(x)).ToArray());

            treeView1.ResumeLayout();
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
            foreach (ClrThread thread in _target.Threads)
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

        private void txtFind_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                btnFind.PerformClick();
            }
        }
    }
}
