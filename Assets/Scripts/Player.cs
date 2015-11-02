using UnityEngine;

//[RequireComponent(typeof(CharacterController))]

namespace Uxtuno
{
	public class Player : MyMonoBehaviour
	{
		[Tooltip("歩く速さ(単位:m/s)"), SerializeField]
		private float maxSpeed = 5.0f; // 移動速度
		[Tooltip("走る速さ(単位:m/s)"), SerializeField]
		private float minSpeed = 1.0f; // 移動速度(ダッシュ時)
		[Tooltip("ジャンプの高さ(単位:m)"), SerializeField]
		private float jumpHeight = 2.0f;
		[Tooltip("ハイジャンプの高さ(単位:m)"), SerializeField]
		private float highJumpHeight = 10.0f;

		private float jumpVY = 0.0f;
		private float jumpPower;

		private CharacterController characterController;
		private CameraController cameraController;

		private Animator animator;
		private Transform playerMesh;
		private int speedId;
		private int isJumpId;

		private Vector3 _moveVector = Vector3.zero;

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
			JumpPossible, // ジャンプ入力可能状態
			Jumping, // ジャンプ中
			HighJumping, // ハイジャンプ中
			Fall, // 落下中
			Attack, // 攻撃状態
		}

		private State currentState; // 現在の状態
		private State oldState; // 前のフレームでの状態
		private int count; // 各状態で使う共通のカウンタ

		void Start()
		{
			// 指定の高さまで飛ぶための初速を計算
			jumpPower = Mathf.Sqrt(2.0f * -Physics.gravity.y * jumpHeight);
			characterController = GetComponent<CharacterController>();
			characterController.detectCollisions = false;
			cameraController = GetComponentInChildren<CameraController>();
			animator = GetComponentInChildren<Animator>(); // アニメーションをコントロールするためのAnimatorを子から取得
			playerMesh = animator.transform; // Animatorがアタッチされているのがメッシュのはずだから

			// ハッシュIDを取得しておく
			speedId = Animator.StringToHash("Speed");
			isJumpId = Animator.StringToHash("IsJump");

			currentState = State.Normal;
			oldState = currentState;
			animator.SetFloat(speedId, maxSpeed);
		}

		void Update()
		{
			//Move(); // プレイヤーの移動など

			switch(currentState)
			{
				case State.Normal:
					Normal();
					break;
				case State.JumpPossible:
					jumpPossibe();
                    break;
				case State.Jumping:
					break;
				case State.HighJumping:
					break;
				case State.Fall:
					break;
				case State.Attack:
					break;
			}

			if (cameraController.targetToDistance < 0.2f)
			{
				isShow = false;
			}
			else if (!isShow)
			{
				isShow = true;
			}
		}

		private void Normal()
		{
			PlayerInput input = PlayerInput.instance;
			Vector3 direction = Vector3.zero;
			// directionは進行方向を表すので上下入力はzに格納
			direction.x = Input.GetAxisRaw(InputName.Horizontal);
			direction.z = Input.GetAxisRaw(InputName.Vertical);
			direction.Normalize();

			float speed;
			if (Input.GetKey(KeyCode.LeftShift))
			{
				speed = minSpeed;
			}
			else
			{
				speed = maxSpeed;
			}

			moveVector = Vector3.zero;
			if (direction != Vector3.zero)
			{
				//float distance = (cameraController.transform.position - cameraController.cameraTransform.position).magnitude;

				Vector3 rotateAngles = Vector3.zero;
				// カメラの方向を加味して進行方向を計算
				direction = cameraController.cameraTransform.rotation * direction;

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

			if (!characterController.isGrounded)
			{
				// ジャンプ可能状態へ移行
				currentState = State.JumpPossible;
			}
		}

		private void jumpPossibe()
		{

		}

		void Move()
		{
			Vector3 direction = Vector3.zero;
			// directionは進行方向を表すので上下入力はzに格納
			direction.x = Input.GetAxisRaw(InputName.Horizontal);
			direction.z = Input.GetAxisRaw(InputName.Vertical);
			direction.Normalize();

			float speed;
			if (Input.GetKey(KeyCode.LeftShift))
			{
				speed = minSpeed;
			}
			else
			{
				speed = maxSpeed;
			}

			moveVector = Vector3.zero;
			if (direction != Vector3.zero)
			{
				//float distance = (cameraController.transform.position - cameraController.cameraTransform.position).magnitude;

				Vector3 rotateAngles = Vector3.zero;
				// カメラの方向を加味して進行方向を計算
				direction = cameraController.cameraTransform.rotation * direction;

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

			if (!characterController.isGrounded)
			{
				// 空中では重力により落下速度を加算する
				jumpVY += Physics.gravity.y * Time.deltaTime;
			}
			else // 地面についている
			{
				// ジャンプさせる
				PlayerInput input = PlayerInput.instance;
				if (input.jump && !animator.GetBool(isJumpId))
				{
					jumpVY = jumpPower;
					animator.SetBool(isJumpId, true);
				}
				else
				{
					animator.SetBool(isJumpId, false);
					jumpVY = 0.0f;
				}
			}

			_moveVector.y = jumpVY;
			if (moveVector != Vector3.zero)
			{
				Vector3 oldCameraPosition = cameraController.cameraTransform.position;
				// プレイヤー移動前
				Vector3 old = transform.position - oldCameraPosition;
				characterController.Move(moveVector * Time.deltaTime);
				// プレイヤー移動後
				Vector3 now = transform.position - oldCameraPosition;

				// プレイヤーが移動した時のY軸方向カメラ回転量を計算
				float rotateAngleY = Mathf.Atan2(now.x * old.z - now.z * old.x, now.x * old.x + now.z * old.z) * Mathf.Rad2Deg / 2.0f;
				cameraController.CameraMove(rotateAngleY, 0.0f);
			}
		}
	}
}
