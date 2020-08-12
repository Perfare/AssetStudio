using System.Collections.Generic;
using System.Text;

namespace SpirV
{
	public class Type
	{
		public virtual StringBuilder ToString(StringBuilder sb)
		{
			return sb;
		}
	}

	public class VoidType : Type
	{
		public override string ToString()
		{
			return "void";
		}

		public override StringBuilder ToString(StringBuilder sb)
		{
			return sb.Append("void");
		}
	}

	public class ScalarType : Type
	{
	}

	public class BoolType : ScalarType
	{
		public override string ToString()
		{
			return "bool";
		}

		public override StringBuilder ToString(StringBuilder sb)
		{
			return sb.Append("bool");
		}
	}

	public class IntegerType : ScalarType
	{
		public IntegerType (int width, bool signed)
		{
			Width = width;
			Signed = signed;
		}

		public override string ToString()
		{
			if (Signed)
			{
				return $"i{Width}";
			}
			else
			{
				return $"u{Width}";
			}
		}

		public override StringBuilder ToString(StringBuilder sb)
		{
			if (Signed)
			{
				sb.Append('i').Append(Width);
			}
			else
			{
				sb.Append('u').Append(Width);
			}
			return sb;
		}

		public int Width { get; }
		public bool Signed { get; }
	}

	public class FloatingPointType : ScalarType
	{
		public FloatingPointType (int width)
		{
			Width = width;
		}

		public override string ToString()
		{
			return $"f{Width}";
		}

		public override StringBuilder ToString(StringBuilder sb)
		{
			return sb.Append('f').Append(Width);
		}

		public int Width { get; }
	}

	public class VectorType : Type
	{
		public VectorType (ScalarType scalarType, int componentCount)
		{
			ComponentType = scalarType;
			ComponentCount = componentCount;
		}

		public override string ToString()
		{
			return $"{ComponentType}_{ComponentCount}";
		}

		public override StringBuilder ToString(StringBuilder sb)
		{
			return ComponentType.ToString(sb).Append('_').Append(ComponentCount);
		}

		public ScalarType ComponentType { get; }
		public int ComponentCount { get; }
	}

	public class MatrixType : Type
	{
		public MatrixType (VectorType vectorType, int columnCount)
		{
			ColumnType = vectorType;
			ColumnCount = columnCount;
		}

		public override string ToString ()
		{
			return $"{ColumnType}x{ColumnCount}";
		}

		public override StringBuilder ToString(StringBuilder sb)
		{
			return sb.Append(ColumnType).Append('x').Append(ColumnCount);
		}

		public VectorType ColumnType { get; }
		public int ColumnCount { get; }
		public int RowCount => ColumnType.ComponentCount;
	}

	public class ImageType : Type
	{
		public ImageType (Type sampledType, Dim dim, int depth, bool isArray, bool isMultisampled, int sampleCount,
			ImageFormat imageFormat, AccessQualifier accessQualifier)
		{
			SampledType = sampledType;
			Dim = dim;
			Depth = depth;
			IsArray = isArray;
			IsMultisampled = isMultisampled;
			SampleCount = sampleCount;
			Format = imageFormat;
			AccessQualifier = accessQualifier;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			ToString(sb);
			return sb.ToString();
		}

		public override StringBuilder ToString(StringBuilder sb)
		{
			switch (AccessQualifier)
			{
				case AccessQualifier.ReadWrite:
					sb.Append("read_write ");
					break;
				case AccessQualifier.WriteOnly:
					sb.Append("write_only ");
					break;
				case AccessQualifier.ReadOnly:
					sb.Append("read_only ");
					break;
			}

			sb.Append("Texture");
			switch (Dim)
			{
				case Dim.Dim1D:
					sb.Append("1D");
					break;
				case Dim.Dim2D:
					sb.Append("2D");
					break;
				case Dim.Dim3D:
					sb.Append("3D");
					break;
				case Dim.Cube:
					sb.Append("Cube");
					break;
			}

			if (IsMultisampled)
			{
				sb.Append("MS");
			}
			if (IsArray)
			{
				sb.Append("Array");
			}
			return sb;
		}

		public Type SampledType { get; }
		public Dim Dim { get; }
		public int Depth { get; }
		public bool IsArray { get; }
		public bool IsMultisampled { get; }
		public int SampleCount { get; }
		public ImageFormat Format { get; }
		public AccessQualifier AccessQualifier { get; }
	}

	public class SamplerType : Type
	{
		public override string ToString()
		{
			return "sampler";
		}

		public override StringBuilder ToString(StringBuilder sb)
		{
			return sb.Append("sampler");
		}
	}

	public class SampledImageType : Type
	{
		public SampledImageType (ImageType imageType)
		{
			ImageType = imageType;
		}

		public override string ToString()
		{
			return $"{ImageType}Sampled";
		}

		public override StringBuilder ToString(StringBuilder sb)
		{
			return ImageType.ToString(sb).Append("Sampled");
		}

		public ImageType ImageType { get; }
	}

	public class ArrayType : Type
	{
		public ArrayType (Type elementType, int elementCount)
		{
			ElementType = elementType;
			ElementCount = elementCount;
		}

		public override string ToString()
		{
			return $"{ElementType}[{ElementCount}]";
		}

		public override StringBuilder ToString(StringBuilder sb)
		{
			return ElementType.ToString(sb).Append('[').Append(ElementCount).Append(']');
		}

		public int ElementCount { get; }
		public Type ElementType { get; }
	}

	public class RuntimeArrayType : Type
	{
		public RuntimeArrayType(Type elementType)
		{
			ElementType = elementType;
		}

		public Type ElementType { get; }
	}

	public class StructType : Type
	{
		public StructType(IReadOnlyList<Type> memberTypes)
		{
			MemberTypes = memberTypes;
			memberNames_ = new List<string>();

			for (int i = 0; i < memberTypes.Count; ++i)
			{
				memberNames_.Add(string.Empty);
			}
		}

		public void SetMemberName(uint member, string name)
		{
			memberNames_[(int)member] = name;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			ToString(sb);
			return sb.ToString();
		}

		public override StringBuilder ToString(StringBuilder sb)
		{
			sb.Append("struct {");
			for (int i = 0; i < MemberTypes.Count; ++i)
			{
				Type memberType = MemberTypes[i];
				memberType.ToString(sb);
				if (!string.IsNullOrEmpty(memberNames_[i]))
				{
					sb.Append(' ');
					sb.Append(MemberNames[i]);
				}

				sb.Append(';');
				if (i < (MemberTypes.Count - 1))
				{
					sb.Append(' ');
				}
			}
			sb.Append('}');
			return sb;
		}

		public IReadOnlyList<Type> MemberTypes { get; }
		public IReadOnlyList<string> MemberNames => memberNames_;

		private List<string> memberNames_;
	}

	public class OpaqueType : Type
	{
	}

	public class PointerType : Type
	{
		public PointerType(StorageClass storageClass, Type type)
		{
			StorageClass = storageClass;
			Type = type;
		}

		public PointerType(StorageClass storageClass)
		{
			StorageClass = storageClass;
		}

		public void ResolveForwardReference(Type t)
		{
			Type = t;
		}

		public override string ToString()
		{
			if (Type == null)
			{
				return $"{StorageClass} *";
			}
			else
			{
				return $"{StorageClass} {Type}*";
			}
		}

		public override StringBuilder ToString(StringBuilder sb)
		{
			sb.Append(StorageClass.ToString()).Append(' ');
			if (Type != null)
			{
				Type.ToString(sb);
			}
			sb.Append('*');
			return sb;
		}

		public StorageClass StorageClass { get; }
		public Type Type { get; private set; }
	}

	public class FunctionType : Type
	{
		public FunctionType(Type returnType, IReadOnlyList<Type> parameterTypes)
		{
			ReturnType = returnType;
			ParameterTypes = parameterTypes;
		}

		public Type ReturnType { get; }
		public IReadOnlyList<Type> ParameterTypes { get; }
	}

	public class EventType : Type
	{
	}

	public class DeviceEventType : Type
	{
	}

	public class ReserveIdType : Type
	{
	}

	public class QueueType : Type
	{
	}

	public class PipeType : Type
	{
	}

	public class PipeStorage : Type
	{
	}

	public class NamedBarrier : Type
	{
	}
}
