using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SpirV
{
	public class Module
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct FloatUIntUnion
		{
			[FieldOffset(0)]
			public uint Int;
			[FieldOffset(0)]
			public float Float;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct DoubleULongUnion
		{
			[FieldOffset(0)]
			public ulong Long;
			[FieldOffset(0)]
			public double Double;
		}

		public Module(ModuleHeader header, IReadOnlyList<ParsedInstruction> instructions)
		{
			Header = header;
			Instructions = instructions;

			Read(Instructions, objects_);
		}

		public static bool IsDebugInstruction(ParsedInstruction instruction)
		{
			return debugInstructions_.Contains(instruction.Instruction.Name);
		}

		private static void Read(IReadOnlyList<ParsedInstruction> instructions, Dictionary<uint, ParsedInstruction> objects)
		{
			// Debug instructions can be only processed after everything
			// else has been parsed, as they may reference types which haven't
			// been seen in the file yet
			List<ParsedInstruction> debugInstructions = new List<ParsedInstruction>();
			// Entry points contain forward references
			// Those need to be resolved afterwards
			List<ParsedInstruction> entryPoints = new List<ParsedInstruction>();
			
			foreach (var instruction in instructions)
			{
				if (IsDebugInstruction(instruction))
				{
					debugInstructions.Add(instruction);
					continue;
				}
				if (instruction.Instruction is OpEntryPoint)
				{
					entryPoints.Add(instruction);
					continue;
				}

				if (instruction.Instruction.Name.StartsWith("OpType", StringComparison.Ordinal))
				{
					ProcessTypeInstruction(instruction, objects);
				}

				instruction.ResolveResultType(objects);
				if (instruction.HasResult)
				{
					objects[instruction.ResultId] = instruction;
				}

				switch (instruction.Instruction)
				{
					// Constants require that the result type has been resolved
					case OpSpecConstant sc:
					case OpConstant oc:
						{
							Type t = instruction.ResultType;
							Debug.Assert (t != null);
							Debug.Assert (t is ScalarType);
							
							object constant = ConvertConstant(instruction.ResultType as ScalarType, instruction.Words, 3);
							instruction.Operands[2].Value = constant;
							instruction.Value = constant;
						}
						break;
				}
			}

			foreach (ParsedInstruction instruction in debugInstructions)
			{
				switch (instruction.Instruction)
				{
					case OpMemberName mn:
						{
							StructType t = (StructType)objects[instruction.Words[1]].ResultType;
							t.SetMemberName((uint)instruction.Operands[1].Value, (string)instruction.Operands[2].Value);
						}
						break;

					case OpName n:
						{
							// We skip naming objects we don't know about
							ParsedInstruction t = objects[instruction.Words[1]];
							t.Name = (string)instruction.Operands[1].Value;
						}
						break;
				}
			}

			foreach (ParsedInstruction instruction in instructions)
			{
				instruction.ResolveReferences(objects);
			}
		}

		public static Module ReadFrom(Stream stream)
		{
			BinaryReader br = new BinaryReader(stream);
			Reader reader = new Reader(br);

			uint versionNumber = reader.ReadDWord();
			int majorVersion = (int)(versionNumber >> 16);
			int minorVersion = (int)((versionNumber >> 8) & 0xFF);
			Version version = new Version(majorVersion, minorVersion);

			uint generatorMagicNumber = reader.ReadDWord();
			int generatorToolId = (int)(generatorMagicNumber >> 16);
			string generatorVendor = "unknown";
			string generatorName = null;

			if (Meta.Tools.ContainsKey(generatorToolId))
			{
				Meta.ToolInfo toolInfo = Meta.Tools[generatorToolId];
				generatorVendor = toolInfo.Vendor;
				if (toolInfo.Name != null)
				{
					generatorName = toolInfo.Name;
				}
			}

			// Read header
			ModuleHeader header = new ModuleHeader();
			header.Version = version;
			header.GeneratorName = generatorName;
			header.GeneratorVendor = generatorVendor;
			header.GeneratorVersion = (int)(generatorMagicNumber & 0xFFFF);
			header.Bound = reader.ReadDWord();
			header.Reserved = reader.ReadDWord();

			List<ParsedInstruction> instructions = new List<ParsedInstruction>();
			while (!reader.EndOfStream)
			{
				uint instructionStart = reader.ReadDWord ();
				ushort wordCount = (ushort)(instructionStart >> 16);
				int opCode = (int)(instructionStart & 0xFFFF);

				uint[] words = new uint[wordCount];
				words[0] = instructionStart;
				for (ushort i = 1; i < wordCount; ++i)
				{
					words[i] = reader.ReadDWord();
				}

				ParsedInstruction instruction = new ParsedInstruction(opCode, words);
				instructions.Add(instruction);
			}

			return new Module(header, instructions);
		}

		/// <summary>
		/// Collect types from OpType* instructions
		/// </summary>
		private static void ProcessTypeInstruction(ParsedInstruction i, IReadOnlyDictionary<uint, ParsedInstruction> objects)
		{
			switch (i.Instruction)
			{
				case OpTypeInt t:
					{
						i.ResultType = new IntegerType((int)i.Words[2], i.Words[3] == 1u);
					}
					break;

				case OpTypeFloat t:
					{
						i.ResultType = new FloatingPointType((int)i.Words[2]);
					}
					break;

				case OpTypeVector t:
					{
						i.ResultType = new VectorType((ScalarType)objects[i.Words[2]].ResultType, (int)i.Words[3]);
					}
					break;

				case OpTypeMatrix t:
					{
						i.ResultType = new MatrixType((VectorType)objects[i.Words[2]].ResultType, (int)i.Words[3]);
					}
					break;

				case OpTypeArray t:
					{
						object constant = objects[i.Words[3]].Value;
						int size = 0;

						switch (constant)
						{
							case ushort u16:
								size = u16;
								break;

							case uint u32:
								size = (int)u32;
								break;

							case ulong u64:
								size = (int)u64;
								break;

							case short i16:
								size = i16;
								break;

							case int i32:
								size = i32;
								break;

							case long i64:
								size = (int)i64;
								break;
						}

						i.ResultType = new ArrayType(objects[i.Words[2]].ResultType, size);
					}
					break;

				case OpTypeRuntimeArray t:
					{
						i.ResultType = new RuntimeArrayType((Type)objects[i.Words[2]].ResultType);
					}
					break;

				case OpTypeBool t:
					{
						i.ResultType = new BoolType();
					}
					break;

				case OpTypeOpaque t:
					{
						i.ResultType = new OpaqueType();
					}
					break;

				case OpTypeVoid t:
					{
						i.ResultType = new VoidType();
					}
					break;

				case OpTypeImage t:
					{
						Type sampledType = objects[i.Operands[1].GetId ()].ResultType;
						Dim dim = i.Operands[2].GetSingleEnumValue<Dim>();
						uint depth = (uint)i.Operands[3].Value;
						bool isArray = (uint)i.Operands[4].Value != 0;
						bool isMultiSampled = (uint)i.Operands[5].Value != 0;
						uint sampled = (uint)i.Operands[6].Value;
						ImageFormat imageFormat = i.Operands[7].GetSingleEnumValue<ImageFormat>();

						i.ResultType = new ImageType(sampledType,
							dim,
							(int)depth, isArray, isMultiSampled,
							(int)sampled, imageFormat,
							i.Operands.Count > 8 ? i.Operands[8].GetSingleEnumValue<AccessQualifier>() : AccessQualifier.ReadOnly);
					}
					break;

				case OpTypeSampler st:
					{
						i.ResultType = new SamplerType();
						break;
					}

				case OpTypeSampledImage t:
					{
						i.ResultType = new SampledImageType((ImageType)objects[i.Words[2]].ResultType);
					}
					break;

				case OpTypeFunction t:
					{
						List<Type> parameterTypes = new List<Type>();
						for (int j = 3; j < i.Words.Count; ++j)
						{
							parameterTypes.Add(objects[i.Words[j]].ResultType);
						}
						i.ResultType = new FunctionType(objects[i.Words[2]].ResultType, parameterTypes);
					}
					break;

				case OpTypeForwardPointer t:
					{
						// We create a normal pointer, but with unspecified type
						// This will get resolved later on
						i.ResultType = new PointerType((StorageClass)i.Words[2]);
					}
					break;

				case OpTypePointer t:
					{
						if (objects.ContainsKey(i.Words[1]))
						{
							// If there is something present, it must have been
							// a forward reference. The storage type must
							// match
							PointerType pt = (PointerType)i.ResultType;
							Debug.Assert (pt != null);
							Debug.Assert (pt.StorageClass == (StorageClass)i.Words[2]);
							pt.ResolveForwardReference (objects[i.Words[3]].ResultType);
						}
						else
						{
							i.ResultType = new PointerType((StorageClass)i.Words[2], objects[i.Words[3]].ResultType);
						}
					}
					break;

				case OpTypeStruct t:
					{
						List<Type> memberTypes = new List<Type>();
						for (int j = 2; j < i.Words.Count; ++j)
						{
							memberTypes.Add(objects[i.Words[j]].ResultType);
						}
						i.ResultType = new StructType(memberTypes);
					}
					break;
			}
		}

		private static object ConvertConstant(ScalarType type, IReadOnlyList<uint> words, int index)
		{
			switch (type)
			{
				case IntegerType i:
					{
						if (i.Signed)
						{
							if (i.Width == 16)
							{
								return unchecked((short)(words[index]));
							}
							else if (i.Width == 32)
							{
								return unchecked((int)(words[index]));
							}
							else if (i.Width == 64)
							{
								return unchecked((long)(words[index] | (ulong)(words[index + 1]) << 32));
							}
						}
						else
						{
							if (i.Width == 16)
							{
								return unchecked((ushort)(words[index]));
							}
							else if (i.Width == 32)
							{
								return words[index];
							}
							else if (i.Width == 64)
							{
								return words[index] | (ulong)(words[index + 1]) << 32;
							}
						}

						throw new Exception ("Cannot construct integer literal.");
					}

				case FloatingPointType f:
					{
						if (f.Width == 32)
						{
							return new FloatUIntUnion { Int = words[0] }.Float;
						}
						else if (f.Width == 64)
						{
							return new DoubleULongUnion { Long = (words[index] | (ulong)(words[index + 1]) << 32) }.Double;
						}
						else
						{
							throw new Exception("Cannot construct floating point literal.");
						}
					}
			}

			return null;
		}

		public ModuleHeader Header { get; }
		public IReadOnlyList<ParsedInstruction> Instructions { get; }

		private static HashSet<string> debugInstructions_ = new HashSet<string>
		{
			"OpSourceContinued",
			"OpSource",
			"OpSourceExtension",
			"OpName",
			"OpMemberName",
			"OpString",
			"OpLine",
			"OpNoLine",
			"OpModuleProcessed"
		};

		private readonly Dictionary<uint, ParsedInstruction> objects_ = new Dictionary<uint, ParsedInstruction>();
	}
}
