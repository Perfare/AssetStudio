#include <fbxsdk.h>

#include "dllexport.h"
#include "bool32_t.h"
#include "asfbx_context.h"
#include "asfbx_skin_context.h"
#include "asfbx_anim_context.h"
#include "asfbx_morph_context.h"
#include "utils.h"

using namespace fbxsdk;

AS_API(void) AsUtilQuaternionToEuler(float qx, float qy, float qz, float qw, float* vx, float* vy, float* vz)
{
	Quaternion q(qx, qy, qz, qw);

	auto v = QuaternionToEuler(q);

	if (vx)
	{
		*vx = v.X;
	}

	if (vy)
	{
		*vy = v.Y;
	}

	if (vz)
	{
		*vz = v.Z;
	}
}

AS_API(void) AsUtilEulerToQuaternion(float vx, float vy, float vz, float* qx, float* qy, float* qz, float* qw)
{
	Vector3 v(vx, vy, vz);

	auto q = EulerToQuaternion(v);

	if (qx)
	{
		*qx = q.X;
	}

	if (qy)
	{
		*qy = q.Y;
	}

	if (qz)
	{
		*qz = q.Z;
	}

	if (qw)
	{
		*qw = q.W;
	}
}

#define MGR_IOS_REF (*(pSdkManager->GetIOSettings()))

static const char* FBXVersion[] =
{
	FBX_2010_00_COMPATIBLE,
	FBX_2011_00_COMPATIBLE,
	FBX_2012_00_COMPATIBLE,
	FBX_2013_00_COMPATIBLE,
	FBX_2014_00_COMPATIBLE,
	FBX_2016_00_COMPATIBLE
};

AS_API(AsFbxContext*) AsFbxCreateContext()
{
	return new AsFbxContext();
}

AS_API(bool32_t) AsFbxInitializeContext(AsFbxContext* pContext, const char* pFileName, float scaleFactor, int32_t versionIndex, bool32_t isAscii, bool32_t is60Fps, const char** pErrMsg) {
	if (pContext == nullptr)
	{
		if (pErrMsg != nullptr)
		{
			*pErrMsg = "null pointer for pContext";
		}

		return false;
	}

	auto pSdkManager = FbxManager::Create();
	pContext->pSdkManager = pSdkManager;

	FbxIOSettings* ios = FbxIOSettings::Create(pSdkManager, IOSROOT);
	pSdkManager->SetIOSettings(ios);

	auto pScene = FbxScene::Create(pSdkManager, "");
	pContext->pScene = pScene;

	MGR_IOS_REF.SetBoolProp(EXP_FBX_MATERIAL, true);
	MGR_IOS_REF.SetBoolProp(EXP_FBX_TEXTURE, true);
	MGR_IOS_REF.SetBoolProp(EXP_FBX_EMBEDDED, false);
	MGR_IOS_REF.SetBoolProp(EXP_FBX_SHAPE, true);
	MGR_IOS_REF.SetBoolProp(EXP_FBX_GOBO, true);
	MGR_IOS_REF.SetBoolProp(EXP_FBX_ANIMATION, true);
	MGR_IOS_REF.SetBoolProp(EXP_FBX_GLOBAL_SETTINGS, true);

	FbxGlobalSettings& globalSettings = pScene->GetGlobalSettings();
	globalSettings.SetSystemUnit(FbxSystemUnit(scaleFactor));

	if (is60Fps)
	{
		globalSettings.SetTimeMode(FbxTime::eFrames60);
	}

	auto pExporter = FbxExporter::Create(pScene, "");
	pContext->pExporter = pExporter;

	int pFileFormat = 0;

	if (versionIndex == 0)
	{
		pFileFormat = 3;

		if (isAscii)
		{
			pFileFormat = 4;
		}
	}
	else
	{
		pExporter->SetFileExportVersion(FBXVersion[versionIndex]);

		if (isAscii)
		{
			pFileFormat = 1;
		}
	}

	if (!pExporter->Initialize(pFileName, pFileFormat, pSdkManager->GetIOSettings()))
	{
		if (pErrMsg != nullptr)
		{
			auto errStr = pExporter->GetStatus().GetErrorString();
			*pErrMsg = errStr;
		}

		return false;
	}

	auto pBindPose = FbxPose::Create(pScene, "BindPose");
	pContext->pBindPose = pBindPose;

	pScene->AddPose(pBindPose);

	return true;
}

AS_API(void) AsFbxDisposeContext(AsFbxContext** ppContext)
{
	if (ppContext == nullptr) {
		return;
	}

	delete (*ppContext);
	*ppContext = nullptr;
}

AS_API(void) AsFbxSetFramePaths(AsFbxContext* pContext, const char* ppPaths[], int32_t count)
{
	if (pContext == nullptr) {
		return;
	}

	auto& framePaths = pContext->framePaths;

	for (auto i = 0; i < count; i += 1)
	{
		const char* path = ppPaths[i];
		framePaths.insert(std::string(path));
	}
}

AS_API(void) AsFbxExportScene(AsFbxContext* pContext)
{
	if (pContext == nullptr)
	{
		return;
	}

	auto pScene = pContext->pScene;
	auto pExporter = pContext->pExporter;

	if (pExporter != nullptr && pScene != nullptr)
	{
		pExporter->Export(pScene);
	}
}

AS_API(FbxNode*) AsFbxGetSceneRootNode(AsFbxContext* pContext)
{
	if (pContext == nullptr)
	{
		return nullptr;
	}

	if (pContext->pScene == nullptr)
	{
		return nullptr;
	}

	return pContext->pScene->GetRootNode();
}

AS_API(FbxNode*) AsFbxExportSingleFrame(AsFbxContext* pContext, FbxNode* pParentNode, const char* pFramePath, const char* pFrameName, float localPositionX, float localPositionY, float localPositionZ, float localRotationX, float localRotationY, float localRotationZ, float localScaleX, float localScaleY, float localScaleZ)
{
	if (pContext == nullptr || pContext->pScene == nullptr)
	{
		return nullptr;
	}

	const auto& framePaths = pContext->framePaths;

	if (!(framePaths.empty() || framePaths.find(pFramePath) != framePaths.end()))
	{
		return nullptr;
	}

	auto pFrameNode = FbxNode::Create(pContext->pScene, pFrameName);

	pFrameNode->LclScaling.Set(FbxDouble3(localScaleX, localScaleY, localScaleZ));
	pFrameNode->LclRotation.Set(FbxDouble3(localRotationX, localRotationY, localRotationZ));
	pFrameNode->LclTranslation.Set(FbxDouble3(localPositionX, localPositionY, localPositionZ));
	pFrameNode->SetPreferedAngle(pFrameNode->LclRotation.Get());

	pParentNode->AddChild(pFrameNode);

	if (pContext->pBindPose != nullptr)
	{
		pContext->pBindPose->Add(pFrameNode, pFrameNode->EvaluateGlobalTransform());
	}

	return pFrameNode;
}

AS_API(void) AsFbxSetJointsNode_CastToBone(AsFbxContext* pContext, FbxNode* pNode, float boneSize)
{
	if (pContext == nullptr || pContext->pScene == nullptr)
	{
		return;
	}

	if (pNode == nullptr)
	{
		return;
	}

	FbxSkeleton* pJoint = FbxSkeleton::Create(pContext->pScene, "");
	pJoint->Size.Set(FbxDouble(boneSize));
	pJoint->SetSkeletonType(FbxSkeleton::eLimbNode);
	pNode->SetNodeAttribute(pJoint);
}

AS_API(void) AsFbxSetJointsNode_BoneInPath(AsFbxContext* pContext, FbxNode* pNode, float boneSize)
{
	if (pContext == nullptr || pContext->pScene == nullptr)
	{
		return;
	}

	if (pNode == nullptr)
	{
		return;
	}

	FbxSkeleton* pJoint = FbxSkeleton::Create(pContext->pScene, "");
	pJoint->Size.Set(FbxDouble(boneSize));
	pJoint->SetSkeletonType(FbxSkeleton::eLimbNode);
	pNode->SetNodeAttribute(pJoint);

	pJoint = FbxSkeleton::Create(pContext->pScene, "");
	pJoint->Size.Set(FbxDouble(boneSize));
	pJoint->SetSkeletonType(FbxSkeleton::eLimbNode);
	pNode->GetParent()->SetNodeAttribute(pJoint);
}

AS_API(void) AsFbxSetJointsNode_Generic(AsFbxContext* pContext, FbxNode* pNode)
{
	if (pContext == nullptr || pContext->pScene == nullptr)
	{
		return;
	}

	if (pNode == nullptr)
	{
		return;
	}

	FbxNull* pNull = FbxNull::Create(pContext->pScene, "");

	if (pNode->GetChildCount() > 0)
	{
		pNull->Look.Set(FbxNull::eNone);
	}

	pNode->SetNodeAttribute(pNull);
}

AS_API(void) AsFbxPrepareMaterials(AsFbxContext* pContext, int32_t materialCount, int32_t textureCount)
{
	if (pContext == nullptr)
	{
		return;
	}

	pContext->pMaterials = new FbxArray<FbxSurfacePhong*>();
	pContext->pTextures = new FbxArray<FbxFileTexture*>();

	pContext->pMaterials->Reserve(materialCount);
	pContext->pTextures->Reserve(textureCount);
}

AS_API(FbxFileTexture*) AsFbxCreateTexture(AsFbxContext* pContext, const char* pMatTexName)
{
	if (pContext == nullptr || pContext->pScene == nullptr)
	{
		return nullptr;
	}

	auto pTex = FbxFileTexture::Create(pContext->pScene, pMatTexName);
	pTex->SetFileName(pMatTexName);
	pTex->SetTextureUse(FbxTexture::eStandard);
	pTex->SetMappingType(FbxTexture::eUV);
	pTex->SetMaterialUse(FbxFileTexture::eModelMaterial);
	pTex->SetSwapUV(false);
	pTex->SetTranslation(0.0, 0.0);
	pTex->SetScale(1.0, 1.0);
	pTex->SetRotation(0.0, 0.0);

	if (pContext->pTextures != nullptr)
	{
		pContext->pTextures->Add(pTex);
	}

	return pTex;
}

AS_API(void) AsFbxLinkTexture(int32_t dest, FbxFileTexture* pTexture, FbxSurfacePhong* pMaterial, float offsetX, float offsetY, float scaleX, float scaleY)
{
	if (pTexture == nullptr || pMaterial == nullptr)
	{
		return;
	}

	pTexture->SetTranslation(offsetX, offsetY);
	pTexture->SetScale(scaleX, scaleY);

	FbxProperty* pProp;

	switch (dest)
	{
	case 0:
		pProp = &pMaterial->Diffuse;
		break;
	case 1:
		pProp = &pMaterial->NormalMap;
		break;
	case 2:
		pProp = &pMaterial->Specular;
		break;
	case 3:
		pProp = &pMaterial->Bump;
		break;
	default:
		pProp = nullptr;
		break;
	}

	if (pProp != nullptr) {
		pProp->ConnectSrcObject(pTexture);
	}
}

AS_API(FbxArray<FbxCluster*>*) AsFbxMeshCreateClusterArray(int32_t boneCount)
{
	return new FbxArray<FbxCluster*>(boneCount);
}

AS_API(void) AsFbxMeshDisposeClusterArray(FbxArray<FbxCluster*>** ppArray)
{
	if (ppArray == nullptr) {
		return;
	}

	delete (*ppArray);
	*ppArray = nullptr;
}

AS_API(FbxCluster*) AsFbxMeshCreateCluster(AsFbxContext* pContext, FbxNode* pBoneNode)
{
	if (pContext == nullptr || pContext->pScene == nullptr) {
		return nullptr;
	}

	if (pBoneNode == nullptr) {
		return nullptr;
	}

	FbxString lClusterName = pBoneNode->GetNameOnly() + FbxString("Cluster");
	FbxCluster* pCluster = FbxCluster::Create(pContext->pScene, lClusterName.Buffer());
	pCluster->SetLink(pBoneNode);
	pCluster->SetLinkMode(FbxCluster::eTotalOne);

	return pCluster;
}

AS_API(void) AsFbxMeshAddCluster(FbxArray<FbxCluster*>* pArray, FbxCluster* pCluster)
{
	if (pArray == nullptr) {
		return;
	}

	pArray->Add(pCluster);
}

AS_API(FbxMesh*) AsFbxMeshCreateMesh(AsFbxContext* pContext, FbxNode* pFrameNode)
{
	if (pContext == nullptr || pContext->pScene == nullptr)
	{
		return nullptr;
	}

	if (pFrameNode == nullptr)
	{
		return nullptr;
	}

	FbxMesh* pMesh = FbxMesh::Create(pContext->pScene, pFrameNode->GetName());
	pFrameNode->SetNodeAttribute(pMesh);

	return pMesh;
}

AS_API(void) AsFbxMeshInitControlPoints(FbxMesh* pMesh, int32_t vertexCount)
{
	if (pMesh == nullptr)
	{
		return;
	}

	pMesh->InitControlPoints(vertexCount);
}

AS_API(void) AsFbxMeshCreateElementNormal(FbxMesh* pMesh)
{
	if (pMesh == nullptr)
	{
		return;
	}

	auto pNormal = pMesh->CreateElementNormal();
	pNormal->SetMappingMode(FbxGeometryElement::eByControlPoint);
	pNormal->SetReferenceMode(FbxGeometryElement::eDirect);
}

AS_API(void) AsFbxMeshCreateDiffuseUV(FbxMesh* pMesh, int32_t uv)
{
	if (pMesh == nullptr)
	{
		return;
	}

	auto pUV = pMesh->CreateElementUV(FbxString("UV") + FbxString(uv), FbxLayerElement::eTextureDiffuse);
	pUV->SetMappingMode(FbxGeometryElement::eByControlPoint);
	pUV->SetReferenceMode(FbxGeometryElement::eDirect);
}

AS_API(void) AsFbxMeshCreateNormalMapUV(FbxMesh* pMesh, int32_t uv)
{
	if (pMesh == nullptr)
	{
		return;
	}

	auto pUV = pMesh->CreateElementUV(FbxString("UV") + FbxString(uv), FbxLayerElement::eTextureNormalMap);
	pUV->SetMappingMode(FbxGeometryElement::eByControlPoint);
	pUV->SetReferenceMode(FbxGeometryElement::eDirect);
}

AS_API(void) AsFbxMeshCreateElementTangent(FbxMesh* pMesh)
{
	if (pMesh == nullptr)
	{
		return;
	}

	auto pTangent = pMesh->CreateElementTangent();
	pTangent->SetMappingMode(FbxGeometryElement::eByControlPoint);
	pTangent->SetReferenceMode(FbxGeometryElement::eDirect);
}

AS_API(void) AsFbxMeshCreateElementVertexColor(FbxMesh* pMesh)
{
	if (pMesh == nullptr)
	{
		return;
	}

	auto pVertexColor = pMesh->CreateElementVertexColor();
	pVertexColor->SetMappingMode(FbxGeometryElement::eByControlPoint);
	pVertexColor->SetReferenceMode(FbxGeometryElement::eDirect);
}

AS_API(void) AsFbxMeshCreateElementMaterial(FbxMesh* pMesh)
{
	if (pMesh == nullptr)
	{
		return;
	}

	auto pMaterial = pMesh->CreateElementMaterial();
	pMaterial->SetMappingMode(FbxGeometryElement::eByPolygon);
	pMaterial->SetReferenceMode(FbxGeometryElement::eIndexToDirect);
}

AS_API(FbxSurfacePhong*) AsFbxCreateMaterial(AsFbxContext* pContext, const char* pMatName,
	float diffuseR, float diffuseG, float diffuseB,
	float ambientR, float ambientG, float ambientB,
	float emissiveR, float emissiveG, float emissiveB,
	float specularR, float specularG, float specularB,
	float reflectR, float reflectG, float reflectB,
	float shininess, float transparency)
{
	if (pContext == nullptr || pContext->pScene == nullptr)
	{
		return nullptr;
	}

	if (pMatName == nullptr)
	{
		return nullptr;
	}

	auto pMat = FbxSurfacePhong::Create(pContext->pScene, pMatName);

	pMat->Diffuse.Set(FbxDouble3(diffuseR, diffuseG, diffuseB));
	pMat->Ambient.Set(FbxDouble3(ambientR, ambientG, ambientB));
	pMat->Emissive.Set(FbxDouble3(emissiveR, emissiveG, emissiveB));
	pMat->Specular.Set(FbxDouble3(specularR, specularG, specularB));
	pMat->Reflection.Set(FbxDouble3(reflectR, reflectG, reflectB));
	pMat->Shininess.Set(FbxDouble(shininess));
	pMat->TransparencyFactor.Set(FbxDouble(transparency));
	pMat->ShadingModel.Set("Phong");

	if (pContext->pMaterials)
	{
		pContext->pMaterials->Add(pMat);
	}

	return pMat;
}

AS_API(int32_t) AsFbxAddMaterialToFrame(FbxNode* pFrameNode, FbxSurfacePhong* pMaterial)
{
	if (pFrameNode == nullptr || pMaterial == nullptr)
	{
		return 0;
	}

	return pFrameNode->AddMaterial(pMaterial);
}

AS_API(void) AsFbxSetFrameShadingModeToTextureShading(FbxNode* pFrameNode)
{
	if (pFrameNode == nullptr)
	{
		return;
	}

	pFrameNode->SetShadingMode(FbxNode::eTextureShading);
}

AS_API(void) AsFbxMeshSetControlPoint(FbxMesh* pMesh, int32_t index, float x, float y, float z)
{
	if (pMesh == nullptr)
	{
		return;
	}

	auto pControlPoints = pMesh->GetControlPoints();

	pControlPoints[index] = FbxVector4(x, y, z, 0);
}

AS_API(void) AsFbxMeshAddPolygon(FbxMesh* pMesh, int32_t materialIndex, int32_t index0, int32_t index1, int32_t index2)
{
	if (pMesh == nullptr)
	{
		return;
	}

	pMesh->BeginPolygon(materialIndex);
	pMesh->AddPolygon(index0);
	pMesh->AddPolygon(index1);
	pMesh->AddPolygon(index2);
	pMesh->EndPolygon();
}

AS_API(void) AsFbxMeshElementNormalAdd(FbxMesh* pMesh, int32_t elementIndex, float x, float y, float z)
{
	if (pMesh == nullptr)
	{
		return;
	}

	auto pElem = pMesh->GetElementNormal(elementIndex);
	auto& array = pElem->GetDirectArray();

	array.Add(FbxVector4(x, y, z, 0));
}

AS_API(void) AsFbxMeshElementUVAdd(FbxMesh* pMesh, int32_t elementIndex, float u, float v)
{
	if (pMesh == nullptr)
	{
		return;
	}

	auto pElem = pMesh->GetElementUV(FbxString("UV") + FbxString(elementIndex));
	auto& array = pElem->GetDirectArray();

	array.Add(FbxVector2(u, v));
}

AS_API(void) AsFbxMeshElementTangentAdd(FbxMesh* pMesh, int32_t elementIndex, float x, float y, float z, float w)
{
	if (pMesh == nullptr)
	{
		return;
	}

	auto pElem = pMesh->GetElementTangent(elementIndex);
	auto& array = pElem->GetDirectArray();

	array.Add(FbxVector4(x, y, z, w));
}

AS_API(void) AsFbxMeshElementVertexColorAdd(FbxMesh* pMesh, int32_t elementIndex, float r, float g, float b, float a)
{
	if (pMesh == nullptr)
	{
		return;
	}

	auto pElem = pMesh->GetElementVertexColor(elementIndex);
	auto& array = pElem->GetDirectArray();

	array.Add(FbxVector4(r, g, b, a));
}

AS_API(void) AsFbxMeshSetBoneWeight(FbxArray<FbxCluster*>* pClusterArray, int32_t boneIndex, int32_t vertexIndex, float weight)
{
	if (pClusterArray == nullptr)
	{
		return;
	}

	auto pCluster = pClusterArray->GetAt(boneIndex);

	if (pCluster != nullptr)
	{
		pCluster->AddControlPointIndex(vertexIndex, weight);
	}
}

AS_API(AsFbxSkinContext*) AsFbxMeshCreateSkinContext(AsFbxContext* pContext, FbxNode* pFrameNode)
{
	return new AsFbxSkinContext(pContext, pFrameNode);
}

AS_API(void) AsFbxMeshDisposeSkinContext(AsFbxSkinContext** ppSkinContext)
{
	if (ppSkinContext == nullptr)
	{
		return;
	}

	delete (*ppSkinContext);
	*ppSkinContext = nullptr;
}

AS_API(bool32_t) FbxClusterArray_HasItemAt(FbxArray<FbxCluster*>* pClusterArray, int32_t index)
{
	if (pClusterArray == nullptr)
	{
		return false;
	}

	auto pCluster = pClusterArray->GetAt(index);

	return pCluster != nullptr;
}

static inline int32_t IndexFrom4x4(int32_t m, int32_t n)
{
	return m * 4 + n;
}

AS_API(void) AsFbxMeshSkinAddCluster(AsFbxSkinContext* pSkinContext, FbxArray<FbxCluster*>* pClusterArray, int32_t index, float pBoneMatrix[16])
{
	if (pSkinContext == nullptr)
	{
		return;
	}

	if (pClusterArray == nullptr)
	{
		return;
	}

	if (pBoneMatrix == nullptr)
	{
		return;
	}

	auto pCluster = pClusterArray->GetAt(index);

	if (pCluster == nullptr)
	{
		return;
	}

	FbxAMatrix boneMatrix;

	for (int m = 0; m < 4; m += 1)
	{
		for (int n = 0; n < 4; n += 1)
		{
			auto index = IndexFrom4x4(m, n);
			boneMatrix.mData[m][n] = pBoneMatrix[index];
		}
	}

	pCluster->SetTransformMatrix(pSkinContext->lMeshMatrix);
	pCluster->SetTransformLinkMatrix(pSkinContext->lMeshMatrix * boneMatrix.Inverse());

	if (pSkinContext->pSkin)
	{
		pSkinContext->pSkin->AddCluster(pCluster);
	}
}

AS_API(void) AsFbxMeshAddDeformer(AsFbxSkinContext* pSkinContext, FbxMesh* pMesh)
{
	if (pSkinContext == nullptr || pSkinContext->pSkin == nullptr)
	{
		return;
	}

	if (pMesh == nullptr)
	{
		return;
	}

	if (pSkinContext->pSkin->GetClusterCount() > 0)
	{
		pMesh->AddDeformer(pSkinContext->pSkin);
	}
}

AS_API(AsFbxAnimContext*) AsFbxAnimCreateContext(bool32_t eulerFilter)
{
	return new AsFbxAnimContext(eulerFilter);
}

AS_API(void) AsFbxAnimDisposeContext(AsFbxAnimContext** ppAnimContext)
{
	if (ppAnimContext == nullptr)
	{
		return;
	}

	delete (*ppAnimContext);
	*ppAnimContext = nullptr;
}

AS_API(void) AsFbxAnimPrepareStackAndLayer(AsFbxContext* pContext, AsFbxAnimContext* pAnimContext, const char* pTakeName)
{
	if (pContext == nullptr || pContext->pScene == nullptr)
	{
		return;
	}

	if (pAnimContext == nullptr)
	{
		return;
	}

	if (pTakeName == nullptr)
	{
		return;
	}

	pAnimContext->lAnimStack = FbxAnimStack::Create(pContext->pScene, pTakeName);
	pAnimContext->lAnimLayer = FbxAnimLayer::Create(pContext->pScene, "Base Layer");

	pAnimContext->lAnimStack->AddMember(pAnimContext->lAnimLayer);
}

AS_API(void) AsFbxAnimLoadCurves(FbxNode* pNode, AsFbxAnimContext* pAnimContext)
{
	if (pNode == nullptr)
	{
		return;
	}

	if (pAnimContext == nullptr)
	{
		return;
	}

	pAnimContext->lCurveSX = pNode->LclScaling.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
	pAnimContext->lCurveSY = pNode->LclScaling.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
	pAnimContext->lCurveSZ = pNode->LclScaling.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
	pAnimContext->lCurveRX = pNode->LclRotation.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
	pAnimContext->lCurveRY = pNode->LclRotation.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
	pAnimContext->lCurveRZ = pNode->LclRotation.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
	pAnimContext->lCurveTX = pNode->LclTranslation.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
	pAnimContext->lCurveTY = pNode->LclTranslation.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
	pAnimContext->lCurveTZ = pNode->LclTranslation.GetCurve(pAnimContext->lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
}

AS_API(void) AsFbxAnimBeginKeyModify(AsFbxAnimContext* pAnimContext)
{
	if (pAnimContext == nullptr)
	{
		return;
	}

	pAnimContext->lCurveSX->KeyModifyBegin();
	pAnimContext->lCurveSY->KeyModifyBegin();
	pAnimContext->lCurveSZ->KeyModifyBegin();
	pAnimContext->lCurveRX->KeyModifyBegin();
	pAnimContext->lCurveRY->KeyModifyBegin();
	pAnimContext->lCurveRZ->KeyModifyBegin();
	pAnimContext->lCurveTX->KeyModifyBegin();
	pAnimContext->lCurveTY->KeyModifyBegin();
	pAnimContext->lCurveTZ->KeyModifyBegin();
}

AS_API(void) AsFbxAnimEndKeyModify(AsFbxAnimContext* pAnimContext)
{
	if (pAnimContext == nullptr)
	{
		return;
	}

	pAnimContext->lCurveSX->KeyModifyEnd();
	pAnimContext->lCurveSY->KeyModifyEnd();
	pAnimContext->lCurveSZ->KeyModifyEnd();
	pAnimContext->lCurveRX->KeyModifyEnd();
	pAnimContext->lCurveRY->KeyModifyEnd();
	pAnimContext->lCurveRZ->KeyModifyEnd();
	pAnimContext->lCurveTX->KeyModifyEnd();
	pAnimContext->lCurveTY->KeyModifyEnd();
	pAnimContext->lCurveTZ->KeyModifyEnd();
}

AS_API(void) AsFbxAnimAddScalingKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z)
{
	if (pAnimContext == nullptr)
	{
		return;
	}

	FbxTime lTime;
	lTime.SetSecondDouble(time);

	pAnimContext->lCurveSX->KeySet(pAnimContext->lCurveSX->KeyAdd(lTime), lTime, x);
	pAnimContext->lCurveSY->KeySet(pAnimContext->lCurveSY->KeyAdd(lTime), lTime, y);
	pAnimContext->lCurveSZ->KeySet(pAnimContext->lCurveSZ->KeyAdd(lTime), lTime, z);
}

AS_API(void) AsFbxAnimAddRotationKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z)
{
	if (pAnimContext == nullptr)
	{
		return;
	}

	FbxTime lTime;
	lTime.SetSecondDouble(time);

	pAnimContext->lCurveRX->KeySet(pAnimContext->lCurveRX->KeyAdd(lTime), lTime, x);
	pAnimContext->lCurveRY->KeySet(pAnimContext->lCurveRY->KeyAdd(lTime), lTime, y);
	pAnimContext->lCurveRZ->KeySet(pAnimContext->lCurveRZ->KeyAdd(lTime), lTime, z);
}

AS_API(void) AsFbxAnimAddTranslationKey(AsFbxAnimContext* pAnimContext, float time, float x, float y, float z)
{
	if (pAnimContext == nullptr)
	{
		return;
	}

	FbxTime lTime;
	lTime.SetSecondDouble(time);

	pAnimContext->lCurveTX->KeySet(pAnimContext->lCurveTX->KeyAdd(lTime), lTime, x);
	pAnimContext->lCurveTY->KeySet(pAnimContext->lCurveTY->KeyAdd(lTime), lTime, y);
	pAnimContext->lCurveTZ->KeySet(pAnimContext->lCurveTZ->KeyAdd(lTime), lTime, z);
}

AS_API(void) AsFbxAnimApplyEulerFilter(AsFbxAnimContext* pAnimContext, float filterPrecision)
{
	if (pAnimContext == nullptr || pAnimContext->lFilter == nullptr)
	{
		return;
	}

	FbxAnimCurve* lCurve[3];
	lCurve[0] = pAnimContext->lCurveRX;
	lCurve[1] = pAnimContext->lCurveRY;
	lCurve[2] = pAnimContext->lCurveRZ;

	auto eulerFilter = pAnimContext->lFilter;

	eulerFilter->Reset();
	eulerFilter->SetQualityTolerance(filterPrecision);
	eulerFilter->Apply(lCurve, 3);
}

AS_API(int32_t) AsFbxAnimGetCurrentBlendShapeChannelCount(AsFbxAnimContext* pAnimContext, fbxsdk::FbxNode* pNode)
{
	if (pAnimContext == nullptr)
	{
		return 0;
	}

	if (pNode == nullptr)
	{
		return 0;
	}

	auto pMesh = pNode->GetMesh();
	pAnimContext->pMesh = pMesh;

	if (pMesh == nullptr)
	{
		return 0;
	}

	auto blendShapeDeformerCount = pMesh->GetDeformerCount(FbxDeformer::eBlendShape);

	if (blendShapeDeformerCount <= 0)
	{
		return 0;
	}

	auto lBlendShape = (FbxBlendShape*)pMesh->GetDeformer(0, FbxDeformer::eBlendShape);
	pAnimContext->lBlendShape = lBlendShape;

	if (lBlendShape == nullptr)
	{
		return 0;
	}

	auto lBlendShapeChannelCount = lBlendShape->GetBlendShapeChannelCount();

	return lBlendShapeChannelCount;
}

AS_API(bool32_t) AsFbxAnimIsBlendShapeChannelMatch(AsFbxAnimContext* pAnimContext, int32_t channelIndex, const char* channelName)
{
	if (pAnimContext == nullptr || pAnimContext->lBlendShape == nullptr)
	{
		return false;
	}

	if (channelName == nullptr)
	{
		return false;
	}

	FbxBlendShapeChannel* lChannel = pAnimContext->lBlendShape->GetBlendShapeChannel(channelIndex);
	auto lChannelName = lChannel->GetNameOnly();

	FbxString chanName(channelName);

	return lChannelName == chanName;
}

AS_API(void) AsFbxAnimBeginBlendShapeAnimCurve(AsFbxAnimContext* pAnimContext, int32_t channelIndex)
{
	if (pAnimContext == nullptr || pAnimContext->pMesh == nullptr || pAnimContext->lAnimLayer == nullptr)
	{
		return;
	}

	pAnimContext->lAnimCurve = pAnimContext->pMesh->GetShapeChannel(0, channelIndex, pAnimContext->lAnimLayer, true);
	pAnimContext->lAnimCurve->KeyModifyBegin();
}

AS_API(void) AsFbxAnimEndBlendShapeAnimCurve(AsFbxAnimContext* pAnimContext)
{
	if (pAnimContext == nullptr || pAnimContext->lAnimCurve == nullptr)
	{
		return;
	}

	pAnimContext->lAnimCurve->KeyModifyEnd();
}

AS_API(void) AsFbxAnimAddBlendShapeKeyframe(AsFbxAnimContext* pAnimContext, float time, float value)
{
	if (pAnimContext == nullptr || pAnimContext->lAnimCurve == nullptr)
	{
		return;
	}

	FbxTime lTime;
	lTime.SetSecondDouble(time);

	auto keyIndex = pAnimContext->lAnimCurve->KeyAdd(lTime);
	pAnimContext->lAnimCurve->KeySetValue(keyIndex, value);
	pAnimContext->lAnimCurve->KeySetInterpolation(keyIndex, FbxAnimCurveDef::eInterpolationCubic);
}

AS_API(AsFbxMorphContext*) AsFbxMorphCreateContext()
{
	return new AsFbxMorphContext();
}

AS_API(void) AsFbxMorphInitializeContext(AsFbxContext* pContext, AsFbxMorphContext* pMorphContext, fbxsdk::FbxNode* pNode)
{
	if (pContext == nullptr || pContext->pScene == nullptr)
	{
		return;
	}

	if (pMorphContext == nullptr)
	{
		return;
	}

	if (pNode == nullptr)
	{
		return;
	}

	auto pMesh = pNode->GetMesh();
	pMorphContext->pMesh = pMesh;

	auto lBlendShape = FbxBlendShape::Create(pContext->pScene, pMesh->GetNameOnly() + FbxString("BlendShape"));
	pMorphContext->lBlendShape = lBlendShape;

	pMesh->AddDeformer(lBlendShape);
}

AS_API(void) AsFbxMorphDisposeContext(AsFbxMorphContext** ppMorphContext)
{
	if (ppMorphContext == nullptr)
	{
		return;
	}

	delete (*ppMorphContext);
	*ppMorphContext = nullptr;
}

AS_API(void) AsFbxMorphAddBlendShapeChannel(AsFbxContext* pContext, AsFbxMorphContext* pMorphContext, const char* channelName)
{
	if (pContext == nullptr || pContext->pScene == nullptr)
	{
		return;
	}

	if (pMorphContext == nullptr)
	{
		return;
	}

	if (channelName == nullptr)
	{
		return;
	}

	auto lBlendShapeChannel = FbxBlendShapeChannel::Create(pContext->pScene, channelName);
	pMorphContext->lBlendShapeChannel = lBlendShapeChannel;

	if (pMorphContext->lBlendShape != nullptr)
	{
		pMorphContext->lBlendShape->AddBlendShapeChannel(lBlendShapeChannel);
	}
}

AS_API(void) AsFbxMorphAddBlendShapeChannelShape(AsFbxContext* pContext, AsFbxMorphContext* pMorphContext, float weight, const char* shapeName)
{
	if (pContext == nullptr || pContext->pScene == nullptr)
	{
		return;
	}

	if (pMorphContext == nullptr)
	{
		return;
	}

	auto lShape = FbxShape::Create(pContext->pScene, shapeName);
	pMorphContext->lShape = lShape;

	if (pMorphContext->lBlendShapeChannel != nullptr) {
		pMorphContext->lBlendShapeChannel->AddTargetShape(lShape, weight);
	}
}

AS_API(void) AsFbxMorphCopyBlendShapeControlPoints(AsFbxMorphContext* pMorphContext)
{
	if (pMorphContext == nullptr || pMorphContext->pMesh == nullptr || pMorphContext->lShape == nullptr)
	{
		return;
	}

	auto vectorCount = pMorphContext->pMesh->GetControlPointsCount();

	auto srcControlPoints = pMorphContext->pMesh->GetControlPoints();

	pMorphContext->lShape->InitControlPoints(vectorCount);

	for (int j = 0; j < vectorCount; j++)
	{
		pMorphContext->lShape->SetControlPointAt(FbxVector4(srcControlPoints[j]), j);;
	}
}

AS_API(void) AsFbxMorphSetBlendShapeVertex(AsFbxMorphContext* pMorphContext, uint32_t index, float x, float y, float z)
{
	if (pMorphContext == nullptr || pMorphContext->lShape == nullptr)
	{
		return;
	}

	pMorphContext->lShape->SetControlPointAt(FbxVector4(x, y, z, 0), index);
}

AS_API(void) AsFbxMorphCopyBlendShapeControlPointsNormal(AsFbxMorphContext* pMorphContext)
{
	if (pMorphContext == nullptr || pMorphContext->pMesh == nullptr || pMorphContext->lShape == nullptr)
	{
		return;
	}

	pMorphContext->lShape->InitNormals(pMorphContext->pMesh);
}

AS_API(void) AsFbxMorphSetBlendShapeVertexNormal(AsFbxMorphContext* pMorphContext, uint32_t index, float x, float y, float z)
{
	if (pMorphContext == nullptr || pMorphContext->lShape == nullptr)
	{
		return;
	}

	pMorphContext->lShape->SetControlPointNormalAt(FbxVector4(x, y, z, 0), index);
}
