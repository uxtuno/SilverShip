using UnityEngine;
using System.Collections;
using System;

namespace Uxtuno
{
	namespace PlayerState
	{
		/// <summary>
		/// 通常時(地上)
		/// </summary>
		public class NormalState : BaseState
		{
			public static NormalState instance = new NormalState();
			private NormalState() { }

			public override void Initialize(Player player)
			{
				//	player.currentJumpState = JumpState.None;
				//	player.jumpVY = 0.0f;
				//	player.animator.SetBool(player.isTrampledID, false);
				//	player.animator.SetBool(player.isGroundedID, true);
				//	player.isAirDashPossible = false;
				//	player.attackFlow.ChangeMode(PlayerAttackFlow.Mode.Ground);
			}

			public override IEnumerator Move(Player player)
			{
				if (!player.isGrounded)
				{
					player.ChangeState(AirState.instance);
					yield break;
				}

				
				player.attackFlow.Move();
				if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.Jump))
				{
					player.Jumping();
					player.ChangeState(AirState.instance);
					yield break;
				}

				// ハイジャンプ入力
				if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.JumpTrampled))
				{
					player.HighJumping();
					player.ChangeState(AirState.instance);
					yield break;
				}
				yield break;
			}
		}
	}
}