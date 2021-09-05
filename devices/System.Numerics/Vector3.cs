// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Numerics
{
    /// <summary>
    /// A structure encapsulating three doule values and provides hardware accelerated methods.
    /// </summary>
    public partial struct Vector3
    {
        #region Public Static Properties
        /// <summary>
        /// Returns the vector (0,0,0).
        /// </summary>
        public static Vector3 Zero { get { return new Vector3(); } }
        /// <summary>
        /// Returns the vector (1,1,1).
        /// </summary>
        public static Vector3 One { get { return new Vector3(1.0, 1.0, 1.0); } }
        /// <summary>
        /// Returns the vector (1,0,0).
        /// </summary>
        public static Vector3 UnitX { get { return new Vector3(1.0, 0.0, 0.0); } }
        /// <summary>
        /// Returns the vector (0,1,0).
        /// </summary>        public static Vector3 UnitY { get { return new Vector3(0.0, 1.0, 0.0); } }
        /// <summary>
        /// Returns the vector (0,0,1).
        /// </summary>
        public static Vector3 UnitZ { get { return new Vector3(0.0, 0.0, 1.0); } }
        #endregion Public Static Properties

        #region Public Instance Methods

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            int hash = this.X.GetHashCode();
            hash = HashCodeHelper.CombineHashCodes(hash, this.Y.GetHashCode());
            hash = HashCodeHelper.CombineHashCodes(hash, this.Z.GetHashCode());
            return hash;
        }

        /// <summary>
        /// Returns a boolean indicating whether the given Object is equal to this Vector3 instance.
        /// </summary>
        /// <param name="obj">The Object to compare against.</param>
        /// <returns>True if the Object is equal to this Vector3; False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Vector3))
                return false;
            return Equals((Vector3)obj);
        }

        /// <summary>
        /// Returns a String representing this Vector3 instance.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return ToString("G");
        }

        /// <summary>
        /// Returns a String representing this Vector3 instance, using the specified format to format individual elements.
        /// </summary>
        /// <param name="format">The format of individual elements.</param>
        /// <returns>The string representation.</returns>
        public string ToString(string format)
        {
            return ToString(format);
        }

        /// <summary>
        /// Returns the length of the vector.
        /// </summary>
        /// <returns>The vector's length.</returns>
        public double Length()
        {
            if (Vector.IsHardwareAccelerated)
            {
                double ls = Vector3.Dot(this, this);
                return Math.Sqrt(ls);
            }
            else
            {
                double ls = X * X + Y * Y + Z * Z;
                return Math.Sqrt(ls);
            }
        }

        /// <summary>
        /// Returns the length of the vector squared. This operation is cheaper than Length().
        /// </summary>
        /// <returns>The vector's length squared.</returns>
        public double LengthSquared()
        {
            if (Vector.IsHardwareAccelerated)
            {
                return Vector3.Dot(this, this);
            }
            else
            {
                return X * X + Y * Y + Z * Z;
            }
        }
        #endregion Public Instance Methods

        #region Public Static Methods
        /// <summary>
        /// Returns the Euclidean distance between the two given points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The distance.</returns>
        public static double Distance(Vector3 value1, Vector3 value2)
        {
            if (Vector.IsHardwareAccelerated)
            {
                Vector3 difference = value1 - value2;
                double ls = Vector3.Dot(difference, difference);
                return Math.Sqrt(ls);
            }
            else
            {
                double dx = value1.X - value2.X;
                double dy = value1.Y - value2.Y;
                double dz = value1.Z - value2.Z;

                double ls = dx * dx + dy * dy + dz * dz;

                return Math.Sqrt(ls);
            }
        }

        /// <summary>
        /// Returns the Euclidean distance squared between the two given points.
        /// </summary>
        /// <param name="value1">The first point.</param>
        /// <param name="value2">The second point.</param>
        /// <returns>The distance squared.</returns>
        public static double DistanceSquared(Vector3 value1, Vector3 value2)
        {
            if (Vector.IsHardwareAccelerated)
            {
                Vector3 difference = value1 - value2;
                return Vector3.Dot(difference, difference);
            }
            else
            {
                double dx = value1.X - value2.X;
                double dy = value1.Y - value2.Y;
                double dz = value1.Z - value2.Z;

                return dx * dx + dy * dy + dz * dz;
            }
        }

        /// <summary>
        /// Returns a vector with the same direction as the given vector, but with a length of 1.
        /// </summary>
        /// <param name="value">The vector to normalize.</param>
        /// <returns>The normalized vector.</returns>
        public static Vector3 Normalize(Vector3 value)
        {
            if (Vector.IsHardwareAccelerated)
            {
                double length = value.Length();
                return value / length;
            }
            else
            {
                double ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
                double length = System.Math.Sqrt(ls);
                return new Vector3(value.X / length, value.Y / length, value.Z / length);
            }
        }

        /// <summary>
        /// Computes the cross product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The cross product.</returns>
        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(
                vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z,
                vector1.X * vector2.Y - vector1.Y * vector2.X);
        }

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">The normal of the surface being reflected off.</param>
        /// <returns>The reflected vector.</returns>
        public static Vector3 Reflect(Vector3 vector, Vector3 normal)
        {
            if (Vector.IsHardwareAccelerated)
            {
                double dot = Vector3.Dot(vector, normal);
                Vector3 temp = normal * dot * 2;
                return vector - temp;
            }
            else
            {
                double dot = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
                double tempX = normal.X * dot * 2f;
                double tempY = normal.Y * dot * 2f;
                double tempZ = normal.Z * dot * 2f;
                return new Vector3(vector.X - tempX, vector.Y - tempY, vector.Z - tempZ);
            }
        }

        /// <summary>
        /// Restricts a vector between a min and max value.
        /// </summary>
        /// <param name="value1">The source vector.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The restricted vector.</returns>
        public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
        {
            // This compare order is very important!!!
            // We must follow HLSL behavior in the case user specified min value is bigger than max value.

            double x = value1.X;
            x = (x > max.X) ? max.X : x;
            x = (x < min.X) ? min.X : x;

            double y = value1.Y;
            y = (y > max.Y) ? max.Y : y;
            y = (y < min.Y) ? min.Y : y;

            double z = value1.Z;
            z = (z > max.Z) ? max.Z : z;
            z = (z < min.Z) ? min.Z : z;

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Linearly interpolates between two vectors based on the given weighting.
        /// </summary>
        /// <param name="value1">The first source vector.</param>
        /// <param name="value2">The second source vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of the second source vector.</param>
        /// <returns>The interpolated vector.</returns>
        public static Vector3 Lerp(Vector3 value1, Vector3 value2, double amount)
        {
            if (Vector.IsHardwareAccelerated)
            {
                Vector3 firstInfluence = value1 * (1 - amount);
                Vector3 secondInfluence = value2 * amount;
                return firstInfluence + secondInfluence;
            }
            else
            {
                return new Vector3(
                    value1.X + (value2.X - value1.X) * amount,
                    value1.Y + (value2.Y - value1.Y) * amount,
                    value1.Z + (value2.Z - value1.Z) * amount);
            }
        }

        /*
        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="position">The source vector.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The transformed vector.</returns>
        public static Vector3 Transform(Vector3 position, Matrix4x4 matrix)
        {
            return new Vector3(
                position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41,
                position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42,
                position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43);
        }

        /// <summary>
        /// Transforms a vector normal by the given matrix.
        /// </summary>
        /// <param name="normal">The source vector.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The transformed vector.</returns>
        public static Vector3 TransformNormal(Vector3 normal, Matrix4x4 matrix)
        {
            return new Vector3(
                normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31,
                normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32,
                normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33);
        }
        */

        /// <summary>
        /// Transforms a vector by the given Quaternion rotation value.
        /// </summary>
        /// <param name="value">The source vector to be rotated.</param>
        /// <param name="rotation">The rotation to apply.</param>
        /// <returns>The transformed vector.</returns>
        public static Vector3 Transform(Vector3 value, Quaternion rotation)
        {
            double x2 = rotation.X + rotation.X;
            double y2 = rotation.Y + rotation.Y;
            double z2 = rotation.Z + rotation.Z;

            double wx2 = rotation.W * x2;
            double wy2 = rotation.W * y2;
            double wz2 = rotation.W * z2;
            double xx2 = rotation.X * x2;
            double xy2 = rotation.X * y2;
            double xz2 = rotation.X * z2;
            double yy2 = rotation.Y * y2;
            double yz2 = rotation.Y * z2;
            double zz2 = rotation.Z * z2;

            return new Vector3(
                value.X * (1.0 - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
                value.X * (xy2 + wz2) + value.Y * (1.0 - xx2 - zz2) + value.Z * (yz2 - wx2),
                value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1.0 - xx2 - yy2));
        }
        #endregion Public Static Methods

        #region Public operator methods

        // All these methods should be inlined as they are implemented
        // over JIT intrinsics

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The summed vector.</returns>
        public static Vector3 Add(Vector3 left, Vector3 right)
        {
            return left + right;
        }

        /// <summary>
        /// Subtracts the second vector from the first.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The difference vector.</returns>
        public static Vector3 Subtract(Vector3 left, Vector3 right)
        {
            return left - right;
        }

        /// <summary>
        /// Multiplies two vectors together.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The product vector.</returns>
        public static Vector3 Multiply(Vector3 left, Vector3 right)
        {
            return left * right;
        }

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        /// <param name="left">The source vector.</param>
        /// <param name="right">The scalar value.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector3 Multiply(Vector3 left, double right)
        {
            return left * right;
        }

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        /// <param name="left">The scalar value.</param>
        /// <param name="right">The source vector.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector3 Multiply(double left, Vector3 right)
        {
            return left * right;
        }

        /// <summary>
        /// Divides the first vector by the second.
        /// </summary>
        /// <param name="left">The first source vector.</param>
        /// <param name="right">The second source vector.</param>
        /// <returns>The vector resulting from the division.</returns>
        public static Vector3 Divide(Vector3 left, Vector3 right)
        {
            return left / right;
        }

        /// <summary>
        /// Divides the vector by the given scalar.
        /// </summary>
        /// <param name="left">The source vector.</param>
        /// <param name="divisor">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        public static Vector3 Divide(Vector3 left, double divisor)
        {
            return left / divisor;
        }

        /// <summary>
        /// Negates a given vector.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The negated vector.</returns>
        public static Vector3 Negate(Vector3 value)
        {
            return -value;
        }
        #endregion Public operator methods
    }
}
