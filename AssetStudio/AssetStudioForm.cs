using System;
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
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static AssetStudio.Studio;
using static AssetStudio.Importer;

namespace AssetStudio
{
    partial class AssetStudioForm : Form
    {
        private AssetPreloadData lastSelectedItem;
        private AssetPreloadData lastLoadedAsset;

        private FMOD.System system;
        private FMOD.Sound sound;
        private FMOD.Channel channel;
        private FMOD.SoundGroup masterSoundGroup;
        private FMOD.MODE loopMode = FMOD.MODE.LOOP_OFF;
        private uint FMODlenms;
        private float FMODVolume = 0.8f;

        private Bitmap imageTexture;

        #region GLControl
        private bool glControlLoaded;
        private int mdx, mdy;
        private bool lmdown, rmdown;
        private int pgmID, pgmColorID, pgmBlackID;
        private int attributeVertexPosition;
        private int attributeNormalDirection;
        private int attributeVertexColor;
        private int uniformModelMatrix;
        private int uniformViewMatrix;
        private int uniformProjMatrix;
        private int vao;
        private Vector3[] vertexData;
        private Vector3[] normalData;
        private Vector3[] normal2Data;
        private Vector4[] colorData;
        private Matrix4 modelMatrixData;
        private Matrix4 viewMatrixData;
        private Matrix4 projMatrixData;
        private int[] indiceData;
        private int wireFrameMode;
        private int shadeMode;
        private int normalMode;
        #endregion

        //asset list sorting helpers
        private int firstSortColumn = -1;
        private int secondSortColumn;
        private bool reverseSort;
        private bool enableFiltering;

        //tree search
        private int nextGObject;
        private List<GameObjectTreeNode> treeSrcResults = new List<GameObjectTreeNode>();

        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);


        private void loadFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                resetForm();
                ThreadPool.QueueUserWorkItem(state =>
                {
                    mainPath = Path.GetDirectoryName(openFileDialog1.FileNames[0]);
                    MergeSplitAssets(mainPath);
                    var readFile = ProcessingSplitFiles(openFileDialog1.FileNames.ToList());
                    foreach (var i in readFile)
                    {
                        importFiles.Add(i);
                        importFilesHash.Add(Path.GetFileName(i).ToUpper());
                    }
                    SetProgressBarValue(0);
                    SetProgressBarMaximum(importFiles.Count);
                    //use a for loop because list size can change
                    for (int f = 0; f < importFiles.Count; f++)
                    {
                        LoadFile(importFiles[f]);
                        ProgressBarPerformStep();
                    }
                    importFilesHash.Clear();
                    assetsfileListHash.Clear();
                    BuildAssetStrucutres();
                });
            }
        }

        private void loadFolder_Click(object sender, EventArgs e)
        {
            var openFolderDialog1 = new OpenFolderDialog();
            if (openFolderDialog1.ShowDialog(this) == DialogResult.OK)
            {
                resetForm();
                ThreadPool.QueueUserWorkItem(state =>
                {
                    mainPath = openFolderDialog1.Folder;
                    MergeSplitAssets(mainPath);
                    var files = Directory.GetFiles(mainPath, "*.*", SearchOption.AllDirectories).ToList();
                    var readFile = ProcessingSplitFiles(files);
                    foreach (var i in readFile)
                    {
                        importFiles.Add(i);
                        importFilesHash.Add(Path.GetFileName(i));
                    }
                    SetProgressBarValue(0);
                    SetProgressBarMaximum(importFiles.Count);
                    //use a for loop because list size can change
                    for (int f = 0; f < importFiles.Count; f++)
                    {
                        LoadFile(importFiles[f]);
                        ProgressBarPerformStep();
                    }
                    importFilesHash.Clear();
                    assetsfileListHash.Clear();
                    BuildAssetStrucutres();
                });
            }
        }

        private void extractFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openBundleDialog = new OpenFileDialog
            {
                Filter = "All types|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = true
            };

            if (openBundleDialog.ShowDialog() == DialogResult.OK)
            {
                progressBar1.Value = 0;
                progressBar1.Maximum = openBundleDialog.FileNames.Length;
                ExtractFile(openBundleDialog.FileNames);
            }
        }

        private void extractFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFolderDialog1 = new OpenFolderDialog();
            if (openFolderDialog1.ShowDialog(this) == DialogResult.OK)
            {
                var files = Directory.GetFiles(openFolderDialog1.Folder, "*.*", SearchOption.AllDirectories);
                progressBar1.Value = 0;
                progressBar1.Maximum = files.Length;
                ExtractFile(files);
            }
        }

        private void BuildAssetStrucutres()
        {
            if (assetsfileList.Count == 0)
            {
                StatusStripUpdate("No file was loaded.");
                return;
            }

            BuildAssetStructures(!dontLoadAssetsMenuItem.Checked, displayAll.Checked, !dontBuildHierarchyMenuItem.Checked, buildClassStructuresMenuItem.Checked, displayOriginalName.Checked);

            BeginInvoke(new Action(() =>
            {
                if (!string.IsNullOrEmpty(productName))
                {
                    Text = $"AssetStudio - {productName} - {assetsfileList[0].unityVersion} - {assetsfileList[0].platformStr}";
                }
                else if (assetsfileList.Count > 0)
                {
                    Text = $"AssetStudio - no productName - {assetsfileList[0].unityVersion} - {assetsfileList[0].platformStr}";
                }
                if (!dontLoadAssetsMenuItem.Checked)
                {
                    assetListView.VirtualListSize = visibleAssets.Count;
                    //will only work if ListView is visible
                    resizeAssetListColumns();
                }
                if (!dontBuildHierarchyMenuItem.Checked)
                {
                    sceneTreeView.BeginUpdate();
                    sceneTreeView.Nodes.AddRange(treeNodeCollection.ToArray());
                    foreach (TreeNode node in sceneTreeView.Nodes)
                    {
                        node.HideCheckBox();
                    }
                    sceneTreeView.EndUpdate();
                }
                if (buildClassStructuresMenuItem.Checked)
                {
                    classesListView.BeginUpdate();
                    foreach (var version in AllTypeMap)
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

                var types = exportableAssets.Select(x => x.Type).Distinct().ToArray();
                foreach (var type in types)
                {
                    var typeItem = new ToolStripMenuItem
                    {
                        CheckOnClick = true,
                        Name = type.ToString(),
                        Size = new Size(180, 22),
                        Text = type.ToString()
                    };
                    typeItem.Click += typeToolStripMenuItem_Click;
                    filterTypeToolStripMenuItem.DropDownItems.Add(typeItem);
                }
                allToolStripMenuItem.Checked = true;
                StatusStripUpdate($"Finished loading {assetsfileList.Count} files with {assetListView.Items.Count} exportable assets.");
                treeSearch.Select();
            }));
        }

        private void typeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var typeItem = (ToolStripMenuItem)sender;
            if (typeItem != allToolStripMenuItem)
            {
                allToolStripMenuItem.Checked = false;
            }
            else if (allToolStripMenuItem.Checked)
            {
                for (var i = 1; i < filterTypeToolStripMenuItem.DropDownItems.Count; i++)
                {
                    var item = (ToolStripMenuItem)filterTypeToolStripMenuItem.DropDownItems[i];
                    item.Checked = false;
                }
            }
            FilterAssetList();
        }

        private void AssetStudioForm_KeyDown(object sender, KeyEventArgs e)
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

            if (glControl1.Visible)
            {
                if (e.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.W:
                            if (e.Control) //Toggle WireFrame
                            {
                                wireFrameMode = (wireFrameMode + 1) % 3;
                                glControl1.Invalidate();
                            }
                            break;
                        case Keys.S:
                            if (e.Control) //Toggle Shade
                            {
                                shadeMode = (shadeMode + 1) % 2;
                                glControl1.Invalidate();
                            }
                            break;
                        case Keys.N:
                            if (e.Control) //Normal mode
                            {
                                normalMode = (normalMode + 1) % 2;
                                createVAO();
                                glControl1.Invalidate();
                            }
                            break;
                    }
                }
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
            if (AllTypeMap.Count > 0)
            {
                var saveFolderDialog1 = new OpenFolderDialog();
                if (saveFolderDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    progressBar1.Value = 0;
                    progressBar1.Maximum = AllTypeMap.Count;

                    var savePath = saveFolderDialog1.Folder;
                    foreach (var version in AllTypeMap)
                    {
                        if (version.Value.Count > 0)
                        {
                            string versionPath = savePath + "\\" + version.Key;
                            Directory.CreateDirectory(versionPath);

                            foreach (var uclass in version.Value)
                            {
                                string saveFile = $"{versionPath}\\{uclass.Key} {uclass.Value.Text}.txt";
                                using (StreamWriter TXTwriter = new StreamWriter(saveFile))
                                {
                                    TXTwriter.Write(uclass.Value.ToString());
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
                switch (lastLoadedAsset.Type)
                {
                    case ClassIDReference.Texture2D:
                    case ClassIDReference.Sprite:
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
                    case ClassIDReference.Shader:
                    case ClassIDReference.TextAsset:
                    case ClassIDReference.MonoBehaviour:
                        textPreviewBox.Visible = !textPreviewBox.Visible;
                        break;
                    case ClassIDReference.Font:
                        fontPreviewBox.Visible = !fontPreviewBox.Visible;
                        break;
                    case ClassIDReference.AudioClip:
                        {
                            FMODpanel.Visible = !FMODpanel.Visible;

                            if (sound != null && channel != null)
                            {
                                var result = channel.isPlaying(out var playing);
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

        private void assetListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = visibleAssets[e.ItemIndex];
        }

        private void tabPageSelected(object sender, TabControlEventArgs e)
        {
            switch (e.TabPageIndex)
            {
                case 0:
                    treeSearch.Select();
                    break;
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

        private void treeSearch_Enter(object sender, EventArgs e)
        {
            if (treeSearch.Text == " Search ")
            {
                treeSearch.Text = "";
                treeSearch.ForeColor = SystemColors.WindowText;
            }
        }

        private void treeSearch_Leave(object sender, EventArgs e)
        {
            if (treeSearch.Text == "")
            {
                treeSearch.Text = " Search ";
                treeSearch.ForeColor = SystemColors.GrayText;
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
                    foreach (var node in treeNodeDictionary.Values)
                    {
                        if (node.Text.IndexOf(treeSearch.Text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            treeSrcResults.Add(node);
                        }
                    }
                }
                if (treeSrcResults.Count > 0)
                {
                    if (nextGObject >= treeSrcResults.Count)
                    {
                        nextGObject = 0;
                    }
                    treeSrcResults[nextGObject].EnsureVisible();
                    sceneTreeView.SelectedNode = treeSrcResults[nextGObject];
                    nextGObject++;
                }
            }
        }

        private void sceneTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode childNode in e.Node.Nodes)
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
            var hasvscroll = (visibleAssets.Count / (float)assetListView.Height) > 0.0567f;
            columnHeaderName.Width = assetListView.Width - columnHeaderType.Width - columnHeaderSize.Width - (hasvscroll ? (5 + vscrollwidth) : 5);
        }

        private void tabPage2_Resize(object sender, EventArgs e)
        {
            resizeAssetListColumns();
        }

        private void listSearch_Enter(object sender, EventArgs e)
        {
            if (listSearch.Text == " Filter ")
            {
                listSearch.Text = "";
                listSearch.ForeColor = SystemColors.WindowText;
                enableFiltering = true;
            }
        }

        private void listSearch_Leave(object sender, EventArgs e)
        {
            if (listSearch.Text == "")
            {
                enableFiltering = false;
                listSearch.Text = " Filter ";
                listSearch.ForeColor = SystemColors.GrayText;
            }
        }

        private void ListSearchTextChanged(object sender, EventArgs e)
        {
            if (enableFiltering)
            {
                FilterAssetList();
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
                    visibleAssets.Sort(delegate (AssetPreloadData a, AssetPreloadData b)
                    {
                        int xdiff = reverseSort ? b.Text.CompareTo(a.Text) : a.Text.CompareTo(b.Text);
                        if (xdiff != 0) return xdiff;
                        return secondSortColumn == 1 ? a.TypeString.CompareTo(b.TypeString) : a.fullSize.CompareTo(b.fullSize);
                    });
                    break;
                case 1:
                    visibleAssets.Sort(delegate (AssetPreloadData a, AssetPreloadData b)
                    {
                        int xdiff = reverseSort ? b.TypeString.CompareTo(a.TypeString) : a.TypeString.CompareTo(b.TypeString);
                        if (xdiff != 0) return xdiff;
                        return secondSortColumn == 2 ? a.fullSize.CompareTo(b.fullSize) : a.Text.CompareTo(b.Text);
                    });
                    break;
                case 2:
                    visibleAssets.Sort(delegate (AssetPreloadData a, AssetPreloadData b)
                    {
                        int xdiff = reverseSort ? b.fullSize.CompareTo(a.fullSize) : a.fullSize.CompareTo(b.fullSize);
                        if (xdiff != 0) return xdiff;
                        return secondSortColumn == 1 ? a.TypeString.CompareTo(b.TypeString) : a.Text.CompareTo(b.Text);
                    });
                    break;
            }

            assetListView.EndUpdate();

            resizeAssetListColumns();
        }

        private void selectAsset(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            previewPanel.BackgroundImage = Properties.Resources.preview;
            previewPanel.BackgroundImageLayout = ImageLayout.Center;
            assetInfoLabel.Visible = false;
            assetInfoLabel.Text = null;
            textPreviewBox.Visible = false;
            fontPreviewBox.Visible = false;
            FMODpanel.Visible = false;
            glControl1.Visible = false;
            lastLoadedAsset = null;
            StatusStripUpdate("");

            FMODreset();

            lastSelectedItem = (AssetPreloadData)e.Item;

            if (e.IsSelected)
            {
                if (enablePreview.Checked)
                {
                    lastLoadedAsset = lastSelectedItem;
                    PreviewAsset(lastLoadedAsset);
                }
                if (displayInfo.Checked && assetInfoLabel.Text != null)//only display the label if asset has info text
                {
                    assetInfoLabel.Text = lastSelectedItem.InfoText;
                    assetInfoLabel.Visible = true;
                }
            }
        }

        private void classesListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                classTextBox.Text = ((TypeItem)classesListView.SelectedItems[0]).ToString();
            }
        }

        private void PreviewAsset(AssetPreloadData asset)
        {
            switch (asset.Type)
            {
                case ClassIDReference.Texture2D:
                    {
                        imageTexture?.Dispose();
                        var m_Texture2D = new Texture2D(asset, true);

                        //Info
                        asset.InfoText = $"Width: {m_Texture2D.m_Width}\nHeight: {m_Texture2D.m_Height}\nFormat: {m_Texture2D.m_TextureFormat}";
                        switch (m_Texture2D.m_FilterMode)
                        {
                            case 0: asset.InfoText += "\nFilter Mode: Point "; break;
                            case 1: asset.InfoText += "\nFilter Mode: Bilinear "; break;
                            case 2: asset.InfoText += "\nFilter Mode: Trilinear "; break;
                        }
                        asset.InfoText += $"\nAnisotropic level: {m_Texture2D.m_Aniso}\nMip map bias: {m_Texture2D.m_MipBias}";
                        switch (m_Texture2D.m_WrapMode)
                        {
                            case 0: asset.InfoText += "\nWrap mode: Repeat"; break;
                            case 1: asset.InfoText += "\nWrap mode: Clamp"; break;
                        }

                        var converter = new Texture2DConverter(m_Texture2D);
                        imageTexture = converter.ConvertToBitmap(true);
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
                            StatusStripUpdate("Unsupported image for preview");
                        }
                        break;
                    }
                case ClassIDReference.AudioClip:
                    {
                        var m_AudioClip = new AudioClip(asset, true);

                        //Info
                        asset.InfoText = "Compression format: ";
                        if (m_AudioClip.version[0] < 5)
                        {
                            switch (m_AudioClip.m_Type)
                            {
                                case AudioType.ACC:
                                    asset.InfoText += "Acc";
                                    break;
                                case AudioType.AIFF:
                                    asset.InfoText += "AIFF";
                                    break;
                                case AudioType.IT:
                                    asset.InfoText += "Impulse tracker";
                                    break;
                                case AudioType.MOD:
                                    asset.InfoText += "Protracker / Fasttracker MOD";
                                    break;
                                case AudioType.MPEG:
                                    asset.InfoText += "MP2/MP3 MPEG";
                                    break;
                                case AudioType.OGGVORBIS:
                                    asset.InfoText += "Ogg vorbis";
                                    break;
                                case AudioType.S3M:
                                    asset.InfoText += "ScreamTracker 3";
                                    break;
                                case AudioType.WAV:
                                    asset.InfoText += "Microsoft WAV";
                                    break;
                                case AudioType.XM:
                                    asset.InfoText += "FastTracker 2 XM";
                                    break;
                                case AudioType.XMA:
                                    asset.InfoText += "Xbox360 XMA";
                                    break;
                                case AudioType.VAG:
                                    asset.InfoText += "PlayStation Portable ADPCM";
                                    break;
                                case AudioType.AUDIOQUEUE:
                                    asset.InfoText += "iPhone";
                                    break;
                                default:
                                    asset.InfoText += "Unknown";
                                    break;
                            }
                        }
                        else
                        {
                            switch (m_AudioClip.m_CompressionFormat)
                            {
                                case AudioCompressionFormat.PCM:
                                    asset.InfoText += "PCM";
                                    break;
                                case AudioCompressionFormat.Vorbis:
                                    asset.InfoText += "Vorbis";
                                    break;
                                case AudioCompressionFormat.ADPCM:
                                    asset.InfoText += "ADPCM";
                                    break;
                                case AudioCompressionFormat.MP3:
                                    asset.InfoText += "MP3";
                                    break;
                                case AudioCompressionFormat.VAG:
                                    asset.InfoText += "PlayStation Portable ADPCM";
                                    break;
                                case AudioCompressionFormat.HEVAG:
                                    asset.InfoText += "PSVita ADPCM";
                                    break;
                                case AudioCompressionFormat.XMA:
                                    asset.InfoText += "Xbox360 XMA";
                                    break;
                                case AudioCompressionFormat.AAC:
                                    asset.InfoText += "AAC";
                                    break;
                                case AudioCompressionFormat.GCADPCM:
                                    asset.InfoText += "Nintendo 3DS/Wii DSP";
                                    break;
                                case AudioCompressionFormat.ATRAC9:
                                    asset.InfoText += "PSVita ATRAC9";
                                    break;
                                default:
                                    asset.InfoText += "Unknown";
                                    break;
                            }
                        }

                        if (m_AudioClip.m_AudioData == null)
                            break;
                        FMOD.CREATESOUNDEXINFO exinfo = new FMOD.CREATESOUNDEXINFO();

                        exinfo.cbsize = Marshal.SizeOf(exinfo);
                        exinfo.length = (uint)m_AudioClip.m_Size;

                        var result = system.createSound(m_AudioClip.m_AudioData, FMOD.MODE.OPENMEMORY | loopMode, ref exinfo, out sound);
                        if (ERRCHECK(result)) { break; }

                        result = sound.getSubSound(0, out var subsound);
                        if (result == FMOD.RESULT.OK)
                        {
                            sound = subsound;
                        }

                        result = sound.getLength(out FMODlenms, FMOD.TIMEUNIT.MS);
                        if (ERRCHECK(result)) { break; }

                        result = system.playSound(sound, null, true, out channel);
                        if (ERRCHECK(result)) { break; }

                        FMODpanel.Visible = true;

                        result = channel.getFrequency(out var frequency);
                        if (ERRCHECK(result)) { break; }

                        FMODinfoLabel.Text = frequency + " Hz";
                        FMODtimerLabel.Text = $"0:0.0 / {FMODlenms / 1000 / 60}:{FMODlenms / 1000 % 60}.{FMODlenms / 10 % 100}";
                        break;
                    }
                case ClassIDReference.Shader:
                    {
                        Shader m_TextAsset = new Shader(asset);
                        string m_Script_Text = Encoding.UTF8.GetString(m_TextAsset.m_Script);
                        m_Script_Text = Regex.Replace(m_Script_Text, "(?<!\r)\n", "\r\n");
                        m_Script_Text = m_Script_Text.Replace("\0", "\\0");
                        textPreviewBox.Text = m_Script_Text;
                        textPreviewBox.Visible = true;
                        break;
                    }
                case ClassIDReference.TextAsset:
                    {
                        TextAsset m_TextAsset = new TextAsset(asset);

                        string m_Script_Text = Encoding.UTF8.GetString(m_TextAsset.m_Script);
                        m_Script_Text = Regex.Replace(m_Script_Text, "(?<!\r)\n", "\r\n");
                        textPreviewBox.Text = m_Script_Text;
                        textPreviewBox.Visible = true;

                        break;
                    }
                case ClassIDReference.MonoBehaviour:
                    {
                        var m_MonoBehaviour = new MonoBehaviour(asset);
                        if (asset.Type1 != asset.Type2 && asset.sourceFile.m_Type.ContainsKey(asset.Type1))
                        {
                            textPreviewBox.Text = asset.Dump();
                        }
                        else
                        {
                            textPreviewBox.Text = GetScriptString(asset);
                        }
                        textPreviewBox.Visible = true;

                        break;
                    }
                case ClassIDReference.Font:
                    {
                        Font m_Font = new Font(asset);
                        if (m_Font.m_FontData != null)
                        {
                            IntPtr data = Marshal.AllocCoTaskMem(m_Font.m_FontData.Length);
                            Marshal.Copy(m_Font.m_FontData, 0, data, m_Font.m_FontData.Length);

                            // We HAVE to do this to register the font to the system (Weird .NET bug !)
                            uint cFonts = 0;
                            var re = AddFontMemResourceEx(data, (uint)m_Font.m_FontData.Length, IntPtr.Zero, ref cFonts);
                            if (re != IntPtr.Zero)
                            {
                                using (var pfc = new PrivateFontCollection())
                                {
                                    pfc.AddMemoryFont(data, m_Font.m_FontData.Length);
                                    Marshal.FreeCoTaskMem(data);
                                    if (pfc.Families.Length > 0)
                                    {
                                        fontPreviewBox.SelectionStart = 0;
                                        fontPreviewBox.SelectionLength = 80;
                                        fontPreviewBox.SelectionFont = new System.Drawing.Font(pfc.Families[0], 16, FontStyle.Regular);
                                        fontPreviewBox.SelectionStart = 81;
                                        fontPreviewBox.SelectionLength = 56;
                                        fontPreviewBox.SelectionFont = new System.Drawing.Font(pfc.Families[0], 12, FontStyle.Regular);
                                        fontPreviewBox.SelectionStart = 138;
                                        fontPreviewBox.SelectionLength = 56;
                                        fontPreviewBox.SelectionFont = new System.Drawing.Font(pfc.Families[0], 18, FontStyle.Regular);
                                        fontPreviewBox.SelectionStart = 195;
                                        fontPreviewBox.SelectionLength = 56;
                                        fontPreviewBox.SelectionFont = new System.Drawing.Font(pfc.Families[0], 24, FontStyle.Regular);
                                        fontPreviewBox.SelectionStart = 252;
                                        fontPreviewBox.SelectionLength = 56;
                                        fontPreviewBox.SelectionFont = new System.Drawing.Font(pfc.Families[0], 36, FontStyle.Regular);
                                        fontPreviewBox.SelectionStart = 309;
                                        fontPreviewBox.SelectionLength = 56;
                                        fontPreviewBox.SelectionFont = new System.Drawing.Font(pfc.Families[0], 48, FontStyle.Regular);
                                        fontPreviewBox.SelectionStart = 366;
                                        fontPreviewBox.SelectionLength = 56;
                                        fontPreviewBox.SelectionFont = new System.Drawing.Font(pfc.Families[0], 60, FontStyle.Regular);
                                        fontPreviewBox.SelectionStart = 423;
                                        fontPreviewBox.SelectionLength = 55;
                                        fontPreviewBox.SelectionFont = new System.Drawing.Font(pfc.Families[0], 72, FontStyle.Regular);
                                        fontPreviewBox.Visible = true;
                                    }
                                }
                                break;
                            }
                        }
                        StatusStripUpdate("Unsupported font for preview. Try to export.");
                        break;
                    }
                case ClassIDReference.Mesh:
                    {
                        var m_Mesh = new Mesh(asset);
                        if (m_Mesh.m_VertexCount > 0)
                        {
                            viewMatrixData = Matrix4.CreateRotationY(-(float)Math.PI / 4) * Matrix4.CreateRotationX(-(float)Math.PI / 6);
                            #region Vertices
                            if (m_Mesh.m_Vertices == null || m_Mesh.m_Vertices.Length == 0)
                            {
                                StatusStripUpdate("Mesh can't be previewed.");
                                return;
                            }
                            int count = 3;
                            if (m_Mesh.m_Vertices.Length == m_Mesh.m_VertexCount * 4)
                            {
                                count = 4;
                            }
                            vertexData = new Vector3[m_Mesh.m_VertexCount];
                            // Calculate Bounding
                            float[] min = new float[3];
                            float[] max = new float[3];
                            for (int i = 0; i < 3; i++)
                            {
                                min[i] = m_Mesh.m_Vertices[i];
                                max[i] = m_Mesh.m_Vertices[i];
                            }
                            for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    min[i] = Math.Min(min[i], m_Mesh.m_Vertices[v * count + i]);
                                    max[i] = Math.Max(max[i], m_Mesh.m_Vertices[v * count + i]);
                                }
                                vertexData[v] = new Vector3(
                                    m_Mesh.m_Vertices[v * count],
                                    m_Mesh.m_Vertices[v * count + 1],
                                    m_Mesh.m_Vertices[v * count + 2]);
                            }

                            // Calculate modelMatrix
                            Vector3 dist = Vector3.One, offset = Vector3.Zero;
                            for (int i = 0; i < 3; i++)
                            {
                                dist[i] = max[i] - min[i];
                                offset[i] = (max[i] + min[i]) / 2;
                            }
                            float d = Math.Max(1e-5f, dist.Length);
                            modelMatrixData = Matrix4.CreateTranslation(-offset) * Matrix4.CreateScale(2f / d);
                            #endregion
                            #region Indicies
                            indiceData = new int[m_Mesh.m_Indices.Count];
                            for (int i = 0; i < m_Mesh.m_Indices.Count; i = i + 3)
                            {
                                indiceData[i] = (int)m_Mesh.m_Indices[i];
                                indiceData[i + 1] = (int)m_Mesh.m_Indices[i + 1];
                                indiceData[i + 2] = (int)m_Mesh.m_Indices[i + 2];
                            }
                            #endregion
                            #region Normals
                            if (m_Mesh.m_Normals != null && m_Mesh.m_Normals.Length > 0)
                            {
                                if (m_Mesh.m_Normals.Length == m_Mesh.m_VertexCount * 3)
                                    count = 3;
                                else if (m_Mesh.m_Normals.Length == m_Mesh.m_VertexCount * 4)
                                    count = 4;
                                normalData = new Vector3[m_Mesh.m_VertexCount];
                                for (int n = 0; n < m_Mesh.m_VertexCount; n++)
                                {
                                    normalData[n] = new Vector3(
                                        m_Mesh.m_Normals[n * count],
                                        m_Mesh.m_Normals[n * count + 1],
                                        m_Mesh.m_Normals[n * count + 2]);
                                }
                            }
                            else
                                normalData = null;
                            // calculate normal by ourself
                            normal2Data = new Vector3[m_Mesh.m_VertexCount];
                            int[] normalCalculatedCount = new int[m_Mesh.m_VertexCount];
                            for (int i = 0; i < m_Mesh.m_VertexCount; i++)
                            {
                                normal2Data[i] = Vector3.Zero;
                                normalCalculatedCount[i] = 0;
                            }
                            for (int i = 0; i < m_Mesh.m_Indices.Count; i = i + 3)
                            {
                                Vector3 dir1 = vertexData[indiceData[i + 1]] - vertexData[indiceData[i]];
                                Vector3 dir2 = vertexData[indiceData[i + 2]] - vertexData[indiceData[i]];
                                Vector3 normal = Vector3.Cross(dir1, dir2);
                                normal.Normalize();
                                for (int j = 0; j < 3; j++)
                                {
                                    normal2Data[indiceData[i + j]] += normal;
                                    normalCalculatedCount[indiceData[i + j]]++;
                                }
                            }
                            for (int i = 0; i < m_Mesh.m_VertexCount; i++)
                            {
                                if (normalCalculatedCount[i] == 0)
                                    normal2Data[i] = new Vector3(0, 1, 0);
                                else
                                    normal2Data[i] /= normalCalculatedCount[i];
                            }
                            #endregion
                            #region Colors
                            if (m_Mesh.m_Colors != null && m_Mesh.m_Colors.Length == m_Mesh.m_VertexCount * 3)
                            {
                                colorData = new Vector4[m_Mesh.m_VertexCount];
                                for (int c = 0; c < m_Mesh.m_VertexCount; c++)
                                {
                                    colorData[c] = new Vector4(
                                        m_Mesh.m_Colors[c * 3],
                                        m_Mesh.m_Colors[c * 3 + 1],
                                        m_Mesh.m_Colors[c * 3 + 2],
                                        1.0f);
                                }
                            }
                            else if (m_Mesh.m_Colors != null && m_Mesh.m_Colors.Length == m_Mesh.m_VertexCount * 4)
                            {
                                colorData = new Vector4[m_Mesh.m_VertexCount];
                                for (int c = 0; c < m_Mesh.m_VertexCount; c++)
                                {
                                    colorData[c] = new Vector4(
                                    m_Mesh.m_Colors[c * 4],
                                    m_Mesh.m_Colors[c * 4 + 1],
                                    m_Mesh.m_Colors[c * 4 + 2],
                                    m_Mesh.m_Colors[c * 4 + 3]);
                                }
                            }
                            else
                            {
                                colorData = new Vector4[m_Mesh.m_VertexCount];
                                for (int c = 0; c < m_Mesh.m_VertexCount; c++)
                                {
                                    colorData[c] = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
                                }
                            }
                            #endregion
                            glControl1.Visible = true;
                            createVAO();
                        }
                        StatusStripUpdate("Using OpenGL Version: " + GL.GetString(StringName.Version) + "\n"
                                        + "'Mouse Left'=Rotate | 'Mouse Right'=Move | 'Mouse Wheel'=Zoom \n"
                                        + "'Ctrl W'=Wireframe | 'Ctrl S'=Shade | 'Ctrl N'=ReNormal ");
                    }
                    break;
                case ClassIDReference.VideoClip:
                case ClassIDReference.MovieTexture:
                    {
                        StatusStripUpdate("Only supported export.");
                        break;
                    }
                case ClassIDReference.Sprite:
                    {
                        imageTexture?.Dispose();
                        imageTexture = SpriteHelper.GetImageFromSprite(new Sprite(asset));
                        if (imageTexture != null)
                        {
                            asset.InfoText = $"Width: {imageTexture.Width}\nHeight: {imageTexture.Height}\n";
                            previewPanel.BackgroundImage = imageTexture;
                            if (imageTexture.Width > previewPanel.Width || imageTexture.Height > previewPanel.Height)
                                previewPanel.BackgroundImageLayout = ImageLayout.Zoom;
                            else
                                previewPanel.BackgroundImageLayout = ImageLayout.Center;
                        }
                        else
                        {
                            StatusStripUpdate("Unsupported sprite for preview.");
                        }
                        break;
                    }
                case ClassIDReference.Animator:
                    {
                        StatusStripUpdate("Can be exported to FBX file.");
                        break;
                    }
                case ClassIDReference.AnimationClip:
                    {
                        StatusStripUpdate("Can be exported with Animator or objects");
                        break;
                    }
                default:
                    {
                        var str = asset.Dump();
                        if (str != null)
                        {
                            textPreviewBox.Text = str;
                            textPreviewBox.Visible = true;
                        }
                        else
                            StatusStripUpdate("Only supported export the raw file.");
                        break;
                    }
            }
        }

        private void FMODinit()
        {
            FMODreset();

            var result = FMOD.Factory.System_Create(out system);
            if (ERRCHECK(result)) { return; }

            result = system.getVersion(out var version);
            ERRCHECK(result);
            if (version < FMOD.VERSION.number)
            {
                MessageBox.Show($"Error!  You are using an old version of FMOD {version:X}.  This program requires {FMOD.VERSION.number:X}.");
                Application.Exit();
            }

            result = system.init(1, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
            if (ERRCHECK(result)) { return; }

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

            if (sound != null && sound.isValid())
            {
                var result = sound.release();
                ERRCHECK(result);
                sound = null;
            }
        }

        private void FMODplayButton_Click(object sender, EventArgs e)
        {
            if (sound != null && channel != null)
            {
                timer.Start();
                var result = channel.isPlaying(out var playing);
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
            if (sound != null && channel != null)
            {
                var result = channel.isPlaying(out var playing);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    if (ERRCHECK(result)) { return; }
                }

                if (playing)
                {
                    result = channel.getPaused(out var paused);
                    if (ERRCHECK(result)) { return; }
                    result = channel.setPaused(!paused);
                    if (ERRCHECK(result)) { return; }

                    if (paused)
                    {
                        FMODstatusLabel.Text = "Playing";
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
            if (channel != null)
            {
                var result = channel.isPlaying(out var playing);
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

            loopMode = FMODloopButton.Checked ? FMOD.MODE.LOOP_NORMAL : FMOD.MODE.LOOP_OFF;

            if (sound != null)
            {
                result = sound.setMode(loopMode);
                if (ERRCHECK(result)) { return; }
            }

            if (channel != null)
            {
                result = channel.isPlaying(out var playing);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    if (ERRCHECK(result)) { return; }
                }

                result = channel.getPaused(out var paused);
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
            FMODVolume = Convert.ToSingle(FMODvolumeBar.Value) / 10;

            var result = masterSoundGroup.setVolume(FMODVolume);
            if (ERRCHECK(result)) { return; }
        }

        private void FMODprogressBar_Scroll(object sender, EventArgs e)
        {
            if (channel != null)
            {
                uint newms = FMODlenms / 1000 * (uint)FMODprogressBar.Value;
                FMODtimerLabel.Text = $"{newms / 1000 / 60}:{newms / 1000 % 60}.{newms / 10 % 100}/{FMODlenms / 1000 / 60}:{FMODlenms / 1000 % 60}.{FMODlenms / 10 % 100}";
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
                uint newms = FMODlenms / 1000 * (uint)FMODprogressBar.Value;

                var result = channel.setPosition(newms, FMOD.TIMEUNIT.MS);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    if (ERRCHECK(result)) { return; }
                }


                result = channel.isPlaying(out var playing);
                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                {
                    if (ERRCHECK(result)) { return; }
                }

                if (playing) { timer.Start(); }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            uint ms = 0;
            bool playing = false;
            bool paused = false;

            if (channel != null)
            {
                var result = channel.getPosition(out ms, FMOD.TIMEUNIT.MS);
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

            FMODtimerLabel.Text = $"{ms / 1000 / 60}:{ms / 1000 % 60}.{ms / 10 % 100} / {FMODlenms / 1000 / 60}:{FMODlenms / 1000 % 60}.{FMODlenms / 10 % 100}";
            FMODprogressBar.Value = (int)(ms * 1000 / FMODlenms);
            FMODstatusLabel.Text = paused ? "Paused " : playing ? "Playing" : "Stopped";

            if (system != null && channel != null)
            {
                system.update();
            }
        }

        private bool ERRCHECK(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                FMODreset();
                StatusStripUpdate($"FMOD error! {result} - {FMOD.Error.String(result)}");
                return true;
            }
            return false;
        }

        private void exportallobjectssplitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sceneTreeView.Nodes.Count > 0)
            {
                var saveFolderDialog1 = new OpenFolderDialog();
                if (saveFolderDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    var savePath = saveFolderDialog1.Folder + "\\";
                    progressBar1.Value = 0;
                    progressBar1.Maximum = sceneTreeView.Nodes.Cast<TreeNode>().Sum(x => x.Nodes.Count);
                    ExportSplitObjects(savePath, sceneTreeView.Nodes);
                }
            }
            else
            {
                StatusStripUpdate("No Objects available for export");
            }
        }

        private void ExportObjects_Click(object sender, EventArgs e)
        {
            if (sceneTreeView.Nodes.Count > 0)
            {
                var exportAll = ((ToolStripItem)sender).Name == "exportallobjectsMenuItem";

                saveFileDialog1.FileName = productName + DateTime.Now.ToString("_yy_MM_dd__HH_mm_ss");

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    var gameObjects = new List<GameObject>();
                    foreach (var node in treeNodeDictionary.Values)
                    {
                        if (node.Checked || exportAll)
                        {
                            gameObjects.Add(node.gameObject);
                        }
                    }

                    progressBar1.Value = 0;
                    progressBar1.Maximum = 1;
                    if (gameObjects.Count == 0)
                    {
                        progressBar1.PerformStep();
                        toolStripStatusLabel1.Text = "Nothing exported.";
                        return;
                    }
                    toolStripStatusLabel1.Text = $"Exporting {Path.GetFileName(saveFileDialog1.FileName)}";
                    FBXExporter.WriteFBX(saveFileDialog1.FileName, gameObjects);
                    toolStripStatusLabel1.Text = $"Finished exporting {Path.GetFileName(saveFileDialog1.FileName)}";
                    progressBar1.PerformStep();
                    if (openAfterExport.Checked && File.Exists(saveFileDialog1.FileName))
                    {
                        Process.Start(Path.GetDirectoryName(saveFileDialog1.FileName));
                    }
                }
            }
            else
            {
                toolStripStatusLabel1.Text = "No Objects available for export";
            }
        }

        private void ExportAssets_Click(object sender, EventArgs e)
        {
            if (exportableAssets.Count > 0)
            {
                var saveFolderDialog1 = new OpenFolderDialog();
                if (saveFolderDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    timer.Stop();

                    List<AssetPreloadData> toExportAssets = null;
                    switch (((ToolStripItem)sender).Name)
                    {
                        case "exportAllAssetsMenuItem":
                            toExportAssets = exportableAssets;
                            break;
                        case "exportFilteredAssetsMenuItem":
                            toExportAssets = visibleAssets;
                            break;
                        case "exportSelectedAssetsMenuItem":
                            toExportAssets = new List<AssetPreloadData>(assetListView.SelectedIndices.Count);
                            foreach (int i in assetListView.SelectedIndices)
                            {
                                toExportAssets.Add((AssetPreloadData)assetListView.Items[i]);
                            }
                            break;
                    }
                    ExportAssets(saveFolderDialog1.Folder, toExportAssets, assetGroupOptions.SelectedIndex, openAfterExport.Checked);
                }
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

        private void ProgressBarMaximumAdd(int value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => { progressBar1.Maximum += value; }));
            }
            else
            {
                progressBar1.Maximum += value;
            }
        }

        public AssetStudioForm()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            InitializeComponent();
            displayOriginalName.Checked = (bool)Properties.Settings.Default["displayOriginalName"];
            displayAll.Checked = (bool)Properties.Settings.Default["displayAll"];
            displayInfo.Checked = (bool)Properties.Settings.Default["displayInfo"];
            enablePreview.Checked = (bool)Properties.Settings.Default["enablePreview"];
            openAfterExport.Checked = (bool)Properties.Settings.Default["openAfterExport"];
            assetGroupOptions.SelectedIndex = (int)Properties.Settings.Default["assetGroupOption"];
            FMODinit();
            //UI
            Studio.SetProgressBarValue = SetProgressBarValue;
            Studio.SetProgressBarMaximum = SetProgressBarMaximum;
            Studio.ProgressBarPerformStep = ProgressBarPerformStep;
            Studio.StatusStripUpdate = StatusStripUpdate;
            Studio.ProgressBarMaximumAdd = ProgressBarMaximumAdd;
        }

        private void initOpenTK()
        {
            changeGLSize(glControl1.Size);
            GL.ClearColor(Color.CadetBlue);
            pgmID = GL.CreateProgram();
            loadShader("vs", ShaderType.VertexShader, pgmID, out int vsID);
            loadShader("fs", ShaderType.FragmentShader, pgmID, out int fsID);
            GL.LinkProgram(pgmID);

            pgmColorID = GL.CreateProgram();
            loadShader("vs", ShaderType.VertexShader, pgmColorID, out vsID);
            loadShader("fsColor", ShaderType.FragmentShader, pgmColorID, out fsID);
            GL.LinkProgram(pgmColorID);

            pgmBlackID = GL.CreateProgram();
            loadShader("vs", ShaderType.VertexShader, pgmBlackID, out vsID);
            loadShader("fsBlack", ShaderType.FragmentShader, pgmBlackID, out fsID);
            GL.LinkProgram(pgmBlackID);

            attributeVertexPosition = GL.GetAttribLocation(pgmID, "vertexPosition");
            attributeNormalDirection = GL.GetAttribLocation(pgmID, "normalDirection");
            attributeVertexColor = GL.GetAttribLocation(pgmColorID, "vertexColor");
            uniformModelMatrix = GL.GetUniformLocation(pgmID, "modelMatrix");
            uniformViewMatrix = GL.GetUniformLocation(pgmID, "viewMatrix");
            uniformProjMatrix = GL.GetUniformLocation(pgmID, "projMatrix");
        }

        private void loadShader(string filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            var str = (string)Properties.Resources.ResourceManager.GetObject(filename);
            GL.ShaderSource(address, str);
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            GL.DeleteShader(address);
        }

        private void createVBO(out int vboAddress, Vector3[] data, int address)
        {
            GL.GenBuffers(1, out vboAddress);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboAddress);
            GL.BufferData(BufferTarget.ArrayBuffer,
                                    (IntPtr)(data.Length * Vector3.SizeInBytes),
                                    data,
                                    BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(address, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(address);
        }

        private void createVBO(out int vboAddress, Vector4[] data, int address)
        {
            GL.GenBuffers(1, out vboAddress);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboAddress);
            GL.BufferData(BufferTarget.ArrayBuffer,
                                    (IntPtr)(data.Length * Vector4.SizeInBytes),
                                    data,
                                    BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(address, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(address);
        }

        private void createVBO(out int vboAddress, Matrix4 data, int address)
        {
            GL.GenBuffers(1, out vboAddress);
            GL.UniformMatrix4(address, false, ref data);
        }

        private void createEBO(out int address, int[] data)
        {
            GL.GenBuffers(1, out address);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, address);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                            (IntPtr)(data.Length * sizeof(int)),
                            data,
                            BufferUsageHint.StaticDraw);
        }

        private void createVAO()
        {
            GL.DeleteVertexArray(vao);
            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);
            createVBO(out var vboPositions, vertexData, attributeVertexPosition);
            if (normalMode == 0)
            {
                createVBO(out var vboNormals, normal2Data, attributeNormalDirection);
            }
            else
            {
                if (normalData != null)
                    createVBO(out var vboNormals, normalData, attributeNormalDirection);
            }
            createVBO(out var vboColors, colorData, attributeVertexColor);
            createVBO(out var vboModelMatrix, modelMatrixData, uniformModelMatrix);
            createVBO(out var vboViewMatrix, viewMatrixData, uniformViewMatrix);
            createVBO(out var vboProjMatrix, projMatrixData, uniformProjMatrix);
            createEBO(out var eboElements, indiceData);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        private void changeGLSize(Size size)
        {
            GL.Viewport(0, 0, size.Width, size.Height);

            if (size.Width <= size.Height)
            {
                float k = 1.0f * size.Width / size.Height;
                projMatrixData = Matrix4.CreateScale(1, k, 1);
            }
            else
            {
                float k = 1.0f * size.Height / size.Width;
                projMatrixData = Matrix4.CreateScale(k, 1, 1);
            }
        }

        private void preview_Resize(object sender, EventArgs e)
        {
            if (glControlLoaded && glControl1.Visible)
            {
                changeGLSize(glControl1.Size);
                glControl1.Invalidate();
            }
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            initOpenTK();
            glControlLoaded = true;
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            glControl1.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.BindVertexArray(vao);
            if (wireFrameMode == 0 || wireFrameMode == 2)
            {
                GL.UseProgram(shadeMode == 0 ? pgmID : pgmColorID);
                GL.UniformMatrix4(uniformModelMatrix, false, ref modelMatrixData);
                GL.UniformMatrix4(uniformViewMatrix, false, ref viewMatrixData);
                GL.UniformMatrix4(uniformProjMatrix, false, ref projMatrixData);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                GL.DrawElements(BeginMode.Triangles, indiceData.Length, DrawElementsType.UnsignedInt, 0);
            }
            //Wireframe
            if (wireFrameMode == 1 || wireFrameMode == 2)
            {
                GL.Enable(EnableCap.PolygonOffsetLine);
                GL.PolygonOffset(-1, -1);
                GL.UseProgram(pgmBlackID);
                GL.UniformMatrix4(uniformModelMatrix, false, ref modelMatrixData);
                GL.UniformMatrix4(uniformViewMatrix, false, ref viewMatrixData);
                GL.UniformMatrix4(uniformProjMatrix, false, ref projMatrixData);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.DrawElements(BeginMode.Triangles, indiceData.Length, DrawElementsType.UnsignedInt, 0);
                GL.Disable(EnableCap.PolygonOffsetLine);
            }
            GL.BindVertexArray(0);
            GL.Flush();
            glControl1.SwapBuffers();
        }

        private void glControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (glControl1.Visible)
            {
                viewMatrixData *= Matrix4.CreateScale(1 + e.Delta / 1000f);
                glControl1.Invalidate();
            }
        }

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            mdx = e.X;
            mdy = e.Y;
            if (e.Button == MouseButtons.Left)
            {
                lmdown = true;
            }
            if (e.Button == MouseButtons.Right)
            {
                rmdown = true;
            }
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (lmdown || rmdown)
            {
                float dx = mdx - e.X;
                float dy = mdy - e.Y;
                mdx = e.X;
                mdy = e.Y;
                if (lmdown)
                {
                    dx *= 0.01f;
                    dy *= 0.01f;
                    viewMatrixData *= Matrix4.CreateRotationX(dy);
                    viewMatrixData *= Matrix4.CreateRotationY(dx);
                }
                if (rmdown)
                {
                    dx *= 0.003f;
                    dy *= 0.003f;
                    viewMatrixData *= Matrix4.CreateTranslation(-dx, dy, 0);
                }
                glControl1.Invalidate();
            }
        }

        private void glControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lmdown = false;
            }
            if (e.Button == MouseButtons.Right)
            {
                rmdown = false;
            }
        }

        private void resetForm()
        {
            Text = "AssetStudio";

            importFiles.Clear();
            foreach (var assetsFile in assetsfileList)
            {
                assetsFile.reader.Dispose();
            }
            assetsfileList.Clear();
            exportableAssets.Clear();
            visibleAssets.Clear();
            foreach (var resourceFileReader in resourceFileReaders)
            {
                resourceFileReader.Value.Dispose();
            }
            resourceFileReaders.Clear();
            sharedFileIndex.Clear();
            productName = "";

            sceneTreeView.Nodes.Clear();

            assetListView.VirtualListSize = 0;
            assetListView.Items.Clear();

            classesListView.Items.Clear();
            classesListView.Groups.Clear();

            previewPanel.BackgroundImage = Properties.Resources.preview;
            previewPanel.BackgroundImageLayout = ImageLayout.Center;
            assetInfoLabel.Visible = false;
            assetInfoLabel.Text = null;
            textPreviewBox.Visible = false;
            fontPreviewBox.Visible = false;
            glControl1.Visible = false;
            lastSelectedItem = null;
            lastLoadedAsset = null;
            firstSortColumn = -1;
            secondSortColumn = 0;
            reverseSort = false;
            enableFiltering = false;
            listSearch.Text = " Filter ";

            var count = filterTypeToolStripMenuItem.DropDownItems.Count;
            for (var i = 1; i < count; i++)
            {
                filterTypeToolStripMenuItem.DropDownItems.RemoveAt(1);
            }

            FMODreset();

            moduleLoaded = false;
            LoadedModuleDic.Clear();
            treeNodeCollection.Clear();
            treeNodeDictionary.Clear();
        }

        private void assetListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && assetListView.SelectedIndices.Count > 0)
            {
                jumpToSceneHierarchyToolStripMenuItem.Visible = false;
                showOriginalFileToolStripMenuItem.Visible = false;
                exportAnimatorwithselectedAnimationClipMenuItem.Visible = false;
                exportobjectswithselectedAnimationClipMenuItem.Visible = false;

                if (assetListView.SelectedIndices.Count == 1)
                {
                    jumpToSceneHierarchyToolStripMenuItem.Visible = true;
                    showOriginalFileToolStripMenuItem.Visible = true;
                }
                if (assetListView.SelectedIndices.Count >= 1)
                {
                    var selectedAssets = GetSelectedAssets();
                    if (selectedAssets.Any(x => x.Type == ClassIDReference.Animator) && selectedAssets.Any(x => x.Type == ClassIDReference.AnimationClip))
                    {
                        exportAnimatorwithselectedAnimationClipMenuItem.Visible = true;
                    }
                    else if (selectedAssets.All(x => x.Type == ClassIDReference.AnimationClip))
                    {
                        exportobjectswithselectedAnimationClipMenuItem.Visible = true;
                    }
                }

                contextMenuStrip1.Show(assetListView, e.X, e.Y);
            }
        }

        private void exportSelectedAssetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFolderDialog1 = new OpenFolderDialog();
            if (saveFolderDialog1.ShowDialog(this) == DialogResult.OK)
            {
                timer.Stop();
                ExportAssets(saveFolderDialog1.Folder, GetSelectedAssets(), assetGroupOptions.SelectedIndex, openAfterExport.Checked);
            }
        }

        private void showOriginalFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectasset = (AssetPreloadData)assetListView.Items[assetListView.SelectedIndices[0]];
            var args = $"/select, \"{selectasset.sourceFile.parentPath ?? selectasset.sourceFile.filePath}\"";
            var pfi = new ProcessStartInfo("explorer.exe", args);
            Process.Start(pfi);
        }

        private void exportAnimatorwithAnimationClipMenuItem_Click(object sender, EventArgs e)
        {
            AssetPreloadData animator = null;
            List<AssetPreloadData> animationList = new List<AssetPreloadData>();
            var selectedAssets = GetSelectedAssets();
            foreach (var assetPreloadData in selectedAssets)
            {
                if (assetPreloadData.Type == ClassIDReference.Animator)
                {
                    animator = assetPreloadData;
                }
                else if (assetPreloadData.Type == ClassIDReference.AnimationClip)
                {
                    animationList.Add(assetPreloadData);
                }
            }

            if (animator != null)
            {
                var saveFolderDialog1 = new OpenFolderDialog();
                if (saveFolderDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    var exportPath = saveFolderDialog1.Folder + "\\Animator\\";
                    progressBar1.Value = 0;
                    progressBar1.Maximum = 1;
                    ExportAnimatorWithAnimationClip(animator, animationList, exportPath);
                }
            }
        }

        private void exportSelectedObjectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sceneTreeView.Nodes.Count > 0)
            {
                var saveFolderDialog1 = new OpenFolderDialog();
                if (saveFolderDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    var exportPath = saveFolderDialog1.Folder + "\\GameObject\\";
                    ExportObjectsWithAnimationClip(exportPath, sceneTreeView.Nodes);
                }
            }
            else
            {
                StatusStripUpdate("No Objects available for export");
            }
        }

        private void exportObjectswithAnimationClipMenuItem_Click(object sender, EventArgs e)
        {
            if (sceneTreeView.Nodes.Count > 0)
            {
                var saveFolderDialog1 = new OpenFolderDialog();
                if (saveFolderDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    var exportPath = saveFolderDialog1.Folder + "\\GameObject\\";
                    var animationList = GetSelectedAssets().Where(x => x.Type == ClassIDReference.AnimationClip).ToList();
                    ExportObjectsWithAnimationClip(exportPath, sceneTreeView.Nodes, animationList.Count == 0 ? null : animationList);
                }
            }
            else
            {
                StatusStripUpdate("No Objects available for export");
            }
        }

        private void jumpToSceneHierarchyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectasset = (AssetPreloadData)assetListView.Items[assetListView.SelectedIndices[0]];
            if (selectasset.gameObject != null)
            {
                sceneTreeView.SelectedNode = treeNodeDictionary[selectasset.gameObject];
                tabControl1.SelectedTab = tabPage1;
            }
        }

        private void exportAllObjectssplitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (sceneTreeView.Nodes.Count > 0)
            {
                var saveFolderDialog1 = new OpenFolderDialog();
                if (saveFolderDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    var savePath = saveFolderDialog1.Folder + "\\";
                    progressBar1.Value = 0;
                    progressBar1.Maximum = sceneTreeView.Nodes.Cast<TreeNode>().Sum(x => x.Nodes.Count); ;
                    ExportSplitObjects(savePath, sceneTreeView.Nodes, true);
                }
            }
            else
            {
                StatusStripUpdate("No Objects available for export");
            }
        }

        private List<AssetPreloadData> GetSelectedAssets()
        {
            var selectedAssets = new List<AssetPreloadData>();
            foreach (int index in assetListView.SelectedIndices)
            {
                selectedAssets.Add((AssetPreloadData)assetListView.Items[index]);
            }

            return selectedAssets;
        }

        private void FilterAssetList()
        {
            assetListView.BeginUpdate();
            assetListView.SelectedIndices.Clear();
            var show = new List<ClassIDReference>();
            if (!allToolStripMenuItem.Checked)
            {
                for (var i = 1; i < filterTypeToolStripMenuItem.DropDownItems.Count; i++)
                {
                    var item = (ToolStripMenuItem)filterTypeToolStripMenuItem.DropDownItems[i];
                    if (item.Checked)
                    {
                        show.Add((ClassIDReference)Enum.Parse(typeof(ClassIDReference), item.Text));
                    }
                }
                visibleAssets = exportableAssets.FindAll(x => show.Contains(x.Type));
            }
            else
            {
                visibleAssets = exportableAssets;
            }
            if (listSearch.Text != " Filter ")
            {
                visibleAssets = visibleAssets.FindAll(x => x.Text.IndexOf(listSearch.Text, StringComparison.CurrentCultureIgnoreCase) >= 0);
            }
            assetListView.VirtualListSize = visibleAssets.Count;
            assetListView.EndUpdate();
        }
    }
}
