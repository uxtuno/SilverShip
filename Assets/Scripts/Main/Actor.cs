using UnityEngine;
using System.Collections;

// 利便性のためnamespaceはつけない

/// <summary>
/// アクター
/// </summary>
public class Actor : MonoBehaviour
{
	protected float hp { get; set; } = 0.0f;
	protected int atack { get; set; }
	protected int defence { get; set; }

	/// <summary>
	/// ダメージを与える
	/// </summary>
	/// <param name="atackPower">攻撃側の攻撃力</param>
	/// <param name="magnification">技倍率</param>
	public virtual void Damage(int atackPower, float magnification)
	{
		hp -= (atackPower / 2.0f - defence / 4.0f) * magnification * Random.Range(0.9f, 1.1f);
	}
}
