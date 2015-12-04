using UnityEngine;
using System.Collections;

namespace Kuvo
{
	/// <summary>
	/// 人食い提灯クラス
	/// </summary>
	public class CannibalLantern : Enemy
	{
		private enum FlyState
		{
			Up,
			Down,
		}

		[Tooltip("弾のプレハブ"), SerializeField]
		private GameObject bulletPrafab = null;		// 弾のプレハブ
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


			//StartCoroutine(Flying(0.5f));
		}

		protected override void Update()
		{
			base.Update();

			if (Input.GetKeyDown(KeyCode.B))
			{
				StartCoroutine(LongRangeAttack());
			}

			if (currentState != oldState)
			{
				switch (currentState)
				{
					case EnemyState.None:
						break;

					case EnemyState.Idle:
						break;

					case EnemyState.Bone:
						break;

					case EnemyState.Search:
						break;

					case EnemyState.SAttack:
						if (!isAttack)
						{
							StartCoroutine(ShortRangeAttack());
						}
						break;

					case EnemyState.LAttack:
						if (!isAttack)
						{
							StartCoroutine(LongRangeAttack());
						}
						break;

					case EnemyState.Stagger:
						break;

					case EnemyState.Death:
						BaseEnemyAI aI = GetComponent<BaseEnemyAI>();
						aI.StopAllCoroutines();
						aI.enabled = false;
						break;
				}

				oldState = currentState;
			}

			if (currentState == EnemyState.Move)
			{
				transform.Translate(Vector3.forward * speed * Time.deltaTime);
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
			if (isAttack)
			{
				yield break;
			}

			isAttack = true;
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
					}
				}

				counter += Time.deltaTime;
				if (counter > 1)
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
			isAttack = false;
		}

		/// <summary>
		/// 遠距離攻撃
		/// </summary>
		public override IEnumerator LongRangeAttack()
		{
			if (isAttack)
			{
				yield break;
			}

			isAttack = true;

			yield return new WaitForSeconds(5f);

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
				bullet.GetSafeComponent<Bullet>().target = player.lockOnPoint;
				bullet.GetSafeComponent<AttackArea>().Set(attack, 1.0f);
				bullet.transform.SetParent(bulletCollecter.transform);
			}

			currentState = EnemyState.None;
			isAttack = false;
		}

		private IEnumerator Flying(float deflectionHeight)
		{
			float y = transform.localPosition.y;
			float max = deflectionHeight / 2 + y;
			float min = -(deflectionHeight / 2) + y;
			float speed = 0.5f;
			FlyState flyState;

			if ((y = Random.Range(min, max)) > transform.localPosition.y)
			{
				flyState = FlyState.Down;
			}
			else
			{
				flyState = FlyState.Up;
			}

			while (true)
			{
				switch (flyState)
				{
					case FlyState.Up:
						y += speed * Time.deltaTime;
						if (y > max)
						{
							y = max;
							flyState = FlyState.Down;
						}
						break;

					case FlyState.Down:
						y -= speed * Time.deltaTime;
						if (y < min)
						{
							y = min;
							flyState = FlyState.Up;
						}
						break;
				}

				transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
				yield return new WaitForEndOfFrame();
			}
		}
	}
}
