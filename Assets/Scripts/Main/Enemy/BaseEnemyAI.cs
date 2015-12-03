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
		
		[Tooltip("出現直後の待機時間"), SerializeField]
		protected float wait = 3;                       // 出現直後の待機時間
		protected ActionState currentState = ActionState.Waiting;

		/// <summary>
		/// 自身のEnemyクラスを格納する
		/// </summary>
		protected Enemy enemy { get; private set; }

		/// <summary>
		/// 行動時間を格納する
		/// </summary>
		protected float actionTime { get; set; }

		/// <summary>
		/// 開始時の時間を格納する
		/// </summary>
		private float startTime { get; set; }

		protected override void Start()
		{
			base.Start();

			enemy = GetComponent<Enemy>();

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