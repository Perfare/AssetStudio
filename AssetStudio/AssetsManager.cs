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
        private HashSet<string> assetsFileListHash = new HashSet<string>();

        public void LoadFiles(params string[] files)
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
            assetsFileListHash.Clear();

            ReadAssets();
            ProcessAssets();
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
            if (!assetsFileListHash.Contains(fileName.ToUpper()))
            {
                Logger.Info($"Loading {fileName}");
                try
                {
                    var assetsFile = new SerializedFile(this, fullName, reader);
                    assetsFileList.Add(assetsFile);
                    assetsFileListHash.Add(assetsFile.upperFileName);

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
                catch
                {
                    reader.Dispose();
                    //Logger.Error($"Unable to load assets file {fileName}");
                }
            }
            else
            {
                reader.Dispose();
            }
        }

        private void LoadAssetsFromMemory(string fullName, EndianBinaryReader reader, string originalPath, string unityVersion = null)
        {
            var upperFileName = Path.GetFileName(fullName).ToUpper();
            if (!assetsFileListHash.Contains(upperFileName))
            {
                try
                {
                    var assetsFile = new SerializedFile(this, fullName, reader);
                    assetsFile.originalPath = originalPath;
                    if (assetsFile.header.m_Version < 7)
                    {
                        assetsFile.SetVersion(unityVersion);
                    }
                    assetsFileList.Add(assetsFile);
                    assetsFileListHash.Add(assetsFile.upperFileName);
                }
                catch
                {
                    //Logger.Error($"Unable to load assets file {fileName} from {Path.GetFileName(originalPath)}");
                }
                finally
                {
                    resourceFileReaders.Add(upperFileName, reader);
                }
            }
        }

        private void LoadBundleFile(string fullName, EndianBinaryReader reader, string parentPath = null)
        {
            var fileName = Path.GetFileName(fullName);
            Logger.Info("Loading " + fileName);
            try
            {
                var bundleFile = new BundleFile(reader, fullName);
                foreach (var file in bundleFile.fileList)
                {
                    var dummyPath = Path.GetDirectoryName(fullName) + "\\" + file.fileName;
                    LoadAssetsFromMemory(dummyPath, new EndianBinaryReader(file.stream), parentPath ?? fullName, bundleFile.versionEngine);
                }
            }
            catch
            {
                /*var str = $"Unable to load bundle file {fileName}";
                if (parentPath != null)
                {
                    str += $" from {Path.GetFileName(parentPath)}";
                }
                Logger.Error(str);*/
            }
            finally
            {
                reader.Dispose();
            }
        }

        private void LoadWebFile(string fullName, EndianBinaryReader reader)
        {
            var fileName = Path.GetFileName(fullName);
            Logger.Info("Loading " + fileName);
            try
            {
                var webFile = new WebFile(reader);
                foreach (var file in webFile.fileList)
                {
                    var dummyPath = Path.GetDirectoryName(fullName) + "\\" + file.fileName;
                    switch (CheckFileType(file.stream, out var fileReader))
                    {
                        case FileType.AssetsFile:
                            LoadAssetsFromMemory(dummyPath, fileReader, fullName);
                            break;
                        case FileType.BundleFile:
                            LoadBundleFile(dummyPath, fileReader, fullName);
                            break;
                        case FileType.WebFile:
                            LoadWebFile(dummyPath, fileReader);
                            break;
                    }
                }
            }
            catch
            {
                //Logger.Error($"Unable to load web file {fileName}");
            }
            finally
            {
                reader.Dispose();
            }
        }

        public void Clear()
        {
            foreach (var assetsFile in assetsFileList)
            {
                assetsFile.Objects.Clear();
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

        private void ReadAssets()
        {
            Logger.Info("Read assets...");

            var progressCount = assetsFileList.Sum(x => x.m_Objects.Count);
            int i = 0;
            Progress.Reset();
            foreach (var assetsFile in assetsFileList)
            {
                assetsFile.Objects = new Dictionary<long, Object>(assetsFile.m_Objects.Count);
                foreach (var objectInfo in assetsFile.m_Objects)
                {
                    var objectReader = new ObjectReader(assetsFile.reader, assetsFile, objectInfo);
                    switch (objectReader.type)
                    {
                        case ClassIDType.Animation:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new Animation(objectReader));
                            break;
                        case ClassIDType.AnimationClip:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new AnimationClip(objectReader));
                            break;
                        case ClassIDType.Animator:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new Animator(objectReader));
                            break;
                        case ClassIDType.AnimatorController:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new AnimatorController(objectReader));
                            break;
                        case ClassIDType.AnimatorOverrideController:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new AnimatorOverrideController(objectReader));
                            break;
                        case ClassIDType.AssetBundle:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new AssetBundle(objectReader));
                            break;
                        case ClassIDType.AudioClip:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new AudioClip(objectReader));
                            break;
                        case ClassIDType.Avatar:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new Avatar(objectReader));
                            break;
                        case ClassIDType.Font:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new Font(objectReader));
                            break;
                        case ClassIDType.GameObject:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new GameObject(objectReader));
                            break;
                        case ClassIDType.Material:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new Material(objectReader));
                            break;
                        case ClassIDType.Mesh:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new Mesh(objectReader));
                            break;
                        case ClassIDType.MeshFilter:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new MeshFilter(objectReader));
                            break;
                        case ClassIDType.MeshRenderer:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new MeshRenderer(objectReader));
                            break;
                        case ClassIDType.MonoBehaviour:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new MonoBehaviour(objectReader));
                            break;
                        case ClassIDType.MonoScript:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new MonoScript(objectReader));
                            break;
                        case ClassIDType.MovieTexture:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new MovieTexture(objectReader));
                            break;
                        case ClassIDType.PlayerSettings:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new PlayerSettings(objectReader));
                            break;
                        case ClassIDType.RectTransform:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new RectTransform(objectReader));
                            break;
                        case ClassIDType.Shader:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new Shader(objectReader));
                            break;
                        case ClassIDType.SkinnedMeshRenderer:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new SkinnedMeshRenderer(objectReader));
                            break;
                        case ClassIDType.Sprite:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new Sprite(objectReader));
                            break;
                        case ClassIDType.SpriteAtlas:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new SpriteAtlas(objectReader));
                            break;
                        case ClassIDType.TextAsset:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new TextAsset(objectReader));
                            break;
                        case ClassIDType.Texture2D:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new Texture2D(objectReader));
                            break;
                        case ClassIDType.Transform:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new Transform(objectReader));
                            break;
                        case ClassIDType.VideoClip:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new VideoClip(objectReader));
                            break;
                        default:
                            assetsFile.Objects.Add(objectInfo.m_PathID, new Object(objectReader));
                            break;
                    }

                    Progress.Report(++i, progressCount);
                }
            }
        }

        private void ProcessAssets()
        {
            Logger.Info("Process Assets...");

            foreach (var assetsFile in assetsFileList)
            {
                foreach (var obj in assetsFile.Objects.Values)
                {
                    if (obj is GameObject m_GameObject)
                    {
                        foreach (var pptr in m_GameObject.m_Components)
                        {
                            if (pptr.TryGet(out var m_Component))
                            {
                                switch (m_Component)
                                {
                                    case Transform m_Transform:
                                        m_GameObject.m_Transform = m_Transform;
                                        break;
                                    case MeshRenderer m_MeshRenderer:
                                        m_GameObject.m_MeshRenderer = m_MeshRenderer;
                                        break;
                                    case MeshFilter m_MeshFilter:
                                        m_GameObject.m_MeshFilter = m_MeshFilter;
                                        break;
                                    case SkinnedMeshRenderer m_SkinnedMeshRenderer:
                                        m_GameObject.m_SkinnedMeshRenderer = m_SkinnedMeshRenderer;
                                        break;
                                    case Animator m_Animator:
                                        m_GameObject.m_Animator = m_Animator;
                                        break;
                                    case Animation m_Animation:
                                        m_GameObject.m_Animation = m_Animation;
                                        break;
                                }
                            }
                        }
                    }
                    else if (obj is SpriteAtlas m_SpriteAtlas)
                    {
                        foreach (var m_PackedSprite in m_SpriteAtlas.m_PackedSprites)
                        {
                            if (m_PackedSprite.TryGet(out var m_Sprite))
                            {
                                if (m_Sprite.m_SpriteAtlas.IsNull)
                                {
                                    m_Sprite.m_SpriteAtlas.Set(m_SpriteAtlas);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}