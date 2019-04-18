#pragma once

#ifdef IOS_REF
#undef  IOS_REF
#define IOS_REF (*(pSdkManager->GetIOSettings()))
#endif

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

#define WITH_MARSHALLED_STRING(name,str,block)\
	{ \
		char* name; \
		try \
		{ \
			name = StringToCharArray(str); \
			block \
		} \
		finally \
		{ \
			Marshal::FreeHGlobal((IntPtr)name); \
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
		static char* StringToCharArray(String^ s);
		static void Init(FbxManager** pSdkManager, FbxScene** pScene);

		ref class Exporter
		{
		public:
			static void Export(String^ path, IImported^ imported, bool eulerFilter, float filterPrecision, bool allFrames, bool allBones, bool skins, float boneSize, float scaleFactor, bool flatInbetween, int versionIndex, bool isAscii);
			static void ExportMorph(String^ path, IImported^ imported, bool morphMask, bool flatInbetween, bool skins, float boneSize, float scaleFactor, int versionIndex, bool isAscii);

		private:
			HashSet<String^>^ frameNames;
			bool exportSkins;
			float boneSize;

			IImported^ imported;

			char* cDest;
			FbxManager* pSdkManager;
			FbxScene* pScene;
			FbxExporter* pExporter;
			FbxArray<FbxSurfacePhong*>* pMaterials;
			FbxArray<FbxFileTexture*>* pTextures;
			FbxArray<FbxNode*>* pMeshNodes;
			FbxPose* pBindPose;

			Exporter(String^ path, IImported^ imported, bool allFrames, bool allBones, bool skins, float boneSize, float scaleFactor, int versionIndex, bool isAscii, bool normals);
			~Exporter();

			void Exporter::LinkTexture(ImportedMaterialTexture^ texture, FbxFileTexture* pTexture, FbxProperty& prop);
			void SetJointsNode(FbxNode* pNode, HashSet<String^>^ boneNames, bool allBones);
			HashSet<String^>^ SearchHierarchy();
			void SearchHierarchy(ImportedFrame^ frame, HashSet<String^>^ exportFrames);
			void SetJointsFromImportedMeshes(bool allBones);
			void ExportFrame(FbxNode* pParentNode, ImportedFrame^ frame);
			void ExportMesh(FbxNode* pFrameNode, ImportedMesh^ meshList, bool normals);
			FbxNode* FindNodeByPath(String ^ path);
			FbxFileTexture* ExportTexture(ImportedTexture^ matTex);
			void ExportAnimations(bool eulerFilter, float filterValue, bool flatInbetween);
			void ExportKeyframedAnimation(ImportedKeyframedAnimation^ parser, FbxString& kTakeName, FbxAnimCurveFilterUnroll* eulerFilter, float filterPrecision, bool flatInbetween);
			void ExportMorphs(IImported^ imported, bool morphMask, bool flatInbetween);
		};
	};
}
