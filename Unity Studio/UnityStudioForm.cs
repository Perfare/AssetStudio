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
using static Unity_Studio.UnityStudio;

namespace Unity_Studio
{
    partial class UnityStudioForm : Form
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
        private float FMODfrequency;

        private Bitmap imageTexture;

        #region OpenTK variables
        int pgmID, pgmColorID, pgmBlackID;
        int attributeVertexPosition;
        int attributeNormalDirection;
        int attributeVertexColor;
        int uniformModelMatrix;
        int uniformViewMatrix;
        int vao;
        int vboPositions;
        int vboNormals;
        int vboColors;
        int vboModelMatrix;
        int vboViewMatrix;
        int eboElements;
        Vector3[] vertexData;
        Vector3[] normalData;
        Vector3[] normal2Data;
        Vector4[] colorData;
        Matrix4 modelMatrixData;
        Matrix4 viewMatrixData;
        int[] indiceData;
        int wireFrameMode;
        int shadeMode;
        int normalMode;
        #endregion

        //asset list sorting helpers
        private int firstSortColumn = -1;
        private int secondSortColumn;
        private bool reverseSort;
        private bool enableFiltering;

        //tree search
        private int nextGObject;
        private List<GameObject> treeSrcResults = new List<GameObject>();

        private PrivateFontCollection pfc = new PrivateFontCollection();

        private AssetPreloadData selectasset;

        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);


        private void loadFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                resetForm();
                ThreadPool.QueueUserWorkItem(state =>
                {
                    var bundle = false;
                    if (openFileDialog1.FilterIndex == 1 || openFileDialog1.FilterIndex == 3)
                    {
                        if (CheckBundleFile(openFileDialog1.FileNames[0]))
                        {
                            if (openFileDialog1.FileNames.Length > 1)
                            {
                                MessageBox.Show($"{Path.GetFileName(openFileDialog1.FileNames[0])} is bundle file, please select bundle file type to load this file");
                                return;
                            }
                            bundle = true;
                        }
                    }
                    else
                    {
                        bundle = true;
                    }
                    if (!bundle)
                    {
                        mainPath = Path.GetDirectoryName(openFileDialog1.FileNames[0]);
                        MergeSplitAssets(mainPath);
                        var readFile = ProcessingSplitFiles(openFileDialog1.FileNames.ToList());
                        foreach (var i in readFile)
                        {
                            unityFiles.Add(i);
                            unityFilesHash.Add(Path.GetFileName(i));
                        }
                        SetProgressBarValue(0);
                        SetProgressBarMaximum(unityFiles.Count);
                        //use a for loop because list size can change
                        for (int f = 0; f < unityFiles.Count; f++)
                        {
                            var fileName = unityFiles[f];
                            StatusStripUpdate("Loading " + Path.GetFileName(fileName));
                            LoadAssetsFile(fileName);
                            ProgressBarPerformStep();
                        }
                    }
                    else
                    {
                        SetProgressBarValue(0);
                        SetProgressBarMaximum(unityFiles.Count);
                        foreach (var filename in openFileDialog1.FileNames)
                        {
                            LoadBundleFile(filename);
                            ProgressBarPerformStep();
                        }
                        BuildSharedIndex();
                    }
                    unityFilesHash.Clear();
                    assetsfileListHash.Clear();
                    sharedFileIndex.Clear();
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
                        unityFiles.Add(i);
                        unityFilesHash.Add(Path.GetFileName(i));
                    }
                    SetProgressBarValue(0);
                    SetProgressBarMaximum(unityFiles.Count);
                    //use a for loop because list size can change
                    for (int f = 0; f < unityFiles.Count; f++)
                    {
                        var fileName = unityFiles[f];
                        StatusStripUpdate("Loading " + Path.GetFileName(fileName));
                        LoadAssetsFile(fileName);
                        ProgressBarPerformStep();
                    }
                    unityFilesHash.Clear();
                    assetsfileListHash.Clear();
                    sharedFileIndex.Clear();
                    BuildAssetStrucutres();
                });
            }
        }

        private void extractBundleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openBundleDialog = new OpenFileDialog();
            openBundleDialog.Filter = "Unity bundle files|*.*";
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
                        extractedCount += extractBundleFile(fileName);
                        ProgressBarPerformStep();
                    }
                    StatusStripUpdate($"Finished extracting {extractedCount} files.");
                });
            }
        }

        private void extractFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int extractedCount = 0;
            var openFolderDialog1 = new OpenFolderDialog();
            if (openFolderDialog1.ShowDialog(this) == DialogResult.OK)
            {
                string startPath = openFolderDialog1.Folder;
                var bundleFiles = Directory.GetFiles(startPath, "*.*", SearchOption.AllDirectories).ToList();
                progressBar1.Value = 0;
                progressBar1.Maximum = bundleFiles.Count;
                ThreadPool.QueueUserWorkItem(delegate
                {
                    foreach (var fileName in bundleFiles)
                    {
                        extractedCount += extractBundleFile(fileName);
                        ProgressBarPerformStep();
                    }
                    StatusStripUpdate($"Finished extracting {extractedCount} files.");
                });
            }
        }

        private void BuildAssetStrucutres()
        {
            bool optionLoadAssetsMenuItem = !dontLoadAssetsMenuItem.Checked;
            bool optionDisplayAll = displayAll.Checked;
            bool optionBuildHierarchyMenuItem = !dontBuildHierarchyMenuItem.Checked;
            bool optionBuildClassStructuresMenuItem = buildClassStructuresMenuItem.Checked;

            BuildAssetStructures(optionLoadAssetsMenuItem, optionDisplayAll, optionBuildHierarchyMenuItem, optionBuildClassStructuresMenuItem, displayOriginalName.Checked);

            BeginInvoke(new Action(() =>
            {
                if (productName != "")
                {
                    Text = $"Unity Studio - {productName} - {assetsfileList[0].m_Version} - {assetsfileList[0].platformStr}";
                }
                else if (assetsfileList.Count > 0)
                {
                    Text = $"Unity Studio - no productName - {assetsfileList[0].m_Version} - {assetsfileList[0].platformStr}";
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
                    sceneTreeView.Nodes.AddRange(fileNodes.ToArray());
                    fileNodes.Clear();
                    sceneTreeView.EndUpdate();
                }
                if (buildClassStructuresMenuItem.Checked)
                {
                    classesListView.BeginUpdate();
                    foreach (var version in AllClassStructures)
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
                StatusStripUpdate($"Finished loading {assetsfileList.Count} files with {assetListView.Items.Count} exportable assets.");
                treeSearch.Select();
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

            if (glControl1.Visible)
            {
                switch (e.KeyCode)
                {
                    case Keys.D: // --> Right
                        if (e.Shift) //Move
                        {
                            viewMatrixData *= Matrix4.CreateTranslation(0.1f, 0, 0);
                        }
                        else //Rotate
                        {
                            viewMatrixData *= Matrix4.CreateRotationY(0.1f);
                        }
                        glControl1.Invalidate();
                        break;
                    case Keys.A: // <-- Left
                        if (e.Shift) //Move
                        {
                            viewMatrixData *= Matrix4.CreateTranslation(-0.1f, 0, 0);
                        }
                        else //Rotate
                        {
                            viewMatrixData *= Matrix4.CreateRotationY(-0.1f);
                        }
                        glControl1.Invalidate();
                        break;
                    case Keys.W: // Up 
                        if (e.Control) //Toggle WireFrame
                        {
                            wireFrameMode = (wireFrameMode + 1) % 3;
                            glControl1.Invalidate();
                        }
                        else if (e.Shift) //Move
                        {
                            viewMatrixData *= Matrix4.CreateTranslation(0, 0.1f, 0);
                        }
                        else //Rotate
                        {
                            viewMatrixData *= Matrix4.CreateRotationX(0.1f);
                        }
                        glControl1.Invalidate();
                        break;
                    case Keys.S: // Down
                        if (e.Control) //Toggle Shade
                        {
                            shadeMode = (shadeMode + 1) % 2;
                            glControl1.Invalidate();
                        }
                        else if (e.Shift) //Move
                        {
                            viewMatrixData *= Matrix4.CreateTranslation(0, -0.1f, 0);
                        }
                        else //Rotate
                        {
                            viewMatrixData *= Matrix4.CreateRotationX(-0.1f);
                        }
                        glControl1.Invalidate();
                        break;
                    case Keys.Q: // Zoom Out
                        viewMatrixData *= Matrix4.CreateScale(0.9f);
                        glControl1.Invalidate();
                        break;
                    case Keys.E: // Zoom In
                        viewMatrixData *= Matrix4.CreateScale(1.1f);
                        glControl1.Invalidate();
                        break;
                }
                // Normal mode
                if (e.Control && e.KeyCode == Keys.N)
                {
                    normalMode = (normalMode + 1) % 2;
                    createVAO();
                    glControl1.Invalidate();
                }
                // Toggle Timer
                if (e.KeyCode == Keys.T)
                {
                    timerOpenTK.Enabled = !timerOpenTK.Enabled;
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
            if (AllClassStructures.Count > 0)
            {
                var saveFolderDialog1 = new OpenFolderDialog();
                if (saveFolderDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    progressBar1.Value = 0;
                    progressBar1.Maximum = AllClassStructures.Count;

                    var savePath = saveFolderDialog1.Folder;
                    foreach (var version in AllClassStructures)
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
                    case 213:
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
                    _3DToolStripMenuItem.Visible = true;
                    exportToolStripMenuItem.Visible = false;
                    break;
                case 1:
                    _3DToolStripMenuItem.Visible = false;
                    exportToolStripMenuItem.Visible = true;
                    resizeAssetListColumns(); //required because the ListView is not visible on app launch
                    classPreviewPanel.Visible = false;
                    previewPanel.Visible = true;
                    listSearch.Select();
                    break;
                case 2:
                    _3DToolStripMenuItem.Visible = false;
                    exportToolStripMenuItem.Visible = false;
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
                    foreach (var aFile in assetsfileList)
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
                assetListView.BeginUpdate();
                assetListView.SelectedIndices.Clear();
                visibleAssets = exportableAssets.FindAll(ListAsset => ListAsset.Text.IndexOf(listSearch.Text, StringComparison.CurrentCultureIgnoreCase) >= 0);
                assetListView.VirtualListSize = visibleAssets.Count;
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
            pfc.Dispose();
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
                        imageTexture?.Dispose();
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
                            StatusStripUpdate("Unsupported image for preview");
                        }
                        break;
                    }
                #endregion
                #region AudioClip
                case 83: //AudioClip
                    {
                        AudioClip m_AudioClip = new AudioClip(asset, true);
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

                        result = channel.getFrequency(out FMODfrequency);
                        if (ERRCHECK(result)) { break; }

                        FMODinfoLabel.Text = FMODfrequency + " Hz";
                        FMODtimerLabel.Text = $"0:0.0 / {FMODlenms / 1000 / 60}:{FMODlenms / 1000 % 60}.{FMODlenms / 10 % 100}";
                        break;
                    }
                #endregion
                #region Shader
                case 48:
                    {
                        Shader m_TextAsset = new Shader(asset, true);
                        string m_Script_Text = Encoding.UTF8.GetString(m_TextAsset.m_Script);
                        m_Script_Text = Regex.Replace(m_Script_Text, "(?<!\r)\n", "\r\n");
                        m_Script_Text = m_Script_Text.Replace("\0", "\\0");
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
                #region Mesh
                case 43: //Mesh
                    {
                        var m_Mesh = new Mesh(asset, true);
                        if (m_Mesh.m_VertexCount > 0)
                        {
                            glControl1.Visible = true;
                            viewMatrixData = Matrix4.CreateRotationY(-(float)Math.PI / 4) * Matrix4.CreateRotationX(-(float)Math.PI / 6);
                            #region Vertices
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
                            if (m_Mesh.m_Colors == null)
                            {
                                colorData = new Vector4[m_Mesh.m_VertexCount];
                                for (int c = 0; c < m_Mesh.m_VertexCount; c++)
                                {
                                    colorData[c] = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
                                }
                            }
                            else if (m_Mesh.m_Colors.Length == m_Mesh.m_VertexCount * 3)
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
                            else
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
                            #endregion
                            createVAO();
                        }
                        StatusStripUpdate("Using OpenGL Version: " + GL.GetString(StringName.Version) + "\n"
                                        + "'T'=Start/Stop Rotation | 'WASD'=Manual Rotate | 'Shift WASD'=Move | 'Q/E'=Zoom \n"
                                        + "'Ctrl W'=Wireframe | 'Ctrl S'=Shade | 'Ctrl N'=ReNormal ");
                    }
                    break;
                #endregion
                #region VideoClip and MovieTexture
                case 329: //VideoClip
                case 152: //MovieTexture
                    {
                        StatusStripUpdate("Only supported export.");
                        break;
                    }
                #endregion
                #region Sprite
                case 213: //Sprite
                    {
                        imageTexture?.Dispose();
                        imageTexture = GetImageFromSprite(asset);
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
                            StatusStripUpdate("Unsupported sprite for preview");
                        }
                        break;
                    }
                #endregion
                default:
                    {
                        var str = asset.ViewStruct();
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

        private void all3DObjectssplitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sceneTreeView.Nodes.Count > 0)
            {
                var saveFolderDialog1 = new OpenFolderDialog();
                if (saveFolderDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    var savePath = saveFolderDialog1.Folder;
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
                                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
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
                                                //处理非法文件名
                                                filename = FixFileName(filename);
                                                //导出FBX
                                                WriteFBX(savePath + filename + ".fbx", false);
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
                bool exportSwitch = ((ToolStripItem)sender).Name == "exportAll3DMenuItem";


                var timestamp = DateTime.Now;
                saveFileDialog1.FileName = productName + timestamp.ToString("_yy_MM_dd__HH_mm_ss");
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
                                    WriteFBX(saveFileDialog1.FileName, exportSwitch);
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
            var saveFolderDialog1 = new OpenFolderDialog();
            if (exportableAssets.Count > 0 && saveFolderDialog1.ShowDialog(this) == DialogResult.OK)
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
                        foreach (var i in assetListView.SelectedIndices.OfType<int>())
                        {
                            toExportAssets.Add((AssetPreloadData)assetListView.Items[i]);
                        }
                        break;
                }
                int assetGroupSelectedIndex = assetGroupOptions.SelectedIndex;

                ThreadPool.QueueUserWorkItem(delegate
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    var savePath = saveFolderDialog1.Folder;

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
                        StatusStripUpdate($"Exporting {asset.TypeString}: {asset.Text}");
                        switch (asset.Type2)
                        {
                            case 28: //Texture2D
                                if (ExportTexture2D(asset, exportpath, true))
                                {
                                    exportedCount++;
                                }
                                break;
                            case 83: //AudioClip
                                if (ExportAudioClip(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case 48: //Shader
                                if (ExportShader(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case 49: //TextAsset
                                if (ExportTextAsset(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case 114: //MonoBehaviour
                                if (ExportMonoBehaviour(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case 128: //Font
                                if (ExportFont(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case 43: //Mesh
                                if (ExportMesh(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case 329: //VideoClip
                                if (ExportVideoClip(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case 152: //MovieTexture
                                if (ExportMovieTexture(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case 213: //Sprite
                                if (ExportSprite(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;
                            default:
                                if (ExportRawFile(asset, exportpath))
                                {
                                    exportedCount++;
                                }
                                break;

                        }
                        ProgressBarPerformStep();
                    }
                    string statusText;
                    switch (exportedCount)
                    {
                        case 0:
                            statusText = "Nothing exported.";
                            break;
                        default:
                            statusText = $"Finished exporting {exportedCount} assets.";
                            break;
                    }

                    if (toExport > exportedCount) { statusText += $" {toExport - exportedCount} assets skipped (not extractable or files already exist)"; }

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

        public UnityStudioForm()
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
            UnityStudio.SetProgressBarValue = SetProgressBarValue;
            UnityStudio.SetProgressBarMaximum = SetProgressBarMaximum;
            UnityStudio.ProgressBarPerformStep = ProgressBarPerformStep;
            UnityStudio.StatusStripUpdate = StatusStripUpdate;
            UnityStudio.ProgressBarMaximumAdd = ProgressBarMaximumAdd;
        }

        private void timerOpenTK_Tick(object sender, EventArgs e)
        {
            if (glControl1.Visible)
            {
                viewMatrixData *= Matrix4.CreateRotationY(-0.1f);
                glControl1.Invalidate();
            }
        }

        private void initOpenTK()
        {
            GL.Viewport(0, 0, glControl1.ClientSize.Width, glControl1.ClientSize.Height);
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
            glControl1.Visible = false;
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
            timerOpenTK.Stop();
            GL.DeleteVertexArray(vao);
            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);
            createVBO(out vboPositions, vertexData, attributeVertexPosition);
            if (normalMode == 0)
            {
                createVBO(out vboNormals, normal2Data, attributeNormalDirection);
            }
            else
            {
                if (normalData != null)
                    createVBO(out vboNormals, normalData, attributeNormalDirection);
            }
            createVBO(out vboColors, colorData, attributeVertexColor);
            createVBO(out vboModelMatrix, modelMatrixData, uniformModelMatrix);
            createVBO(out vboViewMatrix, viewMatrixData, uniformViewMatrix);
            createEBO(out eboElements, indiceData);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            initOpenTK();
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
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.DrawElements(BeginMode.Triangles, indiceData.Length, DrawElementsType.UnsignedInt, 0);
                GL.Disable(EnableCap.PolygonOffsetLine);
            }
            GL.BindVertexArray(0);
            GL.Flush();
            glControl1.SwapBuffers();
        }

        private void resetForm()
        {
            Text = "Unity Studio";

            unityFiles.Clear();
            assetsfileList.Clear();
            exportableAssets.Clear();
            visibleAssets.Clear();
            assetsfileandstream.Clear();

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

            FMODreset();
        }

        private void showOriginalFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var args = $"/select, {selectasset.sourceFile.bundlePath ?? selectasset.sourceFile.filePath}";
            var pfi = new ProcessStartInfo("explorer.exe", args);
            Process.Start(pfi);
        }

        private void assetListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                selectasset = (AssetPreloadData)assetListView.Items[assetListView.SelectedIndices[0]];
                contextMenuStrip1.Show(assetListView, e.X, e.Y);
            }
        }

        private void glControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (glControl1.Visible)
            {
                viewMatrixData *= Matrix4.CreateScale(1 + e.Delta / 1000f);
                glControl1.Invalidate();
            }
        }
    }
}
