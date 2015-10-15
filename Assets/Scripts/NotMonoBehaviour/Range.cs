using System;

/// <summary>
/// 範囲クラス
/// 最大値と最小値の指定が出来る
/// </summary>
/// <typeparam name="T"></typeparam>
public class Range<T> where T : new()
{
	public readonly T min;
	public readonly T max;
	public Range()
	{
		min = new T();
		max = new T();
	}
	/// <param name="max">最大値</param>
	/// <param name="min">最小値</param>
	public Range(T min, T max)
	{
		this.min = min;
		this.max = max;
	}
	/// <param name="max">最大値</param>
	public Range(T max)
	{
		this.min = new T();
		this.max = max;
	}

	public float GetSize()
	{
		float size = (float)(Convert.ToDouble(min) + Convert.ToDouble(max));
		return size;
	}
}
