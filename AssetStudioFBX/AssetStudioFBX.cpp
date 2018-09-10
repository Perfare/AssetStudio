#include <fbxsdk.h>
#include <fbxsdk/fileio/fbxiosettings.h>
#include "AssetStudioFBX.h"

namespace AssetStudio
{
	char* Fbx::StringToCharArray(String^ s)
	{
		return (char*)(void*)Marshal::StringToHGlobalAnsi(s);
	}

	void Fbx::Init(FbxManager** pSdkManager, FbxScene** pScene)
	{
		*pSdkManager = FbxManager::Create();
		if (!pSdkManager)
		{
			throw gcnew Exception(gcnew String("Unable to create the FBX SDK manager"));
		}

		FbxIOSettings* ios = FbxIOSettings::Create(*pSdkManager, IOSROOT);
		(*pSdkManager)->SetIOSettings(ios);
		*pScene = FbxScene::Create(*pSdkManager, "");
	}

	Vector3 Fbx::QuaternionToEuler(Quaternion q)
	{
		FbxAMatrix lMatrixRot;
		lMatrixRot.SetQ(FbxQuaternion(q.X, q.Y, q.Z, q.W));
		FbxVector4 lEuler = lMatrixRot.GetR();
		return Vector3((float)lEuler[0], (float)lEuler[1], (float)lEuler[2]);
	}

	Quaternion Fbx::EulerToQuaternion(Vector3 v)
	{
		FbxAMatrix lMatrixRot;
		lMatrixRot.SetR(FbxVector4(v.X, v.Y, v.Z));
		FbxQuaternion lQuaternion = lMatrixRot.GetQ();
		return Quaternion((float)lQuaternion[0], (float)lQuaternion[1], (float)lQuaternion[2], (float)lQuaternion[3]);
	}
}