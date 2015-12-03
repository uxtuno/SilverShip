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

			currentState = ActionState.Waiting;

			actionTime = initActionTime;
		}

		/// <summary>
		/// 移動処理
		/// </summary>
		protected override void Move()
		{
			if(enemy.isPlayerLocate && !this.HasComponent<TeamAI>())
			{
				gameObject.AddComponent<TeamAI>();
			}
			Action();
		}

		/// <summary>
		/// 各状態の行動
		/// </summary>
		private void Action()
		{
			actionTime -= Time.deltaTime;

			// 行動時間を終えたとき
			if (actionTime <= 0)
			{
				ChangeState();
				actionTime = initActionTime;
			}

			// 攻撃動作中なら何もしない
			if (enemy.isAttack)
			{
				return;
			}

			if (enemy.isPlayerLocate)
			{
				#region - プレイヤーを発見しているときの3態
				switch (currentState)
				{
					case ActionState.Waiting:
						actionTime = 0;
						break;

					case ActionState.Moving:
						Vector3 playerPosition = player.lockOnPoint.position;
						playerPosition.y = transform.position.y;

						// プレイヤーの方向へ向きを変える
						transform.LookAt(playerPosition);

						// プレイヤーに重ならない程度にエネミーを動かす
						if (Mathf.Abs(transform.position.x - playerPosition.x) > 0.5f || Mathf.Abs(transform.position.z - playerPosition.z) > 1f)
						{
							if (enemy.currentState != Enemy.EnemyState.Move)
							{
								enemy.currentState = Enemy.EnemyState.Move;
							}
						}
						else
						{
							if (enemy.currentState != Enemy.EnemyState.Idle)
							{
								enemy.currentState = Enemy.EnemyState.Idle;
							}
						}
						break;

					case ActionState.Attacking:

						if (enemy.currentState != Enemy.EnemyState.SAttack)
						{
							enemy.currentState = Enemy.EnemyState.SAttack;
						}
						break;
				}
				#endregion
			}
			else
			{
				#region - プレイヤーを探しているときの3態
				switch (currentState)
				{
					case ActionState.Waiting:
						if (enemy.currentState != Enemy.EnemyState.Idle)
						{
							enemy.currentState = Enemy.EnemyState.Idle;
						}
						break;
					case ActionState.Moving:
						if (enemy.currentState != Enemy.EnemyState.Move)
						{
							enemy.currentState = Enemy.EnemyState.Move;
						}
						break;
					case ActionState.Attacking:
						actionTime = 0;
						break;
				}
				#endregion
			}
		}

		/// <summary>
		/// 次の状態に変更する
		/// </summary>
		private void ChangeState()
		{
			if (enemy.isPlayerLocate)
			{
				switch (currentState)
				{
					case ActionState.Waiting:
						currentState = ActionState.Moving;
						break;

					case ActionState.Moving:
						// 攻撃可能範囲に入っている場合変更
						if (enemy.CheckDistance(player.lockOnPoint.position, attackStateRange))
						{
							currentState = ActionState.Attacking;
						}
						break;

					case ActionState.Attacking:
						// 攻撃可能範囲から外れた場合変更
						if (!enemy.CheckDistance(player.lockOnPoint.position, attackStateRange))
						{
							currentState = ActionState.Moving;
						}
						break;
				}
			}
			else
			{
				switch (currentState)
				{
					case ActionState.Waiting:
						enemy.transform.eulerAngles += new Vector3(0, Random.Range(0f, 359f), 0);
						currentState = ActionState.Moving;
						break;
					case ActionState.Moving:
						currentState = ActionState.Waiting;
						break;
					case ActionState.Attacking:
						enemy.transform.eulerAngles += new Vector3(0, Random.Range(0f, 359f), 0);
						currentState = ActionState.Moving;
						break;
				}
			}
		}
	}
}