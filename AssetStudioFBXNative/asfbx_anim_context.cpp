#include "asfbx_anim_context.h"

AsFbxAnimContext::AsFbxAnimContext(bool32_t eulerFilter)
	: lFilter(nullptr)
{
	if (eulerFilter)
	{
		lFilter = new FbxAnimCurveFilterUnroll();
	}

	lAnimStack = nullptr;
	lAnimLayer = nullptr;

	lCurveSX = nullptr;
	lCurveSY = nullptr;
	lCurveSZ = nullptr;
	lCurveRX = nullptr;
	lCurveRY = nullptr;
	lCurveRZ = nullptr;
	lCurveTX = nullptr;
	lCurveTY = nullptr;
	lCurveTZ = nullptr;

	pMesh = nullptr;
	lBlendShape = nullptr;
	lAnimCurve = nullptr;
}
