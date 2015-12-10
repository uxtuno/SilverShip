using UnityEngine;
using Uxtuno;

// 利便性のためnamespaceはつけない

/// <summary>
/// アクター
/// </summary>
public class Actor : MyMonoBehaviour
{
	protected virtual float hp { get; set; }
	protected virtual int attack { get; set; }
	protected virtual int defence { get; set; }
	private Transform _lockOnPoint;

	public Transform lockOnPoint
	{
		get
		{
			if(_lockOnPoint == null)
			{
				foreach(Transform child in transform)
				{
					if(child.tag == TagName.LockOnPoint)
					{
						_lockOnPoint = child;
						break;
					}
				}
			}
			return _lockOnPoint;
		}
	}

	/// <summary>
	/// ダメージを与える
	/// </summary>
	/// <param name="attackPower">攻撃側の攻撃力</param>
	/// <param name="magnification">技倍率</param>
	public virtual void Damage(int attackPower, float magnification)
	{
		float n = (attackPower / 2.0f - defence / 4.0f) * magnification * Random.Range(0.9f, 1.1f);
		hp -= n;
	}
}
