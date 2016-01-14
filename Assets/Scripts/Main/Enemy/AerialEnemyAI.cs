using UnityEngine;

namespace Kuvo
{
	/// <summary>
	/// 飛行するエネミーのAI
	/// </summary>
	public class AerialEnemyAI : BaseEnemyAI
	{
		// TODO: 偶発的な移動に関する処理だが、優先度が低いのでやる気になったらやる
		//
		//  private enum LookingWaitState
		//  {
		//  	Start,
		//  	FixedTimeElapsed,
		//  	End,
		//  }

		//  private LookingWaitState lookingWaitState = LookingWaitState.End;

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
			if (baseEnemy.isAttack)
			{
				return;
			}

			// 行動時間を終えたとき
			if (actionTime <= 0)
			{
				ChangeState();
				actionTime = initActionTime;
			}

			if (baseEnemy.isPlayerLocate)
			{
				Vector3 playerPosition = player.lockOnPoint.position;
				playerPosition.y = transform.position.y;

				// プレイヤーの方向へ向きを変える
				transform.LookAt(playerPosition);

				#region - プレイヤーを発見しているときの3態
				switch (currentState)
				{
					case ActionState.Waiting:
						if (baseEnemy.currentState != BaseEnemy.EnemyState.Idle)
						{
							baseEnemy.currentState = BaseEnemy.EnemyState.Idle;
						}

						break;

					case ActionState.Moving:
						// プレイヤーに重ならない程度にエネミーを動かす
						if (!baseEnemy.CheckDistance(player.lockOnPoint.position, attackParameters.UsedRange(isCaptain)))
						{
							if (baseEnemy.currentState != BaseEnemy.EnemyState.Move)
							{
								baseEnemy.currentState = BaseEnemy.EnemyState.Move;
							}
						}
						else
						{
							if (baseEnemy.currentState != BaseEnemy.EnemyState.Idle)
							{
								baseEnemy.currentState = BaseEnemy.EnemyState.Idle;
							}
						}
						break;

					case ActionState.Attacking:
						if (EnemyCreatorSingleton.instance.isCostOver)
						{
							actionTime = 0;
							break;
						}

						// 攻撃を実行(上司なら遠距離/部下なら近接)
						if (isCaptain)
						{
							baseEnemy.currentState = BaseEnemy.EnemyState.LAttack;
						}
						else
						{
							baseEnemy.currentState = BaseEnemy.EnemyState.SAttack;
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
						if (baseEnemy.currentState != BaseEnemy.EnemyState.Idle)
						{
							baseEnemy.currentState = BaseEnemy.EnemyState.Idle;
						}
						break;

					case ActionState.Moving:
						if (baseEnemy.currentState != BaseEnemy.EnemyState.Move)
						{
							baseEnemy.currentState = BaseEnemy.EnemyState.Move;
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
			if (baseEnemy.isPlayerLocate)
			{
				switch (currentState)
				{
					case ActionState.Waiting:
						// TODO: 偶発的な移動に関する処理だが、優先度が低いのでやる気になったらやる
						//
						//  if (lookingWaitState == LookingWaitState.Start)
						//  {
						//  	lookingWaitState = LookingWaitState.FixedTimeElapsed;
						//  	break;
						//  }
						//  else if (lookingWaitState == LookingWaitState.FixedTimeElapsed)
						//  {
						//  	lookingWaitState = LookingWaitState.End;
						//  }

						// 攻撃可能範囲に入っている場合攻撃
						if (baseEnemy.CheckDistance(player.lockOnPoint.position, attackParameters.UsedRange(isCaptain)))
						{
							currentState = ActionState.Attacking;
						}
						else if (!baseEnemy.CheckDistance(player.lockOnPoint.position, attackParameters.lAttackRange))
						{
							currentState = ActionState.Moving;
						}
						break;

					case ActionState.Moving:
						// 攻撃可能範囲に入っている場合変更
						if (baseEnemy.CheckDistance(player.lockOnPoint.position, attackParameters.UsedRange(isCaptain)))
						{
							bool isAttackable = (EnemyCreatorSingleton.instance.maxAttackCost - EnemyCreatorSingleton.instance.currentAttackCostCount) >= attackParameters.largeCost;
							if (isAttackable)
							{
								currentState = ActionState.Attacking;
							}
						}
						break;

					case ActionState.Attacking:
						// 攻撃可能範囲に入っている場合待機
						if (!baseEnemy.CheckDistance(player.lockOnPoint.position, attackParameters.lAttackRange))
						{
							currentState = ActionState.Moving;
						}
						else
						{
							currentState = ActionState.Waiting;

							// TODO: 偶発的な移動に関する処理だが、優先度が低いのでやる気になったらやる
							//
							//  actionTime = 5f;
							//	lookingWaitState = LookingWaitState.Start;
						}
						break;
				}
			}
			else		// プレイヤー未発見時
			{
				// TODO: 偶発的な移動に関する処理だが、優先度が低いのでやる気になったらやる
				//
				//  if (lookingWaitState != LookingWaitState.End)
				//  {
				//  	lookingWaitState = LookingWaitState.End;
				//  }

				switch (currentState)
				{
					case ActionState.Waiting:

						baseEnemy.transform.eulerAngles += new Vector3(0, Random.Range(0f, 359f), 0);
						currentState = ActionState.Moving;
						break;

					case ActionState.Moving:
						currentState = ActionState.Waiting;
						break;

					case ActionState.Attacking:
						baseEnemy.transform.eulerAngles += new Vector3(0, Random.Range(0f, 359f), 0);
						currentState = ActionState.Moving;
						break;
				}
			}
		}
	}
}