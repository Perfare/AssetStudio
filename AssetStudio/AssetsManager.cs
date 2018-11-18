using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AssetStudio.ImportHelper;

namespace AssetStudio
{
    public class AssetsManager
    {
        public List<SerializedFile> assetsFileList = new List<SerializedFile>();
        internal Dictionary<string, int> assetsFileIndexCache = new Dictionary<string, int>();
        internal Dictionary<string, EndianBinaryReader> resourceFileReaders = new Dictionary<string, EndianBinaryReader>();

        private List<string> importFiles = new List<string>();
        private HashSet<string> importFilesHash = new HashSet<string>();
        private HashSet<string> assetsfileListHash = new HashSet<string>();

        public void LoadFiles(string[] files)
        {
            var path = Path.GetDirectoryName(files[0]);
            MergeSplitAssets(path);
            var toReadFile = ProcessingSplitFiles(files.ToList());
            Load(toReadFile);
        }

        public void LoadFolder(string path)
        {
            MergeSplitAssets(path, true);
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();
            var toReadFile = ProcessingSplitFiles(files);
            Load(toReadFile);
        }

        private void Load(string[] files)
        {
            foreach (var file in files)
            {
                importFiles.Add(file);
                importFilesHash.Add(Path.GetFileName(file).ToUpper());
            }
            Progress.Reset();
            //use a for loop because list size can change
            for (var i = 0; i < importFiles.Count; i++)
            {
                LoadFile(importFiles[i]);
                Progress.Report(i + 1, importFiles.Count);
            }
            importFiles.Clear();
            importFilesHash.Clear();
            assetsfileListHash.Clear();
        }

        private void LoadFile(string fullName)
        {
            switch (CheckFileType(fullName, out var reader))
            {
                case FileType.AssetsFile:
                    LoadAssetsFile(fullName, reader);
                    break;
                case FileType.BundleFile:
                    LoadBundleFile(fullName, reader);
                    break;
                case FileType.WebFile:
                    LoadWebFile(fullName, reader);
                    break;
            }
        }

        private void LoadAssetsFile(string fullName, EndianBinaryReader reader)
        {
            var fileName = Path.GetFileName(fullName);
            if (!assetsfileListHash.Contains(fileName.ToUpper()))
            {
                Logger.Info($"Loading {fileName}");
                var assetsFile = new SerializedFile(this, fullName, reader);
                if (assetsFile.valid)
                {
                    assetsFileList.Add(assetsFile);
                    assetsfileListHash.Add(assetsFile.upperFileName);

                    foreach (var sharedFile in assetsFile.m_Externals)
                    {
                        var sharedFilePath = Path.GetDirectoryName(fullName) + "\\" + sharedFile.fileName;
                        var sharedFileName = sharedFile.fileName;

                        if (!importFilesHash.Contains(sharedFileName.ToUpper()))
                        {
                            if (!File.Exists(sharedFilePath))
                            {
                                var findFiles = Directory.GetFiles(Path.GetDirectoryName(fullName), sharedFileName, SearchOption.AllDirectories);
                                if (findFiles.Length > 0)
                                {
                                    sharedFilePath = findFiles[0];
                                }
                            }

                            if (File.Exists(sharedFilePath))
                            {
                                importFiles.Add(sharedFilePath);
                                importFilesHash.Add(sharedFileName.ToUpper());
                            }
                        }
                    }
                }
                else
                {
                    reader.Dispose();
                }
            }
            else
            {
                reader.Dispose();
            }
        }

        private void LoadAssetsFromMemory(string fullName, EndianBinaryReader reader, string originalPath, string unityVersion = null)
        {
            var fileName = Path.GetFileName(fullName);
            if (!assetsfileListHash.Contains(fileName.ToUpper()))
            {
                Logger.Info($"Loading {fileName}");
                var assetsFile = new SerializedFile(this, fullName, reader);
                if (assetsFile.valid)
                {
                    assetsFile.originalPath = originalPath;
                    if (assetsFile.header.m_Version < 7)
                    {
                        assetsFile.SetVersion(unityVersion);
                    }
                    assetsFileList.Add(assetsFile);
                    assetsfileListHash.Add(assetsFile.upperFileName);
                }
                else
                {
                    resourceFileReaders.Add(assetsFile.upperFileName, assetsFile.reader);
                }
            }
        }

        private void LoadBundleFile(string fullName, EndianBinaryReader reader, string parentPath = null)
        {
            var fileName = Path.GetFileName(fullName);
            Logger.Info("Decompressing " + fileName);
            var bundleFile = new BundleFile(reader, fullName);
            reader.Dispose();
            foreach (var file in bundleFile.fileList)
            {
                var dummyPath = Path.GetDirectoryName(fullName) + "\\" + file.fileName;
                LoadAssetsFromMemory(dummyPath, new EndianBinaryReader(file.stream), parentPath ?? fullName, bundleFile.versionEngine);
            }
        }

        private void LoadWebFile(string fullName, EndianBinaryReader reader)
        {
            var fileName = Path.GetFileName(fullName);
            Logger.Info("Loading " + fileName);
            var webFile = new WebFile(reader);
            reader.Dispose();
            foreach (var file in webFile.fileList)
            {
                var dummyPath = Path.GetDirectoryName(fullName) + "\\" + file.fileName;
                switch (CheckFileType(file.stream, out reader))
                {
                    case FileType.AssetsFile:
                        LoadAssetsFromMemory(dummyPath, reader, fullName);
                        break;
                    case FileType.BundleFile:
                        LoadBundleFile(dummyPath, reader, fullName);
                        break;
                    case FileType.WebFile:
                        LoadWebFile(dummyPath, reader);
                        break;
                }
            }
        }

        public void Clear()
        {
            foreach (var assetsFile in assetsFileList)
            {
                assetsFile.reader.Close();
            }
            assetsFileList.Clear();
            foreach (var resourceFileReader in resourceFileReaders)
            {
                resourceFileReader.Value.Close();
            }
            resourceFileReaders.Clear();
            assetsFileIndexCache.Clear();
        }
    }
}
