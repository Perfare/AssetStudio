#pragma once

#include <fbxsdk.h>

#ifdef IOS_REF
#undef  IOS_REF
#define IOS_REF (*(pSdkManager->GetIOSettings()))
#endif

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;

#define WITH_MARSHALLED_STRING(name,str,block)\
	{ \
		char* name; \
		try \
		{ \
			name = StringToUTF8(str); \
			block \
		} \
		finally \
		{ \
			delete name; \
		} \
	}

static char* FBXVersion[] =
{
	FBX_2010_00_COMPATIBLE,
	FBX_2011_00_COMPATIBLE,
	FBX_2012_00_COMPATIBLE,
	FBX_2013_00_COMPATIBLE,
	FBX_2014_00_COMPATIBLE,
	FBX_2016_00_COMPATIBLE
};

namespace AssetStudio {

	public ref class Fbx
	{
	public:
		static Vector3 QuaternionToEuler(Quaternion q);
		static Quaternion EulerToQuaternion(Vector3 v);
		static char* StringToUTF8(String^ s);
		static void Init(FbxManager** pSdkManager, FbxScene** pScene);

		ref class Exporter
		{
		public:
			static void Export(String^ name, IImported^ imported, bool eulerFilter, float filterPrecision, bool allFrames, bool allBones, bool skins, float boneSize, float scaleFactor, bool flatInbetween, int versionIndex, bool isAscii);

		private:
			bool exportSkins;
			float boneSize;
			IImported^ imported;
			HashSet<String^>^ framePaths;
			Dictionary<ImportedFrame^, size_t>^ frameToNode;
			List<ImportedFrame^>^ meshFrames;

			char* cDest;
			FbxManager* pSdkManager;
			FbxScene* pScene;
			FbxExporter* pExporter;
			FbxArray<FbxSurfacePhong*>* pMaterials;
			FbxArray<FbxFileTexture*>* pTextures;
			FbxPose* pBindPose;

			Exporter(String^ path, IImported^ imported, bool allFrames, bool allBones, bool skins, float boneSize, float scaleFactor, int versionIndex, bool isAscii);
			~Exporter();

			void Exporter::LinkTexture(ImportedMaterialTexture^ texture, FbxFileTexture* pTexture, FbxProperty& prop);
			void SetJointsNode(ImportedFrame^ frame, HashSet<String^>^ bonePaths, bool allBones);
			HashSet<String^>^ SearchHierarchy();
			void SearchHierarchy(ImportedFrame^ frame, HashSet<String^>^ exportFrames);
			void SetJointsFromImportedMeshes(bool allBones);
			void ExportFrame(FbxNode* pParentNode, ImportedFrame^ frame);
			void ExportMesh(FbxNode* pFrameNode, ImportedMesh^ meshList);
			FbxFileTexture* ExportTexture(ImportedTexture^ matTex);
			void ExportAnimations(bool eulerFilter, float filterValue, bool flatInbetween);
			void ExportKeyframedAnimation(ImportedKeyframedAnimation^ parser, FbxString& kTakeName, FbxAnimCurveFilterUnroll* eulerFilter, float filterPrecision, bool flatInbetween);
			void ExportMorphs(bool morphMask, bool flatInbetween);
		};
	};
}
