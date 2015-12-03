using UnityEngine;
using System.Collections;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// エネミーのAIとしての共通動作を規定した抽象クラス
	/// </summary>
	[RequireComponent(typeof(Enemy))]
	abstract public class BaseEnemyAI : BaseAI
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
		protected float wait = 3;                       // 出現直後の待機時間
		protected ActionState currentState = ActionState.None;

		/// <summary>
		/// 行動時間を格納する
		/// </summary>
		protected float actionTime { get; set; }

		/// <summary>
		/// 開始時の時間を格納する
		/// </summary>
		private float startTime { get; set; }

		private Enemy _enemy;		// enemyプロパティの実体

		/// <summary>
		/// 自身のEnemyクラスを格納する
		/// </summary>
		protected Enemy enemy
		{
			get
			{
				if (!_enemy)
				{
					_enemy = this.GetComponent<Enemy>();
				}
				return _enemy;
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

		protected virtual void Start()
		{
			if (!enemy)
			{
				Debug.LogError("enemyの取得に失敗しました");
			}
			startTime = Time.time;
		}

		protected override void Update()
		{
			base.Update();

			// 一定時間待機する
			if (Time.time - startTime < wait)
			{
				return;
			}

			Move();
		}

		/// <summary>
		/// 移動処理
		/// </summary>
		protected abstract void Move();
	}
}