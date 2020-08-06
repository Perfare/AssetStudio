#pragma once

#include <cstdint>
#include <string>
#include <unordered_set>

namespace fbxsdk
{
	class FbxManager;
	class FbxScene;
	class FbxExporter;
	template<typename T, const int Alignment = 16>
	class FbxArray;
	class FbxFileTexture;
	class FbxSurfacePhong;
	class FbxPose;
}

struct AsFbxContext
{

	fbxsdk::FbxManager* pSdkManager;
	fbxsdk::FbxScene* pScene;
	fbxsdk::FbxArray<fbxsdk::FbxFileTexture*>* pTextures;
	fbxsdk::FbxArray<fbxsdk::FbxSurfacePhong*>* pMaterials;
	fbxsdk::FbxExporter* pExporter;
	fbxsdk::FbxPose* pBindPose;

	std::unordered_set<std::string> framePaths;

	AsFbxContext();
	~AsFbxContext();
};
