using System;
using System.Collections.Generic;
using System.Text;

namespace SpirV
{
	public struct ModuleHeader
	{
		public Version Version { get; set; }
		public string GeneratorVendor { get; set; }
		public string GeneratorName { get; set; }
		public int GeneratorVersion { get; set; }
		public uint Bound { get; set; }
		public uint Reserved { get; set; }
	}

	[Flags]
	public enum DisassemblyOptions
	{
		None,
		ShowTypes,
		ShowNames,
		Default = ShowTypes | ShowNames
	}

	public class Disassembler
	{
		public string Disassemble (Module module)
		{
			return Disassemble(module, DisassemblyOptions.Default);
		}

		public string Disassemble(Module module, DisassemblyOptions options)
		{
			m_sb.AppendLine("; SPIR-V");
			m_sb.Append("; Version: ").Append(module.Header.Version).AppendLine();
			if (module.Header.GeneratorName == null)
			{
				m_sb.Append("; Generator: unknown; ").Append(module.Header.GeneratorVersion).AppendLine();
			}
			else
			{
				m_sb.Append("; Generator: ").Append(module.Header.GeneratorVendor).Append(' ').
					Append(module.Header.GeneratorName).Append("; ").Append(module.Header.GeneratorVersion).AppendLine();
			}
			m_sb.Append("; Bound: ").Append(module.Header.Bound).AppendLine();
			m_sb.Append("; Schema: ").Append(module.Header.Reserved).AppendLine();

			string[] lines = new string[module.Instructions.Count + 1];
			lines[0] = m_sb.ToString();
			m_sb.Clear();

			for (int i = 0; i < module.Instructions.Count; i++)
			{
				ParsedInstruction instruction = module.Instructions[i];
				PrintInstruction(m_sb, instruction, options);
				lines[i + 1] = m_sb.ToString();
				m_sb.Clear();
			}

			int longestPrefix = 0;
			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				longestPrefix = Math.Max(longestPrefix, line.IndexOf('='));
				if (longestPrefix > 50)
				{
					longestPrefix = 50;
					break;
				}
			}

			m_sb.Append(lines[0]);
			for (int i = 1; i < lines.Length; i++)
			{
				string line = lines[i];
				int index = line.IndexOf('=');
				if (index == -1)
				{
					m_sb.Append(' ', longestPrefix + 4);
					m_sb.Append(line);
				}
				else
				{
					int pad = Math.Max(0, longestPrefix - index);
					m_sb.Append(' ', pad);
					m_sb.Append(line, 0, index);
					m_sb.Append('=');
					m_sb.Append(line, index + 1, line.Length - index - 1);
				}
				m_sb.AppendLine();
			}

			string result = m_sb.ToString();
			m_sb.Clear();
			return result;
		}

		private static void PrintInstruction(StringBuilder sb, ParsedInstruction instruction, DisassemblyOptions options)
		{
			if (instruction.Operands.Count == 0)
			{
				sb.Append(instruction.Instruction.Name);
				return;
			}

			int currentOperand = 0;
			if (instruction.Instruction.Operands[currentOperand].Type is IdResultType)
			{
				if (options.HasFlag(DisassemblyOptions.ShowTypes))
				{
					instruction.ResultType.ToString(sb).Append(' ');
				}
				++currentOperand;
			}

			if (currentOperand < instruction.Operands.Count && instruction.Instruction.Operands[currentOperand].Type is IdResult)
			{
				if (!options.HasFlag(DisassemblyOptions.ShowNames) || string.IsNullOrWhiteSpace(instruction.Name))
				{
					PrintOperandValue(sb, instruction.Operands[currentOperand].Value, options);
				}
				else
				{
					sb.Append(instruction.Name);
				}
				sb.Append(" = ");

				++currentOperand;
			}

			sb.Append(instruction.Instruction.Name);
			sb.Append(' ');

			for (; currentOperand < instruction.Operands.Count; ++currentOperand)
			{
				PrintOperandValue(sb, instruction.Operands[currentOperand].Value, options);
				sb.Append(' ');
			}
		}

		private static void PrintOperandValue(StringBuilder sb, object value, DisassemblyOptions options)
		{
			switch (value)
			{
				case System.Type t:
					sb.Append(t.Name);
					break;

				case string s:
					{
						sb.Append('"');
						sb.Append(s);
						sb.Append('"');
					}
					break;

				case ObjectReference or:
					{
						if (options.HasFlag(DisassemblyOptions.ShowNames) && or.Reference != null && !string.IsNullOrWhiteSpace(or.Reference.Name))
						{
							sb.Append(or.Reference.Name);
						}
						else
						{
							or.ToString(sb);
						}
					}
					break;

				case IBitEnumOperandValue beov:
					PrintBitEnumValue(sb, beov, options);
					break;

				case IValueEnumOperandValue veov:
					PrintValueEnumValue(sb, veov, options);
					break;

				case VaryingOperandValue varOpVal:
					varOpVal.ToString(sb);
					break;

				default:
					sb.Append(value);
					break;
			}
		}

		private static void PrintBitEnumValue(StringBuilder sb, IBitEnumOperandValue enumOperandValue, DisassemblyOptions options)
		{
			foreach (uint key in enumOperandValue.Values.Keys)
			{
				sb.Append(enumOperandValue.EnumerationType.GetEnumName(key));
				IReadOnlyList<object> value = enumOperandValue.Values[key];
				if (value.Count != 0)
				{
					sb.Append(' ');
					foreach (object v in value)
					{
						PrintOperandValue(sb, v, options);
					}
				}
			}
		}

		private static void PrintValueEnumValue(StringBuilder sb, IValueEnumOperandValue valueOperandValue, DisassemblyOptions options)
		{
			sb.Append(valueOperandValue.Key);
			if (valueOperandValue.Value is IList<object> valueList && valueList.Count > 0)
			{
				sb.Append(' ');
				foreach (object v in valueList)
				{
					PrintOperandValue(sb, v, options);
				}
			}
		}

		private readonly StringBuilder m_sb = new StringBuilder();
	}
}
