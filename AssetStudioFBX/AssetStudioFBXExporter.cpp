#include "AssetStudioFBX.h"

namespace AssetStudio
{
	void Fbx::Exporter::Export(String^ path, IImported^ imported, bool eulerFilter, float filterPrecision,
		bool allNodes, bool skins, bool animation, bool blendShape, bool castToBone, float boneSize, float scaleFactor, int versionIndex, bool isAscii)
	{
		FileInfo^ file = gcnew FileInfo(path);
		DirectoryInfo^ dir = file->Directory;
		if (!dir->Exists)
		{
			dir->Create();
		}
		String^ currentDir = Directory::GetCurrentDirectory();
		Directory::SetCurrentDirectory(dir->FullName);
		auto name = Path::GetFileName(path);
		Exporter^ exporter = gcnew Exporter(name, imported, allNodes, skins, castToBone, boneSize, scaleFactor, versionIndex, isAscii);
		if (blendShape)
		{
			exporter->ExportMorphs();
		}
		if (animation)
		{
			exporter->ExportAnimations(eulerFilter, filterPrecision);
		}
		exporter->pExporter->Export(exporter->pScene);
		delete exporter;

		Directory::SetCurrentDirectory(currentDir);
	}

	Fbx::Exporter::Exporter(String^ name, IImported^ imported, bool allNodes, bool skins, bool castToBone, float boneSize, float scaleFactor, int versionIndex, bool isAscii)
	{
		this->imported = imported;
		exportSkins = skins;
		this->boneSize = boneSize;

		cDest = NULL;
		pSdkManager = NULL;
		pScene = NULL;
		pExporter = NULL;
		pMaterials = NULL;
		pTextures = NULL;

		pin_ptr<FbxManager*> pSdkManagerPin = &pSdkManager;
		pin_ptr<FbxScene*> pScenePin = &pScene;
		Init(pSdkManagerPin, pScenePin);

		IOS_REF.SetBoolProp(EXP_FBX_MATERIAL, true);
		IOS_REF.SetBoolProp(EXP_FBX_TEXTURE, true);
		IOS_REF.SetBoolProp(EXP_FBX_EMBEDDED, false);
		IOS_REF.SetBoolProp(EXP_FBX_SHAPE, true);
		IOS_REF.SetBoolProp(EXP_FBX_GOBO, true);
		IOS_REF.SetBoolProp(EXP_FBX_ANIMATION, true);
		IOS_REF.SetBoolProp(EXP_FBX_GLOBAL_SETTINGS, true);

		FbxGlobalSettings& globalSettings = pScene->GetGlobalSettings();
		globalSettings.SetSystemUnit(FbxSystemUnit(scaleFactor));

		if (imported->AnimationList->Count > 0)
		{
			auto ani = imported->AnimationList[0];
			if (ani->SampleRate == 60.0f)
			{
				globalSettings.SetTimeMode(FbxTime::eFrames60);
			}
		}

		cDest = StringToUTF8(name);
		pExporter = FbxExporter::Create(pScene, "");

		int pFileFormat = 0;
		if (versionIndex == 0)
		{
			pFileFormat = 3;
			if (isAscii)
			{
				pFileFormat = 4;
			}
		}
		else
		{
			pExporter->SetFileExportVersion(FBXVersion[versionIndex]);
			if (isAscii)
			{
				pFileFormat = 1;
			}
		}

		if (!pExporter->Initialize(cDest, pFileFormat, pSdkManager->GetIOSettings()))
		{
			throw gcnew Exception(gcnew String("Failed to initialize FbxExporter: ") + gcnew String(pExporter->GetStatus().GetErrorString()));
		}

		framePaths = nullptr;
		if (!allNodes)
		{
			framePaths = SearchHierarchy();
			if (!framePaths)
			{
				return;
			}
		}

		pBindPose = FbxPose::Create(pScene, "BindPose");
		pScene->AddPose(pBindPose);

		frameToNode = gcnew Dictionary<ImportedFrame^, size_t>();
		meshFrames = imported->MeshList != nullptr ? gcnew List<ImportedFrame^>() : nullptr;
		ExportFrame(pScene->GetRootNode(), imported->RootFrame);

		if (imported->MeshList != nullptr)
		{
			SetJointsFromImportedMeshes(castToBone);

			pMaterials = new FbxArray<FbxSurfacePhong*>();
			pTextures = new FbxArray<FbxFileTexture*>();
			pMaterials->Reserve(imported->MaterialList->Count);
			pTextures->Reserve(imported->TextureList->Count);

			for (int i = 0; i < meshFrames->Count; i++)
			{
				auto meshFram = meshFrames[i];
				FbxNode* meshNode = (FbxNode*)frameToNode[meshFram];
				ImportedMesh^ mesh = ImportedHelpers::FindMesh(meshFram->Path, imported->MeshList);
				ExportMesh(meshNode, mesh);
			}
		}
		else
		{
			SetJointsNode(imported->RootFrame, nullptr, true);
		}
	}

	Fbx::Exporter::~Exporter()
	{
		imported = nullptr;
		if (framePaths != nullptr)
		{
			framePaths->Clear();
		}
		if (frameToNode != nullptr)
		{
			frameToNode->Clear();
		}
		if (meshFrames != nullptr)
		{
			meshFrames->Clear();
		}

		if (pMaterials != NULL)
		{
			delete pMaterials;
		}
		if (pTextures != NULL)
		{
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
		if (cDest != NULL)
		{
			delete cDest;
		}
	}

	void Fbx::Exporter::SetJointsNode(ImportedFrame^ frame, HashSet<String^>^ bonePaths, bool castToBone)
	{
		size_t pointer;
		if (frameToNode->TryGetValue(frame, pointer))
		{
			auto pNode = (FbxNode*)pointer;
			if (castToBone)
			{
				FbxSkeleton* pJoint = FbxSkeleton::Create(pScene, "");
				pJoint->Size.Set(FbxDouble(boneSize));
				pJoint->SetSkeletonType(FbxSkeleton::eLimbNode);
				pNode->SetNodeAttribute(pJoint);
			}
			else if (bonePaths->Contains(frame->Path))
			{
				FbxSkeleton* pJoint = FbxSkeleton::Create(pScene, "");
				pJoint->Size.Set(FbxDouble(boneSize));
				pJoint->SetSkeletonType(FbxSkeleton::eLimbNode);
				pNode->SetNodeAttribute(pJoint);

				pJoint = FbxSkeleton::Create(pScene, "");
				pJoint->Size.Set(FbxDouble(boneSize));
				pJoint->SetSkeletonType(FbxSkeleton::eLimbNode);
				pNode->GetParent()->SetNodeAttribute(pJoint);
			}
			else
			{
				FbxNull* pNull = FbxNull::Create(pScene, "");
				if (pNode->GetChildCount() > 0)
				{
					pNull->Look.Set(FbxNull::eNone);
				}

				pNode->SetNodeAttribute(pNull);
			}
		}
		for (int i = 0; i < frame->Count; i++)
		{
			SetJointsNode(frame[i], bonePaths, castToBone);
		}
	}

	HashSet<String^>^ Fbx::Exporter::SearchHierarchy()
	{
		if (imported->MeshList == nullptr || imported->MeshList->Count == 0)
		{
			return nullptr;
		}
		HashSet<String^>^ exportFrames = gcnew HashSet<String^>();
		SearchHierarchy(imported->RootFrame, exportFrames);
		return exportFrames;
	}

	void Fbx::Exporter::SearchHierarchy(ImportedFrame^ frame, HashSet<String^>^ exportFrames)
	{
		ImportedMesh^ meshListSome = ImportedHelpers::FindMesh(frame->Path, imported->MeshList);
		if (meshListSome != nullptr)
		{
			ImportedFrame^ parent = frame;
			while (parent != nullptr)
			{
				exportFrames->Add(parent->Path);
				parent = parent->Parent;
			}

			List<ImportedBone^>^ boneList = meshListSome->BoneList;
			if (boneList != nullptr)
			{
				for (int i = 0; i < boneList->Count; i++)
				{
					if (!exportFrames->Contains(boneList[i]->Path))
					{
						ImportedFrame^ boneParent = imported->RootFrame->FindFrameByPath(boneList[i]->Path);
						while (boneParent != nullptr)
						{
							exportFrames->Add(boneParent->Path);
							boneParent = boneParent->Parent;
						}
					}
				}
			}
		}

		for (int i = 0; i < frame->Count; i++)
		{
			SearchHierarchy(frame[i], exportFrames);
		}
	}

	void Fbx::Exporter::SetJointsFromImportedMeshes(bool castToBone)
	{
		if (!exportSkins)
		{
			return;
		}
		HashSet<String^>^ bonePaths = gcnew HashSet<String^>();
		for (int i = 0; i < imported->MeshList->Count; i++)
		{
			ImportedMesh^ meshList = imported->MeshList[i];
			List<ImportedBone^>^ boneList = meshList->BoneList;
			if (boneList != nullptr)
			{
				for (int j = 0; j < boneList->Count; j++)
				{
					ImportedBone^ bone = boneList[j];
					bonePaths->Add(bone->Path);
				}
			}
		}

		SetJointsNode(imported->RootFrame, bonePaths, castToBone);
	}

	void Fbx::Exporter::ExportFrame(FbxNode* pParentNode, ImportedFrame^ frame)
	{
		if (framePaths == nullptr || framePaths->Contains(frame->Path))
		{
			FbxNode* pFrameNode;
			WITH_MARSHALLED_STRING
			(
				pName,
				frame->Name,
				pFrameNode = FbxNode::Create(pScene, pName);
			);

			pFrameNode->LclScaling.Set(FbxDouble3(frame->LocalScale.X, frame->LocalScale.Y, frame->LocalScale.Z));
			pFrameNode->LclRotation.Set(FbxDouble3(frame->LocalRotation.X, frame->LocalRotation.Y, frame->LocalRotation.Z));
			pFrameNode->LclTranslation.Set(FbxDouble3(frame->LocalPosition.X, frame->LocalPosition.Y, frame->LocalPosition.Z));
			pFrameNode->SetPreferedAngle(pFrameNode->LclRotation.Get());
			pParentNode->AddChild(pFrameNode);
			pBindPose->Add(pFrameNode, pFrameNode->EvaluateGlobalTransform());

			if (imported->MeshList != nullptr && ImportedHelpers::FindMesh(frame->Path, imported->MeshList) != nullptr)
			{
				meshFrames->Add(frame);
			}

			frameToNode->Add(frame, (size_t)pFrameNode);

			for (int i = 0; i < frame->Count; i++)
			{
				ExportFrame(pFrameNode, frame[i]);
			}
		}
	}

	void Fbx::Exporter::ExportMesh(FbxNode* pFrameNode, ImportedMesh^ iMesh)
	{
		List<ImportedBone^>^ boneList = iMesh->BoneList;
		bool hasBones;
		if (exportSkins && boneList != nullptr)
		{
			hasBones = boneList->Count > 0;
		}
		else
		{
			hasBones = false;
		}

		FbxArray<FbxCluster*>* pClusterArray = nullptr;

		try
		{
			if (hasBones)
			{
				pClusterArray = new FbxArray<FbxCluster*>(boneList->Count);

				for (int i = 0; i < boneList->Count; i++)
				{
					auto bone = boneList[i];
					if (bone->Path != nullptr)
					{
						auto frame = imported->RootFrame->FindFrameByPath(bone->Path);
						auto boneNode = (FbxNode*)frameToNode[frame];
						FbxString lClusterName = boneNode->GetNameOnly() + FbxString("Cluster");
						FbxCluster* pCluster = FbxCluster::Create(pScene, lClusterName.Buffer());
						pCluster->SetLink(boneNode);
						pCluster->SetLinkMode(FbxCluster::eTotalOne);
						pClusterArray->Add(pCluster);
					}
					else
					{
						pClusterArray->Add(NULL);
					}
				}
			}

			FbxMesh* pMesh = FbxMesh::Create(pScene, pFrameNode->GetName());
			pFrameNode->SetNodeAttribute(pMesh);

			int vertexCount = 0;
			for (int i = 0; i < iMesh->SubmeshList->Count; i++)
			{
				vertexCount += iMesh->SubmeshList[i]->VertexList->Count;
			}

			pMesh->InitControlPoints(vertexCount);
			FbxVector4* pControlPoints = pMesh->GetControlPoints();

			FbxGeometryElementNormal* lGeometryElementNormal = NULL;
			if (iMesh->hasNormal)
			{
				lGeometryElementNormal = pMesh->CreateElementNormal();
				lGeometryElementNormal->SetMappingMode(FbxGeometryElement::eByControlPoint);
				lGeometryElementNormal->SetReferenceMode(FbxGeometryElement::eDirect);
			}

			FbxGeometryElementUV* lGeometryElementUV = NULL;
			if (iMesh->hasUV)
			{
				lGeometryElementUV = pMesh->CreateElementUV("UV0");
				lGeometryElementUV->SetMappingMode(FbxGeometryElement::eByControlPoint);
				lGeometryElementUV->SetReferenceMode(FbxGeometryElement::eDirect);
			}

			FbxGeometryElementTangent* lGeometryElementTangent = NULL;
			if (iMesh->hasTangent)
			{
				lGeometryElementTangent = pMesh->CreateElementTangent();
				lGeometryElementTangent->SetMappingMode(FbxGeometryElement::eByControlPoint);
				lGeometryElementTangent->SetReferenceMode(FbxGeometryElement::eDirect);
			}

			FbxGeometryElementVertexColor* lGeometryElementVertexColor = NULL;
			if (iMesh->hasColor)
			{
				lGeometryElementVertexColor = pMesh->CreateElementVertexColor();
				lGeometryElementVertexColor->SetMappingMode(FbxGeometryElement::eByControlPoint);
				lGeometryElementVertexColor->SetReferenceMode(FbxGeometryElement::eDirect);
			}

			FbxGeometryElementMaterial* lGeometryElementMaterial = pMesh->CreateElementMaterial();
			lGeometryElementMaterial->SetMappingMode(FbxGeometryElement::eByPolygon);
			lGeometryElementMaterial->SetReferenceMode(FbxGeometryElement::eIndexToDirect);

			int firstVertex = 0;
			for (int i = 0; i < iMesh->SubmeshList->Count; i++)
			{
				ImportedSubmesh^ meshObj = iMesh->SubmeshList[i];
				List<ImportedVertex^>^ vertexList = meshObj->VertexList;
				List<ImportedFace^>^ faceList = meshObj->FaceList;

				int materialIndex = 0;
				ImportedMaterial^ mat = ImportedHelpers::FindMaterial(meshObj->Material, imported->MaterialList);
				if (mat != nullptr)
				{
					char* pMatName = NULL;
					try
					{
						pMatName = StringToUTF8(mat->Name);
						int foundMat = -1;
						for (int j = 0; j < pMaterials->GetCount(); j++)
						{
							FbxSurfacePhong* pMatTemp = pMaterials->GetAt(j);
							if (strcmp(pMatTemp->GetName(), pMatName) == 0)
							{
								foundMat = j;
								break;
							}
						}

						FbxSurfacePhong* pMat;
						if (foundMat >= 0)
						{
							pMat = pMaterials->GetAt(foundMat);
						}
						else
						{
							FbxString lShadingName = "Phong";
							Color diffuse = mat->Diffuse;
							Color ambient = mat->Ambient;
							Color emissive = mat->Emissive;
							Color specular = mat->Specular;
							Color reflection = mat->Reflection;
							pMat = FbxSurfacePhong::Create(pScene, pMatName);
							pMat->Diffuse.Set(FbxDouble3(diffuse.R, diffuse.G, diffuse.B));
							//pMat->DiffuseFactor.Set(FbxDouble(diffuse.A));
							pMat->Ambient.Set(FbxDouble3(ambient.R, ambient.G, ambient.B));
							//pMat->AmbientFactor.Set(FbxDouble(ambient.A));
							pMat->Emissive.Set(FbxDouble3(emissive.R, emissive.G, emissive.B));
							//pMat->EmissiveFactor.Set(FbxDouble(emissive.A));
							pMat->Specular.Set(FbxDouble3(specular.R, specular.G, specular.B));
							//pMat->SpecularFactor.Set(FbxDouble(specular.A));
							pMat->Reflection.Set(FbxDouble3(reflection.R, reflection.G, reflection.B));
							//pMat->ReflectionFactor.Set(FbxDouble(reflection.A));
							pMat->Shininess.Set(FbxDouble(mat->Shininess));
							pMat->TransparencyFactor.Set(FbxDouble(mat->Transparency));
							pMat->ShadingModel.Set(lShadingName);
							pMaterials->Add(pMat);
						}
						materialIndex = pFrameNode->AddMaterial(pMat);

						bool hasTexture = false;

						for each (ImportedMaterialTexture^ texture in mat->Textures)
						{
							auto pTexture = ExportTexture(ImportedHelpers::FindTexture(texture->Name, imported->TextureList));
							if (pTexture != NULL)
							{
								if (texture->Dest == 0)
								{
									LinkTexture(texture, pTexture, pMat->Diffuse);
									hasTexture = true;
								}
								else if (texture->Dest == 1)
								{
									LinkTexture(texture, pTexture, pMat->NormalMap);
									hasTexture = true;
								}
								else if (texture->Dest == 2)
								{
									LinkTexture(texture, pTexture, pMat->Specular);
									hasTexture = true;
								}
								else if (texture->Dest == 3)
								{
									LinkTexture(texture, pTexture, pMat->Bump);
									hasTexture = true;
								}
							}
						}

						if (hasTexture)
						{
							pFrameNode->SetShadingMode(FbxNode::eTextureShading);
						}
					}
					finally
					{
						delete pMatName;
					}
				}

				for (int j = 0; j < vertexList->Count; j++)
				{
					ImportedVertex^ iVertex = vertexList[j];

					Vector3 vertex = iVertex->Vertex;
					pControlPoints[j + firstVertex] = FbxVector4(vertex.X, vertex.Y, vertex.Z, 0);

					if (iMesh->hasNormal)
					{
						Vector3 normal = iVertex->Normal;
						lGeometryElementNormal->GetDirectArray().Add(FbxVector4(normal.X, normal.Y, normal.Z, 0));
					}

					if (iMesh->hasUV)
					{
						array<float>^ uv = iVertex->UV;
						lGeometryElementUV->GetDirectArray().Add(FbxVector2(uv[0], uv[1]));
					}

					if (iMesh->hasTangent)
					{
						Vector4 tangent = iVertex->Tangent;
						lGeometryElementTangent->GetDirectArray().Add(FbxVector4(tangent.X, tangent.Y, tangent.Z, tangent.W));
					}

					if (iMesh->hasColor)
					{
						auto color = iVertex->Color;
						lGeometryElementVertexColor->GetDirectArray().Add(FbxColor(color.R, color.G, color.B, color.A));
					}

					if (hasBones && iVertex->BoneIndices != nullptr)
					{
						auto boneIndices = iVertex->BoneIndices;
						auto weights4 = iVertex->Weights;
						for (int k = 0; k < 4; k++)
						{
							if (boneIndices[k] < boneList->Count && weights4[k] > 0)
							{
								FbxCluster* pCluster = pClusterArray->GetAt(boneIndices[k]);
								if (pCluster)
								{
									pCluster->AddControlPointIndex(j + firstVertex, weights4[k]);
								}
							}
						}
					}
				}

				for (int j = 0; j < faceList->Count; j++)
				{
					ImportedFace^ face = faceList[j];
					pMesh->BeginPolygon(materialIndex);
					pMesh->AddPolygon(face->VertexIndices[0] + firstVertex);
					pMesh->AddPolygon(face->VertexIndices[1] + firstVertex);
					pMesh->AddPolygon(face->VertexIndices[2] + firstVertex);
					pMesh->EndPolygon();
				}

				firstVertex += vertexList->Count;
			}

			if (hasBones)
			{
				FbxSkin* pSkin = FbxSkin::Create(pScene, "");
				FbxAMatrix lMeshMatrix = pFrameNode->EvaluateGlobalTransform();
				for (int j = 0; j < boneList->Count; j++)
				{
					FbxCluster* pCluster = pClusterArray->GetAt(j);
					if (pCluster)
					{
						auto boneMatrix = boneList[j]->Matrix;
						FbxAMatrix lBoneMatrix;
						for (int m = 0; m < 4; m++)
						{
							for (int n = 0; n < 4; n++)
							{
								lBoneMatrix.mData[m][n] = boneMatrix[m, n];
							}
						}

						pCluster->SetTransformMatrix(lMeshMatrix);
						pCluster->SetTransformLinkMatrix(lMeshMatrix * lBoneMatrix.Inverse());

						pSkin->AddCluster(pCluster);
					}
				}

				if (pSkin->GetClusterCount() > 0)
				{
					pMesh->AddDeformer(pSkin);
				}
			}
		}
		finally
		{
			if (pClusterArray != NULL)
			{
				delete pClusterArray;
			}
		}
	}

	FbxFileTexture* Fbx::Exporter::ExportTexture(ImportedTexture^ matTex)
	{
		FbxFileTexture* pTex = NULL;

		if (matTex != nullptr)
		{
			String^ matTexName = matTex->Name;
			char* pTexName = NULL;
			try
			{
				pTexName = StringToUTF8(matTexName);
				int foundTex = -1;
				for (int i = 0; i < pTextures->GetCount(); i++)
				{
					FbxFileTexture* pTexTemp = pTextures->GetAt(i);
					if (strcmp(pTexTemp->GetName(), pTexName) == 0)
					{
						foundTex = i;
						break;
					}
				}

				if (foundTex >= 0)
				{
					pTex = pTextures->GetAt(foundTex);
				}
				else
				{
					pTex = FbxFileTexture::Create(pScene, pTexName);
					pTex->SetFileName(pTexName);
					pTex->SetTextureUse(FbxTexture::eStandard);
					pTex->SetMappingType(FbxTexture::eUV);
					pTex->SetMaterialUse(FbxFileTexture::eModelMaterial);
					pTex->SetSwapUV(false);
					pTex->SetTranslation(0.0, 0.0);
					pTex->SetScale(1.0, 1.0);
					pTex->SetRotation(0.0, 0.0);
					pTextures->Add(pTex);

					FileInfo^ file = gcnew FileInfo(matTex->Name);
					BinaryWriter^ writer = gcnew BinaryWriter(file->Create());
					writer->Write(matTex->Data);
					writer->Close();
				}
			}
			finally
			{
				delete pTexName;
			}
		}

		return pTex;
	}

	void Fbx::Exporter::LinkTexture(ImportedMaterialTexture^ texture, FbxFileTexture* pTexture, FbxProperty& prop)
	{
		pTexture->SetTranslation(texture->Offset.X, texture->Offset.Y);
		pTexture->SetScale(texture->Scale.X, texture->Scale.Y);
		prop.ConnectSrcObject(pTexture);
	}

	void Fbx::Exporter::ExportAnimations(bool eulerFilter, float filterPrecision)
	{
		auto importedAnimationList = imported->AnimationList;
		if (importedAnimationList == nullptr)
		{
			return;
		}

		FbxAnimCurveFilterUnroll* lFilter = eulerFilter ? new FbxAnimCurveFilterUnroll() : NULL;

		for (int i = 0; i < importedAnimationList->Count; i++)
		{
			auto importedAnimation = importedAnimationList[i];
			FbxString kTakeName;
			if (importedAnimation->Name)
			{
				WITH_MARSHALLED_STRING
				(
					pClipName,
					importedAnimation->Name,
					kTakeName = FbxString(pClipName);
				);
			}
			else
			{
				kTakeName = FbxString("Take") + FbxString(i);
			}
			ExportKeyframedAnimation(importedAnimation, kTakeName, lFilter, filterPrecision);
		}
	}

	void Fbx::Exporter::ExportKeyframedAnimation(ImportedKeyframedAnimation^ parser, FbxString& kTakeName, FbxAnimCurveFilterUnroll* eulerFilter, float filterPrecision)
	{
		List<ImportedAnimationKeyframedTrack^>^ pAnimationList = parser->TrackList;

		char* lTakeName = kTakeName.Buffer();

		FbxAnimStack* lAnimStack = FbxAnimStack::Create(pScene, lTakeName);
		FbxAnimLayer* lAnimLayer = FbxAnimLayer::Create(pScene, "Base Layer");
		lAnimStack->AddMember(lAnimLayer);

		for (int j = 0; j < pAnimationList->Count; j++)
		{
			ImportedAnimationKeyframedTrack^ keyframeList = pAnimationList[j];
			if (keyframeList->Path == nullptr)
			{
				continue;
			}
			auto frame = imported->RootFrame->FindFrameByPath(keyframeList->Path);
			if (frame != nullptr)
			{
				FbxNode* pNode = (FbxNode*)frameToNode[frame];

				FbxAnimCurve* lCurveSX = pNode->LclScaling.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
				FbxAnimCurve* lCurveSY = pNode->LclScaling.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
				FbxAnimCurve* lCurveSZ = pNode->LclScaling.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
				FbxAnimCurve* lCurveRX = pNode->LclRotation.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
				FbxAnimCurve* lCurveRY = pNode->LclRotation.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
				FbxAnimCurve* lCurveRZ = pNode->LclRotation.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);
				FbxAnimCurve* lCurveTX = pNode->LclTranslation.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_X, true);
				FbxAnimCurve* lCurveTY = pNode->LclTranslation.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y, true);
				FbxAnimCurve* lCurveTZ = pNode->LclTranslation.GetCurve(lAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z, true);

				lCurveSX->KeyModifyBegin();
				lCurveSY->KeyModifyBegin();
				lCurveSZ->KeyModifyBegin();
				lCurveRX->KeyModifyBegin();
				lCurveRY->KeyModifyBegin();
				lCurveRZ->KeyModifyBegin();
				lCurveTX->KeyModifyBegin();
				lCurveTY->KeyModifyBegin();
				lCurveTZ->KeyModifyBegin();

				FbxTime lTime;

				for each (auto Scaling in keyframeList->Scalings)
				{
					lTime.SetSecondDouble(Scaling->time);

					lCurveSX->KeySet(lCurveSX->KeyAdd(lTime), lTime, Scaling->value.X);
					lCurveSY->KeySet(lCurveSY->KeyAdd(lTime), lTime, Scaling->value.Y);
					lCurveSZ->KeySet(lCurveSZ->KeyAdd(lTime), lTime, Scaling->value.Z);
				}
				for each (auto Rotation in keyframeList->Rotations)
				{
					lTime.SetSecondDouble(Rotation->time);

					lCurveRX->KeySet(lCurveRX->KeyAdd(lTime), lTime, Rotation->value.X);
					lCurveRY->KeySet(lCurveRY->KeyAdd(lTime), lTime, Rotation->value.Y);
					lCurveRZ->KeySet(lCurveRZ->KeyAdd(lTime), lTime, Rotation->value.Z);
				}
				for each (auto Translation in keyframeList->Translations)
				{
					lTime.SetSecondDouble(Translation->time);

					lCurveTX->KeySet(lCurveTX->KeyAdd(lTime), lTime, Translation->value.X);
					lCurveTY->KeySet(lCurveTY->KeyAdd(lTime), lTime, Translation->value.Y);
					lCurveTZ->KeySet(lCurveTZ->KeyAdd(lTime), lTime, Translation->value.Z);
				}

				lCurveSX->KeyModifyEnd();
				lCurveSY->KeyModifyEnd();
				lCurveSZ->KeyModifyEnd();
				lCurveRX->KeyModifyEnd();
				lCurveRY->KeyModifyEnd();
				lCurveRZ->KeyModifyEnd();
				lCurveTX->KeyModifyEnd();
				lCurveTY->KeyModifyEnd();
				lCurveTZ->KeyModifyEnd();

				if (eulerFilter)
				{
					FbxAnimCurve* lCurve[3];
					lCurve[0] = lCurveRX;
					lCurve[1] = lCurveRY;
					lCurve[2] = lCurveRZ;
					eulerFilter->Reset();
					eulerFilter->SetQualityTolerance(filterPrecision);
					eulerFilter->Apply(lCurve, 3);
				}

				//BlendShape
				if (keyframeList->BlendShape != nullptr)
				{
					FbxString channelName;
					WITH_MARSHALLED_STRING
					(
						pClipName,
						keyframeList->BlendShape->ChannelName,
						channelName = FbxString(pClipName);
					);

					auto lGeometry = (FbxGeometry*)pNode->GetNodeAttribute();
					int lBlendShapeDeformerCount = lGeometry->GetDeformerCount(FbxDeformer::eBlendShape);
					if (lBlendShapeDeformerCount > 0)
					{
						FbxBlendShape* lBlendShape = (FbxBlendShape*)lGeometry->GetDeformer(0, FbxDeformer::eBlendShape);
						int lBlendShapeChannelCount = lBlendShape->GetBlendShapeChannelCount();
						for (int lChannelIndex = 0; lChannelIndex < lBlendShapeChannelCount; ++lChannelIndex)
						{
							FbxBlendShapeChannel* lChannel = lBlendShape->GetBlendShapeChannel(lChannelIndex);
							FbxString lChannelName = lChannel->GetNameOnly();
							if (lChannelName == channelName)
							{
								FbxAnimCurve* lAnimCurve = lGeometry->GetShapeChannel(0, lChannelIndex, lAnimLayer, true);
								lAnimCurve->KeyModifyBegin();

								for each (auto keyframe in keyframeList->BlendShape->Keyframes)
								{
									lTime.SetSecondDouble(keyframe->time);
									int lKeyIndex = lAnimCurve->KeyAdd(lTime);
									lAnimCurve->KeySetValue(lKeyIndex, keyframe->value);
									lAnimCurve->KeySetInterpolation(lKeyIndex, FbxAnimCurveDef::eInterpolationCubic);
								}

								lAnimCurve->KeyModifyEnd();
							}
						}
					}
				}
			}
		}
	}

	void Fbx::Exporter::ExportMorphs()
	{
		if (imported->MeshList == nullptr)
		{
			return;
		}
		for each (ImportedMorph^ morph in imported->MorphList)
		{
			auto frame = imported->RootFrame->FindFrameByPath(morph->Path);
			if (frame != nullptr)
			{
				FbxNode* pNode = (FbxNode*)frameToNode[frame];
				FbxMesh* pMesh = pNode->GetMesh();

				FbxBlendShape* lBlendShape = FbxBlendShape::Create(pScene, pMesh->GetNameOnly() + FbxString("BlendShape"));
				pMesh->AddDeformer(lBlendShape);

				for (int i = 0; i < morph->Channels->Count; i++)
				{
					auto channel = morph->Channels[i];

					FbxBlendShapeChannel* lBlendShapeChannel;
					WITH_MARSHALLED_STRING
					(
						pChannelName,
						channel->Name,
						lBlendShapeChannel = FbxBlendShapeChannel::Create(pScene, pChannelName);
					);
					lBlendShape->AddBlendShapeChannel(lBlendShapeChannel);

					for each(ImportedMorphKeyframe^ keyframe in channel->KeyframeList)
					{
						FbxShape* lShape = FbxShape::Create(pScene, FbxString(keyframe->Weight));
						lBlendShapeChannel->AddTargetShape(lShape, keyframe->Weight);

						auto vectorCount = pMesh->GetControlPointsCount();
						FbxVector4* orilVector4 = pMesh->GetControlPoints();
						lShape->InitControlPoints(vectorCount);
						FbxVector4* lVector4 = lShape->GetControlPoints();

						for (int j = 0; j < vectorCount; j++)
						{
							auto vertex = orilVector4[j];
							lVector4[j] = FbxVector4(vertex);
						}
						for (int j = 0; j < keyframe->VertexList->Count; j++)
						{
							auto index = keyframe->VertexList[j]->Index;
							auto vertex = keyframe->VertexList[j]->Vertex->Vertex;
							lVector4[index] = FbxVector4(vertex.X, vertex.Y, vertex.Z, 0);
						}
					}
				}
			}
		}
	}
}