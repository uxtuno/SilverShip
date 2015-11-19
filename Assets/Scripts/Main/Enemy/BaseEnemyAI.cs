using UnityEngine;
using System.Collections;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// エネミーのAIとしての共通動作を規定した抽象クラス
	/// </summary>
	abstract public class BaseEnemyAI : MonoBehaviour
	{
		protected Player player { get; private set; }
		[SerializeField]
		protected float timeOfStartToMove = 3;      // AI開始時の待機時間
		protected float startTime { get; set; }
		[SerializeField]
		protected float speed = 5;                  // 移動速度
		[SerializeField]
		protected float viewAngle = 120;             // 視野角
		[SerializeField]
		protected float viewRange = 10;				// 視認距離

		protected virtual void Start()
		{
			player = GameManager.instance.player;
			startTime = Time.time;
		}

		protected virtual void Update()
		{
			// 一定時間待機する
			if(Time.time - startTime < timeOfStartToMove)
			{
				return;
			}

			Move();
		}

		/// <summary>
		/// 視野内でプレイヤーを探す
		/// </summary>
		protected virtual bool PlayerSearch()
		{
			// プレイヤーが視認距離にいない場合
			if (viewRange < Vector3.Distance(player.transform.position, transform.position))
			{
				//Debug.Log("視認距離にいないよ!");
				return false;
			}

			// プレイヤーが視野角にいない場合
			if (viewAngle / 2 < Vector3.Angle(player.transform.position - transform.position, transform.forward))
			{
				//Debug.Log("視野角にいないよ!");
				return false;
			}

			//Debug.Log("視界にはいった!");
			return true;
		}

		/// <summary>
		/// 移動処理
		/// </summary>
		protected abstract void Move();
	}
}