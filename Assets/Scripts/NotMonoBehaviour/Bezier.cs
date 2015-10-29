using UnityEngine;
using System.Collections;

/// <summary>
/// ベジェ曲線上の座標を求める
/// </summary>
[System.Serializable]
public class Bezier : System.Object
{
	[SerializeField]
	private Vector3 _p0;
	[SerializeField]
	private Vector3 _p1;
	[SerializeField]
	private Vector3 _p2;
	[SerializeField]
	private Vector3 _p3;

	private int pointNumber = 2; // 頂点数
	private int stepNumber = 1; // 処理上のステップ数
	private float h = 0.0f; // ベジェ曲線上の座標を求める際の重み

	/// <summary>
	/// 1つ目の制御点
	/// </summary>
	public Vector3 p0 { get { return _p0; } set { _p0 = value; } }
	/// <summary>
	/// 2つ目の制御点
	/// </summary>
	public Vector3 p1 { get { return _p1; } set { _p1 = value; } }
	/// <summary>
	/// 3つ目の制御点
	/// </summary>
	public Vector3 p2 { get { return _p2; } set { _p2 = value; } }
	/// <summary>
	/// 4つ目の制御点
	/// </summary>
	public Vector3 p3 { get { return _p3; } set { _p3 = value; } }

	private Vector3 oldP0;
	private Vector3 oldP1;
	private Vector3 oldP2;
	private Vector3 oldP3;

	// 曲線計算用の一時変数
	private float ax, bx, cx, dx;
	private float ay, by, cy, dy;
	private float az, bz, cz, dz;
	// ベジェ曲線の各頂点を求めるループ内で加算する値
	private float firstFDX, firstFDY, firstFDZ;
	private float secondFDX, secondFDY, secondFDZ;
	private float thirdFDX, thirdFDY, thirdFDZ;

	private Vector3[] points; // ベジェ曲線に含まれるすべての座標

	public Bezier()
	{
		Initialize(0, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
	}

	public Bezier(int pointNumber)
	{
		Initialize(pointNumber, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
	}

	public Bezier(int pointNumber, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
	{
		Initialize(pointNumber, v0, v1, v2, v3);
	}

	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="pointNumber">頂点数</param>
	/// <param name="v0">1つ目の制御点</param>
	/// <param name="v1">2つ目の制御点</param>
	/// <param name="v2">3つ目の制御点</param>
	/// <param name="v3">4つ目の制御点</param>
	public void Initialize(int pointNumber, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		this.p0 = p0;
		this.p1 = p1;
		this.p2 = p2;
		this.p3 = p3;
		
		this.pointNumber = pointNumber;
		points = new Vector3[pointNumber];
		SetUp();
	}

	/// <summary>
	/// ベジェ曲線の特定位置の座標を求める
	/// </summary>
	/// <param name="t">ベジェ曲線上の位置(0.0 >= t <= 1.0)</param>
	/// <returns></returns>
	public Vector3 GetPointAtTime(float t)
	{
		float rt2 = Mathf.Pow(1.0f - t, 2);
		float rt3 = Mathf.Pow(1.0f - t, 3);
		float t2 = t * t;
		float t3 = t * t * t;
		float x = rt3 * p0.x + 3.0f * rt2 * t * p1.x + 3.0f * (1.0f - t) * t2 * p2.x + t3 * p3.x;
		float y = rt3 * p0.y + 3.0f * rt2 * t * p1.y + 3.0f * (1.0f - t) * t2 * p2.y + t3 * p3.y;
		float z = rt3 * p0.z + 3.0f * rt2 * t * p1.z + 3.0f * (1.0f - t) * t2 * p2.z + t3 * p3.z;
		return new Vector3(x, y, z);
	}

	/// <summary>
	/// ベジェ曲線上の座標すべてを求める
	/// </summary>
	/// <returns>分割数</returns>
	public Vector3[] GetAllPoint()
	{
		if(pointNumber == 0)
		{
			return null;
		}

		CheckChanged();
		return points;
	}

	/// <summary>
	/// 計算できる値をあらかじめ計算しておく
	/// </summary>
	void SetUp()
	{
		ax = -p0.x + 3 * p1.x + -3 * p2.x + p3.x;
		ay = -p0.y + 3 * p1.y + -3 * p2.y + p3.y;
		az = -p0.z + 3 * p1.z + -3 * p2.z + p3.z;

		bx = 3 * p0.x + -6 * p1.x + 3 * p2.x;
		by = 3 * p0.y + -6 * p1.y + 3 * p2.y;
		bz = 3 * p0.z + -6 * p1.z + 3 * p2.z;

		cx = -3 * p0.x + 3 * p1.x;
		cy = -3 * p0.y + 3 * p1.y;
		cz = -3 * p0.z + 3 * p1.z;

		dx = p0.x;
		dy = p0.y;
		dz = p0.z;

		// ステップ数
		stepNumber = pointNumber - 1;
		h = 1.0f / stepNumber; // 重みの増分

		// あらかじめ加算する値を求めておく
		firstFDX = ax * (h * h * h) + bx * (h * h) + cx * h;
		firstFDY = ay * (h * h * h) + by * (h * h) + cy * h;
		firstFDZ = az * (h * h * h) + bz * (h * h) + cz * h;

		secondFDX = 6 * ax * (h * h * h) + 2 * bx * (h * h);
		secondFDY = 6 * ay * (h * h * h) + 2 * by * (h * h);
		secondFDZ = 6 * az * (h * h * h) + 2 * bz * (h * h);

		thirdFDX = 6 * ax * (h * h * h);
		thirdFDY = 6 * ay * (h * h * h);
		thirdFDZ = 6 * az * (h * h * h);
	}

	/// <summary>
	/// 値が変更されていればベジェ曲線上の座標を全て求め
	/// 配列へ格納
	/// </summary>
	private void CheckChanged()
	{
		if (oldP0 != p0 ||
			oldP1 != p1 ||
			oldP2 != p2 ||
			oldP3 != p3)
		{
			SetUp();
			ComputeBezierPoints();
			oldP0 = p0;
			oldP1 = p1;
			oldP2 = p2;
			oldP3 = p3;
		}
	}

	/// <summary>
	/// 設定された頂点数を元に
	/// ベジェ曲線上の座標(頂点)を全て求める
	/// </summary>
	public void ComputeBezierPoints()
	{
		float pointX, pointY, pointZ;

		pointX = dx;
		pointY = dy;
		pointZ = dz;

		points[0].x = pointX;
		points[0].y = pointY;
		points[0].z = pointZ;

		// ベジェ曲線の各頂点を求める
		for (int i = 0; i < stepNumber; i++)
		{
			pointX += firstFDX;
			pointY += firstFDY;
			pointZ += firstFDZ;

			firstFDX += secondFDX;
			firstFDY += secondFDY;
			firstFDZ += secondFDZ;

			secondFDX += thirdFDX;
			secondFDY += thirdFDY;
			secondFDZ += thirdFDZ;

			points[i + 1].x = pointX;
			points[i + 1].y = pointY;
			points[i + 1].z = pointZ;
		}
	}

	/// <summary>
	/// ベジェ曲線かどうかを調べる
	/// </summary>
	/// <returns>ベジェ曲線ならtrue</returns>
	public bool CheckBezier()
	{
		// ただの直線
		if(p0 == p1 && p2 == p3)
		{
			return false;
		}

		return true;
	}

	public float _bezier_linearlen(float t)
	{
		if(t < 0.0f ||t > 1.0f || pointNumber < 4)
		{
			return t;
		}

		float[] ll = new float[pointNumber + 1];
		float ni = 1.0f / pointNumber;
		float tt = 0.0f;
		Vector3 pp = GetPointAtTime(0.0f);
		Vector3 p;
		ll[0] = 0.0f;
		for(int i = 1; i <= pointNumber; ++i)
		{
			tt += ni;
			p = GetPointAtTime(tt);
			ll[i] = ll[i - 1] + (pp - p).magnitude;
			pp = p;
		}

		float n = 1.0f / ll[pointNumber];
		for(int i = 1; i <= pointNumber; ++i)
		{
			ll[i] *= n;
		}

		int _i;
		for(_i = 0; _i < pointNumber - 1; ++_i)
		{
			if(t >= ll[_i] && t <= ll[_i + 1])
			{
				break;
			}

			if(_i >= pointNumber)
			{
				return t;
			}
		}

		n = (ll[_i + 1] - ll[_i]);
		if(n < 0.0001)
		{
			n = 0.0001f;
		}
		n = (t - ll[_i]) / n;
		return (_i * (1.0f - n) + (_i + 1) * n) * ni;
	}

	private float calcBezierX(float t)
	{
		float rt2 = Mathf.Pow(1.0f - t, 2);
		float rt3 = Mathf.Pow(1.0f - t, 3);
		float t2 = t * t;
		float t3 = t * t * t;
		return rt3* p0.x + 3.0f * rt2 * t * p1.x + 3.0f * (1.0f - t) * t2 * p2.x + t3 * p3.x;
    }
}