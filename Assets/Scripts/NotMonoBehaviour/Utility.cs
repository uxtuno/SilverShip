using UnityEngine;
using System.Collections;

public static class Utility {

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
}
