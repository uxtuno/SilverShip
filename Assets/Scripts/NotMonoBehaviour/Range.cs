using System;

/// <summary>
/// 範囲クラス
/// 最大値と最小値の指定が出来る
/// </summary>
/// <typeparam name="T"></typeparam>
public class Range<T> where T : struct, IComparable
{
	public readonly T min;
	public readonly T max;
	public Range()
		: this(new T(), new T())
	{
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
		: this(new T(), max)
	{
	}

	public T GetSize()
	{
		object size = Convert.ToDouble(max) - Convert.ToDouble(min);
		return (T)size;
	}

	/// <summary>
	/// 範囲内に含まれているか
	/// </summary>
	/// <param name="pos"></param>
	/// <returns></returns>
	public bool IsInRange(T pos)
	{
		if (pos.CompareTo(min) < 0)
			return false;
		else if (pos.CompareTo(max) > 0)
			return false;

		return true;
    }
}
