#pragma once

#include "dllexport.h"
#include "bool32_t.h"

namespace fbxsdk
{
	class FbxNode;
	class FbxFileTexture;
	template<typename T, const int Alignment = 16>
	class FbxArray;
	class FbxCluster;
	class FbxMesh;
	class FbxSurfacePhong;
}

struct AsFbxContext;
struct AsFbxSkinContext;
struct AsFbxAnimContext;
struct AsFbxMorphContext;

AS_API(void) AsUtilQuaternionToEuler(float qx, float qy, float qz, float qw, float* vx, float* vy, float* vz);

AS_API(void) AsUtilEulerToQuaternion(float vx, float vy, float vz, float* qx, float* qy, float* qz, float* qw);

// All strings ([const] char *) in this header are UTF-8 strings.

AS_API(AsFbxContext*) AsFbxCreateContext();

// Do not free pErrMsg
AS_API(bool32_t) AsFbxInitializeContext(AsFbxContext* pContext, const char* pFileName, float scaleFactor, int32_t versionIndex, bool32_t isAscii, bool32_t is60Fps, const char** pErrMsg);

AS_API(void) AsFbxDisposeContext(AsFbxContext** ppContext);

AS_API(void) AsFbxSetFramePaths(AsFbxContext* pContext, const char* ppPaths[], int32_t count);

AS_API(void) AsFbxExportScene(AsFbxContext* pContext);

AS_API(fbxsdk::FbxNode*) AsFbxGetSceneRootNode(AsFbxContext* pContext);

AS_API(fbxsdk::FbxNode*) AsFbxExportSingleFrame(AsFbxContext* pContext, fbxsdk::FbxNode* pParentNode, const char* pFramePath, const char* pFrameName, float localPositionX, float localPositionY, float localPositionZ, float localRotationX, float localRotationY, float localRotationZ, float localScaleX, float localScaleY, float localScaleZ);

AS_API(void) AsFbxSetJointsNode_CastToBone(AsFbxContext* pContext, fbxsdk::FbxNode* pNode, float boneSize);

AS_API(void) AsFbxSetJointsNode_BoneInPath(AsFbxContext* pContext, fbxsdk::FbxNode* pNode, float boneSize);

AS_API(void) AsFbxSetJointsNode_Generic(AsFbxContext* pContext, fbxsdk::FbxNode* pNode);

AS_API(void) AsFbxPrepareMaterials(AsFbxContext* pContext, int32_t materialCount, int32_t textureCount);

AS_API(fbxsdk::FbxFileTexture*) AsFbxCreateTexture(AsFbxContext* pContext, const char* pMatTexName);

AS_API(void) AsFbxLinkTexture(int32_t dest, fbxsdk::FbxFileTexture* pTexture, fbxsdk::FbxSurfacePhong* pMaterial, float offsetX, float offsetY, float scaleX, float scaleY);

AS_API(fbxsdk::FbxArray<fbxsdk::FbxCluster*>*) AsFbxMeshCreateClusterArray(int32_t boneCount);

AS_API(void) AsFbxMeshDisposeClusterArray(fbxsdk::FbxArray<fbxsdk::FbxCluster*>** ppArray);

AS_API(fbxsdk::FbxCluster*) AsFbxMeshCreateCluster(AsFbxContext* pContext, fbxsdk::FbxNode* pBoneNode);

AS_API(void) AsFbxMeshAddCluster(fbxsdk::FbxArray<fbxsdk::FbxCluster*>* pArray, /* CanBeNull */ fbxsdk::FbxCluster* pCluster);

AS_API(fbxsdk::FbxMesh*) AsFbxMeshCreateMesh(AsFbxContext* pContext, fbxsdk::FbxNode* pFrameNode);

AS_API(void) AsFbxMeshInitControlPoints(fbxsdk::FbxMesh* pMesh, int32_t vertexCount);

AS_API(void) AsFbxMeshCreateElementNormal(fbxsdk::FbxMesh* pMesh);

AS_API(void) AsFbxMeshCreateDiffuseUV(fbxsdk::FbxMesh* pMesh, int32_t uv);

AS_API(void) AsFbxMeshCreateNormalMapUV(fbxsdk::FbxMesh* pMesh, int32_t uv);

AS_API(void) AsFbxMeshCreateElementTangent(fbxsdk::FbxMesh* pMesh);

AS_API(void) AsFbxMeshCreateElementVertexColor(fbxsdk::FbxMesh* pMesh);

AS_API(void) AsFbxMeshCreateElementMaterial(fbxsdk::FbxMesh* pMesh);

AS_API(fbxsdk::FbxSurfacePhong*) AsFbxCreateMaterial(AsFbxContext* pContext, const char* pMatName,
	float diffuseR, float diffuseG, float diffuseB,
	float ambientR, float ambientG, float ambientB,
	float emissiveR, float emissiveG, float emissiveB,
	float specularR, float specularG, float specularB,
	float reflectR, float reflectG, float reflectB,
	float shininess, float transparency);

AS_API(int32_t) AsFbxAddMaterialToFrame(fbxsdk::FbxNode* pFrameNode, fbxsdk::FbxSurfacePhong* pMaterial);

AS_API(void) AsFbxSetFrameShadingModeToTextureShading(fbxsdk::FbxNode* pFrameNode);

AS_API(void) AsFbxMeshSetControlPoint(fbxsdk::FbxMesh* pMesh, int32_t index, float x, float y, float z);

AS_API(void) AsFbxMeshAddPolygon(fbxsdk::FbxMesh* pMesh, int32_t materialIndex, int32_t index0, int32_t index1, int32_t index2);

AS_API(void) AsFbxMeshElementNormalAdd(fbxsdk::FbxMesh* pMesh, int32_t elementIndex, float x, float y, float z);

AS_API(void) AsFbxMeshElementUVAdd(fbxsdk::FbxMesh* pMesh, int32_t elementIndex, float u, float v);

AS_API(void) AsFbxMeshElementTangentAdd(fbxsdk::FbxMesh* pMesh, int32_t elementIndex, float x, float y, float z, float w);

AS_API(void) AsFbxMeshElementVertexColorAdd(fbxsdk::FbxMesh* pMesh, int32_t elementIndex, float r, float g, float b, float a);

AS_API(void) AsFbxMeshSetBoneWeight(fbxsdk::FbxArray<fbxsdk::FbxCluster*>* pClusterArray, int32_t boneIndex, int32_t vertexIndex, float weight);

AS_API(AsFbxSkinContext*) AsFbxMeshCreateSkinContext(AsFbxContext* pContext, fbxsdk::FbxNode* pFrameNode);

AS_API(void) AsFbxMeshDisposeSkinContext(AsFbxSkinContext** ppSkinContext);

AS_API(bool32_t) FbxClusterArray_HasItemAt(fbxsdk::FbxArray<fbxsdk::FbxCluster*>* pClusterArray, int32_t index);

AS_API(void) AsFbxMeshSkinAddCluster(AsFbxSkinContext* pSkinContext, fbxsdk::FbxArray<fbxsdk::FbxCluster*>* pClusterArray, int32_t index, float pBoneMatrix[16]);

AS_API(void) AsFbxMeshAddDeformer(AsFbxSkinContext* pSkinContext, fbxsdk::FbxMesh* pMesh);

AS_API(AsFbxAnimContext*) AsFbxAnimCreateContext(bool32_t eulerFilter);

AS_API(void) AsFbxAnimDisposeContext(AsFbxAnimContext** ppAnimContext);

AS_API(void) AsFbxAnimPrepareStackAndLayer(AsFbxContext* pContext, AsFbxAnimContext* pAnimContext, const char* pTakeName);

AS_API(void) AsFbxAnimLoadCurves(fbxsdk::FbxNode* pNode, AsFbxAnimContext* pAnimContext);

AS_API(void) AsFbxAnimBeginKeyModify(AsFbxAnimContext* pAnimContext);

AS_API(void) AsFbxAnimEndKeyModify(AsFbxAnimContext* pAnimContext);

AS_API(void) AsFbxAnimAddScalingKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z);

AS_API(void) AsFbxAnimAddRotationKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z);

AS_API(void) AsFbxAnimAddTranslationKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z);

AS_API(void) AsFbxAnimApplyEulerFilter(AsFbxAnimContext* pAnimContext, float filterPrecision);

AS_API(int32_t) AsFbxAnimGetCurrentBlendShapeChannelCount(AsFbxAnimContext* pAnimContext, fbxsdk::FbxNode* pNode);

AS_API(bool32_t) AsFbxAnimIsBlendShapeChannelMatch(AsFbxAnimContext* pAnimContext, int32_t channelIndex, const char* channelName);

AS_API(void) AsFbxAnimBeginBlendShapeAnimCurve(AsFbxAnimContext* pAnimContext, int32_t channelIndex);

AS_API(void) AsFbxAnimEndBlendShapeAnimCurve(AsFbxAnimContext* pAnimContext);

AS_API(void) AsFbxAnimAddBlendShapeKeyframe(AsFbxAnimContext* pAnimContext, float time, float value);

AS_API(AsFbxMorphContext*) AsFbxMorphCreateContext();

AS_API(void) AsFbxMorphInitializeContext(AsFbxContext* pContext, AsFbxMorphContext* pMorphContext, fbxsdk::FbxNode* pNode);

AS_API(void) AsFbxMorphDisposeContext(AsFbxMorphContext** ppMorphContext);

AS_API(void) AsFbxMorphAddBlendShapeChannel(AsFbxContext* pContext, AsFbxMorphContext* pMorphContext, const char* channelName);

AS_API(void) AsFbxMorphAddBlendShapeChannelShape(AsFbxContext* pContext, AsFbxMorphContext* pMorphContext, float weight, const char* shapeName);

AS_API(void) AsFbxMorphCopyBlendShapeControlPoints(AsFbxMorphContext* pMorphContext);

AS_API(void) AsFbxMorphSetBlendShapeVertex(AsFbxMorphContext* pMorphContext, uint32_t index, float x, float y, float z);

AS_API(void) AsFbxMorphCopyBlendShapeControlPointsNormal(AsFbxMorphContext* pMorphContext);

AS_API(void) AsFbxMorphSetBlendShapeVertexNormal(AsFbxMorphContext* pMorphContext, uint32_t index, float x, float y, float z);
