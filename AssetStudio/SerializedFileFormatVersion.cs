using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetStudio
{
    public enum SerializedFileFormatVersion
    {
        kUnsupported = 1,
        kUnknown_2 = 2,
        kUnknown_3 = 3,
        /// <summary>
        /// 1.2.0 to 2.0.0
        /// </summary>
        kUnknown_5 = 5,
        /// <summary>
        /// 2.1.0 to 2.6.1
        /// </summary>
        kUnknown_6 = 6,
        /// <summary>
        /// 3.0.0b
        /// </summary>
        kUnknown_7 = 7,
        /// <summary>
        /// 3.0.0 to 3.4.2
        /// </summary>
        kUnknown_8 = 8,
        /// <summary>
        /// 3.5.0 to 4.7.2
        /// </summary>
        kUnknown_9 = 9,
        /// <summary>
        /// 5.0.0aunk1
        /// </summary>
        kUnknown_10 = 10,
        /// <summary>
        /// 5.0.0aunk2
        /// </summary>
        kHasScriptTypeIndex = 11,
        /// <summary>
        /// 5.0.0aunk3
        /// </summary>
        kUnknown_12 = 12,
        /// <summary>
        /// 5.0.0aunk4
        /// </summary>
        kHasTypeTreeHashes = 13,
        /// <summary>
        /// 5.0.0unk
        /// </summary>
        kUnknown_14 = 14,
        /// <summary>
        /// 5.0.1 to 5.4.0
        /// </summary>
        kSupportsStrippedObject = 15,
        /// <summary>
        /// 5.5.0a
        /// </summary>
        kRefactoredClassId = 16,
        /// <summary>
        /// 5.5.0unk to 2018.4
        /// </summary>
        kRefactorTypeData = 17,
        /// <summary>
        /// 2019.1a
        /// </summary>
        kRefactorShareableTypeTreeData = 18,
        /// <summary>
        /// 2019.1unk
        /// </summary>
        kTypeTreeNodeWithTypeFlags = 19,
        /// <summary>
        /// 2019.2
        /// </summary>
        kSupportsRefObject = 20,
        /// <summary>
        /// 2019.3 to 2019.4
        /// </summary>
        kStoresTypeDependencies = 21,
        /// <summary>
        /// 2020.1 to x
        /// </summary>
        kLargeFilesSupport = 22
    }
}
