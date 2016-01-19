using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Uxtuno
{
	/// <summary>
	/// 結界発生時の処理
	/// </summary>
	public class Barrier : MyMonoBehaviour
	{
		private static readonly int power = 100;
		private static readonly float explosionPower = 200.0f; // 爆発力
		private static readonly float lifeTime = 3.0f; // 生存期間
		[SerializeField, Tooltip("あたり判定を表示するか")]
		private bool showCollision;

		void Start()
		{
			Transform collisionArea = transform.Find("CollisionArea");
			float radius = GetComponentInChildren<SphereCollider>().radius;

			if (collisionArea != null)
			{
				collisionArea.localScale = new Vector3(radius, radius, radius);
			}

			// 結界内のColliderを列挙し、その中から敵のみを抽出
			// tagがEnemyなら BaseEnemyがアタッチされているはず
			IEnumerable<Kuvo.BaseEnemy> inBarrierEnemies = Physics.OverlapSphere(transform.position, radius)
				.Where((obj) => obj.tag == TagName.Enemy)
				.Select((obj) => obj.GetComponent<Kuvo.BaseEnemy>());

			Destroy(gameObject, lifeTime);
		}

		void OnTrrigerEnter(Collider other)
		{
			if (other.tag == TagName.Enemy)
			{
				// 倍率は1固定とする
				other.GetComponent<Actor>().Damage(power, 1.0f);
			}
		}
	}
}