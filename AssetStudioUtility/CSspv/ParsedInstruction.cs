using System;
using System.Collections.Generic;
using System.Text;

namespace SpirV
{
	public class ParsedOperand
	{
		public ParsedOperand(IReadOnlyList<uint> words, int index, int count, object value, Operand operand)
		{
			uint[] array = new uint[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = words[index + i];
			}

			Words = array;
			Value = value;
			Operand = operand;
		}

		public T GetSingleEnumValue<T>()
			where T : Enum
		{
			IValueEnumOperandValue v = (IValueEnumOperandValue)Value;
			if (v.Value.Count == 0)
			{
				// If there's no value at all, the enum is probably something like ImageFormat.
				// In which case we just return the enum value
				return (T)v.Key;
			}
			else
			{
				// This means the enum has a value attached to it, so we return the attached value
				return (T)((IValueEnumOperandValue)Value).Value[0];
			}
		}

		public uint GetId()
		{
			return ((ObjectReference)Value).Id;
		}

		public T GetBitEnumValue<T>()
			where T : Enum
		{
			var v = Value as IBitEnumOperandValue;

			uint result = 0;
			foreach (var k in v.Values.Keys)
			{
				result |= k;
			}

			return (T)(object)result;
		}

		public IReadOnlyList<uint> Words { get; }
		public object Value { get; set; }
		public Operand Operand { get; }
	}

	public class VaryingOperandValue
	{
		public VaryingOperandValue(IReadOnlyList<object> values)
		{
			Values = values;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			ToString(sb);
			return sb.ToString();
		}

		public StringBuilder ToString(StringBuilder sb)
		{
			for (int i = 0; i < Values.Count; ++i)
			{
				if (Values[i] is ObjectReference objRef)
				{
					objRef.ToString(sb);
				}
				else
				{
					sb.Append(Values[i]);
				}
				if (i < (Values.Count - 1))
				{
					sb.Append(' ');
				}
			}
			return sb;
		}

		public IReadOnlyList<object> Values { get; }
	}

	public interface IEnumOperandValue
	{
		System.Type EnumerationType { get; }
	}

	public interface IBitEnumOperandValue : IEnumOperandValue
	{
		IReadOnlyDictionary<uint, IReadOnlyList<object>> Values { get; }
	}

	public interface IValueEnumOperandValue : IEnumOperandValue
	{
		object Key { get; }
		IReadOnlyList<object> Value { get; }
	}

	public class ValueEnumOperandValue<T> : IValueEnumOperandValue
		where T : Enum
	{
		public ValueEnumOperandValue(T key, IReadOnlyList<object> value)
		{
			Key = key;
			Value = value;
		}

		public System.Type EnumerationType => typeof(T);
		public object Key { get; }
		public IReadOnlyList<object> Value { get; }
	}

	public class BitEnumOperandValue<T> : IBitEnumOperandValue
		where T : Enum
	{
		public BitEnumOperandValue(Dictionary<uint, IReadOnlyList<object>> values)
		{
			Values = values;
		}

		public IReadOnlyDictionary<uint, IReadOnlyList<object>> Values { get; }
		public System.Type EnumerationType => typeof(T);
	}

	public class ObjectReference
	{
		public ObjectReference(uint id)
		{
			Id = id;
		}

		public void Resolve(IReadOnlyDictionary<uint, ParsedInstruction> objects)
		{
			Reference = objects[Id];
		}

		public override string ToString()
		{
			return $"%{Id}";
		}

		public StringBuilder ToString(StringBuilder sb)
		{
			return sb.Append('%').Append(Id);
		}

		public uint Id { get; }
		public ParsedInstruction Reference { get; private set; }
	}

	public class ParsedInstruction
	{
		public ParsedInstruction(int opCode, IReadOnlyList<uint> words)
		{
			Words = words;
			Instruction = Instructions.OpcodeToInstruction[opCode];
			ParseOperands();
		}

		private void ParseOperands()
		{
			if (Instruction.Operands.Count == 0)
			{
				return;
			}

			// Word 0 describes this instruction so we can ignore it
			int currentWord = 1;
			int currentOperand = 0;
			List<object> varyingOperandValues = new List<object>();
			int varyingWordStart = 0;
			Operand varyingOperand = null;

			while (currentWord < Words.Count)
			{
				Operand operand = Instruction.Operands[currentOperand];
				operand.Type.ReadValue(Words, currentWord, out object value, out int wordsUsed);
				if (operand.Quantifier == OperandQuantifier.Varying)
				{
					varyingOperandValues.Add(value);
					varyingWordStart = currentWord;
					varyingOperand = operand;
				}
				else
				{
					int wordCount = Math.Min(Words.Count - currentWord, wordsUsed);
					ParsedOperand parsedOperand = new ParsedOperand(Words, currentWord, wordCount, value, operand);
					Operands.Add(parsedOperand);
				}

				currentWord += wordsUsed;
				if (operand.Quantifier != OperandQuantifier.Varying)
				{
					++currentOperand;
				}
			}

			if (varyingOperand != null)
			{
				VaryingOperandValue varOperantValue = new VaryingOperandValue(varyingOperandValues);
				ParsedOperand parsedOperand = new ParsedOperand(Words, currentWord, Words.Count - currentWord, varOperantValue, varyingOperand);
				Operands.Add(parsedOperand);
			}
		}

		public void ResolveResultType(IReadOnlyDictionary<uint, ParsedInstruction> objects)
		{
			if (Instruction.Operands.Count > 0 && Instruction.Operands[0].Type is IdResultType)
			{
				ResultType = objects[(uint)Operands[0].Value].ResultType;
			}
		}

		public void ResolveReferences (IReadOnlyDictionary<uint, ParsedInstruction> objects)
		{
			foreach (var operand in Operands)
			{
				if (operand.Value is ObjectReference objectReference)
				{
					objectReference.Resolve (objects);
				}
			}
		}

		public Type ResultType { get; set; }
		public uint ResultId
		{
			get
			{
				for (int i = 0; i < Instruction.Operands.Count; ++i)
				{
					if (Instruction.Operands[i].Type is IdResult)
					{
						return Operands[i].GetId();
					}
				}
				return 0;
			}
		}
		public bool HasResult => ResultId != 0;

		public IReadOnlyList<uint> Words { get; }
		public Instruction Instruction { get; }
		public IList<ParsedOperand> Operands { get; } = new List<ParsedOperand>();
		public string Name { get; set; }
		public object Value { get; set; }
	}
}
