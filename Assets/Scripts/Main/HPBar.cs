using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image), typeof(CanvasRenderer))]
public class HPBar : MonoBehaviour
{
	[Tooltip("true:Player / false:ロックオン対象"), SerializeField]
	private bool usePlayerHp = false;
	[Tooltip("targetの名前を表示するText(指定しなければ表示しない)"), SerializeField]
	private Text nameDisplayObjct = null;
	[Tooltip("Levelを表示するText(指定しなければ表示しない)"), SerializeField]
	private Text levelDisplayObjct = null;
	private Actor target = null;			// HP表示対象のActor
	private Actor oldTarget = null;			// 前フレーム時のTarget
	private float oldHp = 0f;				// 前フレーム時のHP
	private Vector3 initialPosition;		// hpRectTransform.positionの初期化用
	private float maxWidth;					// HPバーの最大幅
	private float oneHpWidth;				// HP1当たりの幅

	private RectTransform _hpRectTransform;       // hpRectTransformプロパティの実体

	/// <summary>
	/// HPのRectTransformを取得する(キャッシュあり)
	/// </summary>
	private RectTransform hpRectTransform
	{
		get
		{
			if (!_hpRectTransform)
			{
				foreach (RectTransform current in GetComponentsInChildren<RectTransform>())
				{
					if (current.name == "HP")
					{
						_hpRectTransform = current;
						break;
					}
				}

				if (!_hpRectTransform)
				{
					Debug.LogError("HPにRectTransformがアタッチされていませんでした。");
					enabled = false;
				}
			}

			return _hpRectTransform;
		}
	}

	private Image _hpImage;       // hpImageプロパティの実体

	/// <summary>
	/// HPのImageを取得する(キャッシュあり)
	/// </summary>
	private Image hpImage
	{
		get
		{
			if (!_hpImage)
			{
				foreach (Image current in GetComponentsInChildren<Image>())
				{
					if (current.name == "HP")
					{
						_hpImage = current;
						break;
					}
				}

				if (!_hpImage)
				{
					Debug.LogError("HPの可視化に失敗しました");
					enabled = false;
				}
			}

			return _hpImage;
		}
	}

	void Start()
	{
		initialPosition = hpRectTransform.position;
		maxWidth = hpRectTransform.sizeDelta.x;

		if (usePlayerHp)
		{
			target = GameManager.instance.player;	// HP表示対象をプレイヤーに
			if (nameDisplayObjct)
			{
				nameDisplayObjct.text = target ? target.actorName : "";
			}
			if (levelDisplayObjct)
			{
				levelDisplayObjct.text = target ? "Lv." + target.level : "";
			}
			oldTarget = target;
			oneHpWidth = maxWidth / target.hp;
			oldHp = target.maxHp;
		}
		else
		{
			Initialize();
		}
	}

	void Update()
	{
		if (usePlayerHp)
		{
			// Imageを非表示にしないと正常にHPバーが消えなかった
			if (target.hp <= 0)
			{
				hpImage.enabled = false;
			}
			else if (target.hp < oldHp)
			{
				CalcBarLength();
			}
		}
		else
		{
			// tergetの切り替わりチェック
			if (target != GameManager.instance.player.lockOnTarget)
			{
				target = GameManager.instance.player.lockOnTarget;
				if (nameDisplayObjct)
				{
					nameDisplayObjct.text = target ? target.actorName : "";
				}
			}

			if (target != oldTarget)
			{
				Initialize();

				if (target)
				{
					oldTarget = target;
					oneHpWidth = maxWidth / target.maxHp;
					oldHp = target.hp;
					float damage = target.maxHp - target.hp;
					float damageWidth = oneHpWidth * damage;

					hpRectTransform.sizeDelta -= new Vector2(damageWidth, 0);
					Debug.Log(hpRectTransform.sizeDelta);
					hpRectTransform.localPosition += new Vector3(damageWidth / 2f, 0, 0);

					// Actor名が非表示状態の場合、表示に戻す
					if (!nameDisplayObjct.enabled)
					{
						nameDisplayObjct.enabled = true;
					}

					foreach (Image current in GetComponentsInChildren<Image>())
					{
						// バーが非表示状態の場合、表示に戻す
						if (!current.enabled)
						{
							current.enabled = true;
						}
					}
				}
			}

			// Imageを非表示にしないと正常にHPバーが消えなかった
			if (target && target.hp <= 0)
			{
				hpImage.enabled = false;
			}
			else if (target && target.hp < oldHp)
			{
				CalcBarLength();
			}
		}
	}

	/// <summary>
	/// 初期化(しかしinitialPosition及びmaxWidthに値が入っていることを前提とする)
	/// </summary>
	private void Initialize()
	{
		// 各変数・プロパティを初期状態に
		hpRectTransform.position = initialPosition;
		hpRectTransform.sizeDelta = new Vector2(maxWidth, hpRectTransform.sizeDelta.y);
		nameDisplayObjct.enabled = false;
		oldTarget = null;
		oneHpWidth = 0f;
		oldHp = 0f;
		foreach (Image current in GetComponentsInChildren<Image>())
		{
			current.enabled = false;
		}
	}

	/// <summary>
	/// ダメージ量を計算し、HPバーの長さを計算・変更する
	/// </summary>
	private void CalcBarLength()
	{
		if (!target || target.hp <= 0 || oldHp <= 0 || oneHpWidth <= 0)
		{
			Debug.LogError("不正にHPバーの長さを変更しようとしました");
		}

		float damage = oldHp - target.hp;
		float damageWidth = oneHpWidth * damage;

		hpRectTransform.sizeDelta -= new Vector2(damageWidth, 0);
		hpRectTransform.localPosition += new Vector3(damageWidth / 2f, 0, 0);

		oldHp = target.hp;
	}
}
