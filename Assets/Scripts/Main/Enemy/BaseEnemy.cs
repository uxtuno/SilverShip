using System.Collections;
using UnityEngine;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// 敵の共通動作を規定した抽象クラス
	/// </summary>
	abstract public class BaseEnemy : Actor
	{
		/// <summary>
		/// 敵の状態(※AIの状態とは別で定義される)
		/// </summary>
		public enum EnemyState
		{
			Idle,
			Move,
			GoBack,
			SAttack,
			LAttack,
			Stagger,
			Death,
		}

		[Tooltip("移動速度"), SerializeField]
		protected float speed = 1;
		[Tooltip("視野角"), SerializeField]
		protected float viewAngle = 90;
		[Tooltip("視認距離"), SerializeField]
		protected float viewRange = 10;
		[Tooltip("遠距離攻撃の弾の発射位置"), SerializeField]
		protected Transform muzzle = null;
		[Tooltip("攻撃コストを保持する時間")]
		protected float CostKeepSecond = 2;
		private CameraController cameraController;

		private Animator _animator;   // animatorプロパティの実体

		/// <summary>
		/// 自身のAnimatorを取得する(キャッシュあり)
		/// </summary>
		protected new Animator animator
		{
			get
			{
				if (!_animator)
				{
					_animator = GetComponentInChildren<Animator>();
					//_animator = GetComponent<Animator>();
				}

				return _animator;
			}
		}

		private BaseEnemyAI _baseEnemyAI;   // baseEnemyAIプロパティの実体

		/// <summary>
		/// 自身のBaseEnemyAIを取得する(キャッシュあり)
		/// </summary>
		protected BaseEnemyAI baseEnemyAI
		{
			get
			{
				if (!_baseEnemyAI)
				{
					_baseEnemyAI = GetComponent<BaseEnemyAI>();
				}

				return _baseEnemyAI;
			}
		}

		private Player _player;     // playerプロパティの実体

		/// <summary>
		/// プレイヤーの参照を取得する(キャッシュあり)
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
		/// 攻撃判定用のゲームオブジェクトを取得する(キャッシュあり)
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
		/// チームを組んでいるかどうか
		/// </summary>
		public bool isTeamUp
		{
			get
			{
				return GetComponent<BaseEnemyAI>().captain ?? false;
			}
		}

		/// <summary>
		/// 現在の状態
		/// </summary>
		public EnemyState currentState { get; set; }


		/// <summary>
		/// 地面の上に立っているかどうか
		/// </summary>
		public bool haveGround { get; protected set; }

		/// <summary>
		/// プレイヤーを発見しているかどうか
		/// </summary>
		public bool isPlayerLocate
		{
			get
			{
				return isTeamUp ? true : PlayerSearch();
			}
		}

		/// <summary>
		/// 攻撃中かどうか
		/// </summary>
		public bool isAttack { get; protected set; }

		/// <summary>
		/// エネミーを目視することができる最も近い距離
		/// (エネミーの大きさに応じて変更する必要がある)
		/// </summary>
		abstract protected float sight { get; set; }

		protected virtual void Awake()
		{
			currentState = EnemyState.Idle;
			haveGround = false;
			isAttack = false;
		}

		protected virtual void Start()
		{
			if (!baseEnemyAI)
			{
				Debug.LogError("BaseEnemyAIが存在しません");
			}

			if (currentState != EnemyState.Idle)
			{
				currentState = EnemyState.Idle;
			}

			cameraController = GameObject.FindGameObjectWithTag(TagName.CameraController).GetComponent<CameraController>();
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

			// 体力が0を下回っているかを確認
			if (hp <= 0 && currentState != EnemyState.Death)
			{
				if (isAttack)
				{
					// 使用しているコストを解放
					if (baseEnemyAI.isCaptain)
					{
						EnemyManagerSingleton.instance.StartCostAddForSeconds(-baseEnemyAI.attackParameters.lAttackCost, 0);
					}
					else
					{
						EnemyManagerSingleton.instance.StartCostAddForSeconds(-baseEnemyAI.attackParameters.sAttackCost, 0);
					}
					isAttack = false;
				}

				currentState = EnemyState.Death;

				// 自身のAIを停止する(コルーチン含む)
				if (baseEnemyAI.enabled)
				{
					baseEnemyAI.StopAllCoroutines();
					baseEnemyAI.enabled = false;
				}

				// 実行中のすべてのコルーチンを停止
				StopAllCoroutines();
				OnDie();
				return;
			}

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
		/// 対象との距離が指定された範囲内かを調べる(自身のLockOnPointから計測)
		/// </summary>
		/// <param name="targetPosition"> 対象の座標</param>
		/// <param name="range"> 範囲</param>
		/// <returns> 範囲内であればtrue 範囲外であればfalse</returns>
		public bool CheckDistance(Vector3 targetPosition, float range)
		{
			if (range < Vector3.Distance(lockOnPoint.position, targetPosition))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// 死亡する
		/// 
		/// オーバーライドする場合の処理実行順
		///	{ 
		///		// 処理
		///		base.OnDie();
		///	}
		/// </summary>
		/// <param name="second"> インスタンスが消滅するまでの時間</param>
		/// <returns></returns>
		protected virtual void OnDie()
		{
			EnemyManagerSingleton.instance.enemies.Remove(this);
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

		/// <summary>
		/// エネミーが破棄されるとき、アニメーションビヘイビアから呼ばれる
		/// 
		/// オーバーライドする場合の処理実行順
		///	{ 
		///		// 処理
		///		base.DestroyEnemy();
		///	}
		/// </summary>
		/// <param name="waitSeconds"></param>
		public virtual void DestroyEnemy(float waitSeconds)
		{
			Destroy(gameObject, waitSeconds);
		}
	}
}