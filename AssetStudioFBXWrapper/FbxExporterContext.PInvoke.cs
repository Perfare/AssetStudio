using System;
using System.Runtime.InteropServices;

namespace AssetStudio.FbxInterop
{
    partial class FbxExporterContext
    {

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr AsFbxCreateContext();

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AsFbxInitializeContext(IntPtr context, [MarshalAs(UnmanagedType.LPStr)] string fileName, float scaleFactor, int versionIndex, [MarshalAs(UnmanagedType.Bool)] bool isAscii, [MarshalAs(UnmanagedType.Bool)] bool is60Fps, [MarshalAs(UnmanagedType.LPTStr), Out] out string errorMessage);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxDisposeContext(ref IntPtr ppContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern void AsFbxSetFramePaths(IntPtr context, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPTStr)] string[] framePaths, int count);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxExportScene(IntPtr context);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr AsFbxGetSceneRootNode(IntPtr context);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern IntPtr AsFbxExportSingleFrame(IntPtr context, IntPtr parentNode, [MarshalAs(UnmanagedType.LPStr)] string framePath, [MarshalAs(UnmanagedType.LPStr)] string frameName, float localPositionX, float localPositionY, float localPositionZ, float localRotationX, float localRotationY, float localRotationZ, float localScaleX, float localScaleY, float localScaleZ);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxSetJointsNode_CastToBone(IntPtr context, IntPtr node, float boneSize);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxSetJointsNode_BoneInPath(IntPtr context, IntPtr node, float boneSize);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxSetJointsNode_Generic(IntPtr context, IntPtr node);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxPrepareMaterials(IntPtr context, int materialCount, int textureCount);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern IntPtr AsFbxCreateTexture(IntPtr context, [MarshalAs(UnmanagedType.LPStr)] string matTexName);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxLinkTexture(int dest, IntPtr texture, IntPtr material, float offsetX, float offsetY, float scaleX, float scaleY);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr AsFbxMeshCreateClusterArray(int boneCount);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshDisposeClusterArray(ref IntPtr ppArray);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr AsFbxMeshCreateCluster(IntPtr context, IntPtr boneNode);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshAddCluster(IntPtr array, IntPtr cluster);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr AsFbxMeshCreateMesh(IntPtr context, IntPtr frameNode);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshInitControlPoints(IntPtr mesh, int vertexCount);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshCreateElementNormal(IntPtr mesh);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshCreateElementUV(IntPtr mesh, int uv);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshCreateElementTangent(IntPtr mesh);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshCreateElementVertexColor(IntPtr mesh);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshCreateElementMaterial(IntPtr mesh);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern IntPtr AsFbxCreateMaterial(IntPtr pContext, [MarshalAs(UnmanagedType.LPStr)] string pMatName,
            float diffuseR, float diffuseG, float diffuseB,
            float ambientR, float ambientG, float ambientB,
            float emissiveR, float emissiveG, float emissiveB,
            float specularR, float specularG, float specularB,
            float reflectR, float reflectG, float reflectB,
            float shininess, float transparancy);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern int AsFbxAddMaterialToFrame(IntPtr frameNode, IntPtr material);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxSetFrameShadingModeToTextureShading(IntPtr frameNode);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshSetControlPoint(IntPtr mesh, int index, float x, float y, float z);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshAddPolygon(IntPtr mesh, int materialIndex, int index0, int index1, int index2);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshElementNormalAdd(IntPtr mesh, int elementIndex, float x, float y, float z);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshElementUVAdd(IntPtr mesh, int elementIndex, float u, float v);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshElementTangentAdd(IntPtr mesh, int elementIndex, float x, float y, float z, float w);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshElementVertexColorAdd(IntPtr mesh, int elementIndex, float r, float g, float b, float a);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshSetBoneWeight(IntPtr pClusterArray, int boneIndex, int vertexIndex, float weight);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr AsFbxMeshCreateSkinContext(IntPtr context, IntPtr frameNode);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshDisposeSkinContext(ref IntPtr ppSkinContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FbxClusterArray_HasItemAt(IntPtr pClusterArray, int index);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private unsafe static extern void AsFbxMeshSkinAddCluster(IntPtr pSkinContext, IntPtr pClusterArray, int index, float* pBoneMatrix);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMeshAddDeformer(IntPtr pSkinContext, IntPtr pMesh);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr AsFbxAnimCreateContext([MarshalAs(UnmanagedType.Bool)] bool eulerFilter);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxAnimDisposeContext(ref IntPtr ppAnimContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern void AsFbxAnimPrepareStackAndLayer(IntPtr pContext, IntPtr pAnimContext, [MarshalAs(UnmanagedType.LPStr)] string takeName);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxAnimLoadCurves(IntPtr pNode, IntPtr pAnimContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxAnimBeginKeyModify(IntPtr pAnimContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxAnimEndKeyModify(IntPtr pAnimContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxAnimAddScalingKey(IntPtr pAnimContext, float time, float x, float y, float z);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxAnimAddRotationKey(IntPtr pAnimContext, float time, float x, float y, float z);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxAnimAddTranslationKey(IntPtr pAnimContext, float time, float x, float y, float z);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxAnimApplyEulerFilter(IntPtr pAnimContext, float filterPrecision);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern int AsFbxAnimGetCurrentBlendShapeChannelCount(IntPtr pAnimContext, IntPtr pNode);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AsFbxAnimIsBlendShapeChannelMatch(IntPtr pAnimContext, int channelIndex, [MarshalAs(UnmanagedType.LPStr)] string channelName);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxAnimBeginBlendShapeAnimCurve(IntPtr pAnimContext, int channelIndex);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxAnimEndBlendShapeAnimCurve(IntPtr pAnimContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxAnimAddBlendShapeKeyframe(IntPtr pAnimContext, float time, float value);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr AsFbxMorphCreateContext();

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMorphInitializeContext(IntPtr pContext, IntPtr pMorphContext, IntPtr pNode);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMorphDisposeContext(ref IntPtr ppMorphContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern void AsFbxMorphAddBlendShapeChannel(IntPtr pContext, IntPtr pMorphContext, [MarshalAs(UnmanagedType.LPStr)] string channelName);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMorphAddBlendShapeChannelShape(IntPtr pContext, IntPtr pMorphContext, float weight);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMorphCopyBlendShapeControlPoints(IntPtr pMorphContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern void AsFbxMorphSetBlendShapeVertex(IntPtr pMorphContext, uint index, float x, float y, float z);

    }
}
