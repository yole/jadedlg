using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace JadeDlg
{
    public partial class MainForm : Form
    {
        private const string DUMMY_MARKER = "<dummy>";
        private Game _game;

        public MainForm(string jadeEmpirePath)
        {
            _game = new Game(jadeEmpirePath);
            InitializeComponent();
            FillDialogTree();
        }

        private void FillDialogTree()
        {
            foreach(ResourceFile rim in _game.ResourceFiles)
            {
                string path = rim.Path.Substring(_game.DataPath.Length + 1);
                if (ContainsDialogs(path))
                {
                    TreeNode node = _resourceTree.Nodes.Add(path);
                    node.Tag = rim;
                    // could use Win32 interop to set child count without adding real nodes, but better keep things simple
                    node.Nodes.Add(DUMMY_MARKER);
                }
            }
        }

        private static bool ContainsDialogs(string rimPath)
        {
            rimPath = rimPath.ToLower();
            // .rim file in a subdirectory, with a name starting with "a" and no "-a" suffix
            return rimPath == "global-a.rim" || (rimPath.Contains("\\a") && !rimPath.Contains("-a"));
        }

        private void _dlgTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes [0].Text == DUMMY_MARKER)
            {
                e.Node.Nodes.RemoveAt(0);
                FillDialogs(e.Node);
            }
        }

        private void FillDialogs(TreeNode node)
        {
            ResourceFile file = (ResourceFile) node.Tag;
            foreach(Resource resource in file.Resources)
            {
                if (resource.Type == Resource.TYPE_DIALOG)
                {
                    TreeNode dlgNode = node.Nodes.Add(resource.Name);
                    dlgNode.Tag = resource;
                }
            }
        }

        private void _dlgTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Resource)
                DisplayDialog((Resource) e.Node.Tag);
            else
                ClearView();
        }

        private void DisplayDialog(Resource resource)
        {
            ClearView();
            GffStruct gff = GffReader.Read(resource.Data);
            Dialog dlg = new Dialog(_game, gff);
            _structureTree.BeginUpdate();
            try
            {
                ShowStruct(gff, _structureTree.Nodes);
            }
            finally
            {
                _structureTree.EndUpdate();
            }
            foreach(DialogNode node in dlg.StartingNodes)
            {
                AddDialogNode(node, _dialogTree.Nodes);
            }
        }

        private void ClearView()
        {
            _dialogTree.Nodes.Clear();
            _structureTree.Nodes.Clear();
            _detailsTree.Nodes.Clear();
            _textView.Text = "";
        }

        private static void AddDialogNode(DialogNode node, TreeNodeCollection nodes)
        {
            TreeNode treeNode = nodes.Add(node.DisplayText);
            treeNode.Tag = node;
            if (node.HasReplies)
                treeNode.Nodes.Add(DUMMY_MARKER);
        }

        private void ShowStruct(GffStruct gff, TreeNodeCollection nodes)
        {
            foreach(string name in gff.FieldNames)
            {
                object value = gff [name];
                if (value is List<GffStruct>)
                {
                    List<GffStruct> list = (List<GffStruct>) value;
                    TreeNode node = nodes.Add(name);
                    for(int i=0; i<list.Count; i++)
                    {
                        TreeNode structNode = node.Nodes.Add(i.ToString());
                        ShowStruct(list [i], structNode.Nodes);
                    }
                }
                else if (value is TalkRef)
                {
                    TalkRef talkRef = (TalkRef) value;
                    nodes.Add(name + "=\"" + _game.TalkFile[talkRef.Index] + "\"");
                }
                else
                    nodes.Add(name + "=" + value);
            }
        }

        private void _dialogTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _detailsTree.Nodes.Clear();
            DialogNode node = (DialogNode) e.Node.Tag;
            ShowStruct(node.Details, _detailsTree.Nodes);
            _textView.Text = node.Text;
        }

        private void _dialogTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == DUMMY_MARKER)
            {
                e.Node.Nodes.Clear();
                DialogNode node = (DialogNode) e.Node.Tag;
                List<DialogNode> replies = node.Replies;
                foreach(DialogNode reply in replies)
                    AddDialogNode(reply, e.Node.Nodes);
            }
        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            Process.Start(((ToolStripStatusLabel) sender).Text);
        }

    }
}