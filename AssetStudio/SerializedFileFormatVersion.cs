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
        kUnknown_5 = 5,
        kUnknown_6 = 6,
        kUnknown_7 = 7,
        kUnknown_8 = 8,
        kUnknown_9 = 9,
        kUnknown_10 = 10,
        kHasScriptTypeIndex = 11,
        kUnknown_12 = 12,
        kHasTypeTreeHashes = 13,
        kUnknown_14 = 14,
        kSupportsStrippedObject = 15,
        kRefactoredClassId = 16,
        kRefactorTypeData = 17,
        kRefactorShareableTypeTreeData = 18,
        kTypeTreeNodeWithTypeFlags = 19,
        kSupportsRefObject = 20,
        kStoresTypeDependencies = 21,
        kLargeFilesSupport = 22
    }
}
