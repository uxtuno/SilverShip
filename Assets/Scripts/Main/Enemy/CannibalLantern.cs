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
		private GameObject bulletPrafab = null;
		private GameObject bulletCollecter = null;
		private EnemyState oldState = EnemyState.Idle;

		/// <summary>
		/// 人食い提灯を目視することができる最も近い距離
		/// </summary>
		protected override float sight { get; set; }

		protected override void Awake()
		{
			hp = 5;
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
					case EnemyState.Idle:
						break;

					case EnemyState.Move:
						break;

					case EnemyState.GoBack:
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
		}

		private void FixedUpdate()
		{
			if (currentState == EnemyState.Move)
			{
				if (isPlayerLocate)
				{
					Vector3 lookPosition = player.lockOnPoint.position;
					lookPosition.y = transform.position.y;

					transform.LookAt(lookPosition);
					rigidbody.velocity = (player.lockOnPoint.position - lockOnPoint.position).normalized * speed;
				}
				else
				{
					if (transform.position.y < 5f)
					{
						rigidbody.velocity = (transform.forward + Vector3.up / 2) * speed;
					}
					else
					{
						rigidbody.velocity = transform.forward * speed;
					}
				}
			}
			else if (currentState == EnemyState.Idle)
			{
				if (transform.position.y > 1)
				{
					rigidbody.velocity = Vector3.down;
				}
				else
				{
					if (rigidbody.velocity != Vector3.zero)
					{
						rigidbody.velocity = Vector3.zero;
					}
				}
			}
			else if (currentState != EnemyState.GoBack)
			{
				if (rigidbody.velocity != Vector3.zero)
				{
					rigidbody.velocity = Vector3.zero;
				}
			}
		}

		/// <summary>
		/// 空中でよろける
		/// </summary>
		protected override IEnumerator AirStagger()
		{
			EnemyState oldState = currentState;
			currentState = EnemyState.Stagger;
			Debug.Log("ぐはっ！");

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
			EnemyCreatorSingleton.instance.StartCostAddForSeconds(baseEnemyAI.attackParameters.sAttackCost, 0);
			currentState = EnemyState.Move;
			Vector3 startPosition = transform.position;

			// ここに予備動作
			float counter = 0;
			Vector3 playerPosition = player.lockOnPoint.position;
			playerPosition.y = transform.position.y;
			while (true)
			{
				if (CheckDistance(playerPosition, 1f))
				{
					if (currentState != EnemyState.Idle)
					{
						currentState = EnemyState.Idle;
					}
				}

				counter += Time.deltaTime;
				if (counter > baseEnemyAI.attackParameters.sAttackPreOperatSecond)
				{
					if (currentState != EnemyState.Idle)
					{
						currentState = EnemyState.Idle;
					}
					break;
				}

				yield return new WaitForEndOfFrame();
			}

			shortRangeAttackAreaObject.SetActive(true);

			yield return new WaitForSeconds(1.0f);

			shortRangeAttackAreaObject.SetActive(false);
			EnemyCreatorSingleton.instance.StartCostAddForSeconds(-baseEnemyAI.attackParameters.sAttackCost, CostKeepSecond);
			StartCoroutine(MovingPosition(startPosition, baseEnemyAI.attackParameters.sAttackPreOperatSecond));
		}

		/// <summary>
		/// 遠距離攻撃
		/// </summary>
		public override IEnumerator LongRangeAttack()
		{
			isAttack = true;
			EnemyCreatorSingleton.instance.StartCostAddForSeconds(baseEnemyAI.attackParameters.lAttackCost, 0);

			// ここに予備動作
			yield return new WaitForSeconds(baseEnemyAI.attackParameters.lAttackPreOperatSecond);

			// 弾の発射位置・角度を登録
			Transform t = (muzzle != null) ? muzzle : transform;

			// 弾を生成
			GameObject bullet = Instantiate(bulletPrafab, t.position, t.rotation) as GameObject;
			if (!bullet)
			{
				Destroy(bullet);
				isAttack = false;
				yield break;
			}
			else
			{
				// 弾のプロパティ・必須コンポーネントを設定
				bullet.GetSafeComponent<Bullet>().target = player.lockOnPoint;
				bullet.GetSafeComponent<AttackArea>().Set(attack, 1.0f);
				bullet.transform.SetParent(bulletCollecter.transform);
			}

			EnemyCreatorSingleton.instance.StartCostAddForSeconds(-baseEnemyAI.attackParameters.lAttackCost, CostKeepSecond);
			isAttack = false;
		}

		/// <summary>
		/// 指定方向に進む
		/// </summary>
		/// <param name="targetPosition"> 移動する先の座標</param>
		/// <param name="second"> 移動する時間(秒)</param>
		private IEnumerator MovingPosition(Vector3 targetPosition, float second)
		{
			const float min = 0.125f;

			if (!CheckDistance(targetPosition, min))
			{
				if (currentState != EnemyState.GoBack)
				{
					currentState = EnemyState.GoBack;
				}

				for (float elapsedTime = 0; second > elapsedTime; elapsedTime += Time.deltaTime)
				{
					Vector3 lookPosition = targetPosition;
					lookPosition.y = transform.position.y;

					transform.LookAt(lookPosition);
					rigidbody.velocity = (targetPosition - lockOnPoint.position).normalized * speed;

					if (CheckDistance(targetPosition, min))
					{
						break;
					}

					yield return new WaitForFixedUpdate();
				}

				rigidbody.velocity = Vector3.zero;
				currentState = EnemyState.Idle;
			}

			if (isAttack)
			{
				isAttack = false;
			}
		}
	}
}
