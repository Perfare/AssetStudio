using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace SpirV
{
	internal sealed class Reader
	{
		public Reader(BinaryReader reader)
		{
			reader_ = reader;
			uint magicNumber = reader_.ReadUInt32();
			if (magicNumber == Meta.MagicNumber)
			{
				littleEndian_ = true;
			}
			else if (Reverse(magicNumber) == Meta.MagicNumber)
			{
				littleEndian_ = false;
			}
			else
			{
				throw new Exception("Invalid magic number");
			}
		}

		public uint ReadDWord()
		{
			if (littleEndian_)
			{
				return reader_.ReadUInt32 ();
			}
			else
			{
				return Reverse(reader_.ReadUInt32());
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint Reverse(uint u)
		{
			return (u << 24) | (u & 0xFF00U) << 8 | (u >> 8) & 0xFF00U | (u >> 24);
		}

		public bool EndOfStream => reader_.BaseStream.Position == reader_.BaseStream.Length;

		private readonly BinaryReader reader_;
		private readonly bool littleEndian_;
	}
}
