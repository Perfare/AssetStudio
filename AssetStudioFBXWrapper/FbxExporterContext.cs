using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace AssetStudio.FbxInterop
{
    internal sealed partial class FbxExporterContext : IDisposable
    {

        private IntPtr _pContext;
        private readonly Dictionary<ImportedFrame, IntPtr> _frameToNode;
        private readonly List<KeyValuePair<string, IntPtr>> _createdMaterials;
        private readonly Dictionary<string, IntPtr> _createdTextures;

        public FbxExporterContext()
        {
            _pContext = AsFbxCreateContext();
            _frameToNode = new Dictionary<ImportedFrame, IntPtr>();
            _createdMaterials = new List<KeyValuePair<string, IntPtr>>();
            _createdTextures = new Dictionary<string, IntPtr>();
        }

        ~FbxExporterContext()
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
            IsDisposed = true;

            _frameToNode.Clear();
            _createdMaterials.Clear();
            _createdTextures.Clear();

            AsFbxDisposeContext(ref _pContext);
        }

        private void EnsureNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FbxExporterContext));
            }
        }

        internal void Initialize(string fileName, float scaleFactor, int versionIndex, bool isAscii, bool is60Fps)
        {
            EnsureNotDisposed();

            var b = AsFbxInitializeContext(_pContext, fileName, scaleFactor, versionIndex, isAscii, is60Fps, out var errorMessage);

            if (!b)
            {
                var fullMessage = $"Failed to initialize FbxExporter: {errorMessage}";
                throw new ApplicationException(fullMessage);
            }
        }

        internal void SetFramePaths(HashSet<string> framePaths)
        {
            EnsureNotDisposed();

            if (framePaths == null || framePaths.Count == 0)
            {
                return;
            }

            var framePathList = new List<string>(framePaths);
            var framePathArray = framePathList.ToArray();

            AsFbxSetFramePaths(_pContext, framePathArray);
        }

        internal void ExportScene()
        {
            EnsureNotDisposed();

            AsFbxExportScene(_pContext);
        }

        internal void ExportFrame(List<ImportedMesh> meshList, List<ImportedFrame> meshFrames, ImportedFrame rootFrame)
        {
            var rootNode = AsFbxGetSceneRootNode(_pContext);

            Debug.Assert(rootNode != IntPtr.Zero);

            var nodeStack = new Stack<IntPtr>();
            var frameStack = new Stack<ImportedFrame>();

            nodeStack.Push(rootNode);
            frameStack.Push(rootFrame);

            while (nodeStack.Count > 0)
            {
                var parentNode = nodeStack.Pop();
                var frame = frameStack.Pop();

                var childNode = AsFbxExportSingleFrame(_pContext, parentNode, frame.Path, frame.Name, frame.LocalPosition, frame.LocalRotation, frame.LocalScale);

                if (meshList != null && ImportedHelpers.FindMesh(frame.Path, meshList) != null)
                {
                    meshFrames.Add(frame);
                }

                _frameToNode.Add(frame, childNode);

                for (var i = frame.Count - 1; i >= 0; i -= 1)
                {
                    nodeStack.Push(childNode);
                    frameStack.Push(frame[i]);
                }
            }
        }

        internal void SetJointsNode(ImportedFrame rootFrame, HashSet<string> bonePaths, bool castToBone, float boneSize)
        {
            var frameStack = new Stack<ImportedFrame>();

            frameStack.Push(rootFrame);

            while (frameStack.Count > 0)
            {
                var frame = frameStack.Pop();

                if (_frameToNode.TryGetValue(frame, out var node))
                {
                    Debug.Assert(node != IntPtr.Zero);

                    if (castToBone)
                    {
                        AsFbxSetJointsNode_CastToBone(_pContext, node, boneSize);
                    }
                    else
                    {
                        Debug.Assert(bonePaths != null);

                        if (bonePaths.Contains(frame.Path))
                        {
                            AsFbxSetJointsNode_BoneInPath(_pContext, node, boneSize);
                        }
                        else
                        {
                            AsFbxSetJointsNode_Generic(_pContext, node);
                        }
                    }
                }

                for (var i = frame.Count - 1; i >= 0; i -= 1)
                {
                    frameStack.Push(frame[i]);
                }
            }
        }

        internal void PrepareMaterials(int materialCount, int textureCount)
        {
            AsFbxPrepareMaterials(_pContext, materialCount, textureCount);
        }

        internal void ExportMeshFromFrame(ImportedFrame rootFrame, ImportedFrame meshFrame, List<ImportedMesh> meshList, List<ImportedMaterial> materialList, List<ImportedTexture> textureList, bool exportSkins)
        {
            var meshNode = _frameToNode[meshFrame];
            var mesh = ImportedHelpers.FindMesh(meshFrame.Path, meshList);

            ExportMesh(rootFrame, materialList, textureList, meshNode, mesh, exportSkins);
        }

        private IntPtr ExportTexture(ImportedTexture texture)
        {
            if (texture == null)
            {
                return IntPtr.Zero;
            }

            if (_createdTextures.ContainsKey(texture.Name))
            {
                return _createdTextures[texture.Name];
            }

            var pTex = AsFbxCreateTexture(_pContext, texture.Name);

            _createdTextures.Add(texture.Name, pTex);

            var file = new FileInfo(texture.Name);

            using (var writer = new BinaryWriter(file.Create()))
            {
                writer.Write(texture.Data);
            }

            return pTex;
        }

        private void ExportMesh(ImportedFrame rootFrame, List<ImportedMaterial> materialList, List<ImportedTexture> textureList, IntPtr frameNode, ImportedMesh importedMesh, bool exportSkins)
        {
            var boneList = importedMesh.BoneList;
            var totalBoneCount = 0;
            var hasBones = false;
            if (exportSkins && boneList?.Count > 0)
            {
                totalBoneCount = boneList.Count;
                hasBones = true;
            }

            var pClusterArray = IntPtr.Zero;

            try
            {
                if (hasBones)
                {
                    pClusterArray = AsFbxMeshCreateClusterArray(totalBoneCount);

                    foreach (var bone in boneList)
                    {
                        if (bone.Path != null)
                        {
                            var frame = rootFrame.FindFrameByPath(bone.Path);
                            var boneNode = _frameToNode[frame];

                            var cluster = AsFbxMeshCreateCluster(_pContext, boneNode);

                            AsFbxMeshAddCluster(pClusterArray, cluster);
                        }
                        else
                        {
                            AsFbxMeshAddCluster(pClusterArray, IntPtr.Zero);
                        }
                    }
                }

                var mesh = AsFbxMeshCreateMesh(_pContext, frameNode);

                var totalVertexCount = 0;

                foreach (var m in importedMesh.SubmeshList)
                {
                    totalVertexCount += m.VertexList.Count;
                }

                AsFbxMeshInitControlPoints(mesh, totalVertexCount);

                if (importedMesh.hasNormal)
                {
                    AsFbxMeshCreateElementNormal(mesh);
                }

                if (importedMesh.hasUV[0])
                {
                    AsFbxMeshCreateDiffuseUV(mesh, 0);
                }

                if (importedMesh.hasUV[1])
                {
                    AsFbxMeshCreateNormalMapUV(mesh, 1);
                }

                if (importedMesh.hasTangent)
                {
                    AsFbxMeshCreateElementTangent(mesh);
                }

                if (importedMesh.hasColor)
                {
                    AsFbxMeshCreateElementVertexColor(mesh);
                }

                AsFbxMeshCreateElementMaterial(mesh);

                var firstVertex = 0;

                foreach (var meshObj in importedMesh.SubmeshList)
                {
                    var materialIndex = 0;
                    var mat = ImportedHelpers.FindMaterial(meshObj.Material, materialList);

                    if (mat != null)
                    {
                        var foundMat = _createdMaterials.FindIndex(kv => kv.Key == mat.Name);
                        IntPtr pMat;

                        if (foundMat >= 0)
                        {
                            pMat = _createdMaterials[foundMat].Value;
                        }
                        else
                        {
                            var diffuse = mat.Diffuse;
                            var ambient = mat.Ambient;
                            var emissive = mat.Emissive;
                            var specular = mat.Specular;
                            var reflection = mat.Reflection;

                            pMat = AsFbxCreateMaterial(_pContext, mat.Name, in diffuse, in ambient, in emissive, in specular, in reflection, mat.Shininess, mat.Transparency);

                            _createdMaterials.Add(new KeyValuePair<string, IntPtr>(mat.Name, pMat));
                        }

                        materialIndex = AsFbxAddMaterialToFrame(frameNode, pMat);

                        var hasTexture = false;

                        foreach (var texture in mat.Textures)
                        {
                            var tex = ImportedHelpers.FindTexture(texture.Name, textureList);
                            var pTexture = ExportTexture(tex);

                            if (pTexture != IntPtr.Zero)
                            {
                                switch (texture.Dest)
                                {
                                    case 0:
                                    case 1:
                                    case 2:
                                    case 3:
                                        {
                                            AsFbxLinkTexture(texture.Dest, pTexture, pMat, texture.Offset.X, texture.Offset.Y, texture.Scale.X, texture.Scale.Y);
                                            hasTexture = true;
                                            break;
                                        }
                                    default:
                                        break;
                                }
                            }
                        }

                        if (hasTexture)
                        {
                            AsFbxSetFrameShadingModeToTextureShading(frameNode);
                        }
                    }

                    var vertexList = meshObj.VertexList;

                    var vertexCount = vertexList.Count;

                    for (var j = 0; j < vertexCount; j += 1)
                    {
                        var importedVertex = vertexList[j];

                        var vertex = importedVertex.Vertex;
                        AsFbxMeshSetControlPoint(mesh, j + firstVertex, vertex.X, vertex.Y, vertex.Z);

                        if (importedMesh.hasNormal)
                        {
                            var normal = importedVertex.Normal;
                            AsFbxMeshElementNormalAdd(mesh, 0, normal.X, normal.Y, normal.Z);
                        }

                        for (var uvIndex = 0; uvIndex < 2; uvIndex += 1)
                        {
                            if (importedMesh.hasUV[uvIndex])
                            {
                                var uv = importedVertex.UV[uvIndex];
                                AsFbxMeshElementUVAdd(mesh, uvIndex, uv[0], uv[1]);
                            }
                        }

                        if (importedMesh.hasTangent)
                        {
                            var tangent = importedVertex.Tangent;
                            AsFbxMeshElementTangentAdd(mesh, 0, tangent.X, tangent.Y, tangent.Z, tangent.W);
                        }

                        if (importedMesh.hasColor)
                        {
                            var color = importedVertex.Color;
                            AsFbxMeshElementVertexColorAdd(mesh, 0, color.R, color.G, color.B, color.A);
                        }

                        if (hasBones && importedVertex.BoneIndices != null)
                        {
                            var boneIndices = importedVertex.BoneIndices;
                            var boneWeights = importedVertex.Weights;

                            for (var k = 0; k < 4; k += 1)
                            {
                                if (boneIndices[k] < totalBoneCount && boneWeights[k] > 0)
                                {
                                    AsFbxMeshSetBoneWeight(pClusterArray, boneIndices[k], j + firstVertex, boneWeights[k]);
                                }
                            }
                        }
                    }

                    foreach (var face in meshObj.FaceList)
                    {
                        var index0 = face.VertexIndices[0] + firstVertex;
                        var index1 = face.VertexIndices[1] + firstVertex;
                        var index2 = face.VertexIndices[2] + firstVertex;

                        AsFbxMeshAddPolygon(mesh, materialIndex, index0, index1, index2);
                    }

                    firstVertex += vertexCount;
                }

                if (hasBones)
                {
                    IntPtr pSkinContext = IntPtr.Zero;

                    try
                    {
                        pSkinContext = AsFbxMeshCreateSkinContext(_pContext, frameNode);

                        unsafe
                        {
                            var boneMatrix = stackalloc float[16];

                            for (var j = 0; j < totalBoneCount; j += 1)
                            {
                                if (!FbxClusterArray_HasItemAt(pClusterArray, j))
                                {
                                    continue;
                                }

                                var m = boneList[j].Matrix;

                                CopyMatrix4x4(in m, boneMatrix);

                                AsFbxMeshSkinAddCluster(pSkinContext, pClusterArray, j, boneMatrix);
                            }
                        }

                        AsFbxMeshAddDeformer(pSkinContext, mesh);
                    }
                    finally
                    {
                        AsFbxMeshDisposeSkinContext(ref pSkinContext);
                    }
                }
            }
            finally
            {
                AsFbxMeshDisposeClusterArray(ref pClusterArray);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void CopyMatrix4x4(in Matrix4x4 matrix, float* buffer)
        {
            for (var m = 0; m < 4; m += 1)
            {
                for (var n = 0; n < 4; n += 1)
                {
                    var index = IndexFrom4x4(m, n);
                    buffer[index] = matrix[m, n];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int IndexFrom4x4(int m, int n)
        {
            return 4 * m + n;
        }

        internal void ExportAnimations(ImportedFrame rootFrame, List<ImportedKeyframedAnimation> animationList, bool eulerFilter, float filterPrecision)
        {
            if (animationList == null || animationList.Count == 0)
            {
                return;
            }

            var pAnimContext = IntPtr.Zero;

            try
            {
                pAnimContext = AsFbxAnimCreateContext(eulerFilter);

                for (int i = 0; i < animationList.Count; i++)
                {
                    var importedAnimation = animationList[i];
                    string takeName;

                    if (importedAnimation.Name != null)
                    {
                        takeName = importedAnimation.Name;
                    }
                    else
                    {
                        takeName = $"Take{i.ToString()}";
                    }

                    AsFbxAnimPrepareStackAndLayer(_pContext, pAnimContext, takeName);

                    ExportKeyframedAnimation(rootFrame, importedAnimation, pAnimContext, filterPrecision);
                }
            }
            finally
            {
                AsFbxAnimDisposeContext(ref pAnimContext);
            }
        }

        private void ExportKeyframedAnimation(ImportedFrame rootFrame, ImportedKeyframedAnimation parser, IntPtr pAnimContext, float filterPrecision)
        {
            foreach (var track in parser.TrackList)
            {
                if (track.Path == null)
                {
                    continue;
                }

                var frame = rootFrame.FindFrameByPath(track.Path);

                if (frame == null)
                {
                    continue;
                }

                var pNode = _frameToNode[frame];

                AsFbxAnimLoadCurves(pNode, pAnimContext);

                AsFbxAnimBeginKeyModify(pAnimContext);

                foreach (var scaling in track.Scalings)
                {
                    var value = scaling.value;
                    AsFbxAnimAddScalingKey(pAnimContext, scaling.time, value.X, value.Y, value.Z);
                }

                foreach (var rotation in track.Rotations)
                {
                    var value = rotation.value;
                    AsFbxAnimAddRotationKey(pAnimContext, rotation.time, value.X, value.Y, value.Z);
                }

                foreach (var translation in track.Translations)
                {
                    var value = translation.value;
                    AsFbxAnimAddTranslationKey(pAnimContext, translation.time, value.X, value.Y, value.Z);
                }

                AsFbxAnimEndKeyModify(pAnimContext);

                AsFbxAnimApplyEulerFilter(pAnimContext, filterPrecision);

                var blendShape = track.BlendShape;

                if (blendShape != null)
                {
                    var channelCount = AsFbxAnimGetCurrentBlendShapeChannelCount(pAnimContext, pNode);

                    if (channelCount > 0)
                    {
                        for (var channelIndex = 0; channelIndex < channelCount; channelIndex += 1)
                        {
                            if (!AsFbxAnimIsBlendShapeChannelMatch(pAnimContext, channelIndex, blendShape.ChannelName))
                            {
                                continue;
                            }

                            AsFbxAnimBeginBlendShapeAnimCurve(pAnimContext, channelIndex);

                            foreach (var keyframe in blendShape.Keyframes)
                            {
                                AsFbxAnimAddBlendShapeKeyframe(pAnimContext, keyframe.time, keyframe.value);
                            }

                            AsFbxAnimEndBlendShapeAnimCurve(pAnimContext);
                        }
                    }
                }
            }
        }

        internal void ExportMorphs(ImportedFrame rootFrame, List<ImportedMorph> morphList)
        {
            if (morphList == null || morphList.Count == 0)
            {
                return;
            }

            foreach (var morph in morphList)
            {
                var frame = rootFrame.FindFrameByPath(morph.Path);

                if (frame == null)
                {
                    continue;
                }

                var pNode = _frameToNode[frame];

                var pMorphContext = IntPtr.Zero;

                try
                {
                    pMorphContext = AsFbxMorphCreateContext();

                    AsFbxMorphInitializeContext(_pContext, pMorphContext, pNode);

                    foreach (var channel in morph.Channels)
                    {
                        AsFbxMorphAddBlendShapeChannel(_pContext, pMorphContext, channel.Name);

                        for (var i = 0; i < channel.KeyframeList.Count; i++)
                        {
                            var keyframe = channel.KeyframeList[i];

                            AsFbxMorphAddBlendShapeChannelShape(_pContext, pMorphContext, keyframe.Weight, i == 0 ? channel.Name : $"{channel.Name}_{i + 1}");

                            AsFbxMorphCopyBlendShapeControlPoints(pMorphContext);

                            foreach (var vertex in keyframe.VertexList)
                            {
                                var v = vertex.Vertex.Vertex;
                                AsFbxMorphSetBlendShapeVertex(pMorphContext, vertex.Index, v.X, v.Y, v.Z);
                            }

                            if (keyframe.hasNormals)
                            {
                                AsFbxMorphCopyBlendShapeControlPointsNormal(pMorphContext);

                                foreach (var vertex in keyframe.VertexList)
                                {
                                    var v = vertex.Vertex.Normal;
                                    AsFbxMorphSetBlendShapeVertexNormal(pMorphContext, vertex.Index, v.X, v.Y, v.Z);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    AsFbxMorphDisposeContext(ref pMorphContext);
                }
            }
        }

    }
}