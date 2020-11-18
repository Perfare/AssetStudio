#pragma once

#include <fbxsdk.h>

#include "bool32_t.h"

struct AsFbxAnimContext
{

	FbxAnimCurveFilterUnroll* lFilter;

	FbxAnimStack* lAnimStack;
	FbxAnimLayer* lAnimLayer;

	FbxAnimCurve* lCurveSX;
	FbxAnimCurve* lCurveSY;
	FbxAnimCurve* lCurveSZ;
	FbxAnimCurve* lCurveRX;
	FbxAnimCurve* lCurveRY;
	FbxAnimCurve* lCurveRZ;
	FbxAnimCurve* lCurveTX;
	FbxAnimCurve* lCurveTY;
	FbxAnimCurve* lCurveTZ;

	FbxMesh* pMesh;
	FbxBlendShape* lBlendShape;
	FbxAnimCurve* lAnimCurve;

	AsFbxAnimContext(bool32_t eulerFilter);
	~AsFbxAnimContext() = default;

};
