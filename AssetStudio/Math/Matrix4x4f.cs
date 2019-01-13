using System;
using System.Runtime.InteropServices;

namespace AssetStudio
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Matrix4x4 : IEquatable<Matrix4x4>
    {
        public float M00;
        public float M10;
        public float M20;
        public float M30;

        public float M01;
        public float M11;
        public float M21;
        public float M31;

        public float M02;
        public float M12;
        public float M22;
        public float M32;

        public float M03;
        public float M13;
        public float M23;
        public float M33;

        public Matrix4x4(float[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length != 16)
                throw new ArgumentOutOfRangeException(nameof(values), "There must be sixteen and only sixteen input values for Matrix.");

            M00 = values[0];
            M10 = values[1];
            M20 = values[2];
            M30 = values[3];

            M01 = values[4];
            M11 = values[5];
            M21 = values[6];
            M31 = values[7];

            M02 = values[8];
            M12 = values[9];
            M22 = values[10];
            M32 = values[11];

            M03 = values[12];
            M13 = values[13];
            M23 = values[14];
            M33 = values[15];
        }

        public float this[int row, int column]
        {
            get => this[row + column * 4];

            set => this[row + column * 4] = value;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return M00;
                    case 1: return M10;
                    case 2: return M20;
                    case 3: return M30;
                    case 4: return M01;
                    case 5: return M11;
                    case 6: return M21;
                    case 7: return M31;
                    case 8: return M02;
                    case 9: return M12;
                    case 10: return M22;
                    case 11: return M32;
                    case 12: return M03;
                    case 13: return M13;
                    case 14: return M23;
                    case 15: return M33;
                    default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid Matrix4x4 index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: M00 = value; break;
                    case 1: M10 = value; break;
                    case 2: M20 = value; break;
                    case 3: M30 = value; break;
                    case 4: M01 = value; break;
                    case 5: M11 = value; break;
                    case 6: M21 = value; break;
                    case 7: M31 = value; break;
                    case 8: M02 = value; break;
                    case 9: M12 = value; break;
                    case 10: M22 = value; break;
                    case 11: M32 = value; break;
                    case 12: M03 = value; break;
                    case 13: M13 = value; break;
                    case 14: M23 = value; break;
                    case 15: M33 = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid Matrix4x4 index!");
                }
            }
        }

        public override int GetHashCode()
        {
            return GetColumn(0).GetHashCode() ^ (GetColumn(1).GetHashCode() << 2) ^ (GetColumn(2).GetHashCode() >> 2) ^ (GetColumn(3).GetHashCode() >> 1);
        }

        public override bool Equals(object other)
        {
            if (!(other is Matrix4x4))
                return false;
            return Equals((Matrix4x4)other);
        }

        public bool Equals(Matrix4x4 other)
        {
            return GetColumn(0).Equals(other.GetColumn(0))
                   && GetColumn(1).Equals(other.GetColumn(1))
                   && GetColumn(2).Equals(other.GetColumn(2))
                   && GetColumn(3).Equals(other.GetColumn(3));
        }

        public Vector4 GetColumn(int index)
        {
            switch (index)
            {
                case 0: return new Vector4(M00, M10, M20, M30);
                case 1: return new Vector4(M01, M11, M21, M31);
                case 2: return new Vector4(M02, M12, M22, M32);
                case 3: return new Vector4(M03, M13, M23, M33);
                default: throw new IndexOutOfRangeException("Invalid column index!");
            }
        }

        public Vector4 GetRow(int index)
        {
            switch (index)
            {
                case 0: return new Vector4(M00, M01, M02, M03);
                case 1: return new Vector4(M10, M11, M12, M13);
                case 2: return new Vector4(M20, M21, M22, M23);
                case 3: return new Vector4(M30, M31, M32, M33);
                default: throw new IndexOutOfRangeException("Invalid row index!");
            }
        }

        public static Matrix4x4 operator *(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            Matrix4x4 res;
            res.M00 = lhs.M00 * rhs.M00 + lhs.M01 * rhs.M10 + lhs.M02 * rhs.M20 + lhs.M03 * rhs.M30;
            res.M01 = lhs.M00 * rhs.M01 + lhs.M01 * rhs.M11 + lhs.M02 * rhs.M21 + lhs.M03 * rhs.M31;
            res.M02 = lhs.M00 * rhs.M02 + lhs.M01 * rhs.M12 + lhs.M02 * rhs.M22 + lhs.M03 * rhs.M32;
            res.M03 = lhs.M00 * rhs.M03 + lhs.M01 * rhs.M13 + lhs.M02 * rhs.M23 + lhs.M03 * rhs.M33;

            res.M10 = lhs.M10 * rhs.M00 + lhs.M11 * rhs.M10 + lhs.M12 * rhs.M20 + lhs.M13 * rhs.M30;
            res.M11 = lhs.M10 * rhs.M01 + lhs.M11 * rhs.M11 + lhs.M12 * rhs.M21 + lhs.M13 * rhs.M31;
            res.M12 = lhs.M10 * rhs.M02 + lhs.M11 * rhs.M12 + lhs.M12 * rhs.M22 + lhs.M13 * rhs.M32;
            res.M13 = lhs.M10 * rhs.M03 + lhs.M11 * rhs.M13 + lhs.M12 * rhs.M23 + lhs.M13 * rhs.M33;

            res.M20 = lhs.M20 * rhs.M00 + lhs.M21 * rhs.M10 + lhs.M22 * rhs.M20 + lhs.M23 * rhs.M30;
            res.M21 = lhs.M20 * rhs.M01 + lhs.M21 * rhs.M11 + lhs.M22 * rhs.M21 + lhs.M23 * rhs.M31;
            res.M22 = lhs.M20 * rhs.M02 + lhs.M21 * rhs.M12 + lhs.M22 * rhs.M22 + lhs.M23 * rhs.M32;
            res.M23 = lhs.M20 * rhs.M03 + lhs.M21 * rhs.M13 + lhs.M22 * rhs.M23 + lhs.M23 * rhs.M33;

            res.M30 = lhs.M30 * rhs.M00 + lhs.M31 * rhs.M10 + lhs.M32 * rhs.M20 + lhs.M33 * rhs.M30;
            res.M31 = lhs.M30 * rhs.M01 + lhs.M31 * rhs.M11 + lhs.M32 * rhs.M21 + lhs.M33 * rhs.M31;
            res.M32 = lhs.M30 * rhs.M02 + lhs.M31 * rhs.M12 + lhs.M32 * rhs.M22 + lhs.M33 * rhs.M32;
            res.M33 = lhs.M30 * rhs.M03 + lhs.M31 * rhs.M13 + lhs.M32 * rhs.M23 + lhs.M33 * rhs.M33;

            return res;
        }

        public static bool operator ==(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return lhs.GetColumn(0) == rhs.GetColumn(0)
                && lhs.GetColumn(1) == rhs.GetColumn(1)
                && lhs.GetColumn(2) == rhs.GetColumn(2)
                && lhs.GetColumn(3) == rhs.GetColumn(3);
        }

        public static bool operator !=(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return !(lhs == rhs);
        }

        public static Matrix4x4 Scale(Vector3 vector)
        {
            Matrix4x4 m;
            m.M00 = vector.X; m.M01 = 0F; m.M02 = 0F; m.M03 = 0F;
            m.M10 = 0F; m.M11 = vector.Y; m.M12 = 0F; m.M13 = 0F;
            m.M20 = 0F; m.M21 = 0F; m.M22 = vector.Z; m.M23 = 0F;
            m.M30 = 0F; m.M31 = 0F; m.M32 = 0F; m.M33 = 1F;
            return m;
        }

        public static Matrix4x4 Translate(Vector3 vector)
        {
            Matrix4x4 m;
            m.M00 = 1F; m.M01 = 0F; m.M02 = 0F; m.M03 = vector.X;
            m.M10 = 0F; m.M11 = 1F; m.M12 = 0F; m.M13 = vector.Y;
            m.M20 = 0F; m.M21 = 0F; m.M22 = 1F; m.M23 = vector.Z;
            m.M30 = 0F; m.M31 = 0F; m.M32 = 0F; m.M33 = 1F;
            return m;
        }

        public static Matrix4x4 Rotate(Quaternion q)
        {
            float x = q.X * 2.0F;
            float y = q.Y * 2.0F;
            float z = q.Z * 2.0F;
            float xx = q.X * x;
            float yy = q.Y * y;
            float zz = q.Z * z;
            float xy = q.X * y;
            float xz = q.X * z;
            float yz = q.Y * z;
            float wx = q.W * x;
            float wy = q.W * y;
            float wz = q.W * z;

            Matrix4x4 m;
            m.M00 = 1.0f - (yy + zz); m.M10 = xy + wz; m.M20 = xz - wy; m.M30 = 0.0F;
            m.M01 = xy - wz; m.M11 = 1.0f - (xx + zz); m.M21 = yz + wx; m.M31 = 0.0F;
            m.M02 = xz + wy; m.M12 = yz - wx; m.M22 = 1.0f - (xx + yy); m.M32 = 0.0F;
            m.M03 = 0.0F; m.M13 = 0.0F; m.M23 = 0.0F; m.M33 = 1.0F;
            return m;
        }
    }
}
