using System;
using Microsoft.Xna.Framework;

namespace Matrix
{
    class Matrix
    {
        public double A { get; private set; }
        public double B { get; private set; }
        public double C { get; private set; }
        public double D { get; private set; }

        public Matrix()
        {
            A = 0;
            B = 0;
            C = 0;
            D = 0;
        }

        public Matrix(double a, double b, double c, double d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public static Matrix operator +(Matrix left, Matrix right)
        {
            Matrix result = new Matrix();
            result.A = left.A + right.A;
            result.B = left.B + right.B;
            result.C = left.C + right.C;
            result.D = left.D + right.D;
            return result;
        }

        public static Matrix operator +(Matrix left, double right)
        {
            Matrix result = new Matrix();
            result.A = left.A + right;
            result.B = left.B + right;
            result.C = left.C + right;
            result.D = left.D + right;
            return result;
        }

        public static Matrix operator -(Matrix left, Matrix right)
        {
            Matrix result = new Matrix();
            result.A = left.A - right.A;
            result.B = left.B - right.B;
            result.C = left.C - right.C;
            result.D = left.D - right.D;
            return result;
        }

        public static Matrix operator -(Matrix left, double right)
        {
            Matrix result = new Matrix();
            result.A = left.A - right;
            result.B = left.B - right;
            result.C = left.C - right;
            result.D = left.D - right;
            return result;
        }

        public static Matrix operator *(Matrix left, Matrix right)
        {
            Matrix result = new Matrix();
            result.A = left.A * right.A + left.C * right.B;
            result.B = left.B * right.A + left.D * right.B;
            result.C = left.A * right.C + left.C * right.D;
            result.D = left.B * right.C + left.D * right.D;
            return result;
        }

        public static Matrix operator *(Matrix left, double right)
        {
            Matrix result = new Matrix();
            result.A = left.A * right;
            result.B = left.B * right;
            result.C = left.C * right;
            result.D = left.D * right;
            return result;
        }

        public static Matrix operator /(Matrix left, double right)
        {
            Matrix result = new Matrix();
            result.A = left.A / right;
            result.B = left.B / right;
            result.C = left.C / right;
            result.D = left.D / right;
            return result;
        }

        public static Matrix operator %(Matrix left, double right)
        {
            Matrix result = new Matrix();
            result.A = left.A % right;
            result.B = left.B % right;
            result.C = left.C % right;
            result.D = left.D % right;
            return result;
        }

        public double Determinent
        {
            get { return (A * D) - (B * C); }
        }

        public Matrix Inverse
        {
            get { return new Matrix(D, -B, -C, A) / Determinent; }
        }

        public static Matrix Rotation(double angle)
        {
            Matrix result = new Matrix();
            result.A = Math.Cos(angle);
            result.B = Math.Sin(angle);
            result.C = -Math.Sin(angle);
            result.D = Math.Cos(angle);
            return result;
        }

        public static Matrix Rotate(Matrix matrix, double angle)
        {
            return Rotation(angle) * matrix;
        }

        public static Matrix Identity()
        {
            return new Matrix(1, 0, 0, 1);
        }

        public static Matrix Zero()
        {
            return new Matrix();
        }

        public override string ToString()
        {
            return String.Format("Matrix({0}, {1}, {2}, {3})", A, B, C, D);
        }
    }

    class Matrix2
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public Matrix2()
        {
            X = 0;
            Y = 0;
        }

        public Matrix2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Matrix2(Vector2 vec2)
        {
            X = vec2.X;
            Y = vec2.Y;
        }

        public static Matrix2 operator *(Matrix left, Matrix2 right)
        {
            Matrix2 result = new Matrix2();
            result.X = left.A * right.X + left.C * right.Y;
            result.Y = left.B * right.X + left.D * right.Y;
            return result;
        }

        public Matrix2 Rotate(double angle)
        {
            return Rotate(this, angle);
        }

        public static Matrix2 Rotate(Matrix2 vector, double angle)
        {
            return Matrix.Rotation(angle) * vector;
        }

        public Vector2 ToVector2()
        {
            return new Vector2((float)X, (float)Y);
        }

        public static Vector2 ToVector2(Matrix2 matrix)
        {
            return new Vector2((float)matrix.X, (float)matrix.Y);
        }

        public static Matrix2 FromVector2(Vector2 vec)
        {
            return new Matrix2(vec.X, vec.Y);
        }

        public override string ToString()
        {
            return String.Format("Matrix2({0}, {1})", Math.Round(X, 2), Math.Round(Y, 2));
        }
    }
}
