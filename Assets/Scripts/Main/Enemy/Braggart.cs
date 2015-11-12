using UnityEngine;
using System.Collections;

namespace Kuvo
{
	/// <summary>
	/// 天狗クラス
	/// </summary>
	public partial class Braggart : Enemy
	{
		private enum FlyState
		{
			Up,
			Down,
		}


		[SerializeField]
		private GameObject bulletPrefab = null;

		/// <summary>
		/// 天狗を目視することができる最も近い距離
		/// </summary>
		protected override float sight { get; set; }
<<<<<<< HEAD
		[SerializeField]
		private GameObject bulletPrafab = null;
		private GameObject bulletCollecter = null;
=======
>>>>>>> feature/SilverShipKuvo

		public void Awake()
		{
			hp = 30;
			attack = 1;
			defence = 2;
			sight = 1.5f;
			shortRangeAttackAreaObject.GetComponent<AttackArea>().Set(attack, 1.0f);
		}

		protected override void Start()
		{
			base.Start();

<<<<<<< HEAD
			shortRangeAttackAreaObject.GetComponent<AttackArea>().Set(attack, 1.0f);
			bulletCollecter = GameObject.Find("BulletCollecter");

=======
>>>>>>> feature/SilverShipKuvo
			StartCoroutine(Flying(0.5f));
		}

		protected override void Update()
		{
<<<<<<< HEAD
			base.Update();
			if(Input.GetKey(KeyCode.B))
			{
				LongRangeAttack();
=======
			if(Input.GetKey(KeyCode.B))
			{
				LongRengeAttack();
>>>>>>> feature/SilverShipKuvo
			}
		}

		/// <summary>
		/// 近接攻撃
		/// </summary>
		public override IEnumerator ShortRangeAttack()
		{
			shortRangeAttackAreaObject.SetActive(true);

			yield return new WaitForSeconds(1.0f);

			shortRangeAttackAreaObject.SetActive(false);

		}

<<<<<<< HEAD
		/// <summary>
		/// 遠距離攻撃
		/// </summary>
		/// <returns> 発射した弾</returns>
		public GameObject LongRangeAttack()
		{
			GameObject bullet = Instantiate(bulletPrafab, transform.position, transform.rotation) as GameObject;
			if(!bullet)
			{
				Destroy(bullet);
				return null;
			}
			else
			{
				bullet.transform.SetParent(bulletCollecter.transform);
			}

			return bullet;
		}

=======
>>>>>>> feature/SilverShipKuvo
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
