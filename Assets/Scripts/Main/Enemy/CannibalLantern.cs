using System.Collections;
using UnityEngine;

namespace Kuvo
{
	/// <summary>
	/// 人食い提灯クラス
	/// </summary>
	public class CannibalLantern : BaseEnemy
	{
		[Tooltip("弾のプレハブ"), SerializeField]
		private GameObject bulletPrafab = null;     // 弾のプレハブ
		private GameObject bulletCollecter = null;
		private EnemyState oldState = EnemyState.None;

		/// <summary>
		/// 人食い提灯を目視することができる最も近い距離
		/// </summary>
		protected override float sight { get; set; }

		protected override void Awake()
		{
			hp = 30;
			attack = 1;
			defence = 2;
			sight = 1.5f;
		}

		protected override void Start()
		{
			base.Start();

			shortRangeAttackAreaObject.GetComponent<AttackArea>().Set(attack, 1.0f);
			if (!(bulletCollecter = GameObject.Find("BulletCollecter")))
			{
				bulletCollecter = new GameObject("BulletCollecter");
			}
		}

		protected override void Update()
		{
			base.Update();

			if (Input.GetKeyDown(KeyCode.B))
			{
				Damage(int.MaxValue, Mathf.Infinity);
			}

			if (currentState != oldState)
			{
				switch (currentState)
				{
					case EnemyState.None:
						break;

					case EnemyState.Idle:
						break;

					case EnemyState.Move:
						break;

					case EnemyState.Bone:
						break;

					case EnemyState.Search:
						break;

					case EnemyState.SAttack:
						if (!isAttack && !EnemyCreatorSingleton.instance.isCostOver)
						{
							StartCoroutine(ShortRangeAttack());
						}
						break;

					case EnemyState.LAttack:
						if (!isAttack && !EnemyCreatorSingleton.instance.isCostOver)
						{
							StartCoroutine(LongRangeAttack());
						}
						break;

					case EnemyState.Stagger:
						break;

					case EnemyState.Death:
						break;
				}

				oldState = currentState;
			}

			if (currentState == EnemyState.Move)
			{
				//transform.Translate(Vector3.forward * speed * Time.deltaTime);
				//rigidbody.AddRelativeForce(Vector3.forward * speed, ForceMode.Acceleration);
				rigidbody.velocity = (player.lockOnPoint.position - lockOnPoint.position).normalized * speed;
			}
			else
			{
				rigidbody.velocity = Vector3.zero;
			}
		}

		/// <summary>
		/// 空中でよろける
		/// </summary>
		protected override IEnumerator AirStagger()
		{
			EnemyState oldState = currentState;
			currentState = EnemyState.Stagger;
			print("ぐはっ！");

			yield return new WaitForSeconds(1);

			currentState = oldState;
		}

		/// <summary>
		/// 地上でよろける
		/// </summary>
		protected override IEnumerator GroundStagger()
		{
			StartCoroutine(AirStagger());
			yield break;
		}

		/// <summary>
		/// 近接攻撃
		/// </summary>
		public override IEnumerator ShortRangeAttack()
		{
			isAttack = true;
			EnemyCreatorSingleton.instance.StartCostAddForSeconds(attackCosts.shortRange, 0);
			currentState = EnemyState.Move;

			// ここに予備動作
			float counter = 0;
			Vector3 playerPosition = player.lockOnPoint.position;
			playerPosition.y = transform.position.y;
			while (true)
			{
				if (!CheckDistance(playerPosition, 1f))
				{
					// プレイヤーの方向へ向きを変える
					transform.LookAt(playerPosition);
				}
				else
				{
					if (currentState != EnemyState.None)
					{
						currentState = EnemyState.None;
						break;
					}
				}

				counter += Time.deltaTime;
				if (counter > 2)
				{
					if (currentState != EnemyState.None)
					{
						currentState = EnemyState.None;
					}
					break;
				}

				yield return new WaitForEndOfFrame();
			}

			shortRangeAttackAreaObject.SetActive(true);

			yield return new WaitForSeconds(1.0f);

			shortRangeAttackAreaObject.SetActive(false);
			EnemyCreatorSingleton.instance.StartCostAddForSeconds(-attackCosts.shortRange, 2);
			isAttack = false;
		}

		/// <summary>
		/// 遠距離攻撃
		/// </summary>
		public override IEnumerator LongRangeAttack()
		{
			isAttack = true;
			EnemyCreatorSingleton.instance.StartCostAddForSeconds(attackCosts.longRange, 0);

			yield return new WaitForSeconds(1f);

			// 弾の発射位置・角度を登録
			Transform t = (muzzle != null) ? muzzle : transform;

			// 弾を生成
			GameObject bullet = Instantiate(bulletPrafab, t.position, t.rotation) as GameObject;
			if (!bullet)
			{
				Destroy(bullet);
				yield break;
			}
			else
			{
				// 弾のプロパティ・必須コンポーネントを設定
				bullet.GetSafeComponent<Bullet>().target = player.lockOnPoint;
				bullet.GetSafeComponent<AttackArea>().Set(attack, 1.0f);
				bullet.transform.SetParent(bulletCollecter.transform);
			}

			currentState = EnemyState.None;
			EnemyCreatorSingleton.instance.StartCostAddForSeconds(-attackCosts.longRange, 2);
			isAttack = false;
		}
	}
}
