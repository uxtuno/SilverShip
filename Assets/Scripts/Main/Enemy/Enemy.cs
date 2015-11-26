using UnityEngine;
using System.Collections;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// 敵の共通動作を規定した抽象クラス
	/// </summary>
	abstract public class Enemy : Actor
	{
		public enum ActionState
		{
			None,
			Idle,
			Bone,
			Move,
			Attack,
			Stagger,
			Death,
		}

		[SerializeField]
		protected float viewAngle = 120;                    // 視野角
		[SerializeField]
		protected float viewRange = 10;                     // 視認距離
		[SerializeField]
		protected Transform muzzle = null;                  // 遠距離攻撃の弾の発射位置
		protected ContainedObjects contained;
		private CameraController cameraController;

		private Animation _animation;   // animationプロパティの実体

		/// <summary>
		/// 自身のAnimationを取得する
		/// </summary>
		protected new Animation animation
		{
			get
			{
				if (!_animation)
				{
					_animation = GetComponent<Animation>();
				}

				return _animation;
			}
		}

		private Player _player;     // playerプロパティの実体

		/// <summary>
		/// プレイヤーの参照を取得する
		/// </summary>
		protected Player player
		{
			get
			{
				if (!_player)
				{
					_player = GameManager.instance.player;
				}

				return _player;
			}
		}

		private GameObject _shortRangeAttackAreaObject;   // shortRangeAttackAreaObjectプロパティの実体

		/// <summary>
		/// 攻撃判定用のゲームオブジェクトを取得する
		/// </summary>
		protected GameObject shortRangeAttackAreaObject
		{
			get
			{
				if (!_shortRangeAttackAreaObject)
				{
					foreach (GameObject child in gameObject.GetChildren(true))
					{
						if (child.tag == TagName.AttackArea)
						{
							_shortRangeAttackAreaObject = child;
						}
					}

					_shortRangeAttackAreaObject.GetSafeComponent<AttackArea>();

					if (!_shortRangeAttackAreaObject)
					{
						Debug.LogError("攻撃判定用のゲームオブジェクトが見つかりませんでした。");
					}
				}

				return _shortRangeAttackAreaObject;
			}
		}

		/// <summary>
		/// 現在の状態
		/// </summary>
		public ActionState currentState { get; set; }

		/// <summary>
		/// 地面の上に立っているかどうか
		/// </summary>
		public bool haveGround { get; protected set; }

		/// <summary>
		/// プレイヤーを発見しているかどうか
		/// </summary>
		public bool isPlayerLocate { get; private set; }

		/// <summary>
		/// エネミーを目視することができる最も近い距離
		/// (エネミーの大きさに応じて変更する必要がある)
		/// </summary>
		abstract protected float sight { get; set; }

		protected virtual void Awake()
		{
			currentState = ActionState.Bone;
			haveGround = false;
		}

		protected virtual void Start()
		{
			if (currentState != ActionState.Bone)
			{
				currentState = ActionState.Bone;
			}

			cameraController = GameObject.FindGameObjectWithTag(TagName.CameraController).GetComponent<CameraController>();
			contained = GetComponentInChildren<ContainedObjects>();
		}

		protected virtual void Update()
		{
			// カメラとの距離に応じて描画状態を切り替える
			Vector3 cameraToVector = cameraController.cameraTransform.position - transform.position;
			if (cameraToVector.magnitude < sight)
			{
				isShow = false;
			}
			else
			{
				isShow = true;
			}

			isPlayerLocate = PlayerSearch();
		}

		protected virtual void LateUpdate()
		{
			if (hp <= 0 && currentState != ActionState.Death)
			{
				currentState = ActionState.Death;
				StopAllCoroutines();
				StartCoroutine(OnDie(2));
			}

			Debug.Log(contained);
			Vector3 aveVec = Vector3.zero;
			foreach (Transform t in contained)
			{
				aveVec += t.position;
			}
			aveVec /= contained.GetContainedObjects().Count;
			transform.position -= (aveVec - transform.position).normalized * Time.deltaTime;
		}

		protected virtual void OnCollisionExit(Collision collision)
		{
			if (collision.transform.tag == TagName.Scaffold)
			{
				haveGround = false;
			}
		}

		protected virtual void OnCollisionEnter(Collision collision)
		{
			if (collision.transform.tag == TagName.Scaffold)
			{
				haveGround = true;
			}
		}

		public override void Damage(int attackPower, float magnification)
		{
			base.Damage(attackPower, magnification);
			if (haveGround)
			{
				StartCoroutine(GroundStagger());
			}
			else
			{
				StartCoroutine(AirStagger());
			}
		}

		/// <summary>
		/// 視野内でプレイヤーを探す
		/// </summary>
		protected bool PlayerSearch()
		{
			float distance = Vector3.Distance(player.lockOnPoint.position, transform.position);

			// プレイヤーが視認距離にいない場合
			if (viewRange < distance)
			{
				return false;
			}

			Vector3 adjustmentAmount = new Vector3(0, 0.7f, -0.1f);
			float angle = Vector3.Angle(player.lockOnPoint.position - (transform.position + adjustmentAmount), transform.forward);
			// プレイヤーが視野角にいない場合
			if (viewAngle / 2 < angle)
			{
				if (90 < angle || 2f < distance)
				{
					return false;
				}
			}

			Debug.DrawRay((transform.position + adjustmentAmount), player.lockOnPoint.position - (transform.position + adjustmentAmount), Color.red);
			return true;
		}

		/// <summary>
		/// 指定秒をかけて死亡する
		/// </summary>
		/// <param name="second"> インスタンスが消滅するまでの時間</param>
		/// <returns></returns>
		protected virtual IEnumerator OnDie(float second)
		{
			float time = 0.0f;
			while (time < second)
			{
				time += Time.deltaTime;

				yield return new WaitForEndOfFrame();
			}
			Debug.Log("死んだー", gameObject);
			Destroy(gameObject);
		}

		/// <summary>
		/// 空中でよろける
		/// </summary>
		abstract protected IEnumerator AirStagger();

		/// <summary>
		/// 地上でよろける
		/// </summary>
		abstract protected IEnumerator GroundStagger();

		/// <summary>
		/// 近接攻撃
		/// </summary>
		abstract public IEnumerator ShortRangeAttack();

		/// <summary>
		/// 遠距離攻撃
		/// </summary>
		abstract public IEnumerator LongRangeAttack();
	}
}