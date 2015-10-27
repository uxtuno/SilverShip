using UnityEngine;
using System.Collections;

namespace Uxtuno
{
	/// <summary>
	/// モンスターAI
	/// 一定時間置きに停止してプレイヤーを探し、見つかれば
	/// その方向に直進する
	/// ランダムに方向に方向を変え、直進
	/// </summary>
	public class MonsterSimpleSearchAI : MyMonoBehaviour
	{
		float speed = 3.0f;

		// Use this for initialization
		IEnumerator Start()
		{
			for(;;)
			{
				yield return new WaitForSeconds(Random.value * 10.0f);

				Player player = GameManager.instance.player;
				float r = Vector3.Angle(transform.forward, player.transform.position - transform.position);

				if (r < 30.0f)
				{
					transform.forward = player.transform.position - transform.position;
				}
				else
				{
					Vector3 angles = Vector3.zero;
					angles.y = (float)Random.Range(0, 360);
					transform.eulerAngles = angles;
				}

				float t = 0.0f;
				while (t < 3.0f)
				{
					transform.position += transform.forward * speed * Time.deltaTime;
					yield return null;
					t += Time.deltaTime;
				}
			}
		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}
