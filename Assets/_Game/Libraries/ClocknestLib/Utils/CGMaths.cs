// Extension of Corgi Engine's MMMaths. 
// Credits to More Mountain.

using UnityEngine;

namespace ClocknestGames.Library.Utils
{
	/// <summary>
	/// Math helpers
	/// </summary>

	public static class CGMaths
	{
		/// <summary>
		/// Sawps two values.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lhs">First</param>
		/// <param name="rhs">Second</param>
		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

		/// <summary>
		/// Sawps two values.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lhs">First</param>
		/// <param name="rhs">Second</param>
		public static void Swap(ref Vector3 lhs, ref Vector3 rhs)
		{
			Vector3 temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

		/// <summary>
		/// Takes a Vector3 and turns it into a Vector2
		/// </summary>
		/// <returns>The vector2.</returns>
		/// <param name="target">The Vector3 to turn into a Vector2.</param>
		public static Vector2 Vector3ToVector2(Vector3 target)
		{
			return new Vector2(target.x, target.y);
		}

		/// <summary>
		/// Takes a Vector2 and turns it into a Vector3 with a null z value
		/// </summary>
		/// <returns>The vector3.</returns>
		/// <param name="target">The Vector2 to turn into a Vector3.</param>
		public static Vector3 Vector2ToVector3(Vector2 target)
		{
			return new Vector3(target.x, target.y, 0);
		}

		/// <summary>
		/// Takes a Vector2 and turns it into a Vector3 with the specified z value 
		/// </summary>
		/// <returns>The vector3.</returns>
		/// <param name="target">The Vector2 to turn into a Vector3.</param>
		/// <param name="newZValue">New Z value.</param>
		public static Vector3 Vector2ToVector3(Vector2 target, float newZValue)
		{
			return new Vector3(target.x, target.y, newZValue);
		}

		public static Vector3 Vector3ToVector2Z(Vector3 target)
		{
			return new Vector2(target.x, target.z);
		}

		public static Vector3 Vector2ZToVector3(Vector2 target, float newYValue = 0f)
		{
			return new Vector3(target.x, newYValue, target.y);
		}

		/// <summary>
		/// Rounds all components of a Vector3.
		/// </summary>
		/// <returns>The vector3.</returns>
		/// <param name="vector">Vector.</param>
		public static Vector3 RoundVector3(Vector3 vector)
		{
			return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
		}

		/// <summary>
		/// Returns a random Vector2 from 2 defined Vector2.
		/// </summary>
		/// <returns>The random Vector2.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Maximum.</param>
		public static Vector2 RandomVector2(Vector2 minimum, Vector2 maximum)
		{
			return new Vector2(UnityEngine.Random.Range(minimum.x, maximum.x),
											 UnityEngine.Random.Range(minimum.y, maximum.y));
		}

		/// <summary>
		/// Returns a random Vector3 from 2 defined Vector3.
		/// </summary>
		/// <returns>The random Vector3.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Maximum.</param>
		public static Vector3 RandomVector3(Vector3 minimum, Vector3 maximum)
		{
			return new Vector3(UnityEngine.Random.Range(minimum.x, maximum.x),
											 UnityEngine.Random.Range(minimum.y, maximum.y),
											 UnityEngine.Random.Range(minimum.z, maximum.z));
		}

		/// <summary>
		/// Rotates a point around the given pivot.
		/// </summary>
		/// <returns>The new point position.</returns>
		/// <param name="point">The point to rotate.</param>
		/// <param name="pivot">The pivot's position.</param>
		/// <param name="angle">The angle we want to rotate our point.</param>
		public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle)
		{
			angle = angle * (Mathf.PI / 180f);
			var rotatedX = Mathf.Cos(angle) * (point.x - pivot.x) - Mathf.Sin(angle) * (point.y - pivot.y) + pivot.x;
			var rotatedY = Mathf.Sin(angle) * (point.x - pivot.x) + Mathf.Cos(angle) * (point.y - pivot.y) + pivot.y;
			return new Vector3(rotatedX, rotatedY, 0);
		}

		/// <summary>
		/// Rotates a point around the given pivot.
		/// </summary>
		/// <returns>The new point position.</returns>
		/// <param name="point">The point to rotate.</param>
		/// <param name="pivot">The pivot's position.</param>
		/// <param name="angles">The angle as a Vector3.</param>
		public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angle)
		{
			// we get point direction from the point to the pivot
			Vector3 direction = point - pivot;
			// we rotate the direction
			direction = Quaternion.Euler(angle) * direction;
			// we determine the rotated point's position
			point = direction + pivot;
			return point;
		}

		/// <summary>
		/// Rotates a point around the given pivot.
		/// </summary>
		/// <returns>The new point position.</returns>
		/// <param name="point">The point to rotate.</param>
		/// <param name="pivot">The pivot's position.</param>
		/// <param name="angles">The angle as a Vector3.</param>
		public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion quaternion)
		{
			// we get point direction from the point to the pivot
			Vector3 direction = point - pivot;
			// we rotate the direction
			direction = quaternion * direction;
			// we determine the rotated point's position
			point = direction + pivot;
			return point;
		}

		/// <summary>
		/// Rotates a vector2 by the angle (in degrees) specified and returns it
		/// </summary>
		/// <returns>The rotated Vector2.</returns>
		/// <param name="vector">The vector to rotate.</param>
		/// <param name="angle">Degrees.</param>
		public static Vector2 RotateVector2(Vector2 vector, float angle)
		{
			if (angle == 0)
			{
				return vector;
			}
			float sinus = Mathf.Sin(angle * Mathf.Deg2Rad);
			float cosinus = Mathf.Cos(angle * Mathf.Deg2Rad);

			float oldX = vector.x;
			float oldY = vector.y;
			vector.x = (cosinus * oldX) - (sinus * oldY);
			vector.y = (sinus * oldX) + (cosinus * oldY);
			return vector;
		}

		/// <summary>
		/// Computes and returns the angle between two vectors, on a 360° scale
		/// </summary>
		/// <returns>The <see cref="System.Single"/>.</returns>
		/// <param name="vectorA">Vector a.</param>
		/// <param name="vectorB">Vector b.</param>
		public static float AngleBetween(Vector2 vectorA, Vector2 vectorB)
		{
			float angle = Vector2.Angle(vectorA, vectorB);
			Vector3 cross = Vector3.Cross(vectorA, vectorB);

			if (cross.z > 0)
			{
				angle = 360 - angle;
			}

			return angle;
		}

		public static float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n)
		{
			// angle in [0,180]
			float angle = Vector3.Angle(a, b);
			float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

			// angle in [-179,180]
			float signed_angle = angle * sign;

			// angle in [0,360] (not used but included here for completeness)
			//float angle360 =  (signed_angle + 180) % 360;

			return signed_angle;
		}

		/// <summary>
		/// Returns the distance between a point and a line.
		/// </summary>
		/// <returns>The between point and line.</returns>
		/// <param name="point">Point.</param>
		/// <param name="lineStart">Line start.</param>
		/// <param name="lineEnd">Line end.</param>
		public static float DistanceBetweenPointAndLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
		{
			return Vector3.Magnitude(ProjectPointOnLine(point, lineStart, lineEnd) - point);
		}

		/// <summary>
		/// Projects a point on a line (perpendicularly) and returns the projected point.
		/// </summary>
		/// <returns>The point on line.</returns>
		/// <param name="point">Point.</param>
		/// <param name="lineStart">Line start.</param>
		/// <param name="lineEnd">Line end.</param>
		public static Vector3 ProjectPointOnLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
		{
			Vector3 rhs = point - lineStart;
			Vector3 vector2 = lineEnd - lineStart;
			float magnitude = vector2.magnitude;
			Vector3 lhs = vector2;
			if (magnitude > 1E-06f)
			{
				lhs = (Vector3)(lhs / magnitude);
			}
			float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude);
			return (lineStart + ((Vector3)(lhs * num2)));
		}

		/// <summary>
		/// Returns the sum of all the int passed in parameters
		/// </summary>
		/// <param name="thingsToAdd">Things to add.</param>
		public static int Sum(params int[] thingsToAdd)
		{
			int result = 0;
			for (int i = 0; i < thingsToAdd.Length; i++)
			{
				result += thingsToAdd[i];
			}
			return result;
		}

		/// <summary>
		/// Returns the result of rolling a dice of the specified number of sides
		/// </summary>
		/// <returns>The result of the dice roll.</returns>
		/// <param name="numberOfSides">Number of sides of the dice.</param>
		public static int RollADice(int numberOfSides)
		{
			return (UnityEngine.Random.Range(1, numberOfSides + 1));
		}

		/// <summary>
		/// Returns a random success based on X% of chance.
		/// Example : I have 20% of chance to do X, Chance(20) > true, yay!
		/// </summary>
		/// <param name="percent">Percent of chance.</param>
		public static bool Chance(int percent)
		{
			return (UnityEngine.Random.Range(0, 100) <= percent);
		}

		/// <summary>
		/// Moves from "from" to "to" by the specified amount and returns the corresponding value
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="amount">Amount.</param>
		public static float Approach(float from, float to, float amount)
		{
			if (from < to)
			{
				from += amount;
				if (from > to)
				{
					return to;
				}
			}
			else
			{
				from -= amount;
				if (from < to)
				{
					return to;
				}
			}
			return from;
		}


		/// <summary>
		/// Remaps a value x in interval [A,B], to the proportional value in interval [C,D]
		/// </summary>
		/// <param name="x">The value to remap.</param>
		/// <param name="A">the minimum bound of interval [A,B] that contains the x value</param>
		/// <param name="B">the maximum bound of interval [A,B] that contains the x value</param>
		/// <param name="C">the minimum bound of target interval [C,D]</param>
		/// <param name="D">the maximum bound of target interval [C,D]</param>
		public static float Remap(float x, float A, float B, float C, float D)
		{
			float remappedValue = C + (x - A) / (B - A) * (D - C);
			return remappedValue;
		}

		public static double Remap(double x, double A, double B, double C, double D)
		{
			double remappedValue = C + (x - A) / (B - A) * (D - C);
			return remappedValue;
		}

		public static float Normalize(float x, float min, float max)
		{
			return (x - min) / (max - min);
		}

		/// <summary>
		/// Clamps the angle in parameters between a minimum and maximum angle (all angles expressed in degrees)
		/// </summary>
		/// <param name="angle"></param>
		/// <param name="minimumAngle"></param>
		/// <param name="maximumAngle"></param>
		/// <returns></returns>
		public static float ClampAngle(float angle, float minimumAngle, float maximumAngle)
		{
			if (angle < -360)
			{
				angle += 360;
			}
			if (angle > 360)
			{
				angle -= 360;
			}
			return Mathf.Clamp(angle, minimumAngle, maximumAngle);
		}

		/// <summary>
		/// Clamps the angle in parameters between a minimum and maximum angle (all angles expressed in degrees), considering values can be negative
		/// </summary>
		/// <param name="angle"></param>
		/// <param name="minimumAngle"></param>
		/// <param name="maximumAngle"></param>
		/// <returns></returns>
		public static float ClampAngleModular(float angle, float minimumAngle, float maximumAngle)
		{
			angle = (angle > 180) ? angle - 360 : angle;
			return Mathf.Clamp(angle, Mathf.Min(minimumAngle, maximumAngle), Mathf.Max(minimumAngle, maximumAngle));
		}

		public static float PositiveAngle(float angle)
		{
			if ((angle %= 360) < 0f) angle += 360;
			return angle;
		}

		/// <summary>
		/// Returns angle in range -180 to 180.
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static float NormalizeAngle(float angle)
		{
			return angle - 180f * Mathf.Floor((angle + 180f) / 180f);
		}

		public static float RoundToDecimal(float value, int numberOfDecimals)
		{
			return Mathf.Round(value * 10f * numberOfDecimals) / (10f * numberOfDecimals);
		}

		public static Rect GetScreenCoordinates(this RectTransform rectTransform)
		{
			var worldCorners = new Vector3[4];
			rectTransform.GetWorldCorners(worldCorners);
			var result = new Rect(
						  worldCorners[0].x,
						  worldCorners[0].y,
						  worldCorners[2].x - worldCorners[0].x,
						  worldCorners[2].y - worldCorners[0].y);
			return result;
		}

		/// <summary>
		/// Rounds the value passed in parameters to the closest value in the parameter array
		/// </summary>
		/// <param name="value"></param>
		/// <param name="possibleValues"></param>
		/// <returns></returns>
		public static float RoundToClosest(float value, float[] possibleValues)
		{
			if (possibleValues.Length == 0)
			{
				return 0f;
			}

			float closestValue = possibleValues[0];

			foreach (float possibleValue in possibleValues)
			{
				if (Mathf.Abs(closestValue - value) > Mathf.Abs(possibleValue - value))
				{
					closestValue = possibleValue;
				}
			}
			return closestValue;

		}

		/// <summary>
		/// Returns a vector3 based on the angle in parameters
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static Vector3 DirectionFromAngle(float angle, float additionalAngle)
		{
			angle += additionalAngle;

			Vector3 direction = Vector3.zero;
			direction.x = Mathf.Sin(angle * Mathf.Deg2Rad);
			direction.y = 0f;
			direction.z = Mathf.Cos(angle * Mathf.Deg2Rad);
			return direction;
		}

		/// <summary>
		/// Returns a vector3 based on the angle in parameters
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public static Vector3 DirectionFromAngle2D(float angle, float additionalAngle)
		{
			angle += additionalAngle;

			Vector3 direction = Vector3.zero;
			direction.x = Mathf.Sin(angle * Mathf.Deg2Rad);
			direction.y = Mathf.Cos(angle * Mathf.Deg2Rad);
			direction.z = 0f;
			return direction;
		}

		public static bool IsBetween<T>(this T value, T minimum, T maximum) where T : System.IComparable<T>
		{
			if (value.CompareTo(minimum) < 0 || value.CompareTo(maximum) > 0)
				return false;

			return true;
		}

		public static Vector3 Abs(Vector3 vector)
		{
			return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
		}

		public static Vector3 IsLeft(Vector3 testedPoint, Vector3 direction, Vector3 directionPoint)
		{
			Vector3 vec = direction;
			Vector3 dir = testedPoint - directionPoint;

			Vector3 cross = Vector3.Cross(vec, dir);
			return cross;
		}

		static PosRotScale LockTransformToSpace(Transform t, PathSpace space)
		{
			var original = new PosRotScale(t);
			if (space == PathSpace.xy)
			{
				t.eulerAngles = new Vector3(0, 0, t.eulerAngles.z);
				t.position = new Vector3(t.position.x, t.position.y, 0);
			}
			else if (space == PathSpace.xz)
			{
				t.eulerAngles = new Vector3(0, t.eulerAngles.y, 0);
				t.position = new Vector3(t.position.x, 0, t.position.z);
			}

			//float maxScale = Mathf.Max (t.localScale.x * t.parent.localScale.x, t.localScale.y * t.parent.localScale.y, t.localScale.z * t.parent.localScale.z);
			float maxScale = Mathf.Max(t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);

			t.localScale = Vector3.one * maxScale;

			return original;
		}

		public static Vector3 TransformPoint(Vector3 p, Transform t, PathSpace space)
		{
			var original = LockTransformToSpace(t, space);
			Vector3 transformedPoint = t.TransformPoint(p);
			original.SetTransform(t);
			return transformedPoint;
		}

		public static Vector3 InverseTransformPoint(Vector3 p, Transform t, PathSpace space)
		{
			var original = LockTransformToSpace(t, space);
			Vector3 transformedPoint = t.InverseTransformPoint(p);
			original.SetTransform(t);
			return transformedPoint;
		}

		public static Vector3 TransformVector(Vector3 p, Transform t, PathSpace space)
		{
			var original = LockTransformToSpace(t, space);
			Vector3 transformedPoint = t.TransformVector(p);
			original.SetTransform(t);
			return transformedPoint;
		}

		public static Vector3 InverseTransformVector(Vector3 p, Transform t, PathSpace space)
		{
			var original = LockTransformToSpace(t, space);
			Vector3 transformedPoint = t.InverseTransformVector(p);
			original.SetTransform(t);
			return transformedPoint;
		}

		public static Vector3 TransformDirection(Vector3 p, Transform t, PathSpace space)
		{
			var original = LockTransformToSpace(t, space);
			Vector3 transformedPoint = t.TransformDirection(p);
			original.SetTransform(t);
			return transformedPoint;
		}

		public static Vector3 InverseTransformDirection(Vector3 p, Transform t, PathSpace space)
		{
			var original = LockTransformToSpace(t, space);
			Vector3 transformedPoint = t.InverseTransformDirection(p);
			original.SetTransform(t);
			return transformedPoint;
		}

		public static bool LineSegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
		{
			float d = (b2.x - b1.x) * (a1.y - a2.y) - (a1.x - a2.x) * (b2.y - b1.y);
			if (d == 0)
				return false;
			float t = ((b1.y - b2.y) * (a1.x - b1.x) + (b2.x - b1.x) * (a1.y - b1.y)) / d;
			float u = ((a1.y - a2.y) * (a1.x - b1.x) + (a2.x - a1.x) * (a1.y - b1.y)) / d;

			return t >= 0 && t <= 1 && u >= 0 && u <= 1;
		}

		public static bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 a3, Vector2 a4)
		{
			return (a1.x - a2.x) * (a3.y - a4.y) - (a1.y - a2.y) * (a3.x - a4.x) != 0;
		}

		public static Vector2 PointOfLineLineIntersection(Vector2 a1, Vector2 a2, Vector2 a3, Vector2 a4)
		{
			float d = (a1.x - a2.x) * (a3.y - a4.y) - (a1.y - a2.y) * (a3.x - a4.x);
			if (d == 0)
			{
				Debug.LogError("Lines are parallel, please check that this is not the case before calling line intersection method");
				return Vector2.zero;
			}
			else
			{
				float n = (a1.x - a3.x) * (a3.y - a4.y) - (a1.y - a3.y) * (a3.x - a4.x);
				float t = n / d;
				return a1 + (a2 - a1) * t;
			}
		}

		//linePnt - point the line passes through
		//lineDir - unit vector in direction of line, either direction works
		//pnt - the point to find nearest on line for
		public static Vector3 ClosestPointOnDirection(Vector3 lineStart, Vector3 lineStartDir, Vector3 p)
		{
			lineStartDir.Normalize();//this needs to be a unit vector
			var v = p - lineStart;
			var d = Vector3.Dot(v, lineStartDir);
			return lineStart + lineStartDir * d;
		}

		public static Vector2 ClosestPointOnLineSegment(Vector2 p, Vector2 a, Vector2 b)
		{
			Vector2 aB = b - a;
			Vector2 aP = p - a;
			float sqrLenAB = aB.sqrMagnitude;

			if (sqrLenAB == 0)
				return a;

			float t = Mathf.Clamp01(Vector2.Dot(aP, aB) / sqrLenAB);
			return a + aB * t;
		}

		public static Vector3 ClosestPointOnLineSegment(Vector3 p, Vector3 a, Vector3 b)
		{
			Vector3 aB = b - a;
			Vector3 aP = p - a;
			float sqrLenAB = aB.sqrMagnitude;

			if (sqrLenAB == 0)
				return a;

			float t = Mathf.Clamp01(Vector3.Dot(aP, aB) / sqrLenAB);
			return a + aB * t;
		}

		public static int SideOfLine(Vector2 a, Vector2 b, Vector2 c)
		{
			return (int)Mathf.Sign((c.x - a.x) * (-b.y + a.y) + (c.y - a.y) * (b.x - a.x));
		}

		/// returns the smallest angle between ABC. Never greater than 180
		public static float MinAngle(Vector3 a, Vector3 b, Vector3 c)
		{
			return Vector3.Angle((a - b), (c - b));
		}

		public static bool PointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
		{
			float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
			float s = 1 / (2 * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
			float t = 1 / (2 * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
			return s >= 0 && t >= 0 && (s + t) <= 1;
		}

		public static bool PointsAreClockwise(Vector2[] points)
		{
			float signedArea = 0;
			for (int i = 0; i < points.Length; i++)
			{
				int nextIndex = (i + 1) % points.Length;
				signedArea += (points[nextIndex].x - points[i].x) * (points[nextIndex].y + points[i].y);
			}

			return signedArea >= 0;
		}

		public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
		{
			intersection = Vector2.zero;

			var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

			if (d == 0.0f)
			{
				return false;
			}

			var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
			var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

			if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
			{
				return false;
			}

			intersection.x = p1.x + u * (p2.x - p1.x);
			intersection.y = p1.y + u * (p2.y - p1.y);

			return true;
		}
	}

	[System.Serializable]
	public enum PathSpace { xyz, xy, xz };

	class PosRotScale
	{
		public readonly Vector3 position;
		public readonly Quaternion rotation;
		public readonly Vector3 scale;

		public PosRotScale(Transform t)
		{
			this.position = t.position;
			this.rotation = t.rotation;
			this.scale = t.localScale;
		}

		public void SetTransform(Transform t)
		{
			t.position = position;
			t.rotation = rotation;
			t.localScale = scale;

		}
	}

	[System.Serializable]
	public struct Vector2XZ
	{
		public float x;
		public float z;

		public Vector2XZ(float x, float z)
		{
			this.x = x;
			this.z = z;
		}
	}

	[System.Serializable]
	public struct Vector2YZ
	{
		public float y;
		public float z;

		public Vector2YZ(float y, float z)
		{
			this.y = y;
			this.z = z;
		}
	}
}