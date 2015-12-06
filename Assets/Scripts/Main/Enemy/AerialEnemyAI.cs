using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// 飛行するエネミーのAI
	/// </summary>
	public class AerialEnemyAI : BaseEnemyAI
	{
		[System.Serializable]
		private class AttackableRanges
		{
			[Tooltip("近接攻撃")]
			public float shortRange = 5f;
			[Tooltip("遠距離攻撃")]
			public float longRange = 10f;
		}

		[Tooltip("攻撃可能範囲(半径)"), SerializeField]
		private AttackableRanges attackableRanges;

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
			Action();
		}

		/// <summary>
		/// 各状態の行動
		/// </summary>
		private void Action()
		{
			actionTime -= Time.deltaTime;

			// 攻撃動作中なら何もしない
			if (enemy.isAttack)
			{
				return;
			}

			// 行動時間を終えたとき
			if (actionTime <= 0)
			{
				ChangeState();
				actionTime = initActionTime;
			}

			if (enemy.isPlayerLocate)
			{
				#region - プレイヤーを発見しているときの3態
				switch (currentState)
				{
					case ActionState.Waiting:
						Vector3 playerPosition = player.lockOnPoint.position;
						playerPosition.y = transform.position.y;

						// プレイヤーの方向へ向きを変える
						transform.LookAt(playerPosition);
						break;

					case ActionState.Moving:
						playerPosition = player.lockOnPoint.position;
						playerPosition.y = transform.position.y;

						// プレイヤーの方向へ向きを変える
						transform.LookAt(playerPosition);

						// プレイヤーに重ならない程度にエネミーを動かす
						if (Mathf.Abs(transform.position.x - playerPosition.x) > 0.5f || Mathf.Abs(transform.position.z - playerPosition.z) > 1f)
						{
							if (enemy.currentState != BaseEnemy.EnemyState.Move)
							{
								enemy.currentState = BaseEnemy.EnemyState.Move;
							}
						}
						else
						{
							if (enemy.currentState != BaseEnemy.EnemyState.Idle)
							{
								enemy.currentState = BaseEnemy.EnemyState.Idle;
							}
						}
						break;

					case ActionState.Attacking:
						float shortRange = attackableRanges.shortRange;

						// 上司の場合、近距離攻撃可能範囲を部下の1/5とする
						if (isCaptain)
						{
							shortRange /= 5;
						}

						// 近接攻撃可能範囲内なら近接攻撃を実行
						if (enemy.CheckDistance(player.transform.position, shortRange))
						{
							enemy.currentState = BaseEnemy.EnemyState.SAttack;
							break;
						}

						// 遠距離攻撃を実行
						enemy.currentState = BaseEnemy.EnemyState.LAttack;
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
						if (enemy.currentState != BaseEnemy.EnemyState.Idle)
						{
							enemy.currentState = BaseEnemy.EnemyState.Idle;
						}
						break;

					case ActionState.Moving:
						if (enemy.currentState != BaseEnemy.EnemyState.Move)
						{
							enemy.currentState = BaseEnemy.EnemyState.Move;
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
						if (enemy.CheckDistance(player.lockOnPoint.position, attackableRanges.longRange))
						{
							bool isAttackable = (EnemyCreatorSingleton.instance.maxAttackCost - EnemyCreatorSingleton.instance.currentAttackCostCount) > enemy.attackCosts.largeCost;
							if (isAttackable)
							{
								currentState = ActionState.Attacking;
							}
						}
						break;

					case ActionState.Attacking:
						// 攻撃可能範囲に入っている場合待機
						if (enemy.CheckDistance(player.lockOnPoint.position, attackableRanges.longRange))
						{
							currentState = ActionState.Waiting;
						}
						else
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