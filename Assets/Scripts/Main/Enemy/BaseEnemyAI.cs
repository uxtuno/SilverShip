using UnityEngine;
using System.Linq;
using Uxtuno;
using System.Collections.Generic;

namespace Kuvo
{
	/// <summary>
	/// エネミーのAIとしての共通動作を規定した抽象クラス
	/// </summary>
	[RequireComponent(typeof(BaseEnemy))]
	abstract public class BaseEnemyAI : MyMonoBehaviour
	{
		protected enum ActionState
		{
			None,
			Waiting,
			Moving,
			Attacking,
		}

		// actionTimeの初期値一覧
		private readonly float[] actionTimeCollection = { 1f, 1.5f, 2f, 2.5f, 3f };

		[Tooltip("出現直後の待機時間"), SerializeField]
		protected float wait = 3;                   // 出現直後の待機時間
		[Tooltip("チームを組む範囲(半径)"), SerializeField]
		protected float teamRange = 30.0f;          // チームを組む範囲(半径)
		protected ActionState currentState = ActionState.None;

		/// <summary>
		/// 上司を格納する
		/// </summary>
		public BaseEnemyAI captain { get; set; }

		/// <summary>
		/// 部下を格納する
		/// </summary>
		public List<BaseEnemyAI> members { get; set; }

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

		private BaseEnemy _enemy;       // enemyプロパティの実体

		/// <summary>
		/// 自身のBaseEnemyクラスを格納する
		/// </summary>
		public BaseEnemy enemy
		{
			get
			{
				if (!_enemy)
				{
					_enemy = GetComponent<BaseEnemy>();
				}
				return _enemy;
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
			captain = null;
			members = new List<BaseEnemyAI>();
			members.Clear();
		}

		protected virtual void Start()
		{
			if (!enemy)
			{
				Debug.LogError("enemyの取得に失敗しました");
			}

			startTime = Time.time;
		}

		protected virtual void Update()
		{
			// 一定時間待機する
			if (Time.time - startTime < wait)
			{
				return;
			}

			if (enemy.isPlayerLocate)
			{
				if (!enemy.isTeamUp)
				{
					TeamUp();
				}
			}

			// enemyが倒されるとき
			if (enemy.currentState == BaseEnemy.EnemyState.Death)
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

			Move();
		}

		/// <summary>
		/// チームを組む
		/// </summary>
		private void TeamUp()
		{
			enemy.isTeamUp = true;

			// 上司及び部下が設定されていないとき
			if (!captain && members.Count <= 0)
			{
				BaseEnemyAI captainAI = EnemyCreatorSingleton.instance.captainAI;
				if (!captainAI)     // 上司が存在しなければ
				{
					captain = this;     // 自身を上司に登録(これによりisCaptainがtrueになる)

					// teamRange内に存在する自分以外のBaseEnemyAIを取得
					BaseEnemyAI[] enemies = Physics.OverlapSphere(transform.position, teamRange)
											.Select((obj) => obj.GetComponent<BaseEnemyAI>())
											.Where((obj) => obj != null && transform != obj.transform)
											.ToArray();

					BaseEnemyAI member1 = null;     // 部下1
					BaseEnemyAI member2 = null;     // 部下2
					float min = Mathf.Infinity;     // エネミー同士の最短距離
					foreach (BaseEnemyAI current in enemies)
					{
						float distance = Vector3.Distance(enemy.lockOnPoint.position, current.enemy.lockOnPoint.position);
						if (min > distance)
						{
							min = distance;
							member2 = member1;
							member1 = current;
						}
					}

					if (member1)
					{
						member1.captain = this;
					}

					if (member2)
					{
						member2.captain = this;
					}

					members.Add(member1);
					members.Add(member2);
				}
				else        // 上司が存在していれば
				{
					captain = captainAI;
					captain.members.Add(this);
				}
			}
		}

		/// <summary>
		/// 移動処理
		/// </summary>
		protected abstract void Move();
	}
}