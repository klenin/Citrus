using System;
using ProtoBuf;

namespace Lime
{
	[System.Diagnostics.DebuggerStepThrough]
	[ProtoContract]
	public struct Vector2 : IEquatable<Vector2>
	{
		[ProtoMember(1)]
		public float X;

		[ProtoMember(2)]
		public float Y;

		public static readonly Vector2 Zero = new Vector2(0, 0);
		public static readonly Vector2 One = new Vector2(1, 1);
		public static readonly Vector2 Half = new Vector2(0.5f, 0.5f);
		public static readonly Vector2 North = new Vector2(0, -1);
		public static readonly Vector2 South = new Vector2(0, 1);
		public static readonly Vector2 East = new Vector2(1, 0);
		public static readonly Vector2 West = new Vector2(-1, 0);

		public Vector2(float xy)
		{
			X = Y = xy;
		}

		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}

		public static explicit operator IntVector2(Vector2 v)
		{
			return new IntVector2((int)v.X, (int)v.Y);
		}

		public static explicit operator Size(Vector2 v)
		{
			return new Size((int)v.X, (int)v.Y);
		}

		bool IEquatable<Vector2>.Equals(Vector2 rhs)
		{
			return X == rhs.X && Y == rhs.Y;
		}

		public override bool Equals(object o)
		{
			Vector2 rhs = (Vector2)o;
			return X == rhs.X && Y == rhs.Y;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		public static bool operator == (Vector2 lhs, Vector2 rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}

		public static bool operator != (Vector2 lhs, Vector2 rhs)
		{
			return lhs.X != rhs.X || lhs.Y != rhs.Y;
		}

		public bool IsShorterThan(float radius)
		{
			return SquaredLength < radius * radius;
		}

		public bool IsWithin(Rectangle rect)
		{
			return rect.Contains(this);
		}

		public static float AngleBetweenDeg(Vector2 vector1, Vector2 vector2)
		{
			return AngleBetweenRad(vector1, vector2) * Mathf.RadiansToDegrees;
		}

		public static float AngleBetweenRad(Vector2 vector1, Vector2 vector2)
		{
			float sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
			float cos = vector1.X * vector2.X + vector1.Y * vector2.Y;
			return (float)Math.Atan2(sin, cos);
		}

		public static Vector2 Lerp(float t, Vector2 a, Vector2 b)
		{
			Vector2 r = new Vector2();
			r.X = (b.X - a.X) * t + a.X;
			r.Y = (b.Y - a.Y) * t + a.Y;
			return r;
		}

		public static Vector2 operator *(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X * rhs.X, lhs.Y * rhs.Y);
		}

		public static Vector2 operator /(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X / rhs.X, lhs.Y / rhs.Y);
		}

		public static Vector2 operator /(Vector2 lhs, float rhs)
		{
			return new Vector2(lhs.X / rhs, lhs.Y / rhs);
		}

		public static Vector2 Scale(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X * rhs.X, lhs.Y * rhs.Y);
		}

		public static Vector2 operator *(float lhs, Vector2 rhs)
		{
			return new Vector2(lhs * rhs.X, lhs * rhs.Y);
		}

		public static Vector2 operator *(Vector2 lhs, float rhs)
		{
			return new Vector2(rhs * lhs.X, rhs * lhs.Y);
		}

		public static Vector2 operator +(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
		}

		public static Vector2 operator -(Vector2 lhs, Vector2 rhs)
		{
			return new Vector2(lhs.X - rhs.X, lhs.Y - rhs.Y);
		}

		public static Vector2 operator -(Vector2 v)
		{
			return new Vector2(-v.X, -v.Y);
		}

		public static float DotProduct(Vector2 lhs, Vector2 rhs)
		{
			return lhs.X * rhs.X + lhs.Y * rhs.Y;
		}

		public static float CrossProduct(Vector2 lhs, Vector2 rhs)
		{
			return lhs.X * rhs.Y - lhs.Y * rhs.X;
		}

        public static Vector2 Min(Vector2 value1, Vector2 value2)
        {
            return new Vector2(value1.X < value2.X ? value1.X : value2.X,
                               value1.Y < value2.Y ? value1.Y : value2.Y);
        }

        public static Vector2 Max(Vector2 value1, Vector2 value2)
        {
            return new Vector2(value1.X > value2.X ? value1.X : value2.X,
                               value1.Y > value2.Y ? value1.Y : value2.Y);
        }

		public static Vector2 Normalize(Vector2 value)
		{
			float length = value.Length;
			if (length > 0) {
				value.X /= length;
				value.Y /= length;
			}
			return value;
		}

        public Vector2 Normalize()
        {
            float length = this.Length;
            if (length > 0) {
                this.X /= length;
                this.Y /= length;
            }
            return this;
        }

		public static Vector2 HeadingDeg(float degrees)
		{
			return Mathf.CosSin(degrees * Mathf.DegreesToRadians);
		}

		public static Vector2 HeadingRad(float radians)
		{
			return Mathf.CosSin(radians);
		}

		public static Vector2 RotateDeg(Vector2 value, float degrees)
		{
			return value.RotateDeg(degrees);
		}

		public Vector2 RotateDeg(float degrees)
		{
			Vector2 cs = Mathf.CosSin(degrees * Mathf.DegreesToRadians);
			Vector2 result;
			result.X = X * cs.X - Y * cs.Y;
			result.Y = X * cs.Y + Y * cs.X;
			return result;
		}

		public static Vector2 RotateRad(Vector2 value, float radians)
		{
			return value.RotateRad(radians);
		}

		public Vector2 RotateRad(float radians)
		{
			Vector2 cs = Mathf.CosSin(radians);
			Vector2 result;
			result.X = X * cs.X - Y * cs.Y;
			result.Y = X * cs.Y + Y * cs.X;
			return result;
		}

		/// <summary>
		/// Returns ATan of given vector in range (-Pi, Pi]
		/// </summary>
		public float Atan2Rad
		{
			get { return (float)Math.Atan2(Y, X); }
		}

		/// <summary>
		/// Returns ATan of given vector in range (-180, 180]
		/// </summary>
		public float Atan2Deg
		{
			get { return (float)Math.Atan2(Y, X) * Mathf.RadiansToDegrees; }
		}

		public float Length
		{
			get { return (float)Math.Sqrt(X * X + Y * Y); }
		}

		public Vector2 Normalized
		{
			get { return Vector2.Normalize(this); }
		}

		public Vector2 Normal
		{
			get { return new Vector2(-Y, X).Normalized; }
		}

		public float SquaredLength
		{
			get { return X * X + Y * Y; }
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}", X, Y);
		}
	}
}