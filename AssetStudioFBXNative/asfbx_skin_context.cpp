#include "asfbx_skin_context.h"
#include "asfbx_context.h"

AsFbxSkinContext::AsFbxSkinContext(AsFbxContext* pContext, FbxNode* pFrameNode)
	: pSkin(nullptr)
{
	if (pContext != nullptr && pContext->pScene != nullptr)
	{
		pSkin = FbxSkin::Create(pContext->pScene, "");
	}

	if (pFrameNode != nullptr) 
	{
		lMeshMatrix = pFrameNode->EvaluateGlobalTransform();
	}
}
