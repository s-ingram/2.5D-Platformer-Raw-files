using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
	public static class Utils
	{
		private static System.Random rng = new System.Random();

		public enum SceneIndexes
        {
			Setup = 0,
			MainMenu = 1,
			Tutorial = 2,
			LEVEL_ONE = 3
        }

		public static Quaternion GetRandomVector2Direction()
		{
			//	Return a Quaternion that is facing in a random 2D direction.
			return Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
		}

		public static Vector3 GetMouseWorldPosition()
		{
			//	Returns a Vector the that has the (x, y) position of the mouse in the worldSpace.

			Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			pos.z = 0f;
			return pos;
		}

		public static bool MouseIsOnScreen()
		{
			//	Checks whether the mouse screen position is within the Screen boundaries.

			if (Input.mousePosition.x <= 0 || Input.mousePosition.y <= 0 || Input.mousePosition.x >= Screen.width || Input.mousePosition.y >= Screen.height)
				return false;
			else
				return true;
		}

		public static Vector3 RotatePointAroundPoint(Vector3 p1, Vector3 p2, float angle)
		{
			angle = angle * Mathf.Deg2Rad;

			//return new Vector3(Mathf.Cos(angle) * (p1.x - p2.x) - Mathf.Sin(angle) * (p1.y - p2.y) + p2.x, Mathf.Sin(angle) * (p1.x - p2.x) + Mathf.Cos(angle) * (p1.y - p2.y) + p2.y);

			float s = Mathf.Sin(angle);
			float c = Mathf.Cos(angle);

			p1.x -= p2.x;
			p1.y -= p2.y;

			float xnew = p1.x * c - p1.y * s;
			float ynew = p1.x * s + p1.y * c;

			p1.x = xnew + p2.x;
			p1.y = ynew + p2.y;
			return p1;
		}

		public static Vector3 GetRandomPointNear(Vector3 point, float radius)
		{
			Vector3 p = point + Vector3.right * Random.Range(0f, radius);
			float angle = Random.Range(-Mathf.PI, Mathf.PI);
			float s = Mathf.Sin(angle);
			float c = Mathf.Cos(angle);

			p.x -= point.x;
			p.y -= point.y;

			float xnew = p.x * c - p.y * s;
			float ynew = p.x * s + p.y * c;

			p.x = xnew + point.x;
			p.y = ynew + point.y;
			return p;
		}

		public static void Shuffle<T> (this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static Vector3 GetGaussianDistributedVector(Vector3 vector)
		{
			float delta = 1f;
			float x = Random.Range(-3f, 3f);

			float g_x = 1 - (1 / Mathf.Sqrt(2 * Mathf.PI)) - Mathf.Exp(-Mathf.Pow(x, 2) / Mathf.Pow(delta, 2)) / (delta * Mathf.Sqrt(2 * Mathf.PI));

			vector.z += g_x;

			//	change
			return vector;
		}
	}
}