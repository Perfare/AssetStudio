using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AssetStudio.Studio;

namespace AssetStudio
{
    public class PPtr
    {
        //m_FileID 0 means current file
        public int m_FileID;
        //m_PathID acts more like a hash in some games
        public long m_PathID;
    }

    public static class PPtrHelpers
    {
        public static PPtr ReadPPtr(this AssetsFile sourceFile)
        {
            var result = new PPtr();
            var reader = sourceFile.reader;

            int FileID = reader.ReadInt32();
            if (FileID >= 0 && FileID < sourceFile.sharedAssetsList.Count)
            {
                var sharedFile = sourceFile.sharedAssetsList[FileID];
                var index = sharedFile.Index;
                if (index == -2)
                {
                    var name = sharedFile.fileName.ToUpper();
                    if (!sharedFileIndex.TryGetValue(name, out index))
                    {
                        index = assetsfileList.FindIndex(aFile => aFile.upperFileName == name);
                        sharedFileIndex.Add(name, index);
                    }
                    sharedFile.Index = index;
                }
                result.m_FileID = index;
            }

            result.m_PathID = sourceFile.header.m_Version < 14 ? reader.ReadInt32() : reader.ReadInt64();

            return result;
        }

        public static bool TryGetPD(this List<AssetsFile> assetsfileList, PPtr m_elm, out AssetPreloadData result)
        {
            result = null;

            if (m_elm != null && m_elm.m_FileID >= 0 && m_elm.m_FileID < assetsfileList.Count)
            {
                AssetsFile sourceFile = assetsfileList[m_elm.m_FileID];

                //TryGetValue should be safe because m_PathID is 0 when initialized and PathID values range from 1
                if (sourceFile.preloadTable.TryGetValue(m_elm.m_PathID, out result)) { return true; }
            }

            return false;
        }

        public static bool TryGetTransform(this List<AssetsFile> assetsfileList, PPtr m_elm, out Transform m_Transform)
        {
            m_Transform = null;

            if (m_elm != null && m_elm.m_FileID >= 0 && m_elm.m_FileID < assetsfileList.Count)
            {
                AssetsFile sourceFile = assetsfileList[m_elm.m_FileID];

                if (sourceFile.TransformList.TryGetValue(m_elm.m_PathID, out m_Transform)) { return true; }
            }

            return false;
        }

        public static bool TryGetGameObject(this List<AssetsFile> assetsfileList, PPtr m_elm, out GameObject m_GameObject)
        {
            m_GameObject = null;

            if (m_elm != null && m_elm.m_FileID >= 0 && m_elm.m_FileID < assetsfileList.Count)
            {
                AssetsFile sourceFile = assetsfileList[m_elm.m_FileID];

                if (sourceFile.GameObjectList.TryGetValue(m_elm.m_PathID, out m_GameObject)) { return true; }
            }

            return false;
        }
    }
}
