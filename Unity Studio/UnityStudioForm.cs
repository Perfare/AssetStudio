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
using System.Drawing.Imaging;
using Tao.DevIl;
using System.Web.Script.Serialization;


/*TODO
Load parent nodes even if they are not selected to provide transformations?
For extracting bundles, first check if file exists then decompress
Double-check channelgroup argument in new FMOD Studio API system.playSound method
Font index error in Dreamfall Chapters
*/

namespace Unity_Studio
{
    public partial class UnityStudioForm : Form
    {
        private List<string> unityFiles = new List<string>(); //files to load
        public static List<AssetsFile> assetsfileList = new List<AssetsFile>(); //loaded files
        private List<AssetPreloadData> exportableAssets = new List<AssetPreloadData>(); //used to hold all assets while the ListView is filtered
        private List<AssetPreloadData> visibleAssets = new List<AssetPreloadData>(); //used to build the ListView from all or filtered assets
        private AssetPreloadData lastSelectedItem = null;
        private AssetPreloadData lastLoadedAsset = null;
        //private AssetsFile mainDataFile = null;
        private string mainPath = "";
        private string productName = "";
        private string[] fileTypes = new string[7] { "maindata.", "level*.", "*.assets", "*.sharedAssets", "CustomAssetBundle-*", "CAB-*", "BuildPlayer-*" };

        Dictionary<string, Dictionary<string, string>> jsonMats;
        Dictionary<string, SortedDictionary<int, ClassStrStruct>> AllClassStructures = new Dictionary<string, SortedDictionary<int, ClassStrStruct>>();

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
        private int totalAssetCount = 0;
        private int totalTreeNodes = 0;

        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

        //[DllImport("PVRTexLib.dll")]
        //private static extern void test();


        private void loadFile_Click(object sender, System.EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                resetForm();
                mainPath = Path.GetDirectoryName(openFileDialog1.FileNames[0]);

                if (openFileDialog1.FilterIndex == 1)
                {
                    MergeSplitAssets(mainPath);

                    unityFiles.AddRange(openFileDialog1.FileNames);
                    progressBar1.Value = 0;
                    progressBar1.Maximum = unityFiles.Count;

                    //use a for loop because list size can change
                    for (int f = 0; f < unityFiles.Count; f++)
                    {
                        StatusStripUpdate("Loading " + Path.GetFileName(unityFiles[f]));
                        LoadAssetsFile(unityFiles[f]);
                    }
                }
                else
                {
                    progressBar1.Value = 0;
                    progressBar1.Maximum = openFileDialog1.FileNames.Length;

                    foreach (var filename in openFileDialog1.FileNames)
                    {
                        LoadBundleFile(filename);
                        progressBar1.PerformStep();
                    }
                }

                progressBar1.Value = 0;

                BuildAssetStrucutres();
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
                mainPath = openFolderDialog1.FileName;
                if (Path.GetFileName(mainPath) == "Select folder")
                { mainPath = Path.GetDirectoryName(mainPath); }

                if (Directory.Exists(mainPath))
                {
                    resetForm();
                    
                    //TODO find a way to read data directly instead of merging files
                    MergeSplitAssets(mainPath);
                    
                    for (int t = 0; t < fileTypes.Length; t++)
                    {
                        string[] fileNames = Directory.GetFiles(mainPath, fileTypes[t], SearchOption.AllDirectories);
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
                            unityFiles.AddRange(sortedList);
                        }
                        #endregion
                        else { unityFiles.AddRange(fileNames); }
                    }

                    unityFiles = unityFiles.Distinct().ToList();
                    progressBar1.Value = 0;
                    progressBar1.Maximum = unityFiles.Count;

                    //use a for loop because list size can change
                    for (int f = 0; f < unityFiles.Count; f++)
                    {
                        var fileName = unityFiles[f];
                        StatusStripUpdate("Loading " + Path.GetFileName(fileName));
                        LoadAssetsFile(fileName);
                    }

                    progressBar1.Value = 0;

                    BuildAssetStrucutres();
                }
                else { StatusStripUpdate("Selected path deos not exist."); }
            }
        }

        private void MergeSplitAssets(string dirPath)
        {
            string[] splitFiles = Directory.GetFiles(dirPath, "*.split0");
            foreach (var splitFile in splitFiles)
            {
                string destFile = Path.GetFileNameWithoutExtension(splitFile);
                string destPath = Path.GetDirectoryName(splitFile) + "\\";
                if (!File.Exists(destPath + destFile))
                {
                    StatusStripUpdate("Merging " + destFile + " split files...");

                    string[] splitParts = Directory.GetFiles(destPath, destFile + ".split*");
                    using (var destStream = File.Create(destPath + destFile))
                    {
                        for (int i = 0; i < splitParts.Length; i++)
                        {
                            string splitPart = destPath + destFile + ".split" + i.ToString();
                            using (var sourceStream = File.OpenRead(splitPart))
                                sourceStream.CopyTo(destStream); // You can pass the buffer size as second argument.
                        }
                    }
                }
            }
        }

        private void LoadAssetsFile(string fileName)
        {
            var loadedAssetsFile = assetsfileList.Find(aFile => aFile.filePath == fileName);
            if (loadedAssetsFile == null)
            {
                //open file here and pass the stream to facilitate loading memory files
                //also by keeping the stream as a property of AssetsFile, it can be used later on to read assets
                AssetsFile assetsFile = new AssetsFile(fileName, new EndianStream(File.OpenRead(fileName), EndianType.BigEndian));
                //if (Path.GetFileName(fileName) == "mainData") { mainDataFile = assetsFile; }

                totalAssetCount += assetsFile.preloadTable.Count;

                assetsfileList.Add(assetsFile);
                #region for 2.6.x find mainData and get string version
                if (assetsFile.fileGen == 6 && Path.GetFileName(fileName) != "mainData")
                {
                    AssetsFile mainDataFile = assetsfileList.Find(aFile => aFile.filePath == Path.GetDirectoryName(fileName) + "\\mainData");
                    if (mainDataFile != null)
                    {
                        assetsFile.m_Version = mainDataFile.m_Version;
                        assetsFile.version = mainDataFile.version;
                        assetsFile.buildType = mainDataFile.buildType;
                    }
                    else if (File.Exists(Path.GetDirectoryName(fileName) + "\\mainData"))
                    {
                        mainDataFile = new AssetsFile(Path.GetDirectoryName(fileName) + "\\mainData", new EndianStream(File.OpenRead(Path.GetDirectoryName(fileName) + "\\mainData"), EndianType.BigEndian));

                        assetsFile.m_Version = mainDataFile.m_Version;
                        assetsFile.version = mainDataFile.version;
                        assetsFile.buildType = mainDataFile.buildType;
                    }
                }
                #endregion
                progressBar1.PerformStep();

                foreach (var sharedFile in assetsFile.sharedAssetsList)
                {
                    string sharedFilePath = Path.GetDirectoryName(fileName) + "\\" + sharedFile.fileName;
                    string sharedFileName = Path.GetFileName(sharedFile.fileName);

                    //var loadedSharedFile = assetsfileList.Find(aFile => aFile.filePath == sharedFilePath);
                    /*var loadedSharedFile = assetsfileList.Find(aFile => aFile.filePath.EndsWith(Path.GetFileName(sharedFile.fileName)));
                    if (loadedSharedFile != null) { sharedFile.Index = assetsfileList.IndexOf(loadedSharedFile); }
                    else if (File.Exists(sharedFilePath))
                    {
                        //progressBar1.Maximum += 1;
                        sharedFile.Index = assetsfileList.Count;
                        LoadAssetsFile(sharedFilePath);
                    }*/

                    //searching in unityFiles would preserve desired order, but...
                    var quedSharedFile = unityFiles.Find(uFile => String.Equals(Path.GetFileName(uFile), sharedFileName, StringComparison.OrdinalIgnoreCase));
                    if (quedSharedFile == null)
                    {
                        //if (!File.Exists(sharedFilePath)) { sharedFilePath = Path.GetDirectoryName(fileName) + "\\" + sharedFileName; }
                        if (!File.Exists(sharedFilePath))
                        {
                            var findFiles = Directory.GetFiles(Path.GetDirectoryName(fileName), sharedFileName, SearchOption.AllDirectories);
                            if (findFiles.Length > 0) { sharedFilePath = findFiles[0]; }
                        }

                        if (File.Exists(sharedFilePath))
                        {
                            //this would get screwed if the file somehow fails to load
                            sharedFile.Index = unityFiles.Count;
                            unityFiles.Add(sharedFilePath);
                            progressBar1.Maximum++;
                        }
                    }
                    else { sharedFile.Index = unityFiles.IndexOf(quedSharedFile); }
                }
                
            }
        }

        private void LoadBundleFile(string bundleFileName)
        {
            StatusStripUpdate("Decompressing " + Path.GetFileName(bundleFileName) + "...");

            BundleFile b_File = new BundleFile(bundleFileName);

            List<AssetsFile> b_assetsfileList = new List<AssetsFile>();

            foreach (var memFile in b_File.MemoryAssetsFileList) //filter unity files
            {
                bool validAssetsFile = false;
                switch (Path.GetExtension(memFile.fileName))
                {
                    case ".assets":
                    case ".sharedAssets":
                        validAssetsFile = true;
                        break;
                    case "":
                        validAssetsFile = (memFile.fileName == "mainData" || 
                                            Regex.IsMatch(memFile.fileName, "level.*?") || 
                                            Regex.IsMatch(memFile.fileName, "CustomAssetBundle-.*?") || 
                                            Regex.IsMatch(memFile.fileName, "CAB-.*?") || 
                                            Regex.IsMatch(memFile.fileName, "BuildPlayer-.*?"));
                        break;
                }

                if (validAssetsFile)
                {
                    StatusStripUpdate("Loading " + memFile.fileName);
                    //create dummy path to be used for asset extraction
                    memFile.fileName = Path.GetDirectoryName(bundleFileName) + "\\" + memFile.fileName;

                    AssetsFile assetsFile = new AssetsFile(memFile.fileName, new EndianStream(memFile.memStream, EndianType.BigEndian));
                    if (assetsFile.fileGen == 6 && Path.GetFileName(bundleFileName) != "mainData") //2.6.x and earlier don't have a string version before the preload table
                    {
                        //make use of the bundle file version
                        assetsFile.m_Version = b_File.ver3;
                        assetsFile.version = Array.ConvertAll((b_File.ver3.Split(new string[] { ".", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "\n" }, StringSplitOptions.RemoveEmptyEntries)), int.Parse);
                        assetsFile.buildType = b_File.ver3.Split(new string[] { ".", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" }, StringSplitOptions.RemoveEmptyEntries);
                    }

                    b_assetsfileList.Add(assetsFile);
                }
                else
                {
                    memFile.memStream.Close();
                }
            }

            assetsfileList.AddRange(b_assetsfileList);//will the streams still be available for reading data?

            foreach (var assetsFile in b_assetsfileList)
            {
                foreach (var sharedFile in assetsFile.sharedAssetsList)
                {
                    sharedFile.fileName = Path.GetDirectoryName(bundleFileName) + "\\" + sharedFile.fileName;
                    var loadedSharedFile = b_assetsfileList.Find(aFile => aFile.filePath == sharedFile.fileName);
                    if (loadedSharedFile != null) { sharedFile.Index = assetsfileList.IndexOf(loadedSharedFile); }
                }
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
                int extractedCount = extractBundleFile(openBundleDialog.FileName);

                StatusStripUpdate("Finished extracting " + extractedCount.ToString() + " files.");
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

                foreach (var fileName in bundleFiles)
                {
                    extractedCount += extractBundleFile(fileName);
                }

                StatusStripUpdate("Finished extracting " + extractedCount.ToString() + " files.");
            }
        }

        private int extractBundleFile(string bundleFileName)
        {
            int extractedCount = 0;

            StatusStripUpdate("Decompressing " + Path.GetFileName(bundleFileName) + " ,,,");

            string extractPath = bundleFileName + "_unpacked\\";
            Directory.CreateDirectory(extractPath);

            BundleFile b_File = new BundleFile(bundleFileName);

            foreach (var memFile in b_File.MemoryAssetsFileList)
            {
                string filePath = extractPath + memFile.fileName.Replace('/', '\\');
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                }
                if (File.Exists(filePath))
                {
                    StatusStripUpdate("File " + memFile.fileName + " already exists");
                }
                else
                {
                    StatusStripUpdate("Extracting " + Path.GetFileName(memFile.fileName));
                    extractedCount += 1;

                    using (FileStream file = new FileStream(filePath, FileMode.Create, System.IO.FileAccess.Write))
                    {
                        memFile.memStream.WriteTo(file);
                        memFile.memStream.Close();
                    }
                }
            }

            return extractedCount;
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
            if (AllClassStructures.Count > 0)
            {
                if (saveFolderDialog1.ShowDialog() == DialogResult.OK)
                {
                    progressBar1.Value = 0;
                    progressBar1.Maximum = AllClassStructures.Count;

                    var savePath = saveFolderDialog1.FileName;
                    if (Path.GetFileName(savePath) == "Select folder or write folder name to create")
                    { savePath = Path.GetDirectoryName(saveFolderDialog1.FileName); }

                    foreach (var version in AllClassStructures)
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
                                    TXTwriter.Write(uclass.Value.members);
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
                                previewPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
                            }
                            else
                            {
                                previewPanel.BackgroundImage = global::Unity_Studio.Properties.Resources.preview;
                                previewPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
                            }
                        }
                        break;
                    case 48:
                    case 49:
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
                                if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                                {
                                    ERRCHECK(result);
                                }

                                if (playing) //stop previous sound
                                {
                                    result = channel.stop();
                                    ERRCHECK(result);

                                    FMODreset();
                                }
                                else if (enablePreview.Checked)
                                {
                                    result = system.playSound(sound, null, false, out channel);
                                    ERRCHECK(result);

                                    timer.Start();
                                    FMODstatusLabel.Text = "Playing";
                                    //FMODinfoLabel.Text = FMODfrequency.ToString();
                                }
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
        

        private void BuildAssetStrucutres()
        {
            #region first loop - read asset data & create list
            if (!dontLoadAssetsMenuItem.Checked)
            {
                assetListView.BeginUpdate();
                progressBar1.Value = 0;
                progressBar1.Maximum = totalAssetCount;

                string fileIDfmt = "D" + assetsfileList.Count.ToString().Length.ToString();

                foreach (var assetsFile in assetsfileList)
                {
                    StatusStripUpdate("Building asset list from " + Path.GetFileName(assetsFile.filePath));

                    var a_Stream = assetsFile.a_Stream;
                    var fileGen = assetsFile.fileGen;
                    //var m_version = assetsFile.m_version;
                    var version = assetsFile.version;
                    string fileID = assetsfileList.IndexOf(assetsFile).ToString(fileIDfmt);

                    //ListViewGroup assetGroup = new ListViewGroup(Path.GetFileName(assetsFile.filePath));


                    foreach (var asset in assetsFile.preloadTable.Values)
                    {
                        asset.uniqueID = fileID + asset.uniqueID;
                        a_Stream.Position = asset.Offset;

                        switch (asset.Type2)
                        {
                            case 1: //GameObject
                                {
                                    GameObject m_GameObject = new GameObject(asset);
                                    assetsFile.GameObjectList.Add(asset.m_PathID, m_GameObject);
                                    totalTreeNodes++;
                                    break;
                                }
                            case 4: //Transform
                                {
                                    Transform m_Transform = new Transform(asset);
                                    assetsFile.TransformList.Add(asset.m_PathID, m_Transform);
                                    break;
                                }
                            case 224: //RectTransform
                                {
                                    RectTransform m_Rect = new RectTransform(asset);
                                    assetsFile.TransformList.Add(asset.m_PathID, m_Rect.m_Transform);
                                    break;
                                }
                            //case 21: //Material
                            case 28: //Texture2D
                                {
                                    Texture2D m_Texture2D = new Texture2D(asset, false);

                                    asset.Text = m_Texture2D.m_Name;
                                    asset.exportSize = 128 + m_Texture2D.image_data_size;

                                    #region Get Info Text
                                    asset.InfoText = "Width: " + m_Texture2D.m_Width.ToString() + "\nHeight: " + m_Texture2D.m_Height.ToString() + "\nFormat: ";

                                    switch (m_Texture2D.m_TextureFormat)
                                    {
                                        case 1: asset.InfoText += "Alpha8"; break;
                                        case 2: asset.InfoText += "ARGB 4.4.4.4"; break;
                                        case 3: asset.InfoText += "BGR 8.8.8"; break;
                                        case 4: asset.InfoText += "GRAB 8.8.8.8"; break;
                                        case 5: asset.InfoText += "BGRA 8.8.8.8"; break;
                                        case 7: asset.InfoText += "RGB 5.6.5"; break;
                                        case 10: asset.InfoText += "RGB DXT1"; break;
                                        case 12: asset.InfoText += "ARGB DXT5"; break;
                                        case 13: asset.InfoText += "RGBA 4.4.4.4"; break;
                                        case 30: asset.InfoText += "PVRTC_RGB2"; asset.exportSize -= 76; break;
                                        case 31: asset.InfoText += "PVRTC_RGBA2"; asset.exportSize -= 76; break;
                                        case 32: asset.InfoText += "PVRTC_RGB4"; asset.exportSize = 52; break;
                                        case 33: asset.InfoText += "PVRTC_RGBA4"; asset.exportSize -= 76; break;
                                        case 34: asset.InfoText += "ETC_RGB4"; asset.exportSize -= 76; break;
                                        default: asset.InfoText += "unknown"; asset.exportSize -= 128; break;
                                    }

                                    switch (m_Texture2D.m_FilterMode)
                                    {
                                        case 0: asset.InfoText += "\nFilter Mode: Point "; break;
                                        case 1: asset.InfoText += "\nFilter Mode: Bilinear "; break;
                                        case 2: asset.InfoText += "\nFilter Mode: Trilinear "; break;

                                    }

                                    asset.InfoText += "\nAnisotropic level: " + m_Texture2D.m_Aniso.ToString() + "\nMip map bias: " + m_Texture2D.m_MipBias.ToString();

                                    switch (m_Texture2D.m_WrapMode)
                                    {
                                        case 0: asset.InfoText += "\nWrap mode: Repeat"; break;
                                        case 1: asset.InfoText += "\nWrap mode: Clamp"; break;
                                    }
                                    #endregion

                                    assetsFile.exportableAssets.Add(asset);
                                    break;
                                }
                            case 49: //TextAsset
                                {
                                    TextAsset m_TextAsset = new TextAsset(asset, false);

                                    asset.Text = m_TextAsset.m_Name;
                                    asset.exportSize = m_TextAsset.exportSize;
                                    assetsFile.exportableAssets.Add(asset);
                                    break;
                                }
                            case 83: //AudioClip
                                {
                                    AudioClip m_AudioClip = new AudioClip(asset, false);

                                    asset.Text = m_AudioClip.m_Name;
                                    asset.exportSize = (int)m_AudioClip.m_Size;
                                    assetsFile.exportableAssets.Add(asset);
                                    break;
                                }
                            case 48: //Shader
                            case 89: //CubeMap
                            case 128: //Font
                                {
                                    asset.Text = a_Stream.ReadAlignedString(a_Stream.ReadInt32());
                                    assetsFile.exportableAssets.Add(asset);
                                    break;
                                }
                            case 129: //PlayerSettings
                                {
                                    PlayerSettings plSet = new PlayerSettings(asset);
                                    productName = plSet.productName;
                                    base.Text = "Unity Studio - " + productName + " - " + assetsFile.m_Version + " - " + assetsFile.platformStr;
                                    break;
                                }

                        }

                        if (asset.Text == "") { asset.Text = asset.TypeString + " #" + asset.uniqueID; }
                        asset.SubItems.AddRange(new string[] { asset.TypeString, asset.exportSize.ToString() });

                        progressBar1.PerformStep();
                    }

                    exportableAssets.AddRange(assetsFile.exportableAssets);
                    //if (assetGroup.Items.Count > 0) { listView1.Groups.Add(assetGroup); }
                }

                if (base.Text == "Unity Studio" && assetsfileList.Count > 0)
                {
                    base.Text = "Unity Studio - no productName - " + assetsfileList[0].m_Version + " - " + assetsfileList[0].platformStr;
                }

                visibleAssets = exportableAssets;
                assetListView.VirtualListSize = visibleAssets.Count;

                //will only work if ListView is visible
                resizeAssetListColumns();

                assetListView.EndUpdate();
                progressBar1.Value = 0;
            }
            #endregion

            #region second loop - build tree structure
            if (!dontBuildHierarchyMenuItem.Checked)
            {
                sceneTreeView.BeginUpdate();
                progressBar1.Value = 0;
                progressBar1.Maximum = totalTreeNodes;

                foreach (var assetsFile in assetsfileList)
                {
                    StatusStripUpdate("Building tree structure from " + Path.GetFileName(assetsFile.filePath));
                    GameObject fileNode = new GameObject(null);
                    fileNode.Text = Path.GetFileName(assetsFile.filePath);
                    fileNode.m_Name = "RootNode";

                    foreach (var m_GameObject in assetsFile.GameObjectList.Values)
                    {
                        var parentNode = fileNode;

                        Transform m_Transform;
                        if (assetsfileList.TryGetTransform(m_GameObject.m_Transform, out m_Transform))
                        {
                            Transform m_Father;
                            if (assetsfileList.TryGetTransform(m_Transform.m_Father, out m_Father))
                            {
                                //GameObject Parent;
                                if (assetsfileList.TryGetGameObject(m_Father.m_GameObject, out parentNode))
                                {
                                    //parentNode = Parent;
                                }
                            }
                        }

                        parentNode.Nodes.Add(m_GameObject);
                        progressBar1.PerformStep();
                    }


                    if (fileNode.Nodes.Count == 0) { fileNode.Text += " (no children)"; }
                    sceneTreeView.Nodes.Add(fileNode);
                }
                sceneTreeView.EndUpdate();
                progressBar1.Value = 0;

                if (File.Exists(mainPath + "\\materials.json"))
                {
                    string matLine = "";
                    using (StreamReader reader = File.OpenText(mainPath + "\\materials.json"))
                    { matLine = reader.ReadToEnd(); }

                    jsonMats = new JavaScriptSerializer().Deserialize<Dictionary<string, Dictionary<string, string>>>(matLine);
                    //var jsonMats = new JavaScriptSerializer().DeserializeObject(matLine);
                }
            }
            #endregion

            #region build list of class strucutres
            if (buildClassStructuresMenuItem.Checked)
            {
                //group class structures by versionv
                foreach (var assetsFile in assetsfileList)
                {
                    SortedDictionary<int, ClassStrStruct> curVer;
                    if (AllClassStructures.TryGetValue(assetsFile.m_Version, out curVer))
                    {
                        foreach (var uClass in assetsFile.ClassStructures)
                        {
                            curVer[uClass.Key] = uClass.Value;
                        }
                    }
                    else { AllClassStructures.Add(assetsFile.m_Version, assetsFile.ClassStructures); }
                }

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
            #endregion

            StatusStripUpdate("Finished loading " + assetsfileList.Count.ToString() + " files with " + (assetListView.Items.Count + sceneTreeView.Nodes.Count).ToString() + " exportable assets.");
            
            progressBar1.Value = 0;
            treeSearch.Select();
            TexEnv dsd = new TexEnv();
        }

        private void assetListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = visibleAssets[e.ItemIndex];
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
            var hasvscroll = ((float)visibleAssets.Count / (float)assetListView.Height) > 0.0567f;
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
                visibleAssets = exportableAssets.FindAll(ListAsset => ListAsset.Text.IndexOf(listSearch.Text, System.StringComparison.CurrentCultureIgnoreCase) >= 0);
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
                        else return secondSortColumn == 1 ? a.TypeString.CompareTo(b.TypeString) : a.exportSize.CompareTo(b.exportSize);
                    });
                    break;
                case 1:
                    visibleAssets.Sort(delegate (AssetPreloadData a, AssetPreloadData b)
                    {
                        int xdiff = reverseSort ? b.TypeString.CompareTo(a.TypeString) : a.TypeString.CompareTo(b.TypeString);
                        if (xdiff != 0) return xdiff;
                        else return secondSortColumn == 2 ? a.exportSize.CompareTo(b.exportSize) : a.Text.CompareTo(b.Text);
                    });
                    break;
                case 2:
                    visibleAssets.Sort(delegate (AssetPreloadData a, AssetPreloadData b)
                    {
                        int xdiff = reverseSort ? b.exportSize.CompareTo(a.exportSize) : a.exportSize.CompareTo(b.exportSize);
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
            previewPanel.BackgroundImage = global::Unity_Studio.Properties.Resources.preview;
            previewPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            assetInfoLabel.Visible = false;
            assetInfoLabel.Text = null;
            textPreviewBox.Visible = false;
            fontPreviewBox.Visible = false;
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
                classTextBox.Text = ((ClassStrStruct)classesListView.SelectedItems[0]).members;
            }
        }


        private void PreviewAsset(AssetPreloadData asset)
        {
            switch (asset.Type2)
            {
                #region Texture2D
                case 28: //Texture2D
                    {
                        Texture2D m_Texture2D = new Texture2D(asset, true);
                        
                        if (m_Texture2D.m_TextureFormat < 30)
                        {
                            byte[] imageBuffer = new byte[128 + m_Texture2D.image_data_size];

                            imageBuffer[0] = 0x44;
                            imageBuffer[1] = 0x44;
                            imageBuffer[2] = 0x53;
                            imageBuffer[3] = 0x20;
                            imageBuffer[4] = 0x7c;
                            
                            BitConverter.GetBytes(m_Texture2D.dwFlags).CopyTo(imageBuffer, 8);
                            BitConverter.GetBytes(m_Texture2D.m_Height).CopyTo(imageBuffer, 12);
                            BitConverter.GetBytes(m_Texture2D.m_Width).CopyTo(imageBuffer, 16);
                            BitConverter.GetBytes(m_Texture2D.dwPitchOrLinearSize).CopyTo(imageBuffer, 20);
                            BitConverter.GetBytes(m_Texture2D.dwMipMapCount).CopyTo(imageBuffer, 28);
                            BitConverter.GetBytes(m_Texture2D.dwSize).CopyTo(imageBuffer, 76);
                            BitConverter.GetBytes(m_Texture2D.dwFlags2).CopyTo(imageBuffer, 80);
                            BitConverter.GetBytes(m_Texture2D.dwFourCC).CopyTo(imageBuffer, 84);
                            BitConverter.GetBytes(m_Texture2D.dwRGBBitCount).CopyTo(imageBuffer, 88);
                            BitConverter.GetBytes(m_Texture2D.dwRBitMask).CopyTo(imageBuffer, 92);
                            BitConverter.GetBytes(m_Texture2D.dwGBitMask).CopyTo(imageBuffer, 96);
                            BitConverter.GetBytes(m_Texture2D.dwBBitMask).CopyTo(imageBuffer, 100);
                            BitConverter.GetBytes(m_Texture2D.dwABitMask).CopyTo(imageBuffer, 104);
                            BitConverter.GetBytes(m_Texture2D.dwCaps).CopyTo(imageBuffer, 108);
                            BitConverter.GetBytes(m_Texture2D.dwCaps2).CopyTo(imageBuffer, 112);
                            
                            m_Texture2D.image_data.CopyTo(imageBuffer, 128);

                            imageTexture = DDSDataToBMP(imageBuffer);
                            imageTexture.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            previewPanel.BackgroundImage = imageTexture;
                            previewPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
                        }
                        else { StatusStripUpdate("Unsupported image for preview. Try to export."); }
                        break;
                    }
                #endregion
                #region AudioClip
                case 83: //AudioClip
                    {
                        AudioClip m_AudioClip = new AudioClip(asset, true);

                        //MemoryStream memoryStream = new MemoryStream(m_AudioData, true);
                        //System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer(memoryStream);
                        //soundPlayer.Play();

                        FMOD.RESULT result;
                        FMOD.CREATESOUNDEXINFO exinfo = new FMOD.CREATESOUNDEXINFO();
                        
                        exinfo.cbsize = Marshal.SizeOf(exinfo);
                        exinfo.length = (uint)m_AudioClip.m_Size;

                        result = system.createSound(m_AudioClip.m_AudioData, (FMOD.MODE.OPENMEMORY | loopMode), ref exinfo, out sound);
                        if (ERRCHECK(result)) { break; }

                        result = sound.getLength(out FMODlenms, FMOD.TIMEUNIT.MS);
                        if ((result != FMOD.RESULT.OK) && (result != FMOD.RESULT.ERR_INVALID_HANDLE))
                        {
                            if (ERRCHECK(result)) { break; }
                        }

                        result = system.playSound(sound, null, false, out channel);
                        if (ERRCHECK(result)) { break; }

                        timer.Start();
                        FMODstatusLabel.Text = "Playing";
                        FMODpanel.Visible = true;

                        //result = channel.getChannelGroup(out channelGroup);
                        //if (ERRCHECK(result)) { break; }

                        result = channel.getFrequency(out FMODfrequency);
                        ERRCHECK(result);

                        FMODinfoLabel.Text = FMODfrequency.ToString() + " Hz";
                        break;
                    }
                #endregion
                #region Shader & TextAsset
                case 48:
                case 49:
                    {
                        TextAsset m_TextAsset = new TextAsset(asset, true);
                        
                        string m_Script_Text = UnicodeEncoding.UTF8.GetString(m_TextAsset.m_Script);
                        m_Script_Text = Regex.Replace(m_Script_Text, "(?<!\r)\n", "\r\n");
                        textPreviewBox.Text = m_Script_Text;
                        textPreviewBox.Visible = true;

                        break;
                    }
                #endregion
                #region Font
                case 128: //Font
                    {
                        unityFont m_Font = new unityFont(asset);
                        
                        if (m_Font.extension != ".otf" && m_Font.m_FontData != null)
                        {
                            IntPtr data = Marshal.AllocCoTaskMem(m_Font.m_FontData.Length);
                            Marshal.Copy(m_Font.m_FontData, 0, data, m_Font.m_FontData.Length);

                            System.Drawing.Text.PrivateFontCollection pfc = new System.Drawing.Text.PrivateFontCollection();
                            // We HAVE to do this to register the font to the system (Weird .NET bug !)
                            uint cFonts = 0;
                            AddFontMemResourceEx(data, (uint)m_Font.m_FontData.Length, IntPtr.Zero, ref cFonts);

                            pfc.AddMemoryFont(data, m_Font.m_FontData.Length);
                            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(data);

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
                        else { StatusStripUpdate("Unsupported font for preview. Try to export."); }

                        break;
                    }
                #endregion
            }
        }

        public static Bitmap DDSDataToBMP(byte[] DDSData)
        {
            // Create a DevIL image "name" (which is actually a number)
            int img_name;
            Il.ilGenImages(1, out img_name);
            Il.ilBindImage(img_name);

            // Load the DDS file into the bound DevIL image
            Il.ilLoadL(Il.IL_DDS, DDSData, DDSData.Length);

            // Set a few size variables that will simplify later code

            int ImgWidth = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
            int ImgHeight = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
            Rectangle rect = new Rectangle(0, 0, ImgWidth, ImgHeight);

            // Convert the DevIL image to a pixel byte array to copy into Bitmap
            Il.ilConvertImage(Il.IL_BGRA, Il.IL_UNSIGNED_BYTE);

            // Create a Bitmap to copy the image into, and prepare it to get data
            Bitmap bmp = new Bitmap(ImgWidth, ImgHeight);
            BitmapData bmd =
              bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            // Copy the pixel byte array from the DevIL image to the Bitmap
            Il.ilCopyPixels(0, 0, 0,
              Il.ilGetInteger(Il.IL_IMAGE_WIDTH),
              Il.ilGetInteger(Il.IL_IMAGE_HEIGHT),
              1, Il.IL_BGRA, Il.IL_UNSIGNED_BYTE,
              bmd.Scan0);

            // Clean up and return Bitmap
            Il.ilDeleteImages(1, ref img_name);
            bmp.UnlockBits(bmd);
            return bmp;
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
            FMOD.RESULT result;
            timer.Stop();
            FMODprogressBar.Value = 0;
            FMODtimerLabel.Text = "0:00.0 / 0:00.0";
            FMODstatusLabel.Text = "Stopped";
            FMODinfoLabel.Text = "";

            if (sound != null)
            {
                result = sound.release();
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
                /*else
                {
                    /esult = system.playSound(FMOD.CHANNELINDEX.FREE, sound, false, ref channel);
                    ERRCHECK(result);

                    result = channel.setMode(loopMode);
                    ERRCHECK(result);
                }*/
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

            if (system != null)
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


        private void Export3DObjects_Click(object sender, EventArgs e)
        {
            if (sceneTreeView.Nodes.Count > 0)
            {
                bool exportSwitch = (((ToolStripItem)sender).Name == "exportAll3DMenuItem") ? true : false;


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

                            if (openAfterExport.Checked && File.Exists(saveFileDialog1.FileName)) { System.Diagnostics.Process.Start(saveFileDialog1.FileName); }
                            break;
                    }
                }
                
            }
            else { StatusStripUpdate("No Objects available for export"); }
        }

        public void WriteFBX(string FBXfile, bool allNodes)
        {
            var timestamp = DateTime.Now;

            using (StreamWriter FBXwriter = new StreamWriter(FBXfile))
            {
                StringBuilder fbx = new StringBuilder();
                StringBuilder ob = new StringBuilder(); //Objects builder
                StringBuilder cb = new StringBuilder(); //Connections builder
                StringBuilder mb = new StringBuilder(); //Materials builder to get texture count in advance
                StringBuilder cb2 = new StringBuilder(); //and keep connections ordered
                cb.Append("\n}\n");//Objects end
                cb.Append("\nConnections:  {");

                HashSet<GameObject> GameObjects = new HashSet<GameObject>();
                HashSet<GameObject> LimbNodes = new HashSet<GameObject>();
                HashSet<AssetPreloadData> Skins = new HashSet<AssetPreloadData>();
                HashSet<AssetPreloadData> Meshes = new HashSet<AssetPreloadData>();//MeshFilters are not unique!!
                HashSet<AssetPreloadData> Materials = new HashSet<AssetPreloadData>();
                HashSet<AssetPreloadData> Textures = new HashSet<AssetPreloadData>();

                int DeformerCount = 0;
                /*
                uniqueIDs can begin with zero, so they are preceded by a number specific to their type
                this will also make it easier to debug FBX files
                1: Model
                2: NodeAttribute
                3: Geometry
                4: Deformer
                5: CollectionExclusive
                6: Material
                7: Texture
                8: Video
                9: 
                */

                #region loop nodes and collect objects for export
                foreach (var assetsFile in assetsfileList)
                {
                    foreach (var m_GameObject in assetsFile.GameObjectList.Values)
                    {
                        if (m_GameObject.Checked || allNodes)
                        {
                            GameObjects.Add(m_GameObject);
                            
                            AssetPreloadData MeshFilterPD;
                            if (assetsfileList.TryGetPD(m_GameObject.m_MeshFilter, out MeshFilterPD))
                            {
                                //MeshFilters are not unique!
                                //MeshFilters.Add(MeshFilterPD);
                                MeshFilter m_MeshFilter = new MeshFilter(MeshFilterPD);
                                AssetPreloadData MeshPD;
                                if (assetsfileList.TryGetPD(m_MeshFilter.m_Mesh, out MeshPD))
                                {
                                    Meshes.Add(MeshPD);
                                    
                                    //write connections here and Mesh objects separately without having to backtrack through their MEshFilter to het the GameObject ID
                                    //also note that MeshFilters are not unique, they cannot be used for instancing geometry
                                    cb2.AppendFormat("\n\n\t;Geometry::, Model::{0}", m_GameObject.m_Name);
                                    cb2.AppendFormat("\n\tC: \"OO\",3{0},1{1}", MeshPD.uniqueID, m_GameObject.uniqueID);
                                }
                            }

                            #region get Renderer
                            AssetPreloadData RendererPD;
                            if (assetsfileList.TryGetPD(m_GameObject.m_Renderer, out RendererPD))
                            {
                                Renderer m_Renderer = new Renderer(RendererPD);

                                foreach (var MaterialPPtr in m_Renderer.m_Materials)
                                {
                                    AssetPreloadData MaterialPD;
                                    if (assetsfileList.TryGetPD(MaterialPPtr, out MaterialPD))
                                    {
                                        Materials.Add(MaterialPD);
                                        cb2.AppendFormat("\n\n\t;Material::, Model::{0}", m_GameObject.m_Name);
                                        cb2.AppendFormat("\n\tC: \"OO\",6{0},1{1}", MaterialPD.uniqueID, m_GameObject.uniqueID);
                                    }
                                }
                            }
                            #endregion

                            #region get SkinnedMeshRenderer
                            AssetPreloadData SkinnedMeshPD;
                            if (assetsfileList.TryGetPD(m_GameObject.m_SkinnedMeshRenderer, out SkinnedMeshPD))
                            {
                                Skins.Add(SkinnedMeshPD);

                                SkinnedMeshRenderer m_SkinnedMeshRenderer = new SkinnedMeshRenderer(SkinnedMeshPD);

                                foreach (var MaterialPPtr in m_SkinnedMeshRenderer.m_Materials)
                                {
                                    AssetPreloadData MaterialPD;
                                    if (assetsfileList.TryGetPD(MaterialPPtr, out MaterialPD))
                                    {
                                        Materials.Add(MaterialPD);
                                        cb2.AppendFormat("\n\n\t;Material::, Model::{0}", m_GameObject.m_Name);
                                        cb2.AppendFormat("\n\tC: \"OO\",6{0},1{1}", MaterialPD.uniqueID, m_GameObject.uniqueID);
                                    }
                                }

                                if ((bool)Properties.Settings.Default["exportDeformers"])
                                {
                                    DeformerCount += m_SkinnedMeshRenderer.m_Bones.Length;

                                    //collect skeleton dummies to make sure they are exported
                                    foreach (var bonePPtr in m_SkinnedMeshRenderer.m_Bones)
                                    {
                                        Transform b_Transform;
                                        if (assetsfileList.TryGetTransform(bonePPtr, out b_Transform))
                                        {
                                            GameObject m_Bone;
                                            if (assetsfileList.TryGetGameObject(b_Transform.m_GameObject, out m_Bone))
                                            {
                                                LimbNodes.Add(m_Bone);
                                                //also collect the root bone
                                                if (m_Bone.Parent.Level > 0) { LimbNodes.Add((GameObject)m_Bone.Parent); }
                                                //should I collect siblings?
                                            }

                                            #region collect children because m_SkinnedMeshRenderer.m_Bones doesn't contain terminations
                                            foreach (var ChildPPtr in b_Transform.m_Children)
                                            {
                                                Transform ChildTR;
                                                if (assetsfileList.TryGetTransform(ChildPPtr, out ChildTR))
                                                {
                                                    GameObject m_Child;
                                                    if (assetsfileList.TryGetGameObject(ChildTR.m_GameObject, out m_Child))
                                                    {
                                                        //check that the Model doesn't contain a Mesh, although this won't ensure it's part of the skeleton
                                                        if (m_Child.m_MeshFilter == null && m_Child.m_SkinnedMeshRenderer == null)
                                                        {
                                                            LimbNodes.Add(m_Child);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }

                //if ((bool)Properties.Settings.Default["convertDummies"]) { GameObjects.Except(LimbNodes); }
                //else { GameObjects.UnionWith(LimbNodes); LimbNodes.Clear(); }
                //add either way and use LimbNodes to test if a node is Null or LimbNode
                GameObjects.UnionWith(LimbNodes);
                #endregion

                #region write Materials, collect Texture objects
                StatusStripUpdate("Writing Materials");
                foreach (var MaterialPD in Materials)
                {
                    Material m_Material = new Material(MaterialPD);

                    mb.AppendFormat("\n\tMaterial: 6{0}, \"Material::{1}\", \"\" {{", MaterialPD.uniqueID, m_Material.m_Name);
                    mb.Append("\n\t\tVersion: 102");
                    mb.Append("\n\t\tShadingModel: \"phong\"");
                    mb.Append("\n\t\tMultiLayer: 0");
                    mb.Append("\n\t\tProperties70:  {");
                    mb.Append("\n\t\t\tP: \"ShadingModel\", \"KString\", \"\", \"\", \"phong\"");

                    #region write material colors
                    foreach (var m_Color in m_Material.m_Colors)
                    {
                        switch (m_Color.first)
                        {
                            case "_Color":
                            case "gSurfaceColor":
                                mb.AppendFormat("\n\t\t\tP: \"DiffuseColor\", \"Color\", \"\", \"A\",{0},{1},{2}", m_Color.second[0], m_Color.second[1], m_Color.second[2]);
                                break;
                            case "_SpecularColor"://then what is _SpecColor??
                                mb.AppendFormat("\n\t\t\tP: \"SpecularColor\", \"Color\", \"\", \"A\",{0},{1},{2}", m_Color.second[0], m_Color.second[1], m_Color.second[2]);
                                break;
                            case "_ReflectColor":
                                mb.AppendFormat("\n\t\t\tP: \"AmbientColor\", \"Color\", \"\", \"A\",{0},{1},{2}", m_Color.second[0], m_Color.second[1], m_Color.second[2]);
                                break;
                            default:
                                mb.AppendFormat("\n;\t\t\tP: \"{3}\", \"Color\", \"\", \"A\",{0},{1},{2}", m_Color.second[0], m_Color.second[1], m_Color.second[2], m_Color.first);//commented out
                                break;
                        }
                    }
                    #endregion

                    #region write material parameters
                    foreach (var m_Float in m_Material.m_Floats)
                    {
                        switch (m_Float.first)
                        {
                            case "_Shininess":
                                mb.AppendFormat("\n\t\t\tP: \"ShininessExponent\", \"Number\", \"\", \"A\",{0}", m_Float.second);
                                mb.AppendFormat("\n\t\t\tP: \"Shininess\", \"Number\", \"\", \"A\",{0}", m_Float.second);
                                break;
                            case "_Transparency":
                                mb.Append("\n\t\t\tP: \"TransparentColor\", \"Color\", \"\", \"A\",1,1,1");
                                mb.AppendFormat("\n\t\t\tP: \"TransparencyFactor\", \"Number\", \"\", \"A\",{0}", m_Float.second);
                                mb.AppendFormat("\n\t\t\tP: \"Opacity\", \"Number\", \"\", \"A\",{0}", (1 - m_Float.second));
                                break;
                            default:
                                mb.AppendFormat("\n;\t\t\tP: \"{0}\", \"Number\", \"\", \"A\",{1}", m_Float.first, m_Float.second);
                                break;
                        }
                    }
                    #endregion

                    //mb.Append("\n\t\t\tP: \"SpecularFactor\", \"Number\", \"\", \"A\",0");
                    mb.Append("\n\t\t}");
                    mb.Append("\n\t}");

                    #region write texture connections
                    foreach (var m_TexEnv in m_Material.m_TexEnvs)
                    {
                        AssetPreloadData TexturePD;
                        #region get Porsche material from json
                        if (!assetsfileList.TryGetPD(m_TexEnv.m_Texture, out TexturePD) && jsonMats != null)
                        {
                            Dictionary<string, string> matProp;
                            if (jsonMats.TryGetValue(m_Material.m_Name, out matProp))
                            {
                                string texName;
                                if (matProp.TryGetValue(m_TexEnv.name, out texName))
                                {
                                    foreach (var asset in exportableAssets)
                                    {
                                        if (asset.Type2 == 28 && asset.Text == texName)
                                        {
                                            TexturePD = asset;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        if (TexturePD != null && TexturePD.Type2 == 28)
                        {
                            Textures.Add(TexturePD);

                            cb2.AppendFormat("\n\n\t;Texture::, Material::{0}", m_Material.m_Name);
                            cb2.AppendFormat("\n\tC: \"OP\",7{0},6{1}, \"", TexturePD.uniqueID, MaterialPD.uniqueID);

                            switch (m_TexEnv.name)
                            {
                                case "_MainTex":
                                case "gDiffuseSampler":
                                    cb2.Append("DiffuseColor\"");
                                    break;
                                case "_SpecularMap":
                                case "gSpecularSampler":
                                    cb2.Append("SpecularColor\"");
                                    break;
                                case "_NormalMap":
                                case "gNormalSampler":
                                    cb2.Append("NormalMap\"");
                                    break;
                                case "_BumpMap":
                                    cb2.Append("Bump\"");
                                    break;
                                default:
                                    cb2.AppendFormat("{0}\"", m_TexEnv.name);
                                    break;
                            }
                        }
                    }
                    #endregion
                }
                #endregion

                #region write generic FBX data after everything was collected
                fbx.Append("; FBX 7.1.0 project file");
                fbx.Append("\nFBXHeaderExtension:  {\n\tFBXHeaderVersion: 1003\n\tFBXVersion: 7100\n\tCreationTimeStamp:  {\n\t\tVersion: 1000");
                fbx.Append("\n\t\tYear: " + timestamp.Year);
                fbx.Append("\n\t\tMonth: " + timestamp.Month);
                fbx.Append("\n\t\tDay: " + timestamp.Day);
                fbx.Append("\n\t\tHour: " + timestamp.Hour);
                fbx.Append("\n\t\tMinute: " + timestamp.Minute);
                fbx.Append("\n\t\tSecond: " + timestamp.Second);
                fbx.Append("\n\t\tMillisecond: " + timestamp.Millisecond);
                fbx.Append("\n\t}\n\tCreator: \"Unity Studio by Chipicao\"\n}\n");

                fbx.Append("\nGlobalSettings:  {");
                fbx.Append("\n\tVersion: 1000");
                fbx.Append("\n\tProperties70:  {");
                fbx.Append("\n\t\tP: \"UpAxis\", \"int\", \"Integer\", \"\",1");
                fbx.Append("\n\t\tP: \"UpAxisSign\", \"int\", \"Integer\", \"\",1");
                fbx.Append("\n\t\tP: \"FrontAxis\", \"int\", \"Integer\", \"\",2");
                fbx.Append("\n\t\tP: \"FrontAxisSign\", \"int\", \"Integer\", \"\",1");
                fbx.Append("\n\t\tP: \"CoordAxis\", \"int\", \"Integer\", \"\",0");
                fbx.Append("\n\t\tP: \"CoordAxisSign\", \"int\", \"Integer\", \"\",1");
                fbx.Append("\n\t\tP: \"OriginalUpAxis\", \"int\", \"Integer\", \"\",1");
                fbx.Append("\n\t\tP: \"OriginalUpAxisSign\", \"int\", \"Integer\", \"\",1");
                fbx.AppendFormat("\n\t\tP: \"UnitScaleFactor\", \"double\", \"Number\", \"\",{0}", Properties.Settings.Default["scaleFactor"]);
                fbx.Append("\n\t\tP: \"OriginalUnitScaleFactor\", \"double\", \"Number\", \"\",1.0");
                //fbx.Append("\n\t\tP: \"AmbientColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
                //fbx.Append("\n\t\tP: \"DefaultCamera\", \"KString\", \"\", \"\", \"Producer Perspective\"");
                //fbx.Append("\n\t\tP: \"TimeMode\", \"enum\", \"\", \"\",6");
                //fbx.Append("\n\t\tP: \"TimeProtocol\", \"enum\", \"\", \"\",2");
                //fbx.Append("\n\t\tP: \"SnapOnFrameMode\", \"enum\", \"\", \"\",0");
                //fbx.Append("\n\t\tP: \"TimeSpanStart\", \"KTime\", \"Time\", \"\",0");
                //fbx.Append("\n\t\tP: \"TimeSpanStop\", \"KTime\", \"Time\", \"\",153953860000");
                //fbx.Append("\n\t\tP: \"CustomFrameRate\", \"double\", \"Number\", \"\",-1");
                //fbx.Append("\n\t\tP: \"TimeMarker\", \"Compound\", \"\", \"\"");
                //fbx.Append("\n\t\tP: \"CurrentTimeMarker\", \"int\", \"Integer\", \"\",-1");
                fbx.Append("\n\t}\n}\n");

                fbx.Append("\nDocuments:  {");
                fbx.Append("\n\tCount: 1");
                fbx.Append("\n\tDocument: 1234567890, \"\", \"Scene\" {");
                fbx.Append("\n\t\tProperties70:  {");
                fbx.Append("\n\t\t\tP: \"SourceObject\", \"object\", \"\", \"\"");
                fbx.Append("\n\t\t\tP: \"ActiveAnimStackName\", \"KString\", \"\", \"\", \"\"");
                fbx.Append("\n\t\t}");
                fbx.Append("\n\t\tRootNode: 0");
                fbx.Append("\n\t}\n}\n");
                fbx.Append("\nReferences:  {\n}\n");

                fbx.Append("\nDefinitions:  {");
                fbx.Append("\n\tVersion: 100");
                fbx.AppendFormat("\n\tCount: {0}", 1 + 2 * GameObjects.Count + Materials.Count + 2 * Textures.Count + ((bool)Properties.Settings.Default["exportDeformers"] ? Skins.Count + DeformerCount + Skins.Count + 1 : 0));

                fbx.Append("\n\tObjectType: \"GlobalSettings\" {");
                fbx.Append("\n\t\tCount: 1");
                fbx.Append("\n\t}");

                fbx.Append("\n\tObjectType: \"Model\" {");
                fbx.AppendFormat("\n\t\tCount: {0}", GameObjects.Count);
                fbx.Append("\n\t}");

                fbx.Append("\n\tObjectType: \"NodeAttribute\" {");
                fbx.AppendFormat("\n\t\tCount: {0}", GameObjects.Count - Meshes.Count - Skins.Count);
                fbx.Append("\n\t\tPropertyTemplate: \"FbxNull\" {");
                fbx.Append("\n\t\t\tProperties70:  {");
                fbx.Append("\n\t\t\t\tP: \"Color\", \"ColorRGB\", \"Color\", \"\",0.8,0.8,0.8");
                fbx.Append("\n\t\t\t\tP: \"Size\", \"double\", \"Number\", \"\",100");
                fbx.Append("\n\t\t\t\tP: \"Look\", \"enum\", \"\", \"\",1");
                fbx.Append("\n\t\t\t}\n\t\t}\n\t}");

                fbx.Append("\n\tObjectType: \"Geometry\" {");
                fbx.AppendFormat("\n\t\tCount: {0}", Meshes.Count + Skins.Count);
                fbx.Append("\n\t}");

                fbx.Append("\n\tObjectType: \"Material\" {");
                fbx.AppendFormat("\n\t\tCount: {0}", Materials.Count);
                fbx.Append("\n\t}");

                fbx.Append("\n\tObjectType: \"Texture\" {");
                fbx.AppendFormat("\n\t\tCount: {0}", Textures.Count);
                fbx.Append("\n\t}");

                fbx.Append("\n\tObjectType: \"Video\" {");
                fbx.AppendFormat("\n\t\tCount: {0}", Textures.Count);
                fbx.Append("\n\t}");

                if ((bool)Properties.Settings.Default["exportDeformers"])
                {
                    fbx.Append("\n\tObjectType: \"CollectionExclusive\" {");
                    fbx.AppendFormat("\n\t\tCount: {0}", Skins.Count);
                    fbx.Append("\n\t\tPropertyTemplate: \"FbxDisplayLayer\" {");
                    fbx.Append("\n\t\t\tProperties70:  {");
                    fbx.Append("\n\t\t\t\tP: \"Color\", \"ColorRGB\", \"Color\", \"\",0.8,0.8,0.8");
                    fbx.Append("\n\t\t\t\tP: \"Show\", \"bool\", \"\", \"\",1");
                    fbx.Append("\n\t\t\t\tP: \"Freeze\", \"bool\", \"\", \"\",0");
                    fbx.Append("\n\t\t\t\tP: \"LODBox\", \"bool\", \"\", \"\",0");
                    fbx.Append("\n\t\t\t}");
                    fbx.Append("\n\t\t}");
                    fbx.Append("\n\t}");

                    fbx.Append("\n\tObjectType: \"Deformer\" {");
                    fbx.AppendFormat("\n\t\tCount: {0}", DeformerCount + Skins.Count);
                    fbx.Append("\n\t}");

                    fbx.Append("\n\tObjectType: \"Pose\" {");
                    fbx.Append("\n\t\tCount: 1");
                    fbx.Append("\n\t}");
                }

                fbx.Append("\n}\n");
                fbx.Append("\nObjects:  {");

                FBXwriter.Write(fbx);
                fbx.Clear();
                #endregion

                #region write Model nodes and connections
                StatusStripUpdate("Writing Nodes and hierarchy");
                foreach (var m_GameObject in GameObjects)
                {
                    if (m_GameObject.m_MeshFilter == null && m_GameObject.m_SkinnedMeshRenderer == null)
                    {
                        if ((bool)Properties.Settings.Default["exportDeformers"] && (bool)Properties.Settings.Default["convertDummies"] && LimbNodes.Contains(m_GameObject))
                        {
                            ob.AppendFormat("\n\tNodeAttribute: 2{0}, \"NodeAttribute::\", \"LimbNode\" {{", m_GameObject.uniqueID);
                            ob.Append("\n\t\tTypeFlags: \"Skeleton\"");
                            ob.Append("\n\t}");

                            ob.AppendFormat("\n\tModel: 1{0}, \"Model::{1}\", \"LimbNode\" {{", m_GameObject.uniqueID, m_GameObject.m_Name);
                        }
                        else
                        {
                            ob.AppendFormat("\n\tNodeAttribute: 2{0}, \"NodeAttribute::\", \"Null\" {{", m_GameObject.uniqueID);
                            ob.Append("\n\t\tTypeFlags: \"Null\"");
                            ob.Append("\n\t}");

                            ob.AppendFormat("\n\tModel: 1{0}, \"Model::{1}\", \"Null\" {{", m_GameObject.uniqueID, m_GameObject.m_Name);
                        }

                        //connect NodeAttribute to Model
                        cb.AppendFormat("\n\n\t;NodeAttribute::, Model::{0}", m_GameObject.m_Name);
                        cb.AppendFormat("\n\tC: \"OO\",2{0},1{0}", m_GameObject.uniqueID);
                    }
                    else
                    {
                        ob.AppendFormat("\n\tModel: 1{0}, \"Model::{1}\", \"Mesh\" {{", m_GameObject.uniqueID, m_GameObject.m_Name);
                    }

                    ob.Append("\n\t\tVersion: 232");
                    ob.Append("\n\t\tProperties70:  {");
                    ob.Append("\n\t\t\tP: \"InheritType\", \"enum\", \"\", \"\",1");
                    ob.Append("\n\t\t\tP: \"ScalingMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
                    ob.Append("\n\t\t\tP: \"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",0");

                    Transform m_Transform;
                    if (assetsfileList.TryGetTransform(m_GameObject.m_Transform, out m_Transform))
                    {
                        float[] m_EulerRotation = QuatToEuler(new float[] { m_Transform.m_LocalRotation[0], -m_Transform.m_LocalRotation[1], -m_Transform.m_LocalRotation[2], m_Transform.m_LocalRotation[3] });

                        ob.AppendFormat("\n\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A\",{0},{1},{2}", -m_Transform.m_LocalPosition[0], m_Transform.m_LocalPosition[1], m_Transform.m_LocalPosition[2]);
                        ob.AppendFormat("\n\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A\",{0},{1},{2}", m_EulerRotation[0], m_EulerRotation[1], m_EulerRotation[2]);//handedness is switched in quat
                        ob.AppendFormat("\n\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\",{0},{1},{2}", m_Transform.m_LocalScale[0], m_Transform.m_LocalScale[1], m_Transform.m_LocalScale[2]);
                    }

                    //mb.Append("\n\t\t\tP: \"UDP3DSMAX\", \"KString\", \"\", \"U\", \"MapChannel:1 = UVChannel_1&cr;&lf;MapChannel:2 = UVChannel_2&cr;&lf;\"");
                    //mb.Append("\n\t\t\tP: \"MaxHandle\", \"int\", \"Integer\", \"UH\",24");
                    ob.Append("\n\t\t}");
                    ob.Append("\n\t\tShading: T");
                    ob.Append("\n\t\tCulling: \"CullingOff\"\n\t}");

                    //connect Model to parent
                    GameObject parentObject = (GameObject)m_GameObject.Parent;
                    if (GameObjects.Contains(parentObject))
                    {
                        cb.AppendFormat("\n\n\t;Model::{0}, Model::{1}", m_GameObject.m_Name, parentObject.m_Name);
                        cb.AppendFormat("\n\tC: \"OO\",1{0},1{1}", m_GameObject.uniqueID, parentObject.uniqueID);
                    }
                    else
                    {
                        cb.AppendFormat("\n\n\t;Model::{0}, Model::RootNode", m_GameObject.m_Name);
                        cb.AppendFormat("\n\tC: \"OO\",1{0},0", m_GameObject.uniqueID);
                    }


                }
                #endregion

                #region write non-skinnned Geometry
                StatusStripUpdate("Writing Geometry");
                foreach (var MeshPD in Meshes)
                {
                    Mesh m_Mesh = new Mesh(MeshPD);
                    MeshFBX(m_Mesh, MeshPD.uniqueID, ob);

                    //write data 8MB at a time
                    if (ob.Length > (8 * 0x100000))
                    { FBXwriter.Write(ob); ob.Clear(); }
                }
                #endregion

                #region write Deformer objects and skinned Geometry
                StringBuilder pb = new StringBuilder();
                //generate unique ID for BindPose
                pb.Append("\n\tPose: 5123456789, \"Pose::BIND_POSES\", \"BindPose\" {");
                pb.Append("\n\t\tType: \"BindPose\"");
                pb.Append("\n\t\tVersion: 100");
                pb.AppendFormat("\n\t\tNbPoseNodes: {0}", Skins.Count + LimbNodes.Count);

                foreach (var SkinnedMeshPD in Skins)
                {
                    SkinnedMeshRenderer m_SkinnedMeshRenderer = new SkinnedMeshRenderer(SkinnedMeshPD);

                    GameObject m_GameObject;
                    AssetPreloadData MeshPD;
                    if (assetsfileList.TryGetGameObject(m_SkinnedMeshRenderer.m_GameObject, out m_GameObject) && assetsfileList.TryGetPD(m_SkinnedMeshRenderer.m_Mesh, out MeshPD))
                    {
                        //generate unique Geometry ID for instanced mesh objects
                        //I should find a way to preserve instances at least when exportDeformers is not selected
                        var keepID = MeshPD.uniqueID;
                        MeshPD.uniqueID = SkinnedMeshPD.uniqueID;
                        Mesh m_Mesh = new Mesh(MeshPD);
                        MeshFBX(m_Mesh, MeshPD.uniqueID, ob);
                        
                        //write data 8MB at a time
                        if (ob.Length > (8 * 0x100000))
                        { FBXwriter.Write(ob); ob.Clear(); }

                        cb2.AppendFormat("\n\n\t;Geometry::, Model::{0}", m_GameObject.m_Name);
                        cb2.AppendFormat("\n\tC: \"OO\",3{0},1{1}", MeshPD.uniqueID, m_GameObject.uniqueID);
                        
                        if ((bool)Properties.Settings.Default["exportDeformers"])
                        {
                            //add BindPose node
                            pb.Append("\n\t\tPoseNode:  {");
                            pb.AppendFormat("\n\t\t\tNode: 1{0}", m_GameObject.uniqueID);
                            //pb.Append("\n\t\t\tMatrix: *16 {");
                            //pb.Append("\n\t\t\t\ta: ");
                            //pb.Append("\n\t\t\t} ");
                            pb.Append("\n\t\t}");

                            ob.AppendFormat("\n\tCollectionExclusive: 5{0}, \"DisplayLayer::{1}\", \"DisplayLayer\" {{", SkinnedMeshPD.uniqueID, m_GameObject.m_Name);
                            ob.Append("\n\t\tProperties70:  {");
                            ob.Append("\n\t\t}");
                            ob.Append("\n\t}");

                            //connect Model to DisplayLayer
                            cb2.AppendFormat("\n\n\t;Model::{0}, DisplayLayer::", m_GameObject.m_Name);
                            cb2.AppendFormat("\n\tC: \"OO\",1{0},5{1}", m_GameObject.uniqueID, SkinnedMeshPD.uniqueID);
                            
                            //write Deformers
                            if (m_Mesh.m_Skin.Length > 0 && m_Mesh.m_BindPose.Length >= m_SkinnedMeshRenderer.m_Bones.Length)
                            {
                                //write main Skin Deformer
                                ob.AppendFormat("\n\tDeformer: 4{0}, \"Deformer::\", \"Skin\" {{", SkinnedMeshPD.uniqueID);
                                ob.Append("\n\t\tVersion: 101");
                                ob.Append("\n\t\tLink_DeformAcuracy: 50");
                                ob.Append("\n\t}"); //Deformer end

                                //connect Skin Deformer to Geometry
                                cb2.Append("\n\n\t;Deformer::, Geometry::");
                                cb2.AppendFormat("\n\tC: \"OO\",4{0},3{1}", SkinnedMeshPD.uniqueID, MeshPD.uniqueID);

                                for (int b = 0; b < m_SkinnedMeshRenderer.m_Bones.Length; b++)
                                {
                                    Transform m_Transform;
                                    if (assetsfileList.TryGetTransform(m_SkinnedMeshRenderer.m_Bones[b], out m_Transform))
                                    {
                                        GameObject m_Bone;
                                        if (assetsfileList.TryGetGameObject(m_Transform.m_GameObject, out m_Bone))
                                        {
                                            int influences = 0, ibSplit = 0, wbSplit = 0;
                                            StringBuilder ib = new StringBuilder();//indices (vertex)
                                            StringBuilder wb = new StringBuilder();//weights

                                            for (int index = 0; index < m_Mesh.m_Skin.Length; index++)
                                            {
                                                if (m_Mesh.m_Skin[index][0].weight == 0 && (m_Mesh.m_Skin[index].All(x => x.weight == 0) || //if all weights (and indicces) are 0, bone0 has full control
                                                                                             m_Mesh.m_Skin[index][1].weight > 0)) //this implies a second bone exists, so bone0 has control too (otherwise it wouldn't be the first in the series)
                                                { m_Mesh.m_Skin[index][0].weight = 1; }

                                                var influence = m_Mesh.m_Skin[index].Find(x => x.boneIndex == b && x.weight > 0);
                                                if (influence != null)
                                                {
                                                    influences++;
                                                    ib.AppendFormat("{0},", index);
                                                    wb.AppendFormat("{0},", influence.weight);

                                                    if (ib.Length - ibSplit > 2000) { ib.Append("\n"); ibSplit = ib.Length; }
                                                    if (wb.Length - wbSplit > 2000) { wb.Append("\n"); wbSplit = wb.Length; }
                                                }

                                                /*float weight;
                                                if (m_Mesh.m_Skin[index].TryGetValue(b, out weight))
                                                {
                                                    if (weight > 0)
                                                    {
                                                        influences++;
                                                        ib.AppendFormat("{0},", index);
                                                        wb.AppendFormat("{0},", weight);
                                                    }
                                                    else if (m_Mesh.m_Skin[index].Keys.Count == 1)//m_Mesh.m_Skin[index].Values.All(x => x == 0)
                                                    {
                                                        influences++;
                                                        ib.AppendFormat("{0},", index);
                                                        wb.AppendFormat("{0},", 1);
                                                    }

                                                    if (ib.Length - ibSplit > 2000) { ib.Append("\n"); ibSplit = ib.Length; }
                                                    if (wb.Length - wbSplit > 2000) { wb.Append("\n"); wbSplit = wb.Length; }
                                                }*/
                                            }
                                            if (influences > 0)
                                            {
                                                ib.Length--;//remove last comma
                                                wb.Length--;//remove last comma
                                            }

                                            //SubDeformer objects need unique IDs because 2 or more deformers can be linked to the same bone
                                            ob.AppendFormat("\n\tDeformer: 4{0}{1}, \"SubDeformer::\", \"Cluster\" {{", b, SkinnedMeshPD.uniqueID);
                                            ob.Append("\n\t\tVersion: 100");
                                            ob.Append("\n\t\tUserData: \"\", \"\"");

                                            ob.AppendFormat("\n\t\tIndexes: *{0} {{\n\t\t\ta: ", influences);
                                            ob.Append(ib);
                                            ob.Append("\n\t\t}");
                                            ib.Clear();

                                            ob.AppendFormat("\n\t\tWeights: *{0} {{\n\t\t\ta: ", influences);
                                            ob.Append(wb);
                                            ob.Append("\n\t\t}");
                                            wb.Clear();

                                            ob.Append("\n\t\tTransform: *16 {\n\t\t\ta: ");
                                            //ob.Append(string.Join(",", m_Mesh.m_BindPose[b]));
                                            var m = m_Mesh.m_BindPose[b];
                                            ob.AppendFormat("{0},{1},{2},{3},", m[0, 0], -m[1, 0], -m[2, 0], m[3, 0]);
                                            ob.AppendFormat("{0},{1},{2},{3},", -m[0, 1], m[1, 1], m[2, 1], m[3, 1]);
                                            ob.AppendFormat("{0},{1},{2},{3},", -m[0, 2], m[1, 2], m[2, 2], m[3, 2]);
                                            ob.AppendFormat("{0},{1},{2},{3},", -m[0, 3], m[1, 3], m[2, 3], m[3, 3]);
                                            ob.Append("\n\t\t}");

                                            ob.Append("\n\t}"); //SubDeformer end

                                            //connect SubDeformer to Skin Deformer
                                            cb2.Append("\n\n\t;SubDeformer::, Deformer::");
                                            cb2.AppendFormat("\n\tC: \"OO\",4{0}{1},4{1}", b, SkinnedMeshPD.uniqueID);

                                            //connect dummy Model to SubDeformer
                                            cb2.AppendFormat("\n\n\t;Model::{0}, SubDeformer::", m_Bone.m_Name);
                                            cb2.AppendFormat("\n\tC: \"OO\",1{0},4{1}{2}", m_Bone.uniqueID, b, SkinnedMeshPD.uniqueID);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                bool stop = true;
                            }
                        }

                        MeshPD.uniqueID = keepID;
                    }
                }
                
                if ((bool)Properties.Settings.Default["exportDeformers"])
                {
                    foreach (var m_Bone in LimbNodes)
                    {
                        //add BindPose node
                        pb.Append("\n\t\tPoseNode:  {");
                        pb.AppendFormat("\n\t\t\tNode: 1{0}", m_Bone.uniqueID);
                        //pb.Append("\n\t\t\tMatrix: *16 {");
                        //pb.Append("\n\t\t\t\ta: ");
                        //pb.Append("\n\t\t\t} ");
                        pb.Append("\n\t\t}");
                    }
                    pb.Append("\n\t}"); //BindPose end
                    ob.Append(pb); pb.Clear();
                }
                #endregion

                ob.Append(mb); mb.Clear();
                cb.Append(cb2); cb2.Clear();

                #region write & extract Textures
                Directory.CreateDirectory(Path.GetDirectoryName(FBXfile) + "\\Texture2D");

                foreach (var TexturePD in Textures)
                {
                    Texture2D m_Texture2D = new Texture2D(TexturePD, true);
                    
                    #region extract texture
                    string texPath = Path.GetDirectoryName(FBXfile) + "\\Texture2D\\" + TexturePD.Text;
//TODO check texture type and set path accordingly; eg. CubeMap, Texture3D
                    if (uniqueNames.Checked) { texPath += " #" + TexturePD.uniqueID; }
                    if (m_Texture2D.m_TextureFormat < 30) { texPath += ".dds"; }
                    else if (m_Texture2D.m_TextureFormat < 35) { texPath += ".pvr"; }
                    else { texPath += "_" + m_Texture2D.m_Width.ToString() + "x" + m_Texture2D.m_Height.ToString() + "." + m_Texture2D.m_TextureFormat.ToString() + ".tex"; }

                    if (File.Exists(texPath))
                    {
                        StatusStripUpdate("Texture file " + Path.GetFileName(texPath) + " already exists");
                    }
                    else
                    {
                        StatusStripUpdate("Exporting Texture2D: " + Path.GetFileName(texPath));

                        switch (m_Texture2D.m_TextureFormat)
                        {
                            case 1: //Alpha8
                            case 2: //A4R4G4B4
                            case 3: //B8G8R8 //confirmed on X360, iOS //PS3 unsure
                            case 4: //G8R8A8B8 //confirmed on X360, iOS
                            case 5: //B8G8R8A8 //confirmed on X360, PS3, Web, iOS
                            case 7: //R5G6B5 //confirmed switched on X360; confirmed on iOS
                            case 10: //DXT1
                            case 12: //DXT5
                            case 13: //R4G4B4A4, iOS (only?)
                                WriteDDS(texPath, m_Texture2D);
                                break;
                            case 30: //PVRTC_RGB2
                            case 31: //PVRTC_RGBA2
                            case 32: //PVRTC_RGB4
                            case 33: //PVRTC_RGBA4
                            case 34: //ETC_RGB4
                                WritePVR(texPath, m_Texture2D);
                                break;
                            default:
                                {
                                    using (BinaryWriter writer = new BinaryWriter(File.Open(texPath, FileMode.Create)))
                                    {
                                        writer.Write(m_Texture2D.image_data);
                                        writer.Close();
                                    }
                                    break;
                                }
                        }
                    }
                    #endregion

                    ob.AppendFormat("\n\tTexture: 7{0}, \"Texture::{1}\", \"\" {{", TexturePD.uniqueID, TexturePD.Text);
                    ob.Append("\n\t\tType: \"TextureVideoClip\"");
                    ob.Append("\n\t\tVersion: 202");
                    ob.AppendFormat("\n\t\tTextureName: \"Texture::{0}\"", TexturePD.Text);
                    ob.Append("\n\t\tProperties70:  {");
                    ob.Append("\n\t\t\tP: \"UVSet\", \"KString\", \"\", \"\", \"UVChannel_0\"");
                    ob.Append("\n\t\t\tP: \"UseMaterial\", \"bool\", \"\", \"\",1");
                    ob.Append("\n\t\t}");
                    ob.AppendFormat("\n\t\tMedia: \"Video::{0}\"", TexturePD.Text);
                    ob.AppendFormat("\n\t\tFileName: \"{0}\"", texPath);
                    ob.AppendFormat("\n\t\tRelativeFilename: \"Texture2D\\{0}\"", Path.GetFileName(texPath));
                    ob.Append("\n\t}");

                    ob.AppendFormat("\n\tVideo: 8{0}, \"Video::{1}\", \"Clip\" {{", TexturePD.uniqueID, TexturePD.Text);
                    ob.Append("\n\t\tType: \"Clip\"");
                    ob.Append("\n\t\tProperties70:  {");
                    ob.AppendFormat("\n\t\t\tP: \"Path\", \"KString\", \"XRefUrl\", \"\", \"{0}\"", texPath);
                    ob.Append("\n\t\t}");
                    ob.AppendFormat("\n\t\tFileName: \"{0}\"", texPath);
                    ob.AppendFormat("\n\t\tRelativeFilename: \"Texture2D\\{0}\"", Path.GetFileName(texPath));
                    ob.Append("\n\t}");

                    //connect video to texture
                    cb.AppendFormat("\n\n\t;Video::{0}, Texture::{0}", TexturePD.Text);
                    cb.AppendFormat("\n\tC: \"OO\",8{0},7{1}", TexturePD.uniqueID, TexturePD.uniqueID);
                }
                #endregion

                FBXwriter.Write(ob);
                ob.Clear();
                
                cb.Append("\n}");//Connections end
                FBXwriter.Write(cb);
                cb.Clear();

                StatusStripUpdate("Finished exporting " + Path.GetFileName(FBXfile));
            }
        }

        private void MeshFBX(Mesh m_Mesh, string MeshID, StringBuilder ob)
        {
            if (m_Mesh.m_VertexCount > 0)//general failsafe
            {
                StatusStripUpdate("Writing Geometry: " + m_Mesh.m_Name);

                ob.AppendFormat("\n\tGeometry: 3{0}, \"Geometry::\", \"Mesh\" {{", MeshID);
                ob.Append("\n\t\tProperties70:  {");
                var randomColor = RandomColorGenerator(m_Mesh.m_Name);
                ob.AppendFormat("\n\t\t\tP: \"Color\", \"ColorRGB\", \"Color\", \"\",{0},{1},{2}", ((float)randomColor[0] / 255), ((float)randomColor[1] / 255), ((float)randomColor[2] / 255));
                ob.Append("\n\t\t}");

                #region Vertices
                ob.AppendFormat("\n\t\tVertices: *{0} {{\n\t\t\ta: ", m_Mesh.m_VertexCount * 3);

                int c = 3;//vertex components
                          //skip last component in vector4
                if (m_Mesh.m_Vertices.Length == m_Mesh.m_VertexCount * 4) { c++; } //haha

                int lineSplit = ob.Length;
                for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                {
                    ob.AppendFormat("{0},{1},{2},", -m_Mesh.m_Vertices[v * c], m_Mesh.m_Vertices[v * c + 1], m_Mesh.m_Vertices[v * c + 2]);

                    if (ob.Length - lineSplit > 2000)
                    {
                        ob.Append("\n");
                        lineSplit = ob.Length;
                    }
                }
                ob.Length--;//remove last comma
                ob.Append("\n\t\t}");
                #endregion

                #region Indices
                //in order to test topology for triangles/quads we need to store submeshes and write each one as geometry, then link to Mesh Node
                ob.AppendFormat("\n\t\tPolygonVertexIndex: *{0} {{\n\t\t\ta: ", m_Mesh.m_Indices.Count);

                lineSplit = ob.Length;
                for (int f = 0; f < m_Mesh.m_Indices.Count / 3; f++)
                {
                    ob.AppendFormat("{0},{1},{2},", m_Mesh.m_Indices[f * 3], m_Mesh.m_Indices[f * 3 + 2], (-m_Mesh.m_Indices[f * 3 + 1] - 1));

                    if (ob.Length - lineSplit > 2000)
                    {
                        ob.Append("\n");
                        lineSplit = ob.Length;
                    }
                }
                ob.Length--;//remove last comma

                ob.Append("\n\t\t}");
                ob.Append("\n\t\tGeometryVersion: 124");
                #endregion

                #region Normals
                if ((bool)Properties.Settings.Default["exportNormals"] && m_Mesh.m_Normals != null && m_Mesh.m_Normals.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementNormal: 0 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.AppendFormat("\n\t\t\tNormals: *{0} {{\n\t\t\ta: ", (m_Mesh.m_VertexCount * 3));

                    if (m_Mesh.m_Normals.Length == m_Mesh.m_VertexCount * 3) { c = 3; }
                    else if (m_Mesh.m_Normals.Length == m_Mesh.m_VertexCount * 4) { c = 4; }

                    lineSplit = ob.Length;
                    for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                    {
                        ob.AppendFormat("{0},{1},{2},", -m_Mesh.m_Normals[v * c], m_Mesh.m_Normals[v * c + 1], m_Mesh.m_Normals[v * c + 2]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion

                #region Tangents
                if ((bool)Properties.Settings.Default["exportTangents"] && m_Mesh.m_Tangents != null && m_Mesh.m_Tangents.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementTangent: 0 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.AppendFormat("\n\t\t\tTangents: *{0} {{\n\t\t\ta: ", (m_Mesh.m_VertexCount * 3));

                    if (m_Mesh.m_Tangents.Length == m_Mesh.m_VertexCount * 3) { c = 3; }
                    else if (m_Mesh.m_Tangents.Length == m_Mesh.m_VertexCount * 4) { c = 4; }

                    lineSplit = ob.Length;
                    for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                    {
                        ob.AppendFormat("{0},{1},{2},", -m_Mesh.m_Tangents[v * c], m_Mesh.m_Tangents[v * c + 1], m_Mesh.m_Tangents[v * c + 2]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion

                #region Colors
                if ((bool)Properties.Settings.Default["exportColors"] && m_Mesh.m_Colors != null && m_Mesh.m_Colors.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementColor: 0 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"\"");
                    //ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    //ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByPolygonVertex\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"IndexToDirect\"");
                    ob.AppendFormat("\n\t\t\tColors: *{0} {{\n\t\t\ta: ", m_Mesh.m_Colors.Length);
                    //ob.Append(string.Join(",", m_Mesh.m_Colors));

                    lineSplit = ob.Length;
                    for (int i = 0; i < m_Mesh.m_VertexCount; i++)
                    {
                        ob.AppendFormat("{0},{1},{2},{3},", m_Mesh.m_Colors[i * 2], m_Mesh.m_Colors[i * 2 + 1], m_Mesh.m_Colors[i * 2 + 2], m_Mesh.m_Colors[i * 2 + 3]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }

                    }
                    ob.Length--;//remove last comma

                    ob.Append("\n\t\t\t}");
                    ob.AppendFormat("\n\t\t\tColorIndex: *{0} {{\n\t\t\ta: ", m_Mesh.m_Indices.Count);

                    lineSplit = ob.Length;
                    for (int f = 0; f < m_Mesh.m_Indices.Count / 3; f++)
                    {
                        ob.AppendFormat("{0},{1},{2},", m_Mesh.m_Indices[f * 3], m_Mesh.m_Indices[f * 3 + 2], m_Mesh.m_Indices[f * 3 + 1]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma

                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion

                #region UV1
                //does FBX support UVW coordinates?
                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV1 != null && m_Mesh.m_UV1.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementUV: 0 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"UVChannel_1\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.AppendFormat("\n\t\t\tUV: *{0} {{\n\t\t\ta: ", m_Mesh.m_UV1.Length);

                    lineSplit = ob.Length;
                    for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                    {
                        ob.AppendFormat("{0},{1},", m_Mesh.m_UV1[v * 2], 1 - m_Mesh.m_UV1[v * 2 + 1]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion
                #region UV2
                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV2 != null && m_Mesh.m_UV2.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementUV: 1 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"UVChannel_2\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.AppendFormat("\n\t\t\tUV: *{0} {{\n\t\t\ta: ", m_Mesh.m_UV2.Length);

                    lineSplit = ob.Length;
                    for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                    {
                        ob.AppendFormat("{0},{1},", m_Mesh.m_UV2[v * 2], 1 - m_Mesh.m_UV2[v * 2 + 1]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion
                #region UV3
                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV3 != null && m_Mesh.m_UV3.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementUV: 2 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"UVChannel_3\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.AppendFormat("\n\t\t\tUV: *{0} {{\n\t\t\ta: ", m_Mesh.m_UV3.Length);

                    lineSplit = ob.Length;
                    for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                    {
                        ob.AppendFormat("{0},{1},", m_Mesh.m_UV3[v * 2], 1 - m_Mesh.m_UV3[v * 2 + 1]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion
                #region UV4
                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV4 != null && m_Mesh.m_UV4.Length > 0)
                {
                    ob.Append("\n\t\tLayerElementUV: 3 {");
                    ob.Append("\n\t\t\tVersion: 101");
                    ob.Append("\n\t\t\tName: \"UVChannel_4\"");
                    ob.Append("\n\t\t\tMappingInformationType: \"ByVertice\"");
                    ob.Append("\n\t\t\tReferenceInformationType: \"Direct\"");
                    ob.AppendFormat("\n\t\t\tUV: *{0} {{\n\t\t\ta: ", m_Mesh.m_UV4.Length);

                    lineSplit = ob.Length;
                    for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                    {
                        ob.AppendFormat("{0},{1},", m_Mesh.m_UV4[v * 2], 1 - m_Mesh.m_UV4[v * 2 + 1]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                    ob.Append("\n\t\t\t}\n\t\t}");
                }
                #endregion

                #region Material
                ob.Append("\n\t\tLayerElementMaterial: 0 {");
                ob.Append("\n\t\t\tVersion: 101");
                ob.Append("\n\t\t\tName: \"\"");
                ob.Append("\n\t\t\tMappingInformationType: \"");
                if (m_Mesh.m_SubMeshes.Count == 1) { ob.Append("AllSame\""); }
                else { ob.Append("ByPolygon\""); }
                ob.Append("\n\t\t\tReferenceInformationType: \"IndexToDirect\"");
                ob.AppendFormat("\n\t\t\tMaterials: *{0} {{", m_Mesh.m_materialIDs.Count);
                ob.Append("\n\t\t\t\t");
                if (m_Mesh.m_SubMeshes.Count == 1) { ob.Append("0"); }
                else
                {
                    lineSplit = ob.Length;
                    for (int i = 0; i < m_Mesh.m_materialIDs.Count; i++)
                    {
                        ob.AppendFormat("{0},", m_Mesh.m_materialIDs[i]);

                        if (ob.Length - lineSplit > 2000)
                        {
                            ob.Append("\n");
                            lineSplit = ob.Length;
                        }
                    }
                    ob.Length--;//remove last comma
                }
                ob.Append("\n\t\t\t}\n\t\t}");
                #endregion

                #region Layers
                ob.Append("\n\t\tLayer: 0 {");
                ob.Append("\n\t\t\tVersion: 100");
                if ((bool)Properties.Settings.Default["exportNormals"] && m_Mesh.m_Normals != null && m_Mesh.m_Normals.Length > 0)
                {
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementNormal\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 0");
                    ob.Append("\n\t\t\t}");
                }
                if ((bool)Properties.Settings.Default["exportTangents"] && m_Mesh.m_Tangents != null && m_Mesh.m_Tangents.Length > 0)
                {
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementTangent\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 0");
                    ob.Append("\n\t\t\t}");
                }
                ob.Append("\n\t\t\tLayerElement:  {");
                ob.Append("\n\t\t\t\tType: \"LayerElementMaterial\"");
                ob.Append("\n\t\t\t\tTypedIndex: 0");
                ob.Append("\n\t\t\t}");
                //
                /*ob.Append("\n\t\t\tLayerElement:  {");
                ob.Append("\n\t\t\t\tType: \"LayerElementTexture\"");
                ob.Append("\n\t\t\t\tTypedIndex: 0");
                ob.Append("\n\t\t\t}");
                ob.Append("\n\t\t\tLayerElement:  {");
                ob.Append("\n\t\t\t\tType: \"LayerElementBumpTextures\"");
                ob.Append("\n\t\t\t\tTypedIndex: 0");
                ob.Append("\n\t\t\t}");*/
                if ((bool)Properties.Settings.Default["exportColors"] && m_Mesh.m_Colors != null && m_Mesh.m_Colors.Length > 0)
                {
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementColor\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 0");
                    ob.Append("\n\t\t\t}");
                }
                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV1 != null && m_Mesh.m_UV1.Length > 0)
                {
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementUV\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 0");
                    ob.Append("\n\t\t\t}");
                }
                ob.Append("\n\t\t}"); //Layer 0 end

                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV2 != null && m_Mesh.m_UV2.Length > 0)
                {
                    ob.Append("\n\t\tLayer: 1 {");
                    ob.Append("\n\t\t\tVersion: 100");
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementUV\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 1");
                    ob.Append("\n\t\t\t}");
                    ob.Append("\n\t\t}"); //Layer 1 end
                }

                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV3 != null && m_Mesh.m_UV3.Length > 0)
                {
                    ob.Append("\n\t\tLayer: 2 {");
                    ob.Append("\n\t\t\tVersion: 100");
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementUV\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 2");
                    ob.Append("\n\t\t\t}");
                    ob.Append("\n\t\t}"); //Layer 2 end
                }

                if ((bool)Properties.Settings.Default["exportUVs"] && m_Mesh.m_UV4 != null && m_Mesh.m_UV4.Length > 0)
                {
                    ob.Append("\n\t\tLayer: 3 {");
                    ob.Append("\n\t\t\tVersion: 100");
                    ob.Append("\n\t\t\tLayerElement:  {");
                    ob.Append("\n\t\t\t\tType: \"LayerElementUV\"");
                    ob.Append("\n\t\t\t\tTypedIndex: 3");
                    ob.Append("\n\t\t\t}");
                    ob.Append("\n\t\t}"); //Layer 3 end
                }
                #endregion

                ob.Append("\n\t}"); //Geometry end
            }
        }

        private static float[] QuatToEuler(float[] q)
        {
            double eax = 0;
            double eay = 0;
            double eaz = 0;

            float qx = q[0];
            float qy = q[1];
            float qz = q[2];
            float qw = q[3];

            double[,] M = new double[4, 4];

            double Nq = qx * qx + qy * qy + qz * qz + qw * qw;
            double s = (Nq > 0.0) ? (2.0 / Nq) : 0.0;
            double xs = qx * s, ys = qy * s, zs = qz * s;
            double wx = qw * xs, wy = qw * ys, wz = qw * zs;
            double xx = qx * xs, xy = qx * ys, xz = qx * zs;
            double yy = qy * ys, yz = qy * zs, zz = qz * zs;

            M[0, 0] = 1.0 - (yy + zz); M[0, 1] = xy - wz; M[0, 2] = xz + wy;
            M[1, 0] = xy + wz; M[1, 1] = 1.0 - (xx + zz); M[1, 2] = yz - wx;
            M[2, 0] = xz - wy; M[2, 1] = yz + wx; M[2, 2] = 1.0 - (xx + yy);
            M[3, 0] = M[3, 1] = M[3, 2] = M[0, 3] = M[1, 3] = M[2, 3] = 0.0; M[3, 3] = 1.0;

            double test = Math.Sqrt(M[0, 0] * M[0, 0] + M[1, 0] * M[1, 0]);
            if (test > 16 * 1.19209290E-07F)//FLT_EPSILON
            {
                eax = Math.Atan2(M[2, 1], M[2, 2]);
                eay = Math.Atan2(-M[2, 0], test);
                eaz = Math.Atan2(M[1, 0], M[0, 0]);
            }
            else
            {
                eax = Math.Atan2(-M[1, 2], M[1, 1]);
                eay = Math.Atan2(-M[2, 0], test);
                eaz = 0;
            }

            return new float[3] { (float)(eax * 180 / Math.PI), (float)(eay * 180 / Math.PI), (float)(eaz * 180 / Math.PI) };
        }

        private static byte[] RandomColorGenerator(string name)
        {
            int nameHash = name.GetHashCode();
            Random r = new Random(nameHash);
            //Random r = new Random(DateTime.Now.Millisecond);

            byte red = (byte)r.Next(0, 255);
            byte green = (byte)r.Next(0, 255);
            byte blue = (byte)r.Next(0, 255);

            return new byte[3] { red, green, blue };
        }


        private void ExportAssets_Click(object sender, EventArgs e)
        {
            if (exportableAssets.Count > 0)
            {
                saveFolderDialog1.InitialDirectory = mainPath;
                if (saveFolderDialog1.ShowDialog() == DialogResult.OK)
                {
                    var savePath = saveFolderDialog1.FileName;
                    if (Path.GetFileName(savePath) == "Select folder or write folder name to create")
                    { savePath = Path.GetDirectoryName(saveFolderDialog1.FileName); }
                    //Directory.CreateDirectory(saveFolderDialog1.FileName);//this will be created later, when grouping is determined

                    switch (((ToolStripItem)sender).Name)
                    {
                        case "exportAllAssetsMenuItem":
                            ExportAll(savePath, assetGroupOptions.SelectedIndex);
                            break;
                        case "exportFilteredAssetsMenuItem":
                            ExportFiltered(visibleAssets, savePath, assetGroupOptions.SelectedIndex);
                            break;
                        case "exportSelectedAssetsMenuItem":
                            List<AssetPreloadData> selectedAssetList = new List<AssetPreloadData>();
                            var selIndices = assetListView.SelectedIndices;
                            foreach (int index in selIndices) { selectedAssetList.Add((AssetPreloadData)assetListView.Items[index]); }
                            ExportFiltered(selectedAssetList, savePath, assetGroupOptions.SelectedIndex);
                            break;
                    }

                    if (openAfterExport.Checked) { System.Diagnostics.Process.Start(savePath); }
                }

            }
            else
            {
                StatusStripUpdate("No exportable assets loaded");
            }
        }

        private void ExportAll(string selectedPath, int groupFiles)
        {
            int exportedCount = 0;

            foreach (var assetsFile in assetsfileList)
            {
                if (assetsFile.exportableAssets.Count > 0)
                {
                    string exportpath = selectedPath;
                    if (groupFiles == 1) { exportpath += "\\" + Path.GetFileNameWithoutExtension(assetsFile.filePath) + "_export"; }
                    Directory.CreateDirectory(exportpath);

                    foreach (var asset in assetsFile.exportableAssets)
                    {
                        if (groupFiles == 0)
                        {
                            switch (asset.Type2)
                            {
                                case 28:
                                    exportpath = selectedPath + "\\Texture2D";
                                    break;
                                case 83:
                                    exportpath = selectedPath + "\\AudioClip";
                                    break;
                                case 48:
                                    exportpath = selectedPath + "\\Shader";
                                    break;
                                case 49:
                                    exportpath = selectedPath + "\\TextAsset";
                                    break;
                                case 128:
                                    exportpath = selectedPath + "\\Font";
                                    break;
                            }
                            Directory.CreateDirectory(exportpath);
                        }
                        exportedCount += ExportAsset(asset, exportpath);
                    }
                }
            }
            string statusText = "Finished exporting " + exportedCount.ToString() + " assets.";
            if ((exportableAssets.Count - exportedCount) > 0) { statusText += " " + (exportableAssets.Count - exportedCount).ToString() + " assets skipped (not extractable or files already exist)"; }
            StatusStripUpdate(statusText);
        }

        private void ExportFiltered(List<AssetPreloadData> filteredAssetList, string selectedPath, int groupFiles)
        {
            if (filteredAssetList.Count > 0)
            {
                int exportedCount = 0;

                foreach (var asset in filteredAssetList)
                {
                    string exportpath = selectedPath;
                    if (groupFiles == 1) { exportpath += "\\" + Path.GetFileNameWithoutExtension(asset.sourceFile.filePath) + "_export"; }
                    else if (groupFiles == 0)
                    {
                        switch (asset.Type2)
                        {
                            case 28:
                                exportpath = selectedPath + "\\Texture2D";
                                break;
                            case 83:
                                exportpath = selectedPath + "\\AudioClip";
                                break;
                            case 48:
                                exportpath = selectedPath + "\\Shader";
                                break;
                            case 49:
                                exportpath = selectedPath + "\\TextAsset";
                                break;
                            case 128:
                                exportpath = selectedPath + "\\Font";
                                break;
                        }
                    }

                    Directory.CreateDirectory(exportpath);
                    exportedCount += ExportAsset(asset, exportpath);
                }
                
                string statusText = "Finished exporting " + exportedCount.ToString() + " assets.";
                if ((filteredAssetList.Count - exportedCount) > 0) { statusText += " " + (filteredAssetList.Count - exportedCount).ToString() + " assets skipped (not extractable or files already exist)"; }
                StatusStripUpdate(statusText);
            }
            else
            {
                StatusStripUpdate("No exportable assets selected or filtered");
            }
        }

        private int ExportAsset(AssetPreloadData asset, string exportPath)
        {
            int exportCount = 0;
            
            switch (asset.Type2)
            {
                #region Texture2D
                case 28: //Texture2D
                    {
                        Texture2D m_Texture2D = new Texture2D(asset, true);
                        
                        string texPath = exportPath + "\\" + asset.Text;
                        if (uniqueNames.Checked) { texPath += " #" + asset.uniqueID; }
                        if (m_Texture2D.m_TextureFormat < 30) { texPath += ".dds"; }
                        else if (m_Texture2D.m_TextureFormat < 35) { texPath += ".pvr"; }
                        else { texPath += "_" + m_Texture2D.m_Width.ToString() + "x" + m_Texture2D.m_Height.ToString() + "." + m_Texture2D.m_TextureFormat.ToString() + ".tex"; }

                        if (File.Exists(texPath))
                        {
                            StatusStripUpdate("Texture file " + Path.GetFileName(texPath) + " already exists");
                        }
                        else
                        {
                            StatusStripUpdate("Exporting Texture2D: " + Path.GetFileName(texPath));
                            exportCount += 1;

                            switch (m_Texture2D.m_TextureFormat)
                            {
                                case 1: //Alpha8
                                case 2: //A4R4G4B4
                                case 3: //B8G8R8 //confirmed on X360, iOS //PS3 unsure
                                case 4: //G8R8A8B8 //confirmed on X360, iOS
                                case 5: //B8G8R8A8 //confirmed on X360, PS3, Web, iOS
                                case 7: //R5G6B5 //confirmed switched on X360; confirmed on iOS
                                case 10: //DXT1
                                case 12: //DXT5
                                case 13: //R4G4B4A4, iOS (only?)
                                    WriteDDS(texPath, m_Texture2D);
                                    break;
                                case 30: //PVRTC_RGB2
                                case 31: //PVRTC_RGBA2
                                case 32: //PVRTC_RGB4
                                case 33: //PVRTC_RGBA4
                                case 34: //ETC_RGB4
                                    WritePVR(texPath, m_Texture2D);
                                    break;
                                default:
                                    {
                                        using (BinaryWriter writer = new BinaryWriter(File.Open(texPath, FileMode.Create)))
                                        {
                                            writer.Write(m_Texture2D.image_data);
                                            writer.Close();
                                        }
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                #endregion
                #region AudioClip
                case 83: //AudioClip
                    {
                        AudioClip m_AudioClip = new AudioClip(asset, true);
                        
                        string audPath = exportPath + "\\" + asset.Text;
                        if (uniqueNames.Checked) { audPath += " #" + asset.uniqueID; }
                        audPath += m_AudioClip.extension;

                        if (File.Exists(audPath))
                        {
                            StatusStripUpdate("Audio file " + Path.GetFileName(audPath) + " already exists");
                        }
                        else
                        {
                            StatusStripUpdate("Exporting AudioClip: " + Path.GetFileName(audPath));
                            exportCount += 1;
                            
                            using (BinaryWriter writer = new BinaryWriter(File.Open(audPath, FileMode.Create)))
                            {
                                writer.Write(m_AudioClip.m_AudioData);
                                writer.Close();
                            }

                        }
                        break;
                    }
                #endregion
                #region Shader & TextAsset
                case 48: //Shader
                case 49: //TextAsset
                    {
                        TextAsset m_TextAsset = new TextAsset(asset, true);

                        string textAssetPath = exportPath + "\\" + asset.Text;
                        if (uniqueNames.Checked) { textAssetPath += " #" + asset.uniqueID; }
                        textAssetPath += m_TextAsset.extension;

                        if (File.Exists(textAssetPath))
                        {
                            StatusStripUpdate("TextAsset file " + Path.GetFileName(textAssetPath) + " already exists");
                        }
                        else
                        {
                            StatusStripUpdate("Exporting TextAsset: " + Path.GetFileName(textAssetPath));
                            exportCount += 1;

                            using (BinaryWriter writer = new BinaryWriter(File.Open(textAssetPath, FileMode.Create)))
                            {
                                writer.Write(m_TextAsset.m_Script);
                                writer.Close();
                            }
                        }
                        break;
                    }
                #endregion
                #region Font
                case 128: //Font
                    {
                        unityFont m_Font = new unityFont(asset);

                        if (m_Font.m_FontData != null)
                        {
                            string fontPath = exportPath + "\\" + asset.Text;
                            if (uniqueNames.Checked) { fontPath += " #" + asset.uniqueID; }
                            fontPath += m_Font.extension;

                            if (File.Exists(fontPath))
                            {
                                StatusStripUpdate("Font file " + Path.GetFileName(fontPath) + " already exists");
                            }
                            else
                            {
                                StatusStripUpdate("Exporting Font: " + Path.GetFileName(fontPath));

                                using (BinaryWriter writer = new BinaryWriter(File.Open(fontPath, FileMode.Create)))
                                {
                                    writer.Write(m_Font.m_FontData);
                                    writer.Close();
                                }

                                exportCount += 1;
                            }
                        }
                        break;
                    }
                #endregion
                /*default:
                    {
                        string assetPath = exportPath + "\\" + asset.Name + "." + asset.TypeString;
                        byte[] assetData = new byte[asset.Size];
                        Stream.Read(assetData, 0, asset.Size);
                        using (BinaryWriter writer = new BinaryWriter(File.Open(assetPath, FileMode.Create)))
                        {
                            writer.Write(assetData);
                            writer.Close();
                        }
                        exportCount += 1;
                        break;
                    }*/
            }
            return exportCount;
        }

        private void WriteDDS(string DDSfile, Texture2D m_Texture2D)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(DDSfile, FileMode.Create)))
            {
                writer.Write(0x20534444);
                writer.Write(0x7C);
                writer.Write(m_Texture2D.dwFlags);
                writer.Write(m_Texture2D.m_Height);
                writer.Write(m_Texture2D.m_Width);
                writer.Write(m_Texture2D.dwPitchOrLinearSize); //should be main tex size without mips);
                writer.Write((int)0); //dwDepth not implemented
                writer.Write(m_Texture2D.dwMipMapCount);
                writer.Write(new byte[44]); //dwReserved1[11]
                writer.Write(m_Texture2D.dwSize);
                writer.Write(m_Texture2D.dwFlags2);
                writer.Write(m_Texture2D.dwFourCC);
                writer.Write(m_Texture2D.dwRGBBitCount);
                writer.Write(m_Texture2D.dwRBitMask);
                writer.Write(m_Texture2D.dwGBitMask);
                writer.Write(m_Texture2D.dwBBitMask);
                writer.Write(m_Texture2D.dwABitMask);
                writer.Write(m_Texture2D.dwCaps);
                writer.Write(m_Texture2D.dwCaps2);
                writer.Write(new byte[12]); //dwCaps3&4 & dwReserved2

                writer.Write(m_Texture2D.image_data);
                writer.Close();
            }
        }

        private void WritePVR(string PVRfile, Texture2D m_Texture2D)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(PVRfile, FileMode.Create)))
            {
                writer.Write(m_Texture2D.pvrVersion);
                writer.Write(m_Texture2D.pvrFlags);
                writer.Write(m_Texture2D.pvrPixelFormat);
                writer.Write(m_Texture2D.pvrColourSpace);
                writer.Write(m_Texture2D.pvrChannelType);
                writer.Write(m_Texture2D.m_Height);
                writer.Write(m_Texture2D.m_Width);
                writer.Write(m_Texture2D.pvrDepth);
                writer.Write(m_Texture2D.pvrNumSurfaces);
                writer.Write(m_Texture2D.pvrNumFaces);
                writer.Write(m_Texture2D.dwMipMapCount);
                writer.Write(m_Texture2D.pvrMetaDataSize);

                writer.Write(m_Texture2D.image_data);
                writer.Close();
            }
        }


        public UnityStudioForm()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            InitializeComponent();
            uniqueNames.Checked = (bool)Properties.Settings.Default["uniqueNames"];
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

            unityFiles.Clear();
            assetsfileList.Clear();
            exportableAssets.Clear();
            visibleAssets.Clear();

            sceneTreeView.Nodes.Clear();

            assetListView.VirtualListSize = 0;
            assetListView.Items.Clear();
            //assetListView.Groups.Clear();

            classesListView.Items.Clear();
            classesListView.Groups.Clear();

            previewPanel.BackgroundImage = global::Unity_Studio.Properties.Resources.preview;
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

        public void StatusStripUpdate(string statusText)
        {
            toolStripStatusLabel1.Text = statusText;
            statusStrip1.Update();
        }
    }
}
