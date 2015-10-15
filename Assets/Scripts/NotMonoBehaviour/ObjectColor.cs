using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 色の混ざり具合をビットの合成で表現
/// 例) 赤(0001) + 青(0100) = マゼンタ(0101)
/// </summary>
public enum ColorState
{
	NONE = 0,
	RED = 1,					// 赤
	GREEN = 2,					// 緑
	YELLOW = RED | GREEN,		// 黄
	BLUE = 4,					// 青
	MAGENTA = RED | BLUE,		// マゼンタ
	CYAN = GREEN | BLUE,		// シアン
	WHITE = RED | GREEN | BLUE,	// 白
}

[System.Serializable]
/// <summary>
/// オブジェクトの色を表す
/// </summary>
public class ObjectColor
{
	public ObjectColor()
	{
		this.state = ColorState.NONE;
	}

	public ObjectColor(ColorState state)
	{
		this.state = state;
	}

	[SerializeField]
	private ColorState _state = ColorState.NONE;
	/// <summary>
	/// 色の状態。アルファ値は考慮しない
	/// </summary>
	public ColorState state
	{
		get { return _state; }
		set { _state = value; }
	}

	/// <summary>
	/// RGBのみの情報。アルファ値は常に1.0f
	/// </summary>
	public Color rgb
	{
		get { return GetColor(state); }
	}

	/// <summary>
	/// 色コード
	/// </summary>
	public Color color
	{
		get
		{
			Color c = rgb;
            return new Color(c.r, c.g, c.b, alpha); // 透明度の情報を付加して返す
		}
	}

	[SerializeField]
	private float _alpha = 1.0f; // 透明度
	/// <summary>
	/// 透明度
	/// </summary>
	public float alpha
	{
		get { return _alpha; }
		set { _alpha = value; }
	}

	private static readonly Dictionary<ColorState, Color> colorTable = new Dictionary<ColorState, Color>();

	/// <summary>
	/// ColorStateを色コードに変換するための変換テーブルを作成
	/// </summary>
	static ObjectColor()
	{
		colorTable.Add(ColorState.NONE, new Color());
		colorTable.Add(ColorState.RED, Color.red);
		colorTable.Add(ColorState.GREEN, Color.green);
		colorTable.Add(ColorState.YELLOW, new Color(1.0f, 1.0f, 0.0f));
		colorTable.Add(ColorState.BLUE, Color.blue);
		colorTable.Add(ColorState.MAGENTA, Color.magenta);
		colorTable.Add(ColorState.CYAN, Color.cyan);
		colorTable.Add(ColorState.WHITE, Color.white);
	}

	/// <summary>
	/// ColorState型の値を色コードとして返す
	/// </summary>
	/// <param name="color"></param>
	/// <returns></returns>
	public static Color GetColor(ColorState color)
	{
		return colorTable[color];
	}

	/// <summary>
	/// Color型へのキャスト
	/// </summary>
	/// <param name="obj"></param>
	public static explicit operator Color(ObjectColor obj)
	{
		return obj.color;
	}

	/// <summary>
	/// ColorState型を代入できるように
	/// </summary>
	/// <param name="obj"></param>
	public static implicit operator ObjectColor(ColorState obj)
	{
		ObjectColor color = new ObjectColor(obj);
		return color;
	}

	public override string ToString()
	{
		return ((int)(color.r * 255)).ToString("x2") + ((int)color.g * 255).ToString("x2") + ((int)color.b * 255).ToString("x2");
	}

	//public static bool operator==(ObjectColor obj1, ObjectColor obj2)
	//{
	//	return obj1.state == obj2.state;
	//}

	//public static bool operator !=(ObjectColor obj1, ObjectColor obj2)
	//{
	//	return !(obj1 == obj2);
	//}
}

