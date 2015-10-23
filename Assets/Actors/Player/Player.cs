using UnityEngine;
using System.Collections.Generic;

//[RequireComponent(typeof(CharacterController))]

namespace Uxtuno
{
	public class Player : MyMonoBehaviour
	{
		[Tooltip("歩く速さ(単位:m/s)"), SerializeField]
		private float speed = 5.0f; // 移動速度
		[Tooltip("走る速さ(単位:m/s)"), SerializeField]
		private float slowSpeed = 1.0f; // 移動速度(ダッシュ時)
		[Tooltip("ジャンプできる高さ(単位:m)"), SerializeField]
		private float jumpHeight = 2.0f;

		private float jumpVY = 0.0f;
		private float jumpPower;

		private CharacterController characterController = null;
		private CameraController cameraController;

		private Animator animator;
		private Transform playerMesh;
		private int speedId;
		private int isJumpId;

		protected void Awake()
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

			animator.SetFloat(speedId, speed);
		}

		void Start()
		{
		}

		protected void Update()
		{
			//Cursor.lockState = CursorLockMode.Locked;

			Move(); // プレイヤーの移動など

			if(Input.GetMouseButton(0))
			{
				Vector3 position = Camera.main.ScreenToViewportPoint(Input.mousePosition);
				position.x -= 0.5f;
				position.y -= 0.5f;

				if(position.magnitude > 0.1f)
				{
					cameraController.CameraMove(position.x * 20.0f, position.y * 8.0f);
				}
			}
		}

		void Move()
		{
			Vector3 direction = Vector3.zero;
			// directionは進行方向を表すので上下入力はzに格納
			direction.x = Input.GetAxisRaw(InputName.Horizontal);
			direction.z = Input.GetAxisRaw(InputName.Vertical);
			direction.Normalize();
			if(animator.GetFloat(speedId) != 0.0f)
			{
				direction *= animator.GetFloat(speedId);
			}
			cameraController.CameraMove(direction.x, 0.0f);

			Vector3 moveVector = Vector3.zero;
			if (direction != Vector3.zero)
			{
				Vector3 rotateAngles = Vector3.zero;
				// カメラの方向を加味して進行方向を計算
				direction = cameraController.cameraTransform.rotation * direction;

				// xz平面の進行方向から、Y軸回転角を得る
				rotateAngles.y = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
				playerMesh.eulerAngles = rotateAngles;

				if (Input.GetKey(KeyCode.LeftShift))
				{
					moveVector = playerMesh.forward * slowSpeed;
					animator.SetFloat(speedId, slowSpeed);
				}
				else
				{
					moveVector = playerMesh.forward * speed;
					animator.SetFloat(speedId, speed);
				}
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
				if (Input.GetButtonDown("Jump") && !animator.GetBool(isJumpId))
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

			moveVector.y = jumpVY;
			if (moveVector != Vector3.zero)
			{
				characterController.Move(moveVector * Time.deltaTime);
			}
		}
	}
}
