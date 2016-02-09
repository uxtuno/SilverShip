using System.Collections;
using UnityEngine;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// エネミーAI
	/// 一定時間置きに停止してプレイヤーを探し、見つかれば
	/// その方向に直進する
	/// ランダムに方向に方向を変え、直進
	/// </summary>
	public class EnemySimpleSearchAI : MyMonoBehaviour
	{
		private float speed = 3.0f;
		private float waitSeconds = 2.0f; // 待機時間
		private float straightSeconds = 3.0f; // 直進する時間

		// Use this for initialization
		IEnumerator Start()
		{
			for (;;)
			{
				yield return new WaitForSeconds(Random.value * waitSeconds);

				Player player = GameManager.instance.player;
				float r = Vector3.Angle(transform.forward, player.transform.position - transform.position);

				if (r < 30.0f)
				{
					transform.forward = player.transform.position - transform.position;
					Vector3 v1 = player.transform.position;
					v1.y = 0.0f;
					Vector3 v2 = transform.position;
					v2.y = 0.0f;
					transform.rotation = Quaternion.FromToRotation(Vector3.forward, v1 - v2); ;
				}
				else
				{
					Vector3 angles = Vector3.zero;
					angles.y = Random.Range(0, 360);
					transform.eulerAngles = angles;
				}

				float t = 0.0f;
				while (t < straightSeconds)
				{
					transform.position += transform.forward * speed * Time.deltaTime;
					yield return null;
					t += Time.deltaTime;
				}
			}
		}
	}
}
