using UnityEngine;
using System.Collections;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// 飛行するエネミーのAI
	/// </summary>
	public class AerialEnemyAI : BaseEnemyAI
	{
		[Tooltip("攻撃状態に移行する範囲(半径)"), SerializeField]
		private float attackStateRange = 5f;    // 攻撃状態に移行する範囲(半径)

		protected override void Start()
		{
			base.Start();
		}

		/// <summary>
		/// 移動処理
		/// </summary>
		protected override void Move()
		{
			if (enemy.currentState == Enemy.ActionState.Bone || enemy.currentState == Enemy.ActionState.Idle)
			{
				enemy.currentState = Enemy.ActionState.Move;
			}

			if (enemy.isPlayerLocate)
			{
				if (CheckDistance(player.lockOnPoint.position, attackStateRange))
				{
					Attack();
				}

				Vector3 playerPosition = player.lockOnPoint.position;
				playerPosition.y = transform.position.y;

				// プレイヤーの方向へ向きを変える
				transform.LookAt(playerPosition);
				if (Mathf.Abs(transform.position.x - playerPosition.x) > 0.5f || Mathf.Abs(transform.position.z - playerPosition.z) > 0.5f)
				{
					transform.Translate(Vector3.forward * speed * Time.deltaTime);
				}
				else
				{
					if (enemy.currentState != Enemy.ActionState.Attack)
					{
						enemy.currentState = Enemy.ActionState.Idle;
					}
				}
			}
			else
			{
				// 見つからなかったときの処理
					if (enemy.currentState != Enemy.ActionState.Attack)
					{
						enemy.currentState = Enemy.ActionState.Idle;
					}
			}
		}

		private void Attack()
		{
			switch (aILevel)
			{
				case AILevel.None:
					break;

				case AILevel.Fool:
					break;

				case AILevel.Nomal:
					break;

				case AILevel.Smart:
					break;
			}
		}
	}
}