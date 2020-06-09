using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AssetStudio.FbxInterop
{
    internal sealed class FbxExporter : IDisposable
    {

        private FbxExporterContext _context;

        private readonly string _fileName;
        private readonly IImported _imported;
        private readonly bool _allNodes;
        private readonly bool _exportSkins;
        private readonly bool _castToBone;
        private readonly float _boneSize;
        private readonly float _scaleFactor;
        private readonly int _versionIndex;
        private readonly bool _isAscii;

        internal FbxExporter(string fileName, IImported imported, bool allNodes, bool exportSkins, bool castToBone, float boneSize, float scaleFactor, int versionIndex, bool isAscii)
        {
            _context = new FbxExporterContext();

            _fileName = fileName;
            _imported = imported;
            _allNodes = allNodes;
            _exportSkins = exportSkins;
            _castToBone = castToBone;
            _boneSize = boneSize;
            _scaleFactor = scaleFactor;
            _versionIndex = versionIndex;
            _isAscii = isAscii;
        }

        ~FbxExporter()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool IsDisposed { get; private set; }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }

            IsDisposed = true;
        }

        internal void Initialize()
        {
            var is60Fps = _imported.AnimationList.Count > 0 && _imported.AnimationList[0].SampleRate.Equals(60.0f);

            _context.Initialize(_fileName, _scaleFactor, _versionIndex, _isAscii, is60Fps);

            if (!_allNodes)
            {
                var framePaths = SearchHierarchy();

                _context.SetFramePaths(framePaths);
            }
        }

        internal void ExportAll(bool blendShape, bool animation, bool eulerFilter, float filterPrecision)
        {
            var meshFrames = new List<ImportedFrame>();

            ExportRootFrame(meshFrames);

            if (_imported.MeshList != null)
            {
                SetJointsFromImportedMeshes();

                PrepareMaterials();

                ExportMeshFrames(_imported.RootFrame, meshFrames);
            }
            else
            {
                SetJointsNode(_imported.RootFrame, null, true);
            }



            if (blendShape)
            {
                ExportMorphs();
            }

            if (animation)
            {
                ExportAnimations(eulerFilter, filterPrecision);
            }

            ExportScene();
        }

        private void ExportMorphs()
        {
            _context.ExportMorphs(_imported.RootFrame, _imported.MorphList);
        }

        private void ExportAnimations(bool eulerFilter, float filterPrecision)
        {
            _context.ExportAnimations(_imported.RootFrame, _imported.AnimationList, eulerFilter, filterPrecision);
        }

        private void ExportRootFrame(List<ImportedFrame> meshFrames)
        {
            _context.ExportFrame(_imported.MeshList, meshFrames, _imported.RootFrame);
        }

        private void ExportScene()
        {
            _context.ExportScene();
        }

        private void SetJointsFromImportedMeshes()
        {
            if (!_exportSkins)
            {
                return;
            }

            Debug.Assert(_imported.MeshList != null);

            var bonePaths = new HashSet<string>();

            foreach (var mesh in _imported.MeshList)
            {
                var boneList = mesh.BoneList;

                if (boneList != null)
                {
                    foreach (var bone in boneList)
                    {
                        bonePaths.Add(bone.Path);
                    }
                }
            }

            SetJointsNode(_imported.RootFrame, bonePaths, _castToBone);
        }

        private void SetJointsNode(ImportedFrame rootFrame, HashSet<string> bonePaths, bool castToBone)
        {
            _context.SetJointsNode(rootFrame, bonePaths, castToBone, _boneSize);
        }

        private void PrepareMaterials()
        {
            _context.PrepareMaterials(_imported.MaterialList.Count, _imported.TextureList.Count);
        }

        private void ExportMeshFrames(ImportedFrame rootFrame, List<ImportedFrame> meshFrames)
        {
            foreach (var meshFrame in meshFrames)
            {
                _context.ExportMeshFromFrame(rootFrame, meshFrame, _imported.MeshList, _imported.MaterialList, _imported.TextureList, _exportSkins);
            }
        }

        private HashSet<string> SearchHierarchy()
        {
            if (_imported.MeshList == null || _imported.MeshList.Count == 0)
            {
                return null;
            }

            var exportFrames = new HashSet<string>();

            SearchHierarchy(_imported.RootFrame, _imported.MeshList, exportFrames);

            return exportFrames;
        }

        private static void SearchHierarchy(ImportedFrame rootFrame, List<ImportedMesh> meshList, HashSet<string> exportFrames)
        {
            var frameStack = new Stack<ImportedFrame>();

            frameStack.Push(rootFrame);

            while (frameStack.Count > 0)
            {
                var frame = frameStack.Pop();

                var meshListSome = ImportedHelpers.FindMesh(frame.Path, meshList);

                if (meshListSome != null)
                {
                    var parent = frame;

                    while (parent != null)
                    {
                        exportFrames.Add(parent.Path);
                        parent = parent.Parent;
                    }

                    var boneList = meshListSome.BoneList;

                    if (boneList != null)
                    {
                        foreach (var bone in boneList)
                        {
                            if (!exportFrames.Contains(bone.Path))
                            {
                                var boneParent = rootFrame.FindFrameByPath(bone.Path);

                                while (boneParent != null)
                                {
                                    exportFrames.Add(boneParent.Path);
                                    boneParent = boneParent.Parent;
                                }
                            }
                        }
                    }
                }

                for (var i = frame.Count - 1; i >= 0; i -= 1)
                {
                    frameStack.Push(frame[i]);
                }
            }
        }

    }
}
