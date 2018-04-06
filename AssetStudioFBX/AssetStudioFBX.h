#pragma once

#ifdef IOS_REF
#undef  IOS_REF
#define IOS_REF (*(pSdkManager->GetIOSettings()))
#endif

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Runtime::InteropServices;
using namespace SharpDX;

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

namespace AssetStudio {

	public ref class Fbx
	{
	public:
		static Vector3 QuaternionToEuler(Quaternion q);
		static Quaternion EulerToQuaternion(Vector3 v);

		ref class Exporter
		{
		public:
			static void Export(String^ path, IImported^ imported, bool EulerFilter, float filterPrecision, String^ exportFormat, bool allFrames, bool allBones, bool skins, float boneSize, bool flatInbetween, bool compatibility);
			static void ExportMorph(String^ path, IImported^ imported, String^ exportFormat, bool morphMask, bool flatInbetween, bool skins, float boneSize, bool compatibility);

		private:
			HashSet<String^>^ frameNames;
			HashSet<String^>^ meshNames;
			bool EulerFilter;
			float filterPrecision;
			bool exportSkins;
			bool embedMedia;
			float boneSize;

			IImported^ imported;

			char* cDest;
			char* cFormat;
			FbxManager* pSdkManager;
			FbxScene* pScene;
			FbxExporter* pExporter;
			FbxArray<FbxSurfacePhong*>* pMaterials;
			FbxArray<FbxFileTexture*>* pTextures;
			FbxArray<FbxNode*>* pMeshNodes;

			~Exporter();
			!Exporter();
			void Fbx::Exporter::LinkTexture(ImportedMaterial^ mat, int attIndex, FbxFileTexture* pTexture, FbxProperty& prop);
			void SetJointsNode(FbxNode* pNode, HashSet<String^>^ boneNames, bool allBones);

			Exporter(String^ path, IImported^ imported, String^ exportFormat, bool allFrames, bool allBones, bool skins, float boneSize, bool compatibility, bool normals);
			HashSet<String^>^ SearchHierarchy();
			void SearchHierarchy(ImportedFrame^ frame, HashSet<String^>^ exportFrames);
			void SetJointsFromImportedMeshes(bool allBones);
			void ExportFrame(FbxNode* pParentNode, ImportedFrame^ frame);
			void ExportMesh(FbxNode* pFrameNode, ImportedMesh^ meshList, bool normals);
			FbxFileTexture* ExportTexture(ImportedTexture^ matTex, FbxMesh* pMesh);
			void ExportAnimations(bool EulerFilter, float filterValue, bool flatInbetween);
			void ExportKeyframedAnimation(ImportedKeyframedAnimation^ parser, FbxString& kTakeName, FbxAnimCurveFilterUnroll* EulerFilter, float filterPrecision,
				FbxPropertyT<FbxDouble3>& scale, FbxPropertyT<FbxDouble4>& rotate, FbxPropertyT<FbxDouble3>& translate, List<String^>^ pNotFound);
			void ExportSampledAnimation(ImportedSampledAnimation^ parser, FbxString& kTakeName, FbxAnimCurveFilterUnroll* EulerFilter, float filterPrecision, bool flatInbetween,
				FbxPropertyT<FbxDouble3>& scale, FbxPropertyT<FbxDouble4>& rotate, FbxPropertyT<FbxDouble3>& translate, List<String^>^ pNotFound);
			void ExportMorphs(IImported^ imported, bool morphMask, bool flatInbetween);
		};

	private:
		ref class InterpolationHelper
		{
		private:
			FbxScene * pScene;
			FbxAnimLayer* pAnimLayer;
			FbxAnimEvaluator* pAnimEvaluator;

			FbxAnimCurveDef::EInterpolationType interpolationMethod;
			FbxAnimCurveFilterUnroll* lFilter;
			float filterPrecision;

			FbxPropertyT<FbxDouble3>* scale, *translate;
			FbxPropertyT<FbxDouble4>* rotate;
			FbxAnimCurve* pScaleCurveX, *pScaleCurveY, *pScaleCurveZ,
				*pRotateCurveX, *pRotateCurveY, *pRotateCurveZ, *pRotateCurveW,
				*pTranslateCurveX, *pTranslateCurveY, *pTranslateCurveZ;

			array<FbxAnimCurve*>^ allCurves;

		public:
			static const char* pScaleName = "Scale";
			static const char* pRotateName = "Rotate";
			static const char* pTranslateName = "Translate";
		};

		static char* StringToCharArray(String^ s);
		static void Init(FbxManager** pSdkManager, FbxScene** pScene);
	};
}
