using UnityEngine;
using System.Collections;
using Uxtuno;

namespace Kuvo
{
	[RequireComponent(typeof(Enemy))]
	public class AerialEnemyAI : BaseEnemyAI
	{
		private static readonly int limitAngle = 50;

		protected override void Start()
		{
			base.Start();
		}

		/// <summary>
		/// 移動処理
		/// </summary>
		protected override void Move()
		{
			if (PlayerSearch())
			{
				Vector3 playerPosition = player.transform.position;
				playerPosition.y = 0;

				// プレイヤーの方向へ向きを変える
				transform.LookAt(playerPosition);
			}
			else
			{
				// 見つからなかったときの処理
			}
		}

		void LookFront(float value)
		{
			Vector3 eulerAngles = transform.eulerAngles;
			eulerAngles.x = value;
			transform.eulerAngles = eulerAngles;
		}
	}
}