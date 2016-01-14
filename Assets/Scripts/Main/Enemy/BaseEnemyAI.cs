using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// エネミーのAIとしての共通動作を規定した抽象クラス
	/// </summary>
	[RequireComponent(typeof(BaseEnemy))]
	abstract public class BaseEnemyAI : MyMonoBehaviour
	{
		/// <summary>
		/// 敵AIの行動状態(※敵の状態とは別で定義される)
		/// </summary>
		protected enum ActionState
		{
			None,
			Waiting,
			Moving,
			Attacking,
		}

		/// <summary>
		/// 攻撃の各種パラメーターを保持する
		/// </summary>
		[System.Serializable]
		public class AttackParameters
		{
			[Tooltip("近接攻撃範囲(半径)"), SerializeField]
			private float _sAttackRange = 5f;

			public float sAttackRange
			{
				get { return _sAttackRange; }
			}

			[Tooltip("遠距離攻撃範囲(半径)"), SerializeField]
			private float _lAttackRange = 10f;

			public float lAttackRange
			{
				get { return _lAttackRange; }
			}

			[Tooltip("近接攻撃コスト"), SerializeField]
			private int _sAttackCost = 1;

			public int sAttackCost
			{
				get { return _sAttackCost; }
			}

			[Tooltip("遠距離攻撃コスト"), SerializeField]
			private int _lAttackCost = 2;

			public int lAttackCost
			{
				get { return _lAttackCost; }
			}

			[Tooltip("近接攻撃の予備動作時間(秒)")]
			private float _sAttackPreOperatSecond = 2;

			public float sAttackPreOperatSecond
			{
				get { return _sAttackPreOperatSecond; }
			}

			[Tooltip("遠距離攻撃の予備動作時間(秒)")]
			private float _lAttackPreOperatSecond = 2;

			public float lAttackPreOperatSecond
			{
				get { return _lAttackPreOperatSecond; }
			}

			/// <summary>
			/// 自身が通常使用する攻撃可能範囲を取得する
			/// </summary>
			/// <param name="isCaptain">上司かどうか</param>
			/// <returns></returns>
			public float UsedRange(bool isCaptain)
			{
				return isCaptain ? lAttackRange : sAttackRange;
			}

			/// <summary>
			/// コストの大きい方を取得する
			/// </summary>
			public int largeCost
			{
				get { return (lAttackCost > sAttackCost) ? lAttackCost : sAttackCost; }
			}
		}

		// actionTimeの初期値一覧(※配列にはconst修飾子が使えない)
		private readonly float[] actionTimeCollection = { 1f, 1.5f, 2f, 2.5f, 3f };

		[Tooltip("出現直後の待機時間"), SerializeField]
		protected float wait = 3;
		[Tooltip("攻撃の各種パラメーター")]
		public AttackParameters attackParameters = new AttackParameters();
		[Tooltip("チームを組む範囲(半径)"), SerializeField]
		protected float teamRange = 30.0f;
		protected ActionState currentState = ActionState.None;

		/// <summary>
		/// 上司を格納する
		/// </summary>
		public BaseEnemyAI captain { get; set; }

		/// <summary>
		/// 部下を格納する
		/// </summary>
		//public List<BaseEnemyAI> members { get; set; }
		public List<BaseEnemyAI> members = new List<BaseEnemyAI>();

		/// <summary>
		/// 行動時間を格納する
		/// </summary>
		protected float actionTime { get; set; }

		/// <summary>
		/// 開始時の時間を格納する
		/// </summary>
		private float startTime { get; set; }

		/// <summary>
		/// 自身が上司かどうかを格納する
		/// </summary>
		public bool isCaptain
		{
			get
			{
				if (captain)
				{
					if (captain == this)
					{
						return true;
					}
				}
				return false;
			}
		}

		private BaseEnemy _baseEnemy;       // enemyプロパティの実体

		/// <summary>
		/// 自身のBaseEnemyクラスを格納する
		/// </summary>
		public BaseEnemy baseEnemy
		{
			get
			{
				if (!_baseEnemy)
				{
					_baseEnemy = GetComponent<BaseEnemy>();
				}
				return _baseEnemy;
			}
		}

		private Player _player;     // playerプロパティの実体

		/// <summary>
		/// プレイヤーの参照を格納する
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

		/// <summary>
		/// actionTimeの初期化に使用する値
		/// </summary>
		protected float initActionTime
		{
			get
			{
				int random = Random.Range(0, actionTimeCollection.Length);
				return actionTimeCollection[random];
			}
		}

		protected virtual void Awake()
		{
			// 上司・部下の初期化
			captain = null;
			members = new List<BaseEnemyAI>();
			members.Clear();
		}

		protected virtual void Start()
		{
			if (!baseEnemy)
			{
				Debug.LogError("enemyの取得に失敗しました");
			}

			// waitには負の値が入らないようにする
			if (wait < 0)
			{
				wait = Mathf.Abs(wait);
			}

			startTime = Time.time;
		}

		protected virtual void FixedUpdate()
		{
			// 一定時間待機する
			if (Time.time - startTime < wait)
			{
				return;
			}

			if (baseEnemy.isPlayerLocate)
			{
				if (!baseEnemy.isTeamUp)
				{
					TeamUp();
				}
			}

			// チームを組んでいるenemyが倒されるとき
			if (baseEnemy.currentState == BaseEnemy.EnemyState.Death && baseEnemy.isTeamUp)
			{
				if (isCaptain)  // 上司のとき
				{
					if (members.Count > 0)
					{
						// 最初の部下を新たな上司に
						BaseEnemyAI newCaptain = members[0];

						// 部下全員に新たな上司を設定
						foreach (BaseEnemyAI member in members)
						{
							member.captain = newCaptain;
						}
					}
				}
				else if (captain.members.Contains(this))    // 部下のとき
				{
					captain.members.Remove(this);
				}
			}

			// 上司のteamRangeからプレイヤーが外れたとき
			if (isCaptain && !baseEnemy.CheckDistance(player.lockOnPoint.position, teamRange))
			{
				TeamDisbanded();
			}

			Move();
		}

		public void OnDrawGizmos()
		{
			if (isCaptain && Application.loadedLevelName == "enemytest")
			{
				Gizmos.DrawSphere(baseEnemy.lockOnPoint.position, teamRange);
			}
		}

		/// <summary>
		/// チームを組む
		/// </summary>
		private void TeamUp()
		{
			// 上司及び部下が設定されていないとき
			if (!captain && members.Count <= 0)
			{
				// 上司をキャッシュ
				BaseEnemyAI captainAI = EnemyCreatorSingleton.instance.captainAI;

				// 上司が存在しなければ
				if (!captainAI)
				{
					// 自身を上司に登録(これによりisCaptainおよびenemy.isTeamUpがtrueになる)
					captain = this;

					// Linqを使用してteamRange内に存在する自分以外のBaseEnemyAIを取得
					BaseEnemyAI[] enemies = Physics.OverlapSphere(transform.position, teamRange)
											.Select((obj) => obj.GetComponent<BaseEnemyAI>())
											.Where((obj) => obj != null && transform != obj.transform)
											.ToArray();

					BaseEnemyAI member1 = null;     // 部下1
					BaseEnemyAI member2 = null;     // 部下2
					float min = Mathf.Infinity;     // エネミー同士の最短距離
					float oldMin = min;             // 前回のmin
					foreach (BaseEnemyAI current in enemies)
					{
						// メンバーとの距離を計算
						float distance = Vector3.Distance(baseEnemy.lockOnPoint.position, current.baseEnemy.lockOnPoint.position);

						// 最短ならば
						if (min > distance)
						{
							oldMin = min;
							min = distance;

							if (member1)
							{
								member2 = member1;
							}

							member1 = current;
						}
						// 2番目に短ければ
						else if (oldMin > distance)
						{
							member2 = current;
						}
					}

					if (member1)
					{
						member1.captain = this;
						members.Add(member1);
					}

					if (member2)
					{
						member2.captain = this;
						members.Add(member2);
					}

				}
				// 上司が存在していれば
				else
				{
					captain = captainAI;
					captain.members.Add(this);
				}
			}
		}

		/// <summary>
		/// チームを解散する
		/// </summary>
		private void TeamDisbanded()
		{
			// 自身が上司でない場合何もしない
			if (!isCaptain)
			{
				return;
			}

			// 全員の上司をnullに
			captain = null;
			foreach (BaseEnemyAI current in members)
			{
				current.captain = null;
			}

			// チーム解散
			members.Clear();
		}

		/// <summary>
		/// 移動処理
		/// </summary>
		protected abstract void Move();
	}
}