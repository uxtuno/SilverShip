using UnityEngine;
using Uxtuno;

// 利便性のためnamespaceはつけない

/// <summary>
/// アクター
/// </summary>
public abstract class Actor : MyMonoBehaviour
{
	[Tooltip("画面上などに表示する際のキャラクターの名前"), SerializeField]
	private string _actorName = string.Empty;		// actorNameプロパティの実体

	/// <summary>
	/// 画面上などに表示する際のキャラクターの名前を取得する
	/// </summary>
	public string actorName { get { return _actorName; } }

	[Tooltip("HPの最大値"), SerializeField]
	private float _maxHp;							// maxHpプロパティの実体
	/// <summary>
	/// HPの最大値(読み取り専用)
	/// </summary>
	public virtual float maxHp { get { return _maxHp; } }

	private float _hp = float.NegativeInfinity;		// hpプロパティの実体(初期値として負の無限数を使用する)

	public virtual float hp
	{
		get
		{
			if (_hp == float.NegativeInfinity)
			{
				_hp = maxHp;
			}
			return _hp;
		}
		protected set
		{
			_hp = value;
		}
	}

	public virtual float level { get; protected set; }
	protected virtual int attack { get; set; }
	protected virtual int defence { get; set; }

	private Transform _lockOnPoint;

	public Transform lockOnPoint
	{
		get
		{
			if (_lockOnPoint == null)
			{
				foreach (Transform child in transform)
				{
					if (child.tag == TagName.LockOnPoint)
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
