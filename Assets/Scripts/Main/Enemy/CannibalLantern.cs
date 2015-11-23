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

		/// <summary>
		/// 人食い提灯を目視することができる最も近い距離
		/// </summary>
		protected override float sight { get; set; }
		[SerializeField]
		private GameObject bulletPrafab = null;
		private GameObject bulletCollecter = null;

		protected override void Awake()
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

			shortRangeAttackAreaObject.GetComponent<AttackArea>().Set(attack, 1.0f);
			bulletCollecter = GameObject.Find("BulletCollecter");

			//StartCoroutine(Flying(0.5f));
		}

		float counter = 0;
		protected override void Update()
		{
			base.Update();
			counter += Time.deltaTime;
			if (Input.GetKeyDown(KeyCode.B))
			{
				StartCoroutine(LongRangeAttack());
			}

			switch (currentState)
			{
				case ActionState.Idle:
					animation.Play("idle");
					break;

				case ActionState.Bone:
					animation.Play("idle");
					break;

				case ActionState.Move:
					animation.Play("walk");
					break;

				case ActionState.Attack:
					animation.Play("attack");
					break;

				case ActionState.Stagger:
					animation.Play("fall");
					break;

				case ActionState.Death:
					BaseEnemyAI aI = GetComponent<BaseEnemyAI>();
					aI.StopAllCoroutines();
					aI.enabled = false;
					animation.Play("die");
					break;
			}
		}

		/// <summary>
		/// 空中でよろける
		/// </summary>
		protected override IEnumerator AirStagger()
		{
			ActionState oldState = currentState;
			currentState = ActionState.Stagger;
			print("ぐはっ！");
			//GameObject primitiveCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			//primitiveCube.transform.position = lockOnPoint.position;
			//primitiveCube.transform.rotation = lockOnPoint.rotation;
			//primitiveCube.transform.SetParent(transform);

			yield return new WaitForSeconds(1);

			//Destroy(primitiveCube);
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
			if (currentState == ActionState.Attack)
			{
				yield break;
			}

			currentState = ActionState.Attack;

			// ここに予備動作
			float counter = 0;
			while (true)
			{
				counter += Time.deltaTime;
				transform.localScale *= 0.2f;
				if (counter > 5)
				{
					transform.localScale = Vector3.one;
					break;
				}
				yield return new WaitForEndOfFrame();
			}

			shortRangeAttackAreaObject.SetActive(true);

			yield return new WaitForSeconds(1.0f);

			shortRangeAttackAreaObject.SetActive(false);
			currentState = ActionState.Move;
		}

		/// <summary>
		/// 遠距離攻撃
		/// </summary>
		public override IEnumerator LongRangeAttack()
		{
			if (currentState == ActionState.Attack)
			{
				yield break;
			}

			ActionState oldState = currentState;
			currentState = ActionState.Attack;

			yield return new WaitForSeconds(0.5f);

			GameObject bullet = Instantiate(bulletPrafab, transform.position, transform.rotation) as GameObject;
			if (!bullet)
			{
				Destroy(bullet);
				currentState = oldState;
				yield break;
			}
			else
			{
				bullet.GetSafeComponent<Bullet>().target = player.lockOnPoint;
				bullet.GetSafeComponent<AttackArea>().Set(attack, 1.0f);
				bullet.transform.SetParent(bulletCollecter.transform);
			}

			currentState = oldState;
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
