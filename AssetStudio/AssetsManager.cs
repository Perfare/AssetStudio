using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AssetStudio.ImportHelper;

namespace AssetStudio
{
    public class AssetsManager
    {
        public List<SerializedFile> assetsFileList = new List<SerializedFile>();
        internal Dictionary<string, int> assetsFileIndexCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        internal Dictionary<string, BinaryReader> resourceFileReaders = new Dictionary<string, BinaryReader>(StringComparer.OrdinalIgnoreCase);

        private List<string> importFiles = new List<string>();
        private HashSet<string> importFilesHash = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> assetsFileListHash = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                importFilesHash.Add(Path.GetFileName(file));
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
            if (!assetsFileListHash.Contains(fileName))
            {
                Logger.Info($"Loading {fileName}");
                try
                {
                    var assetsFile = new SerializedFile(this, fullName, reader);
                    assetsFileList.Add(assetsFile);
                    assetsFileListHash.Add(assetsFile.fileName);

                    foreach (var sharedFile in assetsFile.m_Externals)
                    {
                        var sharedFilePath = Path.Combine(Path.GetDirectoryName(fullName), sharedFile.fileName);
                        var sharedFileName = sharedFile.fileName;

                        if (!importFilesHash.Contains(sharedFileName))
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
                                importFilesHash.Add(sharedFileName);
                            }
                        }
                    }
                }
                catch
                {
                    reader.Dispose();
                    //Logger.Warning($"Unable to load assets file {fileName}");
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
            if (!assetsFileListHash.Contains(fileName))
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
                    assetsFileListHash.Add(assetsFile.fileName);
                }
                catch
                {
                    //Logger.Error($"Unable to load assets file {fileName} from {Path.GetFileName(originalPath)}");
                    resourceFileReaders.Add(fileName, reader);
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
                    var subReader = new EndianBinaryReader(file.stream);
                    if (SerializedFile.IsSerializedFile(subReader))
                    {
                        var dummyPath = Path.GetDirectoryName(fullName) + Path.DirectorySeparatorChar + file.fileName;
                        LoadAssetsFromMemory(dummyPath, subReader, parentPath ?? fullName, bundleFile.m_Header.unityRevision);
                    }
                    else
                    {
                        resourceFileReaders.Add(file.fileName, subReader);
                    }
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
                    var dummyPath = Path.Combine(Path.GetDirectoryName(fullName), file.fileName);
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
                        case FileType.ResourceFile:
                            resourceFileReaders.Add(file.fileName, fileReader);
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
                foreach (var objectInfo in assetsFile.m_Objects)
                {
                    var objectReader = new ObjectReader(assetsFile.reader, assetsFile, objectInfo);
                    try
                    {
                        Object obj;
                        switch (objectReader.type)
                        {
                            case ClassIDType.Animation:
                                obj = new Animation(objectReader);
                                break;
                            case ClassIDType.AnimationClip:
                                obj = new AnimationClip(objectReader);
                                break;
                            case ClassIDType.Animator:
                                obj = new Animator(objectReader);
                                break;
                            case ClassIDType.AnimatorController:
                                obj = new AnimatorController(objectReader);
                                break;
                            case ClassIDType.AnimatorOverrideController:
                                obj = new AnimatorOverrideController(objectReader);
                                break;
                            case ClassIDType.AssetBundle:
                                obj = new AssetBundle(objectReader);
                                break;
                            case ClassIDType.AudioClip:
                                obj = new AudioClip(objectReader);
                                break;
                            case ClassIDType.Avatar:
                                obj = new Avatar(objectReader);
                                break;
                            case ClassIDType.Font:
                                obj = new Font(objectReader);
                                break;
                            case ClassIDType.GameObject:
                                obj = new GameObject(objectReader);
                                break;
                            case ClassIDType.Material:
                                obj = new Material(objectReader);
                                break;
                            case ClassIDType.Mesh:
                                obj = new Mesh(objectReader);
                                break;
                            case ClassIDType.MeshFilter:
                                obj = new MeshFilter(objectReader);
                                break;
                            case ClassIDType.MeshRenderer:
                                obj = new MeshRenderer(objectReader);
                                break;
                            case ClassIDType.MonoBehaviour:
                                obj = new MonoBehaviour(objectReader);
                                break;
                            case ClassIDType.MonoScript:
                                obj = new MonoScript(objectReader);
                                break;
                            case ClassIDType.MovieTexture:
                                obj = new MovieTexture(objectReader);
                                break;
                            case ClassIDType.PlayerSettings:
                                obj = new PlayerSettings(objectReader);
                                break;
                            case ClassIDType.RectTransform:
                                obj = new RectTransform(objectReader);
                                break;
                            case ClassIDType.Shader:
                                obj = new Shader(objectReader);
                                break;
                            case ClassIDType.SkinnedMeshRenderer:
                                obj = new SkinnedMeshRenderer(objectReader);
                                break;
                            case ClassIDType.Sprite:
                                obj = new Sprite(objectReader);
                                break;
                            case ClassIDType.SpriteAtlas:
                                obj = new SpriteAtlas(objectReader);
                                break;
                            case ClassIDType.TextAsset:
                                obj = new TextAsset(objectReader);
                                break;
                            case ClassIDType.Texture2D:
                                obj = new Texture2D(objectReader);
                                break;
                            case ClassIDType.Transform:
                                obj = new Transform(objectReader);
                                break;
                            case ClassIDType.VideoClip:
                                obj = new VideoClip(objectReader);
                                break;
                            case ClassIDType.ResourceManager:
                                obj = new ResourceManager(objectReader);
                                break;
                            default:
                                obj = new Object(objectReader);
                                break;
                        }
                        assetsFile.AddObject(obj);
                    }
                    catch (Exception e)
                    {
                        /*var sb = new StringBuilder();
                        sb.AppendLine("Unable to load object")
                            .AppendLine($"Assets {assetsFile.fileName}")
                            .AppendLine($"Type {objectReader.type}")
                            .AppendLine($"PathID {objectInfo.m_PathID}")
                            .Append(e);
                        Logger.Error(sb.ToString());*/
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
                foreach (var obj in assetsFile.Objects)
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