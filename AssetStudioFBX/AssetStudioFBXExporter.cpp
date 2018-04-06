#include <fbxsdk.h>
#include <fbxsdk/fileio/fbxiosettings.h>
#include "AssetStudioFBX.h"

namespace AssetStudio
{
	Fbx::Exporter::~Exporter()
	{
		this->!Exporter();
	}

	Fbx::Exporter::!Exporter()
	{
		if (pMeshNodes != NULL)
		{
			delete pMeshNodes;
		}
		if (pMaterials != NULL)
		{
			delete pMaterials;
		}
		if (pTextures != NULL)
		{
			if (embedMedia)
			{
				for (int i = 0; i < pTextures->GetCount(); i++)
				{
					FbxFileTexture* tex = pTextures->GetAt(i);
					File::Delete(gcnew String(tex->GetFileName()));
				}
			}
			delete pTextures;
		}
		if (pExporter != NULL)
		{
			pExporter->Destroy();
		}
		if (pScene != NULL)
		{
			pScene->Destroy();
		}
		if (pSdkManager != NULL)
		{
			pSdkManager->Destroy();
		}
		if (cFormat != NULL)
		{
			Marshal::FreeHGlobal((IntPtr)cFormat);
		}
		if (cDest != NULL)
		{
			Marshal::FreeHGlobal((IntPtr)cDest);
		}
	}

	void Fbx::Exporter::SetJointsNode(FbxNode* pNode, HashSet<String^>^ boneNames, bool allBones)
	{
		String^ nodeName = gcnew String(pNode->GetName());
		if (allBones || boneNames->Contains(nodeName))
		{
			FbxSkeleton* pJoint = FbxSkeleton::Create(pSdkManager, "");
			pJoint->Size.Set((double)boneSize);
			pJoint->SetSkeletonType(FbxSkeleton::eLimbNode);
			pNode->SetNodeAttribute(pJoint);
		}
		else
		{
			FbxNull* pNull = FbxNull::Create(pSdkManager, "");
			if (pNode->GetChildCount() > 0)
			{
				pNull->Look.Set(FbxNull::eNone);
			}

			pNode->SetNodeAttribute(pNull);
		}

		for (int i = 0; i < pNode->GetChildCount(); i++)
		{
			SetJointsNode(pNode->GetChild(i), boneNames, allBones);
		}
	}
}