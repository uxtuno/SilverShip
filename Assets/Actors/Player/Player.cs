using UnityEngine;
using System.Collections.Generic;

//[RequireComponent(typeof(CharacterController))]

public class Player : MyMonoBehaviour
{
	//　移動モード
	private enum moveType
	{
		Type1, // カメラの方向に合わせる
		Type2, // カメラの方向に合わせず移動
	}
	[SerializeField]
	private moveType debugMoveType = moveType.Type1;

	[SerializeField]
	private float speed = 1.0f; // 移動速度
	private float highSpeed = 5.0f; // 移動速度(ダッシュ時)
	private CharacterController characterController = null;

	private float jumpVY = 0.0f;
	private float jumpPower = 7.0f;

	private Transform cameraTransform = null;   // プレイヤーカメラのトランスフォーム

	private Animator animator;
	private int speedId;
	private int isJumpId;

	protected override void Awake()
	{
		base.Awake();
		characterController = GetComponent<CharacterController>();
		characterController.detectCollisions = false;
		animator = GetComponentInChildren<Animator>(); // アニメーションをコントロールするためのAnimatorを子から取得
		speedId = Animator.StringToHash("Speed"); // ハッシュIDを取得しておく
		isJumpId = Animator.StringToHash("IsJump"); // ハッシュIDを取得しておく
	}

	void Start()
	{
		cameraTransform = Camera.main.transform;
	}

	protected override void LateUpdate()
	{
		Cursor.lockState = CursorLockMode.Locked;

		Move(); // プレイヤーの移動など

		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");
	}

	void Move()
	{
		Vector3 direction = Vector3.zero;
		// directionは進行方向を表すので上下入力はzに格納
		direction.x = Input.GetAxisRaw("Horizontal");
		direction.z = Input.GetAxisRaw("Vertical");

		Vector3 moveVector = Vector3.zero;
		if (direction != Vector3.zero)
		{
			Vector3 rotateAngles = Vector3.zero;

			// プレイヤーを進行方向に向ける処理
			//direction = cameraTransform.rotation * direction.normalized;

			switch (debugMoveType)
			{
				case moveType.Type1: // カメラ方向を考慮した移動
					direction = cameraTransform.rotation * direction.normalized;
					direction = Vector3.RotateTowards(transform.forward, direction, 0.2f, 0.0f);
					break;

				case moveType.Type2: //　カメラ方向関係ない移動
					direction = direction.normalized;
					break;
			}

			rotateAngles.y = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg; // xz平面の進行方向から、Y軸回転角を得る
			transform.eulerAngles = rotateAngles;

			if (Input.GetKey(KeyCode.LeftShift))
			{
				moveVector = transform.forward * highSpeed;
				animator.SetFloat(speedId, highSpeed);
			}
			else
			{
				moveVector = transform.forward * speed;
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
		if(moveVector != Vector3.zero)
		{
			characterController.Move(moveVector * Time.deltaTime);
		}
	}
}
