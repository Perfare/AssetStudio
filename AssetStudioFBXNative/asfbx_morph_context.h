#pragma once

#include <fbxsdk.h>

struct AsFbxMorphContext
{

	FbxMesh* pMesh;
	FbxBlendShape* lBlendShape;
	FbxBlendShapeChannel* lBlendShapeChannel;
	FbxShape* lShape;

	AsFbxMorphContext();
	~AsFbxMorphContext() = default;

};
