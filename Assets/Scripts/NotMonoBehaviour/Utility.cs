using UnityEngine;
using System.Collections;

public static class Utility
{

	public static void CalculateBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		const int nbit = 10;
		const float n2bit = nbit + nbit;
		const int N = 1 << nbit;
		float N2 = Mathf.Pow(2.0f, n2bit);

		Vector3 NBezier2 = p0 * N2;

		Vector3 dNBezier2 = p2 + (2 * N - 2) * p1 + (1 - 2 * N) * p0;

		Vector3 d2NBezier2 = 2 * p2 - 4 * p1 + 2 * p0;

		for (int i = 0; i <= N; i++)
		{
			Debug.Log(NBezier2.x / N2);

			NBezier2 += dNBezier2;

			dNBezier2 += d2NBezier2;
		}
	}

	/// <summary>
	/// 扇型と点のあたり判定
	/// </summary>
	/// <param name="cx">中心X</param>
	/// <param name="cy">中心Y</param>
	/// <param name="rad">半径</param>
	/// <param name="startAng">開始角</param>
	/// <param name="endAng">終了角</param>
	/// <param name="px">点X</param>
	/// <param name="py">点Y</param>
	/// <returns></returns>
	public static bool hitTestArcPoint(float cx, float cy, float rad, float startAng, float endAng, float px, float py)
	{
		float dx = px - cx;
		float dy = py - cy;
		float sx = (float)Mathf.Cos(startAng * Mathf.Deg2Rad);
		float sy = (float)Mathf.Sin(startAng * Mathf.Deg2Rad);
		float ex = (float)Mathf.Cos(endAng * Mathf.Deg2Rad);
		float ey = (float)Mathf.Sin(endAng * Mathf.Deg2Rad);
		if (dx * dx + dy * dy < rad * rad)
		{
			if (sx * ey - ex * sy > 0)
			{
				if (sx * dy - dx * sy < 0) return false; // second test
				if (ex * dy - dx * ey > 0) return false; // third test
				return true; // all test passed
			}
			else
			{
				if (sx * dy - dx * sy > 0) return true; // second test
				if (ex * dy - dx * ey < 0) return true; // third test
				return false; // all test failed
			}
		}
		return false;
	}
}
