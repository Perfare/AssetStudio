#include <fbxsdk.h>

#include "asfbx_context.h"

AsFbxContext::AsFbxContext()
{
	cDest = nullptr;
	pSdkManager = nullptr;
	pScene = nullptr;
	pTextures = nullptr;
	pMaterials = nullptr;
	pExporter = nullptr;
	pBindPose = nullptr;
}

AsFbxContext::~AsFbxContext()
{
	framePaths.clear();

	delete[] cDest;

	delete pMaterials;
	delete pTextures;

	if (pExporter != nullptr) {
		pExporter->Destroy();
	}

	if (pScene != nullptr) {
		pScene->Destroy();
	}

	if (pSdkManager != nullptr) {
		pSdkManager->Destroy();
	}
}
