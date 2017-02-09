﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Drawing.Text;
using System.Threading.Tasks;

/*TODO
For extracting bundles, first check if file exists then decompress
Font index error in Dreamfall Chapters
*/

namespace Unity_Studio
{
    partial class UnityStudioForm : Form
    {
        private UnityStudio ustudio = new UnityStudio();

        private AssetPreloadData lastSelectedItem = null;
        private AssetPreloadData lastLoadedAsset = null;
        //private AssetsFile mainDataFile = null;
              
        private string[] fileTypes = new string[] { "globalgamemanagers", "maindata.", "level*.", "*.assets", "*.sharedAssets", "CustomAssetBundle-*", "CAB-*", "BuildPlayer-*" };
 
        private FMOD.System system = null;
        private FMOD.Sound sound = null;
        private FMOD.Channel channel = null;
        private FMOD.SoundGroup masterSoundGroup = null;
        //private FMOD.ChannelGroup channelGroup = null;
        private FMOD.MODE loopMode = FMOD.MODE.LOOP_OFF;
        private uint FMODlenms = 0;
        private float FMODVolume = 0.8f;
        private float FMODfrequency;

        private Bitmap imageTexture = null;

        //asset list sorting helpers
        private int firstSortColumn = -1;
        private int secondSortColumn = 0;
        private bool reverseSort = false;
        private bool enableFiltering = false;

        //tree search
        private int nextGObject = 0;
        List<GameObject> treeSrcResults = new List<GameObject>();

        //counters for progress bar
        //private int totalAssetCount = 0;
        //private int totalTreeNodes = 0;

        PrivateFontCollection pfc = new PrivateFontCollection();

        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);


        private void loadFile_Click(object sender, System.EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                resetForm();
                ustudio.mainPath = Path.GetDirectoryName(openFileDialog1.FileNames[0]);
                Task task = null;
                if (openFileDialog1.FilterIndex == 1)
                {
                    ustudio.MergeSplitAssets(ustudio.mainPath);

                    //unityFiles.AddRange(openFileDialog1.FileNames);
                    foreach (var i in openFileDialog1.FileNames)
                    {
                        ustudio.unityFiles.Add(i);
                        ustudio.unityFilesHash.Add(Path.GetFileName(i));
                    }
                    progressBar1.Value = 0;
                    progressBar1.Maximum = ustudio.unityFiles.Count;
                    task = new Task(() =>
                    {
                        //use a for loop because list size can change
                        for (int f = 0; f < ustudio.unityFiles.Count; f++)
                        {
                            StatusStripUpdate("Loading " + Path.GetFileName(ustudio.unityFiles[f]));
                            ustudio.LoadAssetsFile(ustudio.unityFiles[f]);
                            ProgressBarPerformStep();
                        }
                    });
                }
                else
                {
                    progressBar1.Value = 0;
                    progressBar1.Maximum = openFileDialog1.FileNames.Length;
                    task = new Task(() =>
                    {
                        foreach (var filename in openFileDialog1.FileNames)
                        {
                            ustudio.LoadBundleFile(filename);
                            ProgressBarPerformStep();
                        }
                    });
                }
                task.ContinueWith(task2 => { BuildAssetStrucutres(); });
                task.ContinueWith(task2 => { ustudio.unityFilesHash.Clear(); ustudio.assetsfileListHash.Clear(); });
                task.Start();
            }
        }

        private void loadFolder_Click(object sender, System.EventArgs e)
        {
            /*FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();

            folderBrowserDialog1.Description = "Load all Unity assets from folder and subfolders";
            folderBrowserDialog1.ShowNewFolderButton = false;
            //folderBrowserDialog1.SelectedPath = "E:\\Assets\\Unity";
            folderBrowserDialog1.SelectedPath = "E:\\Assets\\Unity\\WebPlayer\\Porsche\\92AAF1\\defaultGeometry";*/

            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                //mainPath = folderBrowserDialog1.SelectedPath;
                ustudio.mainPath = openFolderDialog1.FileName;
                if (Path.GetFileName(ustudio.mainPath) == "Select folder")
                { ustudio.mainPath = Path.GetDirectoryName(ustudio.mainPath); }

                if (Directory.Exists(ustudio.mainPath))
                {
                    resetForm();

                    //TODO find a way to read data directly instead of merging files
                    ustudio.MergeSplitAssets(ustudio.mainPath);

                    for (int t = 0; t < fileTypes.Length; t++)
                    {
                        string[] fileNames = Directory.GetFiles(ustudio.mainPath, fileTypes[t], SearchOption.AllDirectories);
                        #region  sort specific types alphanumerically
                        if (fileNames.Length > 0 && (t == 1 || t == 2))
                        {
                            var sortedList = fileNames.ToList();
                            sortedList.Sort((s1, s2) =>
                            {
                                string pattern = "([A-Za-z\\s]*)([0-9]*)";
                                string h1 = Regex.Match(Path.GetFileNameWithoutExtension(s1), pattern).Groups[1].Value;
                                string h2 = Regex.Match(Path.GetFileNameWithoutExtension(s2), pattern).Groups[1].Value;
                                if (h1 != h2)
                                    return h1.CompareTo(h2);
                                string t1 = Regex.Match(Path.GetFileNameWithoutExtension(s1), pattern).Groups[2].Value;
                                string t2 = Regex.Match(Path.GetFileNameWithoutExtension(s2), pattern).Groups[2].Value;
                                if (t1 != "" && t2 != "")
                                    return int.Parse(t1).CompareTo(int.Parse(t2));
                                return 0;
                            });
                            foreach (var i in sortedList)
                            {
                                ustudio.unityFiles.Add(i);
                                ustudio.unityFilesHash.Add(Path.GetFileName(i));
                            }

                        }
                        #endregion
                        else
                        {
                            foreach (var i in fileNames)
                            {
                                ustudio.unityFiles.Add(i);
                                ustudio.unityFilesHash.Add(Path.GetFileName(i));
                            }
                        }
                    }

                    ustudio.unityFiles = ustudio.unityFiles.Distinct().ToList();
                    progressBar1.Value = 0;
                    progressBar1.Maximum = ustudio.unityFiles.Count;
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        //use a for loop because list size can change
                        for (int f = 0; f < ustudio.unityFiles.Count; f++)
                        {
                            var fileName = ustudio.unityFiles[f];
                            StatusStripUpdate("Loading " + Path.GetFileName(fileName));
                            ustudio.LoadAssetsFile(fileName);
                            ProgressBarPerformStep();
                        }
                        ustudio.unityFilesHash.Clear();
                        ustudio.assetsfileListHash.Clear();
                        BuildAssetStrucutres();
                    });
                }
                else { StatusStripUpdate("Selected path deos not exist."); }
            }
        }

        private void extractBundleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openBundleDialog = new OpenFileDialog();
            openBundleDialog.Filter = "Unity bundle files|*.unity3d; *.unity3d.lz4; *.assetbundle; *.bundle; *.bytes|All files (use at your own risk!)|*.*";
            openBundleDialog.FilterIndex = 1;
            openBundleDialog.RestoreDirectory = true;
            openBundleDialog.Multiselect = true;

            if (openBundleDialog.ShowDialog() == DialogResult.OK)
            {
                progressBar1.Value = 0;
                progressBar1.Maximum = openBundleDialog.FileNames.Length;
                int extractedCount = 0;
                ThreadPool.QueueUserWorkItem(delegate
                {
                    foreach (var fileName in openBundleDialog.FileNames)
                    {
                        extractedCount += ustudio.extractBundleFile(fileName);
                        ProgressBarPerformStep();
                    }
                    StatusStripUpdate("Finished extracting " + extractedCount.ToString() + " files.");
                });
            }
        }

        private void extractFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int extractedCount = 0;
            List<string> bundleFiles = new List<string>();

            /*FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "Extract all Unity bundles from folder and subfolders";
            folderBrowserDialog1.ShowNewFolderButton = false;*/

            if (openFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                string startPath = openFolderDialog1.FileName;
                if (Path.GetFileName(startPath) == "Select folder")
                { startPath = Path.GetDirectoryName(startPath); }

                string[] fileTypes = new string[6] { "*.unity3d", "*.unity3d.lz4", "*.assetbundle", "*.assetbundle-*", "*.bundle", "*.bytes" };
                foreach (var fileType in fileTypes)
                {
                    string[] fileNames = Directory.GetFiles(startPath, fileType, SearchOption.AllDirectories);
                    bundleFiles.AddRange(fileNames);
                }
                progressBar1.Value = 0;
                progressBar1.Maximum = bundleFiles.Count;
                ThreadPool.QueueUserWorkItem(delegate
                {
                    foreach (var fileName in bundleFiles)
                    {
                        extractedCount += ustudio.extractBundleFile(fileName);
                        ProgressBarPerformStep();
                    }
                    StatusStripUpdate("Finished extracting " + extractedCount.ToString() + " files.");
                });
            }
        }

        private void BuildAssetStrucutres()
        {
            bool optionLoadAssetsMenuItem = !dontLoadAssetsMenuItem.Checked;
            bool optionDisplayAll = displayAll.Checked;
            bool optionBuildHierarchyMenuItem = !dontBuildHierarchyMenuItem.Checked;
            bool optionBuildClassStructuresMenuItem = buildClassStructuresMenuItem.Checked;

            ustudio.BuildAssetStructures(optionLoadAssetsMenuItem, optionDisplayAll, optionBuildHierarchyMenuItem, optionBuildClassStructuresMenuItem);

            BeginInvoke(new Action(() =>
            {
                if (ustudio.productName != "")
                {
                    this.Text = "Unity Studio - " + ustudio.productName + " - " + ustudio.assetsfileList[0].m_Version + " - " + ustudio.assetsfileList[0].platformStr;
                }
                else if (ustudio.assetsfileList.Count > 0)
                {
                    this.Text = "Unity Studio - no productName - " + ustudio.assetsfileList[0].m_Version + " - " + ustudio.assetsfileList[0].platformStr;
                }
                if (!dontLoadAssetsMenuItem.Checked)
                {
                    assetListView.VirtualListSize = ustudio.visibleAssets.Count;
                    resizeAssetListColumns();
                }
                if (!dontBuildHierarchyMenuItem.Checked)
                {
                    sceneTreeView.BeginUpdate();
                    sceneTreeView.Nodes.AddRange(ustudio.fileNodes.ToArray());
                    ustudio.fileNodes.Clear();
                    sceneTreeView.EndUpdate();
                }
                if (buildClassStructuresMenuItem.Checked)
                {
                    classesListView.BeginUpdate();
                    foreach (var version in ustudio.AllClassStructures)
                    {
                        ListViewGroup versionGroup = new ListViewGroup(version.Key);
                        classesListView.Groups.Add(versionGroup);

                        foreach (var uclass in version.Value)
                        {
                            uclass.Value.Group = versionGroup;
                            classesListView.Items.Add(uclass.Value);
                        }
                    }
                    classesListView.EndUpdate();
                }
                StatusStripUpdate("Finished loading " + ustudio.assetsfileList.Count.ToString() + " files with " + (assetListView.Items.Count + sceneTreeView.Nodes.Count).ToString() + " exportable assets.");
                treeSearch.Select();
                saveFolderDialog1.InitialDirectory = ustudio.mainPath;
            }));
        }

        private void UnityStudioForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.Alt && e.KeyCode == Keys.D)
            {
                debugMenuItem.Visible = !debugMenuItem.Visible;
                buildClassStructuresMenuItem.Checked = debugMenuItem.Visible;
                dontLoadAssetsMenuItem.Checked = debugMenuItem.Visible;
                dontBuildHierarchyMenuItem.Checked = debugMenuItem.Visible;
                if (tabControl1.TabPages.Contains(tabPage3)) { tabControl1.TabPages.Remove(tabPage3); }
                else { tabControl1.TabPages.Add(tabPage3); }
            }
        }

        private void dontLoadAssetsMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (dontLoadAssetsMenuItem.Checked)
            {
                dontBuildHierarchyMenuItem.Checked = true;
                dontBuildHierarchyMenuItem.Enabled = false;
            }
            else { dontBuildHierarchyMenuItem.Enabled = true; }
        }

        private void exportClassStructuresMenuItem_Click(object sender, EventArgs e)
        {
            if (ustudio.AllClassStructures.Count > 0)
            {
                if (saveFolderDialog1.ShowDialog() == DialogResult.OK)
                {
                    progressBar1.Value = 0;
                    progressBar1.Maximum = ustudio.AllClassStructures.Count;

                    var savePath = saveFolderDialog1.FileName;
                    if (Path.GetFileName(savePath) == "Select folder or write folder name to create")
                    { savePath = Path.GetDirectoryName(saveFolderDialog1.FileName); }

                    foreach (var version in ustudio.AllClassStructures)
                    {
                        if (version.Value.Count > 0)
                        {
                            string versionPath = savePath + "\\" + version.Key;
                            Directory.CreateDirectory(versionPath);

                            foreach (var uclass in version.Value)
                            {
                                string saveFile = versionPath + "\\" + uclass.Key + " " + uclass.Value.Text + ".txt";
                                using (StreamWriter TXTwriter = new StreamWriter(saveFile))
                                {
                                    TXTwriter.Write(uclass.Value.membersstr);
                                }
                            }
                        }

                        progressBar1.PerformStep();
                    }

                    StatusStripUpdate("Finished exporting class structures");
                    progressBar1.Value = 0;
                }
            }
        }

        private void enablePreview_Check(object sender, EventArgs e)
        {
            if (lastLoadedAsset != null)
            {
                switch (lastLoadedAsset.Type2)
                {
                    case 28:
                        {
                            if (enablePreview.Checked && imageTexture != null)
                            {
                                previewPanel.BackgroundImage = imageTexture;
                            }
                            else
                            {
                                previewPanel.BackgroundImage = Properties.Resources.preview;
                                previewPanel.BackgroundImageLayout = ImageLayout.Center;
                            }
                        }
                        break;
                    case 48:
                    case 49:
                    case 114:
                        textPreviewBox.Visible = !textPreviewBox.Visible;
                        break;
                    case 128:
                        fontPreviewBox.Visible = !fontPreviewBox.Visible;
                        break;
                    case 83:
                        {
                            FMODpanel.Visible = !FMODpanel.Visible;

                            if (sound != null && channel != null)
                            {
                                FMOD.RESULT result;

                                bool playing = false;
                                result = channel.isPlaying(out playing);
                                if (result == FMOD.RESULT.OK && playing)
                                {
                                    result = channel.stop();
                                    FMODreset();
                                }
                            }
                            else if (FMODpanel.Visible)
                            {
                                PreviewAsset(lastLoadedAsset);
                            }

                            break;
                        }

                }

            }
            else if (lastSelectedItem != null && enablePreview.Checked)
            {
                lastLoadedAsset = lastSelectedItem;
                PreviewAsset(lastLoadedAsset);
            }

            Properties.Settings.Default["enablePreview"] = enablePreview.Checked;
            Properties.Settings.Default.Save();
        }

        private void displayAssetInfo_Check(object sender, EventArgs e)
        {
            if (displayInfo.Checked && assetInfoLabel.Text != null) { assetInfoLabel.Visible = true; }
            else { assetInfoLabel.Visible = false; }

            Properties.Settings.Default["displayInfo"] = displayInfo.Checked;
            Properties.Settings.Default.Save();
        }

        private void MenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default[((ToolStripMenuItem)sender).Name] = ((ToolStripMenuItem)sender).Checked;
            Properties.Settings.Default.Save();
        }

        private void assetGroupOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default["assetGroupOption"] = ((ToolStripComboBox)sender).SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void showExpOpt_Click(object sender, EventArgs e)
        {
            ExportOptions exportOpt = new ExportOptions();
            exportOpt.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutWindow = new AboutBox();
            aboutWindow.ShowDialog();
        }

        private void assetListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = ustudio.visibleAssets[e.ItemIndex];
        }

        private void tabPageSelected(object sender, TabControlEventArgs e)
        {
            switch (e.TabPageIndex)
            {
                case 0: treeSearch.Select(); break;
                case 1:
                    resizeAssetListColumns(); //required because the ListView is not visible on app launch
                    classPreviewPanel.Visible = false;
                    previewPanel.Visible = true;
                    listSearch.Select();
                    break;
                case 2:
                    previewPanel.Visible = false;
                    classPreviewPanel.Visible = true;
                    break;
            }
        }

        private void treeSearch_MouseEnter(object sender, EventArgs e)
        {
            treeTip.Show("Search with * ? widcards. Enter to scroll through results, Ctrl+Enter to select all results.", treeSearch, 5000);
        }

        private void treeSearch_Enter(object sender, EventArgs e)
        {
            if (treeSearch.Text == " Search ")
            {
                treeSearch.Text = "";
                treeSearch.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        private void treeSearch_Leave(object sender, EventArgs e)
        {
            if (treeSearch.Text == "")
            {
                treeSearch.Text = " Search ";
                treeSearch.ForeColor = System.Drawing.SystemColors.GrayText;
            }
        }

        private void recurseTreeCheck(TreeNodeCollection start)
        {
            foreach (GameObject GObject in start)
            {
                if (GObject.Text.Like(treeSearch.Text))
                {
                    GObject.Checked = !GObject.Checked;
                    if (GObject.Checked) { GObject.EnsureVisible(); }
                }
                else { recurseTreeCheck(GObject.Nodes); }
            }
        }

        private void treeSearch_TextChanged(object sender, EventArgs e)
        {
            treeSrcResults.Clear();
            nextGObject = 0;
        }

        private void treeSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (treeSrcResults.Count == 0)
                {
                    foreach (var aFile in ustudio.assetsfileList)
                    {
                        foreach (var GObject in aFile.GameObjectList.Values)
                        {
                            if (GObject.Text.Like(treeSearch.Text)) { treeSrcResults.Add(GObject); }
                        }
                    }
                }


                if (e.Control) //toggle all matching nodes
                {
                    sceneTreeView.BeginUpdate();
                    //loop TreeView recursively to avoid children already checked by parent
                    recurseTreeCheck(sceneTreeView.Nodes);
                    sceneTreeView.EndUpdate();
                }
                else //make visible one by one
                {
                    if (treeSrcResults.Count > 0)
                    {
                        if (nextGObject >= treeSrcResults.Count) { nextGObject = 0; }
                        treeSrcResults[nextGObject].EnsureVisible();
                        sceneTreeView.SelectedNode = treeSrcResults[nextGObject];
                        nextGObject++;
                    }
                }
            }
        }

        private void sceneTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            foreach (GameObject childNode in e.Node.Nodes)
            {
                childNode.Checked = e.Node.Checked;
            }
        }

        private void resizeAssetListColumns()
        {
            assetListView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.HeaderSize);
            assetListView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            assetListView.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.HeaderSize);
            assetListView.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);

            var vscrollwidth = SystemInformation.VerticalScrollBarWidth;
            var hasvscroll = ((float)ustudio.visibleAssets.Count / (float)assetListView.Height) > 0.0567f;
            columnHeaderName.Width = assetListView.Width - columnHeaderType.Width - columnHeaderSize.Width - (hasvscroll ? (5 + vscrollwidth) : 5);
        }

        private void tabPage2_Resize(object sender, EventArgs e)
        {
            resizeAssetListColumns();
        }

        /*private void splitContainer1_Resize(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 1: resizeAssetListColumns(); break;
            }
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 1: resizeAssetListColumns(); break;
            }
        }*/

        private void listSearch_Enter(object sender, EventArgs e)
        {
            if (listSearch.Text == " Filter ")
            {
                listSearch.Text = "";
                listSearch.ForeColor = System.Drawing.SystemColors.WindowText;
                enableFiltering = true;
            }
        }

        private void listSearch_Leave(object sender, EventArgs e)
        {
            if (listSearch.Text == "")
            {
                enableFiltering = false;
                listSearch.Text = " Filter ";
                listSearch.ForeColor = System.Drawing.SystemColors.GrayText;
            }
        }

        private void ListSearchTextChanged(object sender, EventArgs e)
        {
            if (enableFiltering)
            {
                assetListView.BeginUpdate();
                assetListView.SelectedIndices.Clear();
                //visibleListAssets = exportableAssets.FindAll(ListAsset => ListAsset.Text.StartsWith(ListSearch.Text, System.StringComparison.CurrentCultureIgnoreCase));
                ustudio.visibleAssets = ustudio.exportableAssets.FindAll(ListAsset => ListAsset.Text.IndexOf(listSearch.Text, System.StringComparison.CurrentCultureIgnoreCase) >= 0);
                assetListView.VirtualListSize = ustudio.visibleAssets.Count;
                assetListView.EndUpdate();
            }
        }

        private void assetListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (firstSortColumn != e.Column)
            {
                //sorting column has been changed
                reverseSort = false;
                secondSortColumn = firstSortColumn;
            }
            else { reverseSort = !reverseSort; }
            firstSortColumn = e.Column;

            assetListView.BeginUpdate();
            assetListView.SelectedIndices.Clear();
            switch (e.Column)
            {
                case 0:
                    ustudio.visibleAssets.Sort(delegate (AssetPreloadData a, AssetPreloadData b)
                    {
                        int xdiff = reverseSort ? b.Text.CompareTo(a.Text) : a.Text.CompareTo(b.Text);
                        if (xdiff != 0) return xdiff;
                        else return secondSortColumn == 1 ? a.TypeString.CompareTo(b.TypeString) : a.Size.CompareTo(b.Size);
                    });
                    break;
                case 1:
                    ustudio.visibleAssets.Sort(delegate (AssetPreloadData a, AssetPreloadData b)
                    {
                        int xdiff = reverseSort ? b.TypeString.CompareTo(a.TypeString) : a.TypeString.CompareTo(b.TypeString);
                        if (xdiff != 0) return xdiff;
                        else return secondSortColumn == 2 ? a.Size.CompareTo(b.Size) : a.Text.CompareTo(b.Text);
                    });
                    break;
                case 2:
                    ustudio.visibleAssets.Sort(delegate (AssetPreloadData a, AssetPreloadData b)
                    {
                        int xdiff = reverseSort ? b.Size.CompareTo(a.Size) : a.Size.CompareTo(b.Size);
                        if (xdiff != 0) return xdiff;
                        else return secondSortColumn == 1 ? a.TypeString.CompareTo(b.TypeString) : a.Text.CompareTo(b.Text);
                    });
                    break;
            }

            assetListView.EndUpdate();

            resizeAssetListColumns();
        }

        private void selectAsset(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            previewPanel.BackgroundImage = Properties.Resources.preview;
            previewPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            assetInfoLabel.Visible = false;
            assetInfoLabel.Text = null;
            textPreviewBox.Visible = false;
            fontPreviewBox.Visible = false;
            pfc.Dispose();
            FMODpanel.Visible = false;
            lastLoadedAsset = null;
            StatusStripUpdate("");

            FMODreset();

            lastSelectedItem = (AssetPreloadData)e.Item;

            if (e.IsSelected)
            {
                assetInfoLabel.Text = lastSelectedItem.InfoText;
                if (displayInfo.Checked && assetInfoLabel.Text != null) { assetInfoLabel.Visible = true; } //only display the label if asset has info text

                if (enablePreview.Checked)
                {
                    lastLoadedAsset = lastSelectedItem;
                    PreviewAsset(lastLoadedAsset);
                }
            }
        }

        private void classesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                classTextBox.Text = ((ClassStruct)classesListView.SelectedItems[0]).membersstr;
            }
        }

        private void PreviewAsset(AssetPreloadData asset)
        { 
            switch (asset.Type2)
            {
                #region Texture2D
                case 28: //Texture2D
                    {
                        if (imageTexture != null)
                            imageTexture.Dispose();
                        var m_Texture2D = new Texture2D(asset, true);
                        imageTexture = m_Texture2D.ConvertToBitmap(true);
                        if (imageTexture != null)
                        {
                            previewPanel.BackgroundImage = imageTexture;
                            if (imageTexture.Width > previewPanel.Width || imageTexture.Height > previewPanel.Height)
                                previewPanel.BackgroundImageLayout = ImageLayout.Zoom;
                            else
                                previewPanel.BackgroundImageLayout = ImageLayout.Center;
                        }
                        else
                        {
                            StatusStripUpdate("Unsupported image for preview. Can only export the texture file.");
                        }
                        break;
                    }
                #endregion
                #region AudioClip
                case 83: //AudioClip
                    {
                        AudioClip m_AudioClip = new AudioClip(asset, true);

                        FMOD.RESULT result;
                        FMOD.CREATESOUNDEXINFO exinfo = new FMOD.CREATESOUNDEXINFO();

                        exinfo.cbsize = Marshal.SizeOf(exinfo);
                        exinfo.length = (uint)m_AudioClip.m_Size;

                        result = system.createSound(m_AudioClip.m_AudioData, (FMOD.MODE.OPENMEMORY | loopMode), ref exinfo, out sound);
                        if (ERRCHECK(result)) { break; }

                        FMOD.Sound subsound;
                        result = sound.getSubSound(0, out subsound);
                        if (result == FMOD.RESULT.OK)
                        {
                            sound = subsound;
                        }

                        result = sound.getLength(out FMODlenms, FMOD.TIMEUNIT.MS);
                        if (ERRCHECK(result)) { break; }

                        result = system.playSound(sound, null, true, out channel);
                        if (ERRCHECK(result)) { break; }

                        FMODpanel.Visible = true;

                        result = channel.getFrequency(out FMODfrequency);
                        if (ERRCHECK(result)) { break; }

                        FMODinfoLabel.Text = FMODfrequency.ToString() + " Hz";
                        FMODtimerLabel.Text = "0:0.0 / " + (FMODlenms / 1000 / 60) + ":" + (FMODlenms / 1000 % 60) + "." + (FMODlenms / 10 % 100);
                        break;
                    }
                #endregion
                #region Shader
                case 48:
                    {
                        Shader m_TextAsset = new Shader(asset, true);
                        string m_Script_Text = Encoding.UTF8.GetString(m_TextAsset.m_Script);
                        m_Script_Text = Regex.Replace(m_Script_Text, "(?<!\r)\n", "\r\n");
                        textPreviewBox.Text = m_Script_Text;
                        textPreviewBox.Visible = true;
                        break;
                    }
                #endregion
                #region TextAsset
                case 49:
                    {
                        TextAsset m_TextAsset = new TextAsset(asset, true);

                        string m_Script_Text = Encoding.UTF8.GetString(m_TextAsset.m_Script);
                        m_Script_Text = Regex.Replace(m_Script_Text, "(?<!\r)\n", "\r\n");
                        textPreviewBox.Text = m_Script_Text;
                        textPreviewBox.Visible = true;

                        break;
                    }
                #endregion
                #region MonoBehaviour
                case 114:
                    {
                        MonoBehaviour m_MonoBehaviour = new MonoBehaviour(asset, true);
                        textPreviewBox.Text = m_MonoBehaviour.serializedText;
                        textPreviewBox.Visible = true;

                        break;
                    }
                #endregion
                #region Font
                case 128: //Font
                    {
                        unityFont m_Font = new unityFont(asset, true);
                        if (m_Font.m_FontData != null)
                        {
                            IntPtr data = Marshal.AllocCoTaskMem(m_Font.m_FontData.Length);
                            Marshal.Copy(m_Font.m_FontData, 0, data, m_Font.m_FontData.Length);

                            // We HAVE to do this to register the font to the system (Weird .NET bug !)
                            uint cFonts = 0;
                            var re = AddFontMemResourceEx(data, (uint)m_Font.m_FontData.Length, IntPtr.Zero, ref cFonts);
                            if (re != IntPtr.Zero)
                            {
                                pfc = new PrivateFontCollection();
                                pfc.AddMemoryFont(data, m_Font.m_FontData.Length);
                                Marshal.FreeCoTaskMem(data);
                                if (pfc.Families.Length > 0)
                                {
                                    //textPreviewBox.Font = new Font(pfc.Families[0], 16, FontStyle.Regular);
                                    //textPreviewBox.Text = "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWYZ\r\n1234567890.:,;'\"(!?)+-*/=\r\nThe quick brown fox jumps over the lazy dog. 1234567890";
                                    fontPreviewBox.SelectionStart = 0;
                                    fontPreviewBox.SelectionLength = 80;
                                    fontPreviewBox.SelectionFont = new Font(pfc.Families[0], 16, FontStyle.Regular);
                                    fontPreviewBox.SelectionStart = 81;
                                    fontPreviewBox.SelectionLength = 56;
                                    fontPreviewBox.SelectionFont = new Font(pfc.Families[0], 12, FontStyle.Regular);
                                    fontPreviewBox.SelectionStart = 138;
                                    fontPreviewBox.SelectionLength = 56;
                                    fontPreviewBox.SelectionFont = new Font(pfc.Families[0], 18, FontStyle.Regular);
                                    fontPreviewBox.SelectionStart = 195;
                                    fontPreviewBox.SelectionLength = 56;
                                    fontPreviewBox.SelectionFont = new Font(pfc.Families[0], 24, FontStyle.Regular);
                                    fontPreviewBox.SelectionStart = 252;
                                    fontPreviewBox.SelectionLength = 56;
                                    fontPreviewBox.SelectionFont = new Font(pfc.Families[0], 36, FontStyle.Regular);
                                    fontPreviewBox.SelectionStart = 309;
                                    fontPreviewBox.SelectionLength = 56;
                                    fontPreviewBox.SelectionFont = new Font(pfc.Families[0], 48, FontStyle.Regular);
                                    fontPreviewBox.SelectionStart = 366;
                                    fontPreviewBox.SelectionLength = 56;
                                    fontPreviewBox.SelectionFont = new Font(pfc.Families[0], 60, FontStyle.Regular);
                                    fontPreviewBox.SelectionStart = 423;
                                    fontPreviewBox.SelectionLength = 55;
                                    fontPreviewBox.SelectionFont = new Font(pfc.Families[0], 72, FontStyle.Regular);
                                    fontPreviewBox.Visible = true;
                                }
                                break;
                            }
                        }
                        StatusStripUpdate("Unsupported font for preview. Try to export.");
                        break;
                    }
                #endregion
                default:
                    {
                        StatusStripUpdate("Only supported export the raw file.");
                        break;
                    }
            }
        }

        private void FMODinit()
        {
            FMODreset();

            FMOD.RESULT result;
            uint version = 0;

            result = FMOD.Factory.System_Create(out system);
            if (ERRCHECK(result)) { return; }

            result = system.getVersion(out version);
            ERRCHECK(result);
            if (version < FMOD.VERSION.number)
            {
                MessageBox.Show("Error!  You are using an old version of FMOD " + version.ToString("X") + ".  This program requires " + FMOD.VERSION.number.ToString("X") + ".");
                Application.Exit();
            }

            result = system.init(1, FMOD.INITFLAGS.NORMAL, (IntPtr)null);
            if (ERRCHECK(result)) { return; }

            //result = system.getMasterChannelGroup(out channelGroup);
            //if (ERRCHECK(result)) { return; }

            result = system.getMasterSoundGroup(out masterSoundGroup);
            if (ERRCHECK(result)) { return; }

            result = masterSoundGroup.setVolume(FMODVolume);
            if (ERRCHECK(result)) { return; }
        }

        private void FMODreset()
        {
            timer.Stop();
            FMODprogressBar.Value = 0;
            FMODtimerLabel.Text = "0:00.0 / 0:00.0";
            FMODstatusLabel.Text = "Stopped";
            FMODinfoLabel.Text = "";

            if (sound != null)
            {
                var result = sound.release();
                if (result != FMOD.RESULT.OK) { StatusStripUpdate("FMOD error! " + result + " - " + FMOD.Error.String(result)); }
                sound = null;
            }
        }

        private void FMODplayButton_Click(object sender, EventArgs e)
        {
            FMOD.RESULT result;
            if (sound != null && channel != null)
            {
                timer.Start();
                bool playing = false;
                result = channel.isPlaying(out playing);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    if (ERRCHECK(result)) { return; }
                }

                if (playing)
                {
                    result = channel.stop();
                    if (ERRCHECK(result)) { return; }

                    result = system.playSound(sound, null, false, out channel);
                    if (ERRCHECK(result)) { return; }

                    FMODpauseButton.Text = "Pause";
                }
                else
                {
                    result = system.playSound(sound, null, false, out channel);
                    if (ERRCHECK(result)) { return; }
                    FMODstatusLabel.Text = "Playing";
                    //FMODinfoLabel.Text = FMODfrequency.ToString();

                    if (FMODprogressBar.Value > 0)
                    {
                        uint newms = FMODlenms / 1000 * (uint)FMODprogressBar.Value;

                        result = channel.setPosition(newms, FMOD.TIMEUNIT.MS);
                        if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                        {
                            if (ERRCHECK(result)) { return; }
                        }

                    }
                }
            }
        }

        private void FMODpauseButton_Click(object sender, EventArgs e)
        {
            FMOD.RESULT result;

            if (sound != null && channel != null)
            {
                bool playing = false;
                bool paused = false;

                result = channel.isPlaying(out playing);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    if (ERRCHECK(result)) { return; }
                }

                if (playing)
                {
                    result = channel.getPaused(out paused);
                    if (ERRCHECK(result)) { return; }
                    result = channel.setPaused(!paused);
                    if (ERRCHECK(result)) { return; }

                    //FMODstatusLabel.Text = (!paused ? "Paused" : playing ? "Playing" : "Stopped");
                    //FMODpauseButton.Text = (!paused ? "Resume" : playing ? "Pause" : "Pause");

                    if (paused)
                    {
                        FMODstatusLabel.Text = (playing ? "Playing" : "Stopped");
                        FMODpauseButton.Text = "Pause";
                        timer.Start();
                    }
                    else
                    {
                        FMODstatusLabel.Text = "Paused";
                        FMODpauseButton.Text = "Resume";
                        timer.Stop();
                    }
                }
            }
        }

        private void FMODstopButton_Click(object sender, EventArgs e)
        {
            FMOD.RESULT result;
            if (channel != null)
            {
                bool playing = false;
                result = channel.isPlaying(out playing);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    if (ERRCHECK(result)) { return; }
                }

                if (playing)
                {
                    result = channel.stop();
                    if (ERRCHECK(result)) { return; }
                    //channel = null;
                    //don't FMODreset, it will nullify the sound
                    timer.Stop();
                    FMODprogressBar.Value = 0;
                    FMODtimerLabel.Text = "0:00.0 / 0:00.0";
                    FMODstatusLabel.Text = "Stopped";
                    FMODpauseButton.Text = "Pause";
                }
            }
        }

        private void FMODloopButton_CheckedChanged(object sender, EventArgs e)
        {
            FMOD.RESULT result;

            if (FMODloopButton.Checked)
            {
                loopMode = FMOD.MODE.LOOP_NORMAL;
            }
            else
            {
                loopMode = FMOD.MODE.LOOP_OFF;
            }

            if (sound != null)
            {
                result = sound.setMode(loopMode);
                if (ERRCHECK(result)) { return; }
            }

            if (channel != null)
            {
                bool playing = false;
                result = channel.isPlaying(out playing);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    if (ERRCHECK(result)) { return; }
                }

                bool paused = false;
                result = channel.getPaused(out paused);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    if (ERRCHECK(result)) { return; }
                }

                if (playing || paused)
                {
                    result = channel.setMode(loopMode);
                    if (ERRCHECK(result)) { return; }
                }
            }
        }

        private void FMODvolumeBar_ValueChanged(object sender, EventArgs e)
        {
            FMOD.RESULT result;
            FMODVolume = Convert.ToSingle(FMODvolumeBar.Value) / 10;

            result = masterSoundGroup.setVolume(FMODVolume);
            if (ERRCHECK(result)) { return; }
        }

        private void FMODprogressBar_Scroll(object sender, EventArgs e)
        {
            if (channel != null)
            {
                uint newms = FMODlenms / 1000 * (uint)FMODprogressBar.Value;
                FMODtimerLabel.Text = (newms / 1000 / 60) + ":" + (newms / 1000 % 60) + "." + (newms / 10 % 100) + "/" + (FMODlenms / 1000 / 60) + ":" + (FMODlenms / 1000 % 60) + "." + (FMODlenms / 10 % 100);
            }
        }

        private void FMODprogressBar_MouseDown(object sender, MouseEventArgs e)
        {
            timer.Stop();
        }

        private void FMODprogressBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (channel != null)
            {
                FMOD.RESULT result;

                uint newms = FMODlenms / 1000 * (uint)FMODprogressBar.Value;

                result = channel.setPosition(newms, FMOD.TIMEUNIT.MS);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    if (ERRCHECK(result)) { return; }
                }

                bool playing = false;

                result = channel.isPlaying(out playing);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    if (ERRCHECK(result)) { return; }
                }

                if (playing) { timer.Start(); }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            FMOD.RESULT result;
            uint ms = 0;
            bool playing = false;
            bool paused = false;

            if (channel != null)
            {
                result = channel.getPosition(out ms, FMOD.TIMEUNIT.MS);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    ERRCHECK(result);
                }

                result = channel.isPlaying(out playing);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    ERRCHECK(result);
                }

                result = channel.getPaused(out paused);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    ERRCHECK(result);
                }
            }

            //statusBar.Text = "Time " + (ms / 1000 / 60) + ":" + (ms / 1000 % 60) + ":" + (ms / 10 % 100) + "/" + (lenms / 1000 / 60) + ":" + (lenms / 1000 % 60) + ":" + (lenms / 10 % 100) + " : " + (paused ? "Paused " : playing ? "Playing" : "Stopped");
            FMODtimerLabel.Text = (ms / 1000 / 60) + ":" + (ms / 1000 % 60) + "." + (ms / 10 % 100) + " / " + (FMODlenms / 1000 / 60) + ":" + (FMODlenms / 1000 % 60) + "." + (FMODlenms / 10 % 100);
            FMODprogressBar.Value = (int)(ms * 1000 / FMODlenms);
            FMODstatusLabel.Text = (paused ? "Paused " : playing ? "Playing" : "Stopped");

            if (system != null && channel != null)
            {
                system.update();
            }
        }

        private bool ERRCHECK(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                //FMODinit();
                FMODreset();
                StatusStripUpdate("FMOD error! " + result + " - " + FMOD.Error.String(result));
                //Environment.Exit(-1);
                return true;
            }
            else { return false; }
        }

        private void all3DObjectssplitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sceneTreeView.Nodes.Count > 0)
            {
                if (saveFolderDialog1.ShowDialog() == DialogResult.OK)
                {
                    var savePath = saveFolderDialog1.FileName;
                    if (Path.GetFileName(savePath) == "Select folder or write folder name to create")
                    { savePath = Path.GetDirectoryName(saveFolderDialog1.FileName); }
                    savePath = savePath + "\\";
                    switch ((bool)Properties.Settings.Default["showExpOpt"])
                    {
                        case true:
                            ExportOptions exportOpt = new ExportOptions();
                            if (exportOpt.ShowDialog() == DialogResult.OK) { goto case false; }
                            break;
                        case false:
                            {
                                progressBar1.Value = 0;
                                progressBar1.Maximum = sceneTreeView.Nodes.Count;
                                //防止主界面假死
                                ThreadPool.QueueUserWorkItem(delegate
                                {
                                    sceneTreeView.Invoke(new Action(() =>
                                    {
                                        //挂起控件防止更新
                                        sceneTreeView.BeginUpdate();
                                        //先取消所有Node的选中
                                        foreach (TreeNode i in sceneTreeView.Nodes)
                                        {
                                            i.Checked = false;
                                        }
                                    }));
                                    //遍历根节点
                                    foreach (TreeNode i in sceneTreeView.Nodes)
                                    {
                                        if (i.Nodes.Count > 0)
                                        {
                                            //遍历一级子节点
                                            foreach (TreeNode j in i.Nodes)
                                            {
                                                //加上时间，因为可能有重名的object
                                                var filename = j.Text + DateTime.Now.ToString("_mm_ss_ffff");
                                                //选中它和它的子节点
                                                sceneTreeView.Invoke(new Action(() => j.Checked = true));
                                                //导出FBX
                                                ustudio.WriteFBX(savePath + filename + ".fbx", false);
                                                //取消选中
                                                sceneTreeView.Invoke(new Action(() => j.Checked = false));
                                            }
                                        }
                                        ProgressBarPerformStep();
                                    }
                                    //取消挂起
                                    sceneTreeView.Invoke(new Action(() => sceneTreeView.EndUpdate()));
                                    if (openAfterExport.Checked) { Process.Start(savePath); }
                                });
                                break;
                            }
                    }
                }

            }
            else { StatusStripUpdate("No Objects available for export"); }
        }

        private void Export3DObjects_Click(object sender, EventArgs e)
        {
            if (sceneTreeView.Nodes.Count > 0)
            {
                bool exportSwitch = (((ToolStripItem)sender).Name == "exportAll3DMenuItem") ? true : false;


                var timestamp = DateTime.Now;
                saveFileDialog1.FileName = ustudio.productName + timestamp.ToString("_yy_MM_dd__HH_mm_ss");
                //extension will be added by the file save dialog

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    switch ((bool)Properties.Settings.Default["showExpOpt"])
                    {
                        case true:
                            ExportOptions exportOpt = new ExportOptions();
                            if (exportOpt.ShowDialog() == DialogResult.OK) { goto case false; }
                            break;
                        case false:
                            switch (saveFileDialog1.FilterIndex)
                            {
                                case 1:
                                    ustudio.WriteFBX(saveFileDialog1.FileName, exportSwitch);
                                    break;
                                case 2:
                                    break;
                            }

                            if (openAfterExport.Checked && File.Exists(saveFileDialog1.FileName)) { try { Process.Start(saveFileDialog1.FileName); } catch { } }
                            break;
                    }
                }

            }
            else { StatusStripUpdate("No Objects available for export"); }
        }

        private void ExportAssets_Click(object sender, EventArgs e)
        {
            if (ustudio.exportableAssets.Count > 0 && saveFolderDialog1.ShowDialog() == DialogResult.OK)
            {
                timer.Stop();
                List<AssetPreloadData> toExportAssets = null;
                if (((ToolStripItem)sender).Name == "exportAllAssetsMenuItem")
                {
                    toExportAssets = ustudio.exportableAssets;
                }
                else if (((ToolStripItem)sender).Name == "exportFilteredAssetsMenuItem")
                {
                    toExportAssets = ustudio.visibleAssets;
                }
                else if (((ToolStripItem)sender).Name == "exportSelectedAssetsMenuItem")
                {
                    toExportAssets = new List<AssetPreloadData>(assetListView.SelectedIndices.Count);
                    foreach (var i in assetListView.SelectedIndices.OfType<int>())
                    {
                        toExportAssets.Add((AssetPreloadData)assetListView.Items[i]);
                    }
                }
                int assetGroupSelectedIndex = assetGroupOptions.SelectedIndex;

                ThreadPool.QueueUserWorkItem(delegate
                {
                    var savePath = saveFolderDialog1.FileName;
                    if (Path.GetFileName(savePath) == "Select folder or write folder name to create")
                    { savePath = Path.GetDirectoryName(saveFolderDialog1.FileName); }

                    int toExport = toExportAssets.Count;
                    int exportedCount = 0;

                    SetProgressBarValue(0);
                    SetProgressBarMaximum(toExport);
                    //looping assetsFiles will optimize HDD access
                    //but will also have a small performance impact when exporting only a couple of selected assets
                    foreach (var asset in toExportAssets)
                    {
                        string exportpath = savePath + "\\";
                        if (assetGroupSelectedIndex == 1) { exportpath += Path.GetFileNameWithoutExtension(asset.sourceFile.filePath) + "_export\\"; }
                        else if (assetGroupSelectedIndex == 0) { exportpath = savePath + "\\" + asset.TypeString + "\\"; }

                        //AudioClip and Texture2D extensions are set when the list is built
                        //so their overwrite tests can be done without loading them again
                        switch (asset.Type2)
                        {
                            case 28:
                                if (ustudio.ExportTexture(asset, exportpath, true))
                                {
                                    exportedCount++;
                                }
                                break;
                            case 83:
                                if (ustudio.ExportAudioClip(asset, exportpath + asset.Text, asset.extension))
                                {
                                    exportedCount++;
                                }
                                break;
                            case 48:
                                if (!ustudio.ExportFileExists(exportpath + asset.Text + asset.extension, asset.TypeString))
                                {
                                    ustudio.ExportShader(new Shader(asset, true), exportpath + asset.Text + ".txt");
                                    exportedCount++;
                                }
                                break;
                            case 49:
                                TextAsset m_TextAsset = new TextAsset(asset, true);
                                if (!ustudio.ExportFileExists(exportpath + asset.Text + asset.extension, asset.TypeString))
                                {
                                    ustudio.ExportText(m_TextAsset, exportpath + asset.Text + asset.extension);
                                    exportedCount++;
                                }
                                break;
                            case 114:
                                MonoBehaviour m_MonoBehaviour = new MonoBehaviour(asset, true);
                                if (!ustudio.ExportFileExists(exportpath + asset.Text + asset.extension, asset.TypeString))
                                {
                                    ustudio.ExportMonoBehaviour(m_MonoBehaviour, exportpath + asset.Text + asset.extension);
                                    exportedCount++;
                                }
                                break;
                            case 128:
                                unityFont m_Font = new unityFont(asset, true);
                                if (!ustudio.ExportFileExists(exportpath + asset.Text + asset.extension, asset.TypeString))
                                {
                                    ustudio.ExportFont(m_Font, exportpath + asset.Text + asset.extension);
                                    exportedCount++;
                                }
                                break;
                            default:
                                if (!ustudio.ExportFileExists(exportpath + asset.Text + asset.extension, asset.TypeString))
                                {
                                    ustudio.ExportRawFile(asset, exportpath + asset.Text + asset.extension);
                                    exportedCount++;
                                }
                                break;

                        }
                        ProgressBarPerformStep();
                    }
                    string statusText = "";
                    switch (exportedCount)
                    {
                        case 0:
                            statusText = "Nothing exported.";
                            break;
                        default:
                            statusText = "Finished exporting " + exportedCount.ToString() + " assets.";
                            break;
                    }

                    if (toExport > exportedCount) { statusText += " " + (toExport - exportedCount).ToString() + " assets skipped (not extractable or files already exist)"; }

                    StatusStripUpdate(statusText);

                    if (openAfterExport.Checked && exportedCount > 0) { Process.Start(savePath); }
                });
            }
            else
            {
                StatusStripUpdate("No exportable assets loaded");
            }
        }

        private void SetProgressBarValue(int value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => { progressBar1.Value = value; }));
            }
            else
            {
                progressBar1.Value = value;
            }
        }

        private void SetProgressBarMaximum(int value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => { progressBar1.Maximum = value; }));
            }
            else
            {
                progressBar1.Maximum = value;
            }
        }

        private void ProgressBarPerformStep()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => { progressBar1.PerformStep(); }));
            }
            else
            {
                progressBar1.PerformStep();
            }
        }

        private void StatusStripUpdate(string statusText)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => { toolStripStatusLabel1.Text = statusText; }));
            }
            else
            {
                toolStripStatusLabel1.Text = statusText;
            }
        }

        public UnityStudioForm()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            InitializeComponent();
            displayAll.Checked = (bool)Properties.Settings.Default["displayAll"];
            displayInfo.Checked = (bool)Properties.Settings.Default["displayInfo"];
            enablePreview.Checked = (bool)Properties.Settings.Default["enablePreview"];
            openAfterExport.Checked = (bool)Properties.Settings.Default["openAfterExport"];
            assetGroupOptions.SelectedIndex = (int)Properties.Settings.Default["assetGroupOption"];
            FMODinit();
        }

        private void resetForm()
        {
            /*Properties.Settings.Default["uniqueNames"] = uniqueNamesMenuItem.Checked;
            Properties.Settings.Default["enablePreview"] = enablePreviewMenuItem.Checked;
            Properties.Settings.Default["displayInfo"] = displayAssetInfoMenuItem.Checked;
            Properties.Settings.Default.Save();*/

            base.Text = "Unity Studio";

            ustudio.unityFiles.Clear();
            ustudio.assetsfileList.Clear();
            ustudio.exportableAssets.Clear();
            ustudio.visibleAssets.Clear();
            UnityStudio.assetsfileandstream.Clear();

            sceneTreeView.Nodes.Clear();

            assetListView.VirtualListSize = 0;
            assetListView.Items.Clear();
            //assetListView.Groups.Clear();

            classesListView.Items.Clear();
            classesListView.Groups.Clear();

            previewPanel.BackgroundImage = Properties.Resources.preview;
            previewPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            assetInfoLabel.Visible = false;
            assetInfoLabel.Text = null;
            textPreviewBox.Visible = false;
            fontPreviewBox.Visible = false;
            lastSelectedItem = null;
            lastLoadedAsset = null;
            firstSortColumn = -1;
            secondSortColumn = 0;
            reverseSort = false;
            enableFiltering = false;

            //FMODinit();
            FMODreset();

        }

        private void UnityStudioForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*Properties.Settings.Default["uniqueNames"] = uniqueNamesMenuItem.Checked;
            Properties.Settings.Default["enablePreview"] = enablePreviewMenuItem.Checked;
            Properties.Settings.Default["displayInfo"] = displayAssetInfoMenuItem.Checked;
            Properties.Settings.Default.Save();

            foreach (var assetsFile in assetsfileList) { assetsFile.a_Stream.Dispose(); } //is this needed?*/
        }
    }
}
