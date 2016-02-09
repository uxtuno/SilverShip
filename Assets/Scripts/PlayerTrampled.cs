using UnityEngine;
using System.Collections;

namespace Uxtuno
{
	/// <summary>
	/// 踏みつけジャンプ
	/// </summary>
	public class PlayerTrampled : MyMonoBehaviour
	{
		private ContainedObjects trampledEnemies; // 踏みつけ対象エネミー
		//private PlayerInput input = PlayerInput.instance;

		void Start()
		{
			trampledEnemies = this.GetSafeComponent<ContainedObjects>();
			trampledEnemies.AddTagName(TagName.Enemy);
		}

		/// <summary>
		/// 踏みつけジャンプにより敵にダメージを与える
		/// </summary>
		/// <param name="magnification"></param>
		public void Trampled(int attackPower, float magnification)
		{
			foreach(Transform enemy in trampledEnemies)
			{
				enemy.GetComponent<Kuvo.BaseEnemy>().Damage(attackPower, magnification);
			}
		}

		/// <summary>
		/// 踏みつけられるターゲットが存在するか
		/// </summary>
		/// <returns></returns>
		public bool hasTarget()
		{
			return trampledEnemies.GetContainedObjects().Count > 0;
		}
    }
}