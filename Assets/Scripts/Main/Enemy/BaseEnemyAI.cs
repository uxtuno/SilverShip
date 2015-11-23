using UnityEngine;
using System.Collections;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// エネミーのAIとしての共通動作を規定した抽象クラス
	/// </summary>
	[RequireComponent(typeof(Enemy))]
	abstract public class BaseEnemyAI : MonoBehaviour
	{
		/// <summary>
		/// AIレベル一覧
		/// </summary>
		protected enum AILevel
		{
			None,
			Fool,
			Nomal,
			Smart,
		}

		[SerializeField]
		protected AILevel aILevel = AILevel.None;       // AIのレベル
		[SerializeField]
		protected float wait = 3;                       // 開始時の待機時間
		[SerializeField]
		protected float speed = 1;                      // 移動速度

		/// <summary>
		/// プレイヤーの参照を格納する
		/// </summary>
		protected Player player { get; private set; }

		/// <summary>
		/// 自身のEnemyクラスを格納する
		/// </summary>
		protected Enemy enemy { get; private set; }

		/// <summary>
		/// 開始時の時間を格納する
		/// </summary>
		private float startTime { get; set; }

		protected virtual void Start()
		{
			player = GameManager.instance.player;
			enemy = GetComponent<Enemy>();

			startTime = Time.time;
		}

		protected virtual void Update()
		{
			// レベルが設定されていない場合何もしない
			if (aILevel == AILevel.None)
			{
				return;
			}

			// 一定時間待機する
			if (Time.time - startTime < wait)
			{
				return;
			}

			Move();
		}

		/// <summary>
		/// 対象との距離が指定された範囲内かを調べる
		/// </summary>
		/// <param name="targetPosition"> 対象の座標</param>
		/// <param name="range"> 範囲</param>
		/// <returns> 範囲内であればtrue 範囲外であればfalse</returns>
		protected bool CheckDistance(Vector3 targetPosition, float range)
		{
			if (range < Vector3.Distance(enemy.lockOnPoint.position, targetPosition))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// 移動処理
		/// </summary>
		protected abstract void Move();
	}
}