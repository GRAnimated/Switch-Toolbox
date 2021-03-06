﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Switch_Toolbox.Library;
using Switch_Toolbox.Library.IO;
using BarsLib;
using WeifenLuo.WinFormsUI.Docking;
using VGAudio.Formats;
using VGAudio;
using VGAudio.Containers.NintendoWare;
using VGAudio.Containers.Wave;
using NAudio.Wave;

namespace FirstPlugin
{
    public class BARS : TreeNodeFile, IFileFormat
    {
        public bool CanSave { get; set; } = false;
        public bool FileIsEdited { get; set; } = false;
        public bool FileIsCompressed { get; set; } = false;
        public string[] Description { get; set; } = new string[] { "Sound Archive" };
        public string[] Extension { get; set; } = new string[] { "*.bars" };
        public string Magic { get; set; } = "BARS";
        public CompressionType CompressionType { get; set; } = CompressionType.None;
        public byte[] Data { get; set; }
        public string FileName { get; set; }
        public bool IsActive { get; set; } = false;
        public bool UseEditMenu { get; set; } = false;
        public int Alignment { get; set; } = 0;
        public string FilePath { get; set; }
        public IFileInfo IFileInfo { get; set; }

        public Type[] Types
        {
            get
            {
                List<Type> types = new List<Type>();
                return types.ToArray();
            }
        }

        public class AudioEntry : TreeNodeCustom
        {
            public AudioType Type;
            public byte[] Data;
            BFAVEditor BFAVEditor;

            public AudioEntry()
            {
                ContextMenu = new ContextMenu();
                MenuItem export = new MenuItem("Export");
                ContextMenu.MenuItems.Add(export);
                export.Click += Export;

                MenuItem replace = new MenuItem("Replace");
                ContextMenu.MenuItems.Add(replace);
                replace.Click += Replace;
            }
            public void SetupMusic()
            {
                if (Type == AudioType.Bfwav)
                {
                    ImageKey = "bfwav";
                    SelectedImageKey = "bfwav";
                }
                else if (Type == AudioType.Bfstp)
                {
                    ImageKey = "bfstp";
                    SelectedImageKey = "bfstp";
                }
                else
                {
                    ImageKey = "fileBlank";
                    SelectedImageKey = "fileBlank";
                }
            }
            public AudioData GetAudioData()
            {
                BCFstmReader reader = new BCFstmReader();
                return reader.Read(Data);
            }
            public byte[] BfwavToWav()
            {
                MemoryStream mem = new MemoryStream();
                WaveWriter writer = new WaveWriter();

                AudioData audioData = GetAudioData();
                writer.WriteToStream(audioData, mem);

                return mem.ToArray();
            }

            private void Export(object sender, EventArgs args)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = Text;
                sfd.DefaultExt = Path.GetExtension(Text);
                sfd.Filter = "All files(*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, Data);
                }
            }

            private void Replace(object sender, EventArgs args)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.FileName = Text;
                ofd.DefaultExt = Path.GetExtension(Text);
                ofd.Filter = "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Data = File.ReadAllBytes(ofd.FileName);
                    UpdateEditor();
                }
            }
            public void UpdateEditor()
            {
                if (Viewport.Instance.gL_ControlModern1.Visible == false)
                    PluginRuntime.FSHPDockState = WeifenLuo.WinFormsUI.Docking.DockState.Document;

                BFAVEditor docked = (BFAVEditor)LibraryGUI.Instance.GetContentDocked(new BFAVEditor());
                if (docked == null)
                {
                    docked = new BFAVEditor();
                    LibraryGUI.Instance.LoadDockContent(docked, PluginRuntime.FSHPDockState);
                }
                docked.Text = Text;
                docked.Dock = DockStyle.Fill;
                docked.LoadFile(this);
            }

            public override void OnClick(TreeView treeview)
            {
                if (Type == AudioType.Bfwav)
                {
                    UpdateEditor();
                }
            
            } bool EditorIsActive(DockContent dock)
            {
                foreach (Control ctrl in dock.Controls)
                {
                    if (ctrl is BFAVEditor)
                    {
                        dock.Text = Text;
                        ((BFAVEditor)ctrl).LoadFile(this);
                        return true;
                    }
                }

                return false;
            }
        }

        public BarsLib.BARS bars;
        public void Load()
        {
            IsActive = true;
            CanSave = true;

            Text = FileName;

            bars = new BarsLib.BARS(new MemoryStream(Data));
            Nodes.Add("AMTA");
            Nodes.Add("Audio");
            for (int i = 0; i < bars.AmtaList.Count; i++)
            {
                string audioName = bars.AmtaList[i].Name;

                Nodes[0].Nodes.Add(audioName + ".amta");
                BARSAudioFile audio = bars.audioList[i];

                AudioEntry node = new AudioEntry();
                node.Type = audio.AudioType;
                node.Data = audio.data;
                node.SetupMusic();

                if (audio.AudioType == AudioType.Bfwav)
                    node.Text = audioName + ".bfwav";
                else if (audio.AudioType == AudioType.Bfstp)
                    node.Text = audioName + ".bfstp";
                else 
                    node.Text = audioName + ".UNKOWN";

                Nodes[1].Nodes.Add(node);
            }

            ContextMenu = new ContextMenu();
            MenuItem save = new MenuItem("Save");
            ContextMenu.MenuItems.Add(save);
            save.Click += Save;
        }
        public void Unload()
        {

        }
        private void Save(object sender, EventArgs args)
        {
            List<IFileFormat> formats = new List<IFileFormat>();

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = Utils.GetAllFilters(formats);
            sfd.FileName = FileName;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                STFileSaver.SaveFileFormat(this, sfd.FileName);
            }
        }
        public byte[] Save()
        {
            MemoryStream mem = new MemoryStream();

            foreach (TreeNode node in Nodes[1].Nodes)
            {
                for (int i = 0; i < bars.AmtaList.Count; i++)
                {
                    string audioName = bars.AmtaList[i].Name;

                    if (Path.GetFileNameWithoutExtension(node.Text) == audioName)
                    {
                        Console.WriteLine(audioName);
                        bars.audioList[i].data = ((AudioEntry)node).Data;
                    }
                }
            }

            bars.Save(mem);
            return mem.ToArray();
        }
    }
}
