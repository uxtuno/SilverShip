using UnityEngine;
using System.Linq;
using Kuvo;

//[RequireComponent(typeof(CharacterController))]

namespace Uxtuno
{
	/// <summary>
	/// TODO :
	/// 状態遷移プログラミングにより、各状態の処理はすべて状態メソッドの中に記述している
	/// それにより重複部分が多くなっているので、後でコードを見直す必要あり
	/// </summary>
	public class Player : Actor
	{
		[Tooltip("歩く速さ(単位:m/s)"), SerializeField]
		private float maxSpeed = 5.0f; // 移動速度
									   //[Tooltip("走る速さ(単位:m/s)"), SerializeField]
									   //private float minSpeed = 1.0f; // 移動速度(ダッシュ時)
		[Tooltip("ジャンプの高さ(単位:m)"), SerializeField]
		private float jumpHeight = 2.0f;
		[Tooltip("ハイジャンプの高さ(単位:m)"), SerializeField]
		private float highJumpHeight = 10.0f;
		[Tooltip("水平方向のカメラ移動速度"), SerializeField]
		private float horizontalRotationSpeed = 120.0f; // 水平方向へのカメラ移動速度
		[Tooltip("垂直方向のカメラ移動速度"), SerializeField]
		private float verticaltalRotationSpeed = 40.0f; // 垂直方向へのカメラ移動速度
		private const float near = 0.5f; // カメラに映る最小距離
		private const float maxCameraRotateY = 5.0f; // プレイヤーが移動したときの最大カメラ回転量
		private float gravityForce = 1.5f;

		private float jumpVY = 0.0f;
		private float jumpPower;
		private float highJumpPower;

		private CharacterController characterController;
		private CameraController cameraController;
		private ContainedObjects containedObjects;

		private Animator animator;
		private Transform playerMesh;
		private int speedId;
		private int isJumpId;

		private PlayerInput playerInput;
		private Vector3 _moveVector = Vector3.zero;
		private bool isGrounded; // 地面に着いているか

		private static readonly Vector3 cameraFront = new Vector3(0.0f, -0.1f, 1.0f);

		/// <summary>
		/// 直前の移動ベクトル
		/// </summary>
		public Vector3 moveVector
		{
			get { return _moveVector; }
			private set { _moveVector = value; }
		}

		// プレイヤーの状態
		private enum State
		{
			None,
			Normal, // 待機、移動
			Depression, // 踏み込み中
			Jumping, // ジャンプ中
			HighJumping, // ハイジャンプ中
			Fall, // 落下中
			Attack, // 攻撃状態
			TwoJump, // 二段ジャンプ
		}

		private State currentState; // 現在の状態
		private State oldState; // 前のフレームでの状態
		private float count; // 各状態で使う共通のカウンタ

		private float twoJumpForce;
		private float twoJumpAttenuation = 0.5f;
		private Vector3 twoJumpDirection;

		private Actor lockOnTarget; // ロックオン対象エネミー
		[SerializeField]
		private GameObject lockOnIconPrefab = null;
		private Transform lockOnIcon = null;

		[SerializeField]
		private GameObject playerAttackEffectPrefab = null;
		private GameObject playerAttackEffect;

		void Start()
		{
			// 指定の高さまで飛ぶための初速を計算
			jumpPower = Mathf.Sqrt(2.0f * -Physics.gravity.y * gravityForce * jumpHeight);
			highJumpPower = Mathf.Sqrt(2.0f * -Physics.gravity.y * gravityForce * highJumpHeight);
			characterController = GetComponent<CharacterController>();
			characterController.detectCollisions = false;
			cameraController = GetComponentInChildren<CameraController>();
			if (cameraController == null)
			{
				Debug.LogError("プレイヤーにカメラがありません");
			}
			containedObjects = GetComponentInChildren<ContainedObjects>();

			animator = GetComponentInChildren<Animator>(); // アニメーションをコントロールするためのAnimatorを子から取得
			playerMesh = animator.transform; // Animatorがアタッチされているのがメッシュのはずだから

			// ハッシュIDを取得しておく
			speedId = Animator.StringToHash("Speed");
			isJumpId = Animator.StringToHash("IsJump");

			currentState = State.Normal;
			oldState = currentState;
			animator.SetFloat(speedId, maxSpeed);

			// プレイヤーの入力を管理するクラス
			playerInput = PlayerInput.instance;
			ChangeState(State.Normal);
		}

		void Update()
		{
			// カメラに近すぎると非表示に
			if (cameraController.targetToDistance < near)
			{
				isShow = false;
			}
			else if (!isShow)
			{
				isShow = true;
			}

			//Move(); // プレイヤーの移動など
			moveVector = Vector3.zero;

			do
			{
				oldState = currentState;
				switch (currentState)
				{
					case State.Normal:
						Normal();
						break;
					case State.Depression:
						Depression();
						break;
					case State.Jumping:
					case State.HighJumping:
						Jumping();
						break;
					case State.Fall:
						Fall();
						break;
					case State.Attack:
						Attack();
						break;
					case State.TwoJump:
						TwoJump();
						break;
				}
			} while (oldState != currentState);

			// カメラの回転入力
			Vector2 cameraMove = Vector3.zero;
			cameraMove.x = playerInput.cameraHorizontal;
			cameraMove.y = playerInput.cameraVertical;

			float cameraRotateY = 0.0f; // プレイヤーが移動したときのカメラ回転量
			_moveVector.y = jumpVY;
			if (moveVector != Vector3.zero)
			{
				Vector3 oldCameraPosition = cameraController.cameraTransform.position;
				// プレイヤー移動前
				Vector3 old = transform.position - oldCameraPosition;
				characterController.Move(moveVector * Time.deltaTime);
				// プレイヤー移動後
				Vector3 now = transform.position - oldCameraPosition;

				// プレイヤーが移動した時のY軸カメラ回転量を計算
				cameraRotateY = Mathf.Atan2(now.x * old.z - now.z * old.x, now.x * old.x + now.z * old.z) * Mathf.Rad2Deg;
				cameraRotateY = Mathf.Clamp(cameraRotateY, -maxCameraRotateY, maxCameraRotateY);

				float minDistance2 = 999.0f;
				Transform tempLockOnTarget = null; // ロックオン対象候補を入れる
				foreach (Transform enemy in containedObjects)
				{
					float distance2 = (transform.position - enemy.position).sqrMagnitude;
					if (minDistance2 > distance2)
					{
						minDistance2 = distance2;
						tempLockOnTarget = enemy;
					}
				}

				// ロックオン対象が決定したので正式にロックオン
				if (tempLockOnTarget != null && lockOnTarget != tempLockOnTarget.GetComponent<Actor>())
				{
					lockOnTarget = tempLockOnTarget.GetComponent<Actor>();
					if (lockOnIcon == null)
					{
						lockOnIcon = Instantiate(lockOnIconPrefab).transform;
					}
					lockOnIcon.parent = lockOnTarget.transform;
					print(lockOnTarget.ToString() + "をロックオンしました");
					//Camera.main.WorldToViewportPoint(lockOnTarget.lockOnPoint.position);
                    lockOnIcon.position = lockOnTarget.lockOnPoint.position;
					lockOnIcon.GetSafeComponent<LockOnIcon>().lockOnPoint = lockOnTarget.lockOnPoint;
				}
			}

			float limitDistance = 6.0f;
			limitDistance *= limitDistance;
			// ロックオン対象から離れすぎると解除
			if (lockOnTarget != null)
			{
				if ((lockOnTarget.lockOnPoint.position - transform.position).sqrMagnitude > limitDistance)
				{
					lockOnTarget = null;
					Destroy(lockOnIcon.gameObject);
					print("ロックオンを解除しました");
				}
			}

			if (cameraMove != Vector2.zero)
			{
				cameraController.CameraMove(cameraMove.x * horizontalRotationSpeed * Time.deltaTime, cameraMove.y * verticaltalRotationSpeed * Time.deltaTime);
			}
			else if (cameraRotateY != 0.0f)
			{
				cameraController.CameraMove(cameraRotateY, 0.0f);
			}
		}

		private void Normal()
		{
			Gravity();
			Vector3 direction = Vector3.zero;

			if (playerInput.lockOn)
			{
				LockOn();
			}

			if (playerInput.cameraToFront)
			{
				cameraController.SetRotation(Quaternion.LookRotation(playerMesh.rotation * cameraFront));
			}

			if (lockOnTarget != null)
			{
				if (playerInput.attack)
				{
					Vector3 rotateAngles = Vector3.zero;
					direction = lockOnTarget.lockOnPoint.position - transform.position;
					// xz平面の進行方向から、Y軸回転角を得る
					rotateAngles.y = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
					playerMesh.eulerAngles = rotateAngles;
					ChangeState(State.Attack);
					return;
				}
			}

			// 数フレームは地面に着いているものとし、ジャンプ入力を有効にする
			if (isGrounded)
			{
				if (!characterController.isGrounded)
				{
					if (count > 0.1f)
					{
						isGrounded = false;
						// 落下状態へ移行
						ChangeState(State.Fall);
						return;
					}
					else
					{
						count += Time.deltaTime;
					}
				}
				else
				{
					jumpVY = 0.0f;
					count = 0;
				}
			}
			else
			{
				if (characterController.isGrounded)
				{
					jumpVY = 0.0f;
					isGrounded = true;
				}
			}

			direction = calclateMoveDirection();
			float speed = maxSpeed;
			if (direction != Vector3.zero)
			{
				Vector3 rotateAngles = Vector3.zero;

				// xz平面の進行方向から、Y軸回転角を得る
				rotateAngles.y = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
				playerMesh.eulerAngles = rotateAngles;
				moveVector = playerMesh.forward * speed;
				animator.SetFloat(speedId, speed);
			}
			else
			{
				animator.SetFloat(speedId, 0.0f); // 待機アニメーション
			}

			if (isGrounded)
			{
				// ジャンプさせる
				if (playerInput.jump)
				{
					ChangeState(State.Depression);
				}
			}
		}

		/// <summary>
		/// 踏み込み中
		/// </summary>
		private void Depression()
		{
			if (count < 0.3f)
			{
				count += Time.deltaTime;
				if (playerInput.attack)
				{
					jumpVY = highJumpPower;
					ChangeState(State.HighJumping);
				}
			}
			else
			{
				jumpVY = jumpPower;
				ChangeState(State.Jumping);
			}
		}

		/// <summary>
		/// ジャンプ中
		/// </summary>
		private void Jumping()
		{
			Vector3 direction = calclateMoveDirection();
			if (direction != Vector3.zero)
			{
				//// xz平面の進行方向から、Y軸回転角を得る
				Vector3 angles = playerMesh.eulerAngles;
				angles.y = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
				playerMesh.eulerAngles = angles;
			}

			if (playerInput.jump && currentState == State.Jumping && count > 0.3f)
			{
				twoJumpDirection = playerMesh.forward;
				twoJumpForce = 15.0f;
				ChangeState(State.TwoJump);
				jumpVY = 0.0f;
				return;
			}

			float speed = 3.0f;
			if (direction != Vector3.zero)
			{
				moveVector = direction * speed;
			}

			// ジャンプしてからの経過時間
			count += Time.deltaTime;
			Gravity();

			if (characterController.isGrounded && count > 1.0f)
			{
				ChangeState(State.Normal);
				return;
			}

			if (jumpVY < -3.0f)
			{
				// 落下状態へ移行
				currentState = State.Fall;
			}
		}

		private void Fall()
		{
			Vector3 direction = Vector3.zero;
			// directionは進行方向を表すので上下入力はzに格納
			direction.x = Input.GetAxisRaw(InputName.Horizontal);
			direction.z = Input.GetAxisRaw(InputName.Vertical);
			direction.Normalize();

			float speed = 3.0f;
			moveVector = twoJumpDirection * twoJumpForce;
			twoJumpForce -= twoJumpAttenuation * 6.0f;
			if (twoJumpForce < 0.0f)
			{
				twoJumpForce = 0.0f;
			}

			if (direction != Vector3.zero)
			{
				// カメラの方向を加味して進行方向を計算
				direction = cameraController.cameraTransform.rotation * direction;
				moveVector += direction * speed;
			}

			Gravity();
			if (characterController.isGrounded)
			{
				ChangeState(State.Normal);
				jumpVY = 0.0f;
			}
		}

		private void TwoJump()
		{
			moveVector = twoJumpDirection * twoJumpForce;
			twoJumpForce -= twoJumpAttenuation * Time.deltaTime;
			if (twoJumpForce < 13.0f || !Input.GetButton(InputName.Jump))
			{
				jumpVY = 0.0f;
				ChangeState(State.Fall);
			}
		}

		private void Attack()
		{
			count += Time.deltaTime;
			if (count > 1.0f)
			{
				Destroy(playerAttackEffect);
				ChangeState(State.Normal);
			}
		}

		/// <summary>
		/// 入力方向とカメラの向きから進行方向ベクトルを計算
		/// </summary>
		/// <returns>進行方向</returns>
		private Vector3 calclateMoveDirection()
		{
			Vector3 direction = Vector3.zero;
			// directionは進行方向を表すので上下入力はzに格納
			direction.x = playerInput.horizontal;
			direction.z = playerInput.vertical;
			direction.Normalize();

			// カメラの方向を加味して進行方向を計算
			return cameraController.cameraTransform.rotation * direction;
		}

		/// <summary>
		/// 重力による加速度の計算をする
		/// </summary>
		private void Gravity()
		{
			jumpVY += Physics.gravity.y * gravityForce * Time.deltaTime;
		}

		private const float LockOnAngleHulfRange = 45.0f; // ロックオン可能角度の半分
		private const float LockOnDistance = 20.0f; // ロックオン可能距離

		/// <summary>
		/// ロックオン動作
		/// </summary>
		private void LockOn()
		{
			Transform actor = null;
			// 敵のリストを取得
			Transform[] enemies = GameObject.FindGameObjectsWithTag(TagName.Enemy).Select((obj) => obj.transform).ToArray();
			float playerAngle = cameraController.cameraTransform.eulerAngles.y;
			foreach (Transform enemy in enemies)
			{
				if (Utility.hitTestArcPoint(cameraController.cameraTransform.position.z, cameraController.cameraTransform.position.x, LockOnDistance, playerAngle - LockOnAngleHulfRange, playerAngle + LockOnAngleHulfRange, enemy.position.z, enemy.position.x))
				{
					actor = enemy;
					print((actor.position - cameraController.cameraTransform.position).magnitude);
				}
			}

			if (actor != null)
			{
				Quaternion q = Quaternion.LookRotation(actor.GetComponent<Actor>().lockOnPoint.position - cameraController.transform.position);
				cameraController.SetRotation(q);
			}
		}

		/// <summary>
		/// 状態を変更
		/// 変更した瞬間の処理はここに書く
		/// </summary>
		/// <param name="newState"></param>
		private void ChangeState(State newState)
		{
			count = 0;
			switch (newState)
			{
				case State.Normal:
					animator.SetBool(isJumpId, false);
					twoJumpForce = 0.0f;
					break;
				case State.Depression:
					animator.SetBool(isJumpId, true);
					break;
				case State.Jumping:
					break;
				case State.TwoJump:
					break;
				case State.Fall:
					break;
				case State.Attack:
					playerAttackEffect = (GameObject)Instantiate(playerAttackEffectPrefab, transform.position, playerMesh.rotation);
					Vector3 position = cameraController.transform.position;
					//position.y = cameraController.cameraTransform.position.y; // 高さは現在のカメラの高さを参照

					//cameraController.SetRotation(Quaternion.LookRotation(lockOnTarget.lockOnPoint.position - position));
					break;
			}

			currentState = newState;
		}
	}
}
