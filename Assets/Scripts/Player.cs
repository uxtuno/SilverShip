using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Kuvo;

//[RequireComponent(typeof(CharacterController))]

namespace Uxtuno
{
	public class Player : Actor
	{
		[Tooltip("歩く速さ(単位:m/s)"), SerializeField]
		private float maxSpeed = 5.0f; // 移動速度
		[Tooltip("ジャンプの高さ(単位:m)"), SerializeField]
		private float jumpHeight = 5.0f;
		[Tooltip("ハイジャンプの高さ(単位:m)"), SerializeField]
		private float highJumpHeight = 10.0f;
		[Tooltip("水平方向のカメラ移動速度"), SerializeField]
		private float horizontalRotationSpeed = 120.0f; // 水平方向へのカメラ移動速度
		[Tooltip("垂直方向のカメラ移動速度"), SerializeField]
		private float verticalRotationSpeed = 40.0f; // 垂直方向へのカメラ移動速度
		private static readonly float near = 0.5f; // カメラに映る最小距離
		private static readonly float maxCameraRotateY = 5.0f; // プレイヤーが移動したときの最大カメラ回転量

		private PlayerInput playerInput = PlayerInput.instance;
		private CharacterController characterController; // キャラクターコントローラー
		private CameraController cameraController; // キャラクターコントローラー
		private Transform meshRoot; // プレイヤーメッシュのルート
		private Animator animator; // アニメーションのコントロール用
		private PlayerCamera playerCamera; // カメラ動作を委譲

		private int speedID;
		private int isJumpID;

		/// <summary>
		/// ジャンプ状態
		/// </summary>
		public enum JumpState
		{
			None, // ジャンプ中ではない
			Jumping, // ジャンプ状態
			HighJumping, // ハイジャンプ状態
		}

		private JumpState currentJumpState = JumpState.None; // 現在のジャンプ状態
		private float jumpPower; // ジャンプの初速
		private float highJumpPower; // ハイジャンプの初速
		private float jumpVY = 0.0f; // ジャンプ中のY軸方向の移動量

		private abstract class BaseState
		{
			protected PlayerInput playerInput = PlayerInput.instance;
			protected Player player;
			public BaseState(Player player)
			{
				this.player = player;
			}

			/// <summary>
			/// 状態ごとの動作
			/// </summary>
			public abstract void Move();
		}

		private BaseState currentState; // 現在の状態

		#region - 状態クラス

		/// <summary>
		/// 通常時(地上)
		/// </summary>
		private class NormalState : BaseState
		{
			private enum HighJumpInput
			{
				None,
				Jump,
				Attack,
				HighJump = Jump | Attack,
			}

			private HighJumpInput highJumpInput; // ハイジャンプ入力受付用
			private static readonly float highJumpInputSeconds = 0.1f; // ハイジャンプ入力同時押し猶予時間
			private float highJumpInputCount; // ハイジャンプ入力受付カウンタ

			public NormalState(Player player)
				: base(player)
			{
				// 接地しているのでジャンプ状態を解除
				player.currentJumpState = JumpState.None;
				player.jumpVY = 0.0f;
				player.animator.SetBool(player.isJumpID, false);
				player.isAirDashPossible = false;
			}

			public override void Move()
			{
				if (!player.isGrounded && highJumpInput == HighJumpInput.None)
				{
					player.currentState = new AirState(player);
					return;
				}

				Vector3 moveDirection = player.calclateMoveDirection();
				float speed = player.maxSpeed;

				if (highJumpInput != HighJumpInput.None)
				{
					highJumpInputCount += Time.deltaTime;
					if (highJumpInputCount >= highJumpInputSeconds)
					{
						// 同時押しではなかったので通常の動作
						if (highJumpInput == HighJumpInput.Jump)
						{
							player.jumpVY = player.jumpPower;
							player.currentJumpState = JumpState.Jumping;
							player.currentState = new DepressionState(player);
							player.isAirDashPossible = true;
							return;
						}

						highJumpInputCount = 0.0f;
						highJumpInput = HighJumpInput.None;
					}
				}

				if (player.playerInput.jump)
				{
					highJumpInput |= HighJumpInput.Jump;
				}

				if (player.playerInput.attack)
				{
					highJumpInput |= HighJumpInput.Attack;
				}

				// ハイジャンプ入力
				if (highJumpInput == HighJumpInput.HighJump)
				{
					player.jumpVY = player.highJumpPower;
					player.currentJumpState = JumpState.HighJumping;
					highJumpInput = HighJumpInput.None;
					player.currentState = new DepressionState(player);
					return;
				}
				else if (highJumpInput != HighJumpInput.None)
				{
					return;
				}

				// 地上にいるので重力による落下量は0から計算
				player.jumpVY = 0.0f;
				player.Gravity();
				Vector3 moveVector = moveDirection * speed;
				moveVector.y = player.jumpVY;

				player.Move(moveVector * Time.deltaTime);
				if (moveDirection != Vector3.zero)
				{
					Vector3 newAngles = Vector3.zero;
					newAngles.y = Mathf.Atan2(moveVector.x, moveVector.z) * Mathf.Rad2Deg;
					player.meshRoot.eulerAngles = newAngles;
					player.animator.SetFloat(player.speedID, speed);
				}
				else
				{
					player.animator.SetFloat(player.speedID, 0.0f);
				}
			}
		}

		/// <summary>
		/// 空中
		/// </summary>
		private class AirState : BaseState
		{
			private static readonly float movementRestriction = 0.5f; // この値を掛けることで移動速度を制限
			private static readonly float airDashPossibleSeconds = 0.4f; // 空中ダッシュが可能になる時間
			private static readonly float airDashDisableSeconds = 1.4f; // 空中ダッシュが可能になる時間
			private float airDashPossibleCount; // 空中ダッシュが可能になる時間のカウンタ
			public AirState(Player player)
				: base(player)
			{
			}

			public override void Move()
			{
				airDashPossibleCount += Time.deltaTime;

				// 接地しているので通常状態に
				if (player.isGrounded && player.jumpVY <= 0.0f)
				{
					player.currentState = new NormalState(player);
					return;
				}

				Vector3 moveDirection = player.calclateMoveDirection();
				float speed = player.maxSpeed * movementRestriction;

				if (player.isGrounded && player.jumpVY <= 0.0f)
				{
					player.currentState = new NormalState(player);
				}

				player.Gravity();
				Vector3 moveVector = moveDirection * speed;
				moveVector.y = player.jumpVY;

				player.Move(moveVector * Time.deltaTime);
				if (moveDirection != Vector3.zero)
				{
					Vector3 newAngles = Vector3.zero;
					newAngles.y = Mathf.Atan2(moveVector.x, moveVector.z) * Mathf.Rad2Deg;
					player.meshRoot.eulerAngles = newAngles;
				}

				if (playerInput.jump && player.currentJumpState == JumpState.Jumping)
				{
					// 一定期間内なら空中ダッシュ
					if (airDashPossibleCount > airDashPossibleSeconds && airDashPossibleCount < airDashDisableSeconds)
					{
						if (player.isAirDashPossible)
						{
							player.currentState = new AirDashState(player);
							player.isAirDashPossible = false;
						}
					}
					return;
				}
			}
		}

		/// <summary>
		/// 空中ダッシュ
		/// </summary>
		private class AirDashState : BaseState
		{
			private static readonly float initialVelocity = 22.0f; // 初速
			private static readonly float fallStartSpeed = 18.0f; // 落下開始速度
			private static readonly float transitionSpeed = 12.0f; // 次の状態へ遷移する速度
			private static readonly float fallDeceleration = 0.4f; // 落下中の減速量
			private static readonly float deceleration = 0.2f; // 減速量
			private float speed; // 速度
			private static readonly float controllSpeed = 2.5f; // 入力による速度

			public AirDashState(Player player)
				: base(player)
			{
				speed = initialVelocity;
				player.jumpVY = 0.0f;
			}

			public override void Move()
			{
				Vector3 moveVector = player.GetDirectionXZ();
				moveVector *= speed;

				moveVector += player.calclateMoveDirection() * controllSpeed;
				if (speed <= fallStartSpeed)
				{
					player.Gravity();
					moveVector.y = player.jumpVY;

					if (speed <= transitionSpeed)
					{
						player.currentState = new AirState(player);
						return;
					}
					speed -= fallDeceleration;

					if (!Input.GetButton(InputName.Jump))
					{
						player.currentState = new AirState(player);
						return;
					}
				}
				else
				{
					speed -= deceleration;
				}

				player.Move(moveVector * Time.deltaTime);
			}
		}

		/// <summary>
		/// 踏み込み状態
		/// </summary>
		private class DepressionState : BaseState
		{
			private float transitionCount; // 次の状態へ遷移するまでの時間をカウント
			private static readonly float transitionSeconds = 0.1f; // 次の状態へ遷移するまでの時間
			public DepressionState(Player player)
				: base(player)
			{
				player.animator.SetBool(player.isJumpID, true);
			}

			public override void Move()
			{
				transitionCount += Time.deltaTime;
				if (transitionCount > transitionSeconds)
				{
					player.currentState = new AirState(player);
				}
			}
		}

		/// <summary>
		/// 対象へダッシュ(ロックオン対象など)
		/// </summary>
		private class DashToTargetState : BaseState
		{
			public DashToTargetState(Player player)
				: base(player)
			{
			}

			public override void Move()
			{
			}
		}

		#endregion

		#region - フィールド
		private ContainedObjects containedObjects;
		private Actor _lockOnTarget; // ロックオン対象エネミー

		/// <summary>
		/// ロックオン対象
		/// </summary>
		public Actor lockOnTarget
		{
			get { return _lockOnTarget; }
			private set { _lockOnTarget = value; }
		}

		[SerializeField]
		private GameObject autoLockOnIconPrefab = null;
		private Transform autoLockOnIcon;

		private bool isManualLockOn = false;
		[SerializeField]
		private GameObject manualLockOnIconPrefab = null;
		private Transform manualLockOnIcon = null;

		[SerializeField]
		//private GameObject playerAttackEffectPrefab = null;
		private GameObject playerAttackEffect;

		private Transform lookPoint; // カメラの中止点
		private bool isCameraInvalidControll = false; // カメラ操作不能状態

		private PlayerTrampled playerTrampled; // 踏みつけジャンプ動作

		private bool isAirDashPossible = false; // 空中ダッシュができるか

		/// <summary>
		/// ロックオンの状態
		/// </summary>
		public enum LockOnState
		{
			None,
			Auto,
			Manual,
		}
		private FollowIcon lockOnIcon; // ロックオンアイコン
		private LockOnState _lockOnState = LockOnState.None; // ロックオン状態

		/// <summary>
		/// ロックオン状態
		/// </summary>
		public LockOnState lockOnState {
			get { return _lockOnState; }
			private set { _lockOnState = value; }
		}

		// ロックオンアイコン画像
		private Sprite autoLockOnIconSprite;
		private Sprite manualLockOnIconSprite;

		private static readonly float autoLockOnLimitDistance = 6.0f; // オートロックオン限界距離
		private static readonly float manualLockOnLimitDistance = 30.0f; // マニュアルロックオン限界距離

		#endregion

		void Start()
		{
			// リソースロード
			autoLockOnIconSprite = Resources.Load<Sprite>("Sprites/AutoLockOnIcon");
			manualLockOnIconSprite = Resources.Load<Sprite>("Sprites/ManualRockOnIcon");

			characterController = GetComponent<CharacterController>();
			cameraController = this.GetComponentInChildren<CameraController>();
			playerCamera = new PlayerCamera(cameraController, horizontalRotationSpeed, verticalRotationSpeed);
			animator = GetComponentInChildren<Animator>(); // アニメーションをコントロールするためのAnimatorを子から取得
			meshRoot = animator.transform; // Animatorがアタッチされているのがメッシュのはずだから

			// ジャンプできる高さから初速を計算(√2gh)
			jumpPower = Mathf.Sqrt(2.0f * -Physics.gravity.y * jumpHeight);
			highJumpPower = Mathf.Sqrt(2.0f * -Physics.gravity.y * highJumpHeight);

			containedObjects = GetComponentInChildren<ContainedObjects>();
			if (containedObjects == null)
			{
				Debug.Log(typeof(ContainedObjects) + " が見つかりません");
			}

			speedID = Animator.StringToHash("Speed");
			isJumpID = Animator.StringToHash("IsJump");

			// 初期状態へ
			currentState = new NormalState(this);

			// ロックオンアイコン用のCanvas
			UICanvasGenerator.FollowIconCanvasGenerate();
			GameObject lockOnIconPrefab = Resources.Load<GameObject>("Prefabs/UI/LockOnIcon");
			lockOnIcon = Instantiate(lockOnIconPrefab).GetSafeComponent<FollowIcon>();
			lockOnIcon.Hide();
		}

		Vector3 cameraFront = new Vector3(0.0f, -0.2f, 1.0f);

		void Update()
		{
			if (playerInput.cameraToFront)
			{
				cameraController.SetRotation(Quaternion.LookRotation(meshRoot.rotation * cameraFront), 0.3f, CameraController.InterpolationMode.Curve);
				isCameraInvalidControll = true;
			}
			checkGrounded();

			// プレイヤーが移動する前の「カメラ→プレイヤー」ベクトルを保持
			Vector3 oldCameraToPlayer = transform.position - cameraController.cameraTransform.position;
			Vector3 oldCameraPosition = cameraController.cameraTransform.position;
			Vector3 oldPosition = transform.position;
			BaseState oldState;
			do
			{
				oldState = currentState;
				// 現在の状態の動作を実行
				currentState.Move();
			} while (currentState != oldState);
			// 移動後の「カメラ→プレイヤー」ベクトル
			Vector3 nowCameraToPlayer = transform.position - oldCameraPosition;

			LockOn();
		}

		void LateUpdate()
		{
			if (lockOnState == LockOnState.Manual)
			{
				playerCamera.LockOnCamera();
			}
			else
			{
				playerCamera.CameraInput();
			}
		}

		/// <summary>
		/// ロックオン状態のカメラ
		/// </summary>
		private void LockOnCamera()
		{
			
		}

		private const float LockOnAngleHulfRange = 45.0f; // ロックオン可能角度の半分
		private const float LockOnDistance = 20.0f; // ロックオン可能距離

		/// <summary>
		/// ロックオン関係処理
		/// </summary>
		private void LockOn()
		{
			// マニュアルロックオン
			if (playerInput.lockOn)
			{
				if (lockOnState != LockOnState.Manual)
				{
					ManualLockOn();
				}
				else
				{
					LockOnRelease();
				}
			}
			//　オートロックオン
			if (lockOnState != LockOnState.Manual)
			{
				AutoLockOn();
			}

			// それぞれのロックオン状態での限界距離
			float limitDistance = 0.0f;
			if (lockOnState == LockOnState.Auto)
			{
				limitDistance = autoLockOnLimitDistance;
			}
			else if (lockOnState == LockOnState.Manual)
			{
				limitDistance = manualLockOnLimitDistance;
			}

			// ロックオン対象から離れすぎると解除
			if (lockOnTarget != null)
			{
				if ((lockOnTarget.lockOnPoint.position - transform.position).sqrMagnitude > limitDistance * limitDistance)
				{
					LockOnRelease();
				}
			}
		}

		/// <summary>
		/// マニュアルロックオン
		/// </summary>
		private void ManualLockOn()
		{
			// ロックオンする敵
			Transform lockOnEnemy = null;
			// 敵のリストを取得
			Transform[] enemies = GameObject.FindGameObjectsWithTag(TagName.Enemy)
				.Select(obj => obj.transform)
				.ToArray();
			float playerAngle = cameraController.cameraTransform.eulerAngles.y;
			float minDistance = float.PositiveInfinity;
			foreach (Transform enemy in enemies)
			{
				// カメラの前方の円弧上の範囲をロックオン可能範囲とする
				if (Utility.hitTestArcPoint(cameraController.cameraTransform.position.z, cameraController.cameraTransform.position.x, LockOnDistance, playerAngle - LockOnAngleHulfRange, playerAngle + LockOnAngleHulfRange, enemy.position.z, enemy.position.x))
				{
					float distance = (enemy.position - transform.position).sqrMagnitude;
					// 最も近い対象をロックオン
					if (distance < minDistance)
					{
						minDistance = distance;
						lockOnEnemy = enemy;
					}
				}
			}

			// ロックオン
			if (lockOnEnemy != null)
			{
				lockOnTarget = lockOnEnemy.GetComponent<Actor>();
				playerCamera.BeginLockOn(lockOnTarget.lockOnPoint);
				lockOnState = LockOnState.Manual;
				lockOnIcon.Set(lockOnTarget.lockOnPoint, manualLockOnIconSprite);
			}
		}

		/// <summary>
		/// オートロックオン
		/// ロックオン可能な相手を探して自動でロックオンする
		/// </summary>
		void AutoLockOn()
		{
			float minDistance2 = float.PositiveInfinity;
			Transform tempLockOnTarget = null; // ロックオン対象候補を入れる
			foreach (Transform enemy in containedObjects)
			{
				// 距離の二乗のまま計算
				float distance2 = (transform.position - enemy.position).sqrMagnitude;
				if (minDistance2 > distance2)
				{
					minDistance2 = distance2;
					tempLockOnTarget = enemy;
				}
			}
			// ロックオン対象が決定したので正式にロックオン
			if (tempLockOnTarget != null)
			{
				Actor nextLockOnTarget = tempLockOnTarget.GetComponent<Actor>();
				// ロックオン対象が変更された時
				if (lockOnTarget != nextLockOnTarget)
				{
					lockOnTarget = nextLockOnTarget;
					lockOnIcon.Set(lockOnTarget.lockOnPoint, autoLockOnIconSprite);
					lockOnState = LockOnState.Auto;
				}
			}
		}

		/// <summary>
		/// ロックオン解除
		/// </summary>
		private void LockOnRelease()
		{
			// Todo :
			lockOnTarget = null;
			lockOnIcon.Hide();
			lockOnState = LockOnState.None;
		}

		private const float unGroundedSeconds = 0.08f; // 地面から離れたとみなす時間
		private float ungroundedCount = 0.0f; // characterController.isGroundedがfalseを返してからの時間
		private bool isGrounded; // 実際に地面に接触しているか

		/// <summary>
		/// 実際に地面に接触しているかを調べる
		/// CharacterControllerのisGroundedが信用ならないので、数フレーム分のisGroundedを見る
		/// </summary>
		private void checkGrounded()
		{
			if (!characterController.isGrounded)
			{
				ungroundedCount += Time.deltaTime;
				if (ungroundedCount > unGroundedSeconds)
				{
					// 地面から離れた
					isGrounded = false;
				}
			}
			else
			{
				// 接地
				isGrounded = true;
				ungroundedCount = 0.0f;
			}
		}

		/// <summary>
		/// 重力計算
		/// </summary>
		private void Gravity()
		{
			jumpVY += Physics.gravity.y * Time.deltaTime;
		}

		/// <summary>
		/// XZ平面上で現在向いている方向を返す
		/// </summary>
		/// <returns></returns>
		public Vector3 GetDirectionXZ()
		{
			Vector3 forward = meshRoot.forward;
			forward.y = 0.0f;
			return forward.normalized;
		}

		/// <summary>
		/// 入力方向とカメラの向きから進行方向ベクトルを計算
		/// </summary>
		/// <returns>進行方向</returns>
		private Vector3 calclateMoveDirection()
		{
			Vector3 input = Vector3.zero;
			// directionは進行方向を表すので上下入力はzに格納
			input.x = playerInput.horizontal;
			input.z = playerInput.vertical;
			input.Normalize();
			Vector3 direction = cameraController.cameraTransform.rotation * input;
			direction.y = 0.0f;
			// カメラの方向を加味して進行方向を計算
			return direction;
		}

		/// <summary>
		/// プレイヤーを移動させる
		/// </summary>
		/// <param name="vx">X軸移動量</param>
		/// <param name="vy">Y軸移動量</param>
		/// <param name="vz">Z軸移動量</param>
		public void Move(float vx, float vy, float vz)
		{
		}

		/// <summary>
		/// プレイヤーを移動させる
		/// </summary>
		/// <param name="moveVector">移動ベクトル</param>
		public void Move(Vector3 moveVector)
		{
			Vector3 oldPosition = transform.position; ;
			characterController.Move(moveVector);

			// 移動前後の座標から実際の移動量を計算
			moveVector = transform.position - oldPosition;

			// カメラ入力がされて無い時のみカメラの回転を行う
			if (playerInput.cameraHorizontal == 0.0f &&
				playerInput.cameraVertical == 0.0f &&
				lockOnState != LockOnState.Manual
				)
			{
				playerCamera.PlayerMoveToCameraRotation(moveVector);
			}
		}
	}
}