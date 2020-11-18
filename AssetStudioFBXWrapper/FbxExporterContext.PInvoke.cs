using System;
using System.Runtime.InteropServices;
using AssetStudio.PInvoke;

namespace AssetStudio.FbxInterop
{
    partial class FbxExporterContext
    {

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr AsFbxCreateContext();

        private static bool AsFbxInitializeContext(IntPtr context, string fileName, float scaleFactor, int versionIndex, bool isAscii, bool is60Fps, out string errorMessage)
        {
            bool b;
            IntPtr pErrMsg;

            using (var fileNameUtf8 = new Utf8StringHandle(fileName))
            {
                b = AsFbxInitializeContext(context, fileNameUtf8.DangerousGetHandle(), scaleFactor, versionIndex, isAscii, is60Fps, out pErrMsg);
            }

            errorMessage = Utf8StringHandle.ReadUtf8StringFromPointer(pErrMsg);

            return b;
        }

        // Do not free the pointer strErrorMessage
        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AsFbxInitializeContext(IntPtr context, IntPtr strFileName, float scaleFactor, int versionIndex, [MarshalAs(UnmanagedType.Bool)] bool isAscii, [MarshalAs(UnmanagedType.Bool)] bool is60Fps, out IntPtr strErrorMessage);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxDisposeContext(ref IntPtr ppContext);

        private static void AsFbxSetFramePaths(IntPtr context, string[] framePaths)
        {
            var framePathCount = framePaths.Length;

            if (framePathCount == 0)
            {
                AsFbxSetFramePaths(context, Array.Empty<IntPtr>(), 0);
            }
            else
            {
                var utf8Paths = new Utf8StringHandle[framePathCount];

                try
                {
                    for (var i = 0; i < framePathCount; i += 1)
                    {
                        utf8Paths[i] = new Utf8StringHandle(framePaths[i]);
                    }

                    var pathPointers = new IntPtr[framePathCount];

                    for (var i = 0; i < framePathCount; i += 1)
                    {
                        pathPointers[i] = utf8Paths[i].DangerousGetHandle();
                    }

                    AsFbxSetFramePaths(context, pathPointers, framePathCount);
                }
                finally
                {
                    foreach (var path in utf8Paths)
                    {
                        path?.Dispose();
                    }
                }
            }
        }

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxSetFramePaths(IntPtr context, [MarshalAs(UnmanagedType.LPArray)] IntPtr[] strFramePaths, int count);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxExportScene(IntPtr context);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr AsFbxGetSceneRootNode(IntPtr context);

        private static IntPtr AsFbxExportSingleFrame(IntPtr context, IntPtr parentNode, string framePath, string frameName, in Vector3 localPosition, in Vector3 localRotation, in Vector3 localScale)
        {
            using (var framePathUtf8 = new Utf8StringHandle(framePath))
            {
                using (var frameNameUtf8 = new Utf8StringHandle(frameName))
                {
                    return AsFbxExportSingleFrame(context, parentNode, framePathUtf8.DangerousGetHandle(), frameNameUtf8.DangerousGetHandle(), localPosition.X, localPosition.Y, localPosition.Z, localRotation.X, localRotation.Y, localRotation.Z, localScale.X, localScale.Y, localScale.Z);
                }
            }
        }

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr AsFbxExportSingleFrame(IntPtr context, IntPtr parentNode, IntPtr strFramePath, IntPtr strFrameName, float localPositionX, float localPositionY, float localPositionZ, float localRotationX, float localRotationY, float localRotationZ, float localScaleX, float localScaleY, float localScaleZ);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxSetJointsNode_CastToBone(IntPtr context, IntPtr node, float boneSize);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxSetJointsNode_BoneInPath(IntPtr context, IntPtr node, float boneSize);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxSetJointsNode_Generic(IntPtr context, IntPtr node);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxPrepareMaterials(IntPtr context, int materialCount, int textureCount);

        private static IntPtr AsFbxCreateTexture(IntPtr context, string matTexName)
        {
            using (var matTexNameUtf8 = new Utf8StringHandle(matTexName))
            {
                return AsFbxCreateTexture(context, matTexNameUtf8.DangerousGetHandle());
            }
        }

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr AsFbxCreateTexture(IntPtr context, IntPtr strMatTexName);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxLinkTexture(int dest, IntPtr texture, IntPtr material, float offsetX, float offsetY, float scaleX, float scaleY);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr AsFbxMeshCreateClusterArray(int boneCount);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshDisposeClusterArray(ref IntPtr ppArray);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr AsFbxMeshCreateCluster(IntPtr context, IntPtr boneNode);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshAddCluster(IntPtr array, IntPtr cluster);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr AsFbxMeshCreateMesh(IntPtr context, IntPtr frameNode);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshInitControlPoints(IntPtr mesh, int vertexCount);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshCreateElementNormal(IntPtr mesh);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshCreateDiffuseUV(IntPtr mesh, int uv);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshCreateNormalMapUV(IntPtr mesh, int uv);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshCreateElementTangent(IntPtr mesh);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshCreateElementVertexColor(IntPtr mesh);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshCreateElementMaterial(IntPtr mesh);

        private static IntPtr AsFbxCreateMaterial(IntPtr pContext, string matName, in Color diffuse, in Color ambient, in Color emissive, in Color specular, in Color reflection, float shininess, float transparency)
        {
            using (var matNameUtf8 = new Utf8StringHandle(matName))
            {
                return AsFbxCreateMaterial(pContext, matNameUtf8.DangerousGetHandle(), diffuse.R, diffuse.G, diffuse.B, ambient.R, ambient.G, ambient.B, emissive.R, emissive.G, emissive.B, specular.R, specular.G, specular.B, reflection.R, reflection.G, reflection.B, shininess, transparency);
            }
        }

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr AsFbxCreateMaterial(IntPtr pContext, IntPtr pMatName,
            float diffuseR, float diffuseG, float diffuseB,
            float ambientR, float ambientG, float ambientB,
            float emissiveR, float emissiveG, float emissiveB,
            float specularR, float specularG, float specularB,
            float reflectR, float reflectG, float reflectB,
            float shininess, float transparency);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern int AsFbxAddMaterialToFrame(IntPtr frameNode, IntPtr material);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxSetFrameShadingModeToTextureShading(IntPtr frameNode);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshSetControlPoint(IntPtr mesh, int index, float x, float y, float z);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshAddPolygon(IntPtr mesh, int materialIndex, int index0, int index1, int index2);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshElementNormalAdd(IntPtr mesh, int elementIndex, float x, float y, float z);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshElementUVAdd(IntPtr mesh, int elementIndex, float u, float v);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshElementTangentAdd(IntPtr mesh, int elementIndex, float x, float y, float z, float w);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshElementVertexColorAdd(IntPtr mesh, int elementIndex, float r, float g, float b, float a);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshSetBoneWeight(IntPtr pClusterArray, int boneIndex, int vertexIndex, float weight);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr AsFbxMeshCreateSkinContext(IntPtr context, IntPtr frameNode);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshDisposeSkinContext(ref IntPtr ppSkinContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FbxClusterArray_HasItemAt(IntPtr pClusterArray, int index);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private unsafe static extern void AsFbxMeshSkinAddCluster(IntPtr pSkinContext, IntPtr pClusterArray, int index, float* pBoneMatrix);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMeshAddDeformer(IntPtr pSkinContext, IntPtr pMesh);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr AsFbxAnimCreateContext([MarshalAs(UnmanagedType.Bool)] bool eulerFilter);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxAnimDisposeContext(ref IntPtr ppAnimContext);

        private static void AsFbxAnimPrepareStackAndLayer(IntPtr pContext, IntPtr pAnimContext, string takeName)
        {
            using (var takeNameUtf8 = new Utf8StringHandle(takeName))
            {
                AsFbxAnimPrepareStackAndLayer(pContext, pAnimContext, takeNameUtf8.DangerousGetHandle());
            }
        }

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxAnimPrepareStackAndLayer(IntPtr pContext, IntPtr pAnimContext, IntPtr strTakeName);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxAnimLoadCurves(IntPtr pNode, IntPtr pAnimContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxAnimBeginKeyModify(IntPtr pAnimContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxAnimEndKeyModify(IntPtr pAnimContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxAnimAddScalingKey(IntPtr pAnimContext, float time, float x, float y, float z);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxAnimAddRotationKey(IntPtr pAnimContext, float time, float x, float y, float z);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxAnimAddTranslationKey(IntPtr pAnimContext, float time, float x, float y, float z);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxAnimApplyEulerFilter(IntPtr pAnimContext, float filterPrecision);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern int AsFbxAnimGetCurrentBlendShapeChannelCount(IntPtr pAnimContext, IntPtr pNode);

        private static bool AsFbxAnimIsBlendShapeChannelMatch(IntPtr pAnimContext, int channelIndex, string channelName)
        {
            using (var channelNameUtf8 = new Utf8StringHandle(channelName))
            {
                return AsFbxAnimIsBlendShapeChannelMatch(pAnimContext, channelIndex, channelNameUtf8.DangerousGetHandle());
            }
        }

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AsFbxAnimIsBlendShapeChannelMatch(IntPtr pAnimContext, int channelIndex, IntPtr strChannelName);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxAnimBeginBlendShapeAnimCurve(IntPtr pAnimContext, int channelIndex);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxAnimEndBlendShapeAnimCurve(IntPtr pAnimContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxAnimAddBlendShapeKeyframe(IntPtr pAnimContext, float time, float value);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr AsFbxMorphCreateContext();

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMorphInitializeContext(IntPtr pContext, IntPtr pMorphContext, IntPtr pNode);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMorphDisposeContext(ref IntPtr ppMorphContext);

        private static void AsFbxMorphAddBlendShapeChannel(IntPtr pContext, IntPtr pMorphContext, string channelName)
        {
            using (var channelNameUtf8 = new Utf8StringHandle(channelName))
            {
                AsFbxMorphAddBlendShapeChannel(pContext, pMorphContext, channelNameUtf8.DangerousGetHandle());
            }
        }

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMorphAddBlendShapeChannel(IntPtr pContext, IntPtr pMorphContext, IntPtr strChannelName);

        private static void AsFbxMorphAddBlendShapeChannelShape(IntPtr pContext, IntPtr pMorphContext, float weight, string shapeName)
        {
            using (var shapeNameUtf8 = new Utf8StringHandle(shapeName))
            {
                AsFbxMorphAddBlendShapeChannelShape(pContext, pMorphContext, weight, shapeNameUtf8.DangerousGetHandle());
            }
        }

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMorphAddBlendShapeChannelShape(IntPtr pContext, IntPtr pMorphContext, float weight, IntPtr strShapeName);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMorphCopyBlendShapeControlPoints(IntPtr pMorphContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMorphSetBlendShapeVertex(IntPtr pMorphContext, uint index, float x, float y, float z);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMorphCopyBlendShapeControlPointsNormal(IntPtr pMorphContext);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsFbxMorphSetBlendShapeVertexNormal(IntPtr pMorphContext, uint index, float x, float y, float z);

    }
}
