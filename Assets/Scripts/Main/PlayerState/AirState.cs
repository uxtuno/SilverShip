using UnityEngine;
using System.Collections;
using System;

namespace Uxtuno
{
	namespace PlayerState
	{
		/// <summary>
		/// 空中
		/// </summary>
		public class AirState : BaseState
		{
			public static AirState instance = new AirState();
			private AirState() { }

			private static readonly float movementRestriction = 0.5f; // この値を掛けることで移動速度を制限
			private static readonly float airDashPossibleSeconds = 0.4f; // 空中ダッシュが可能になる時間
			private static readonly float airDashDisableSeconds = 1.4f; // 空中ダッシュが可能になる時間
			private float airDashPossibleCount; // 空中ダッシュが可能になる時間のカウンタ

			public override IEnumerator Move(Player player)
			{
				player.attackFlow.Move();

				if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.Jump))
				{
					if (player.isAirDashPossible)
					{
						player.currentState = new AirDashState(player);
						yield break;
					}
				}

				// 踏みつけジャンプ入力成功
				if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.JumpTrampled))
				{
					if (player.lockOnTarget != null)
					{
						// todo : 空中ダッシュ入力受付時間をそのまま踏みつけジャンプの受付時間に利用している
						// そのうち整理するだろう(希望的観測
						if (airDashPossibleCount > airDashPossibleSeconds && airDashPossibleCount < airDashDisableSeconds)
						{
							player.currentState = new DashToTargetState(player, player.lockOnTarget.lockOnPoint.position);
						}
					}
					else
					{
						Ray ray = new Ray(player.transform.position, player.meshRoot.forward);
						RaycastHit hit;
						//if (player.__footContained.GetContainedObjects().Count != 0)
						if (Physics.Raycast(ray, out hit, 0.5f))
						{
							// 壁付近ならジャンプ
							if (hit.transform.tag == "Wall")
							{
								player.Jumping();
								player.currentState = new WallKick(player);
								Vector3 angles = Vector3.zero;
								angles.y = Mathf.Atan2(hit.normal.x, hit.normal.z) * Mathf.Rad2Deg;
								player.meshRoot.eulerAngles = angles;
							}
						}
					}
					return;
				}

				// 接地しているので通常状態に
				if (player.isGrounded && player.jumpVY <= 0.0f)
				{
					player.currentState = new NormalState(player);
					return;
				}

				Vector3 moveDirection = player.calclateMoveDirection();
				float speed = player.maxSpeed * movementRestriction;

				if (player.isGrounded && player.jumpVY <= 0.0f)
				{
					player.currentState = new NormalState(player);
				}

				if (player.jumpVY < 0.0f)
				{
					// 落下中は速度を落とす
					player.FallGravity();
				}
				else
				{
					// 上昇中の重力
					player.Gravity();
				}
				Vector3 moveVector = moveDirection * speed;
				moveVector.y = player.jumpVY;

				player.Move(moveVector * Time.deltaTime);
				if (moveDirection != Vector3.zero)
				{
					Vector3 newAngles = Vector3.zero;
					newAngles.y = Mathf.Atan2(moveVector.x, moveVector.z) * Mathf.Rad2Deg;
					player.meshRoot.eulerAngles = newAngles;
				}
			}

			public override void Initialize(Player player)
			{
				player.attackFlow.ChangeMode(PlayerAttackFlow.Mode.Air);
			}

			public override IEnumerator Move(Player player)
			{
				while (airDashPossibleCount < airDashPossibleSeconds)
				{
					airDashPossibleCount += Time.deltaTime;
					yield return null;
				}
			}
		}
	}
}