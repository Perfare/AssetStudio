using System.Collections.Generic;

namespace SpirV
{
	internal class Meta
    {
        public class ToolInfo
        {
            public ToolInfo(string vendor)
            {
                Vendor = vendor;
            }

            public ToolInfo(string vendor, string name)
            {
                Vendor = vendor;
                Name = name;
            }

            public string Name { get; }
            public string Vendor { get; }
        }

		public static uint MagicNumber => 119734787U;
		public static uint Version => 66048U;
		public static uint Revision => 2U;
		public static uint OpCodeMask => 65535U;
		public static uint WordCountShift => 16U;

		public static IReadOnlyDictionary<int, ToolInfo> Tools => toolInfos_;

		private readonly static Dictionary<int, ToolInfo> toolInfos_ = new Dictionary<int, ToolInfo>
		{
			{ 0, new ToolInfo("Khronos") },
			{ 1, new ToolInfo("LunarG") },
			{ 2, new ToolInfo("Valve") },
			{ 3, new ToolInfo("Codeplay") },
			{ 4, new ToolInfo("NVIDIA") },
			{ 5, new ToolInfo("ARM") },
			{ 6, new ToolInfo("Khronos", "LLVM/SPIR-V Translator") },
			{ 7, new ToolInfo("Khronos", "SPIR-V Tools Assembler") },
			{ 8, new ToolInfo("Khronos", "Glslang Reference Front End") },
			{ 9, new ToolInfo("Qualcomm") },
			{ 10, new ToolInfo("AMD") },
			{ 11, new ToolInfo("Intel") },
			{ 12, new ToolInfo("Imagination") },
			{ 13, new ToolInfo("Google", "Shaderc over Glslang") },
			{ 14, new ToolInfo("Google", "spiregg") },
			{ 15, new ToolInfo("Google", "rspirv") },
			{ 16, new ToolInfo("X-LEGEND", "Mesa-IR/SPIR-V Translator") },
			{ 17, new ToolInfo("Khronos", "SPIR-V Tools Linker") },
		};
	}
}