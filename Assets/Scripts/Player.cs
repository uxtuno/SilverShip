using UnityEngine;
using System.Linq;
using System.Reflection;
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
		[SerializeField]
		private int _attack = 5;
		protected override int attack
		{
			get
			{
				return _attack;
			}
			set
			{
				_attack = value;
			}
		}

		//private static readonly float near = 0.5f; // カメラに映る最小距離
		//private static readonly float maxCameraRotateY = 5.0f; // プレイヤーが移動したときの最大カメラ回転量

		private PlayerInput playerInput = PlayerInput.instance;
		private CharacterController characterController; // キャラクターコントローラー
		private CameraController cameraController; // カメラコントローラー
												   //private PlayerTrampled playerTrampled; // 踏みつけジャンプ用クラス
		private Transform meshRoot; // プレイヤーメッシュのルート
		private Animator animator; // アニメーションのコントロール用
		private PlayerCamera playerCamera; // カメラ動作を委譲

		// アニメーション用ID
		private int speedID;
		private int isJumpID;
		private int isGroundedID;
		private int isTrampledID;

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

		// プレーヤーの各状態をそれぞれクラスとして表す
		#region - 状態クラス

		/// <summary>
		/// 通常時(地上)
		/// </summary>
		private class NormalState : BaseState
		{
			public NormalState(Player player)
				: base(player)
			{
				// 接地しているのでジャンプ状態を解除
				player.currentJumpState = JumpState.None;
				player.jumpVY = 0.0f;
				player.animator.SetBool(player.isTrampledID, false);
				player.animator.SetBool(player.isGroundedID, true);
				player.isAirDashPossible = false;
				player.attackFlow.ChangeMode(PlayerAttackFlow.Mode.Ground);
			}

			public override void Move()
			{
				if (!player.isGrounded)
				{
					player.animator.SetFloat(player.speedID, 0.0f);
					player.currentState = new AirState(player);
					return;
				}

				Vector3 moveDirection = player.calclateMoveDirection();
				float speed = player.maxSpeed;

				if (playerInput.jump)
				{
					player.Jumping();
					player.isAirDashPossible = true;
					return;
				}
				else if (playerInput.attack)
				{
					player.Attack();
				}

				// ハイジャンプ入力
				if (playerInput.jumpTrampled)
				{
					player.HighJumping();
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
			private enum TrampledJumpInput
			{
				None,
				Jump,
				Attack,
				TrampledJump = Jump | Attack,
			}
			TrampledJumpInput trampledJumpInput = TrampledJumpInput.None; // 踏みつけジャンプ入力検知用
			private static readonly float trampledJumpInputSeconds = 0.1f; // 踏みつけジャンプ入力同時押し猶予時間
			private float trampledJumpInputCount; // 踏みつけジャンプ入力受付カウンタ

			private static readonly float movementRestriction = 0.5f; // この値を掛けることで移動速度を制限
			private static readonly float airDashPossibleSeconds = 0.4f; // 空中ダッシュが可能になる時間
			private static readonly float airDashDisableSeconds = 1.4f; // 空中ダッシュが可能になる時間
			private float airDashPossibleCount; // 空中ダッシュが可能になる時間のカウンタ
			public AirState(Player player)
				: base(player)
			{
				player.attackFlow.ChangeMode(PlayerAttackFlow.Mode.Air);
			}

			public override void Move()
			{
				airDashPossibleCount += Time.deltaTime;

				AnimatorStateInfo state = player.animator.GetCurrentAnimatorStateInfo(0);
				if (state.IsName("Base Layer.Trampled"))
				{
					player.animator.SetBool(player.isTrampledID, false);
				}

				player.attackFlow.Move();

				if (trampledJumpInput != TrampledJumpInput.None)
				{
					trampledJumpInputCount += Time.deltaTime;
					if (trampledJumpInputCount >= trampledJumpInputSeconds)
					{
						// 同時押しではなかったので通常の動作
						if (trampledJumpInput == TrampledJumpInput.Jump && player.currentJumpState == JumpState.Jumping)
						{
							// 入力受付時間内なら空中ダッシュ
							if (airDashPossibleCount > airDashPossibleSeconds && airDashPossibleCount < airDashDisableSeconds)
							{
								if (player.isAirDashPossible)
								{
									player.currentState = new AirDashState(player);
									return;
								}
							}
						}
						else if (trampledJumpInput == TrampledJumpInput.Attack)
						{
							player.Attack();
						}

						trampledJumpInputCount = 0.0f;
						trampledJumpInput = TrampledJumpInput.None;
					}
				}

				// 同時押しのそれぞれのボタンの入力情報を追加していく
				if (playerInput.jump)
				{
					trampledJumpInput |= TrampledJumpInput.Jump;
				}

				if (playerInput.attack)
				{
					trampledJumpInput |= TrampledJumpInput.Attack;
				}

				// 踏みつけジャンプ入力成功
				if (trampledJumpInput == TrampledJumpInput.TrampledJump)
				{
					trampledJumpInput = TrampledJumpInput.None;
					if (player.lockOnTarget != null)
					{
						// todo : 空中ダッシュ入力受付時間をそのまま踏みつけジャンプの受付時間に利用している
						// そのうち整理するだろう(希望的観測
						if (airDashPossibleCount > airDashPossibleSeconds && airDashPossibleCount < airDashDisableSeconds)
						{
							player.currentState = new DashToTargetState(player);
						}
					}
					return;
				}

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

				if (player.jumpVY < 0.0f)
				{
					// 落下中は速度を落とす
					player.FallGravity();
				}
				else
				{
					// 上昇中の重力
					player.Gravity();
				}
				Vector3 moveVector = moveDirection * speed;
				moveVector.y = player.jumpVY;

				player.Move(moveVector * Time.deltaTime);
				if (moveDirection != Vector3.zero)
				{
					Vector3 newAngles = Vector3.zero;
					newAngles.y = Mathf.Atan2(moveVector.x, moveVector.z) * Mathf.Rad2Deg;
					player.meshRoot.eulerAngles = newAngles;
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
				player.isAirDashPossible = false;
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
				player.animator.SetTrigger(player.isJumpID);
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
		/// 対象へダッシュ(ロックオン対象)
		/// </summary>
		private class DashToTargetState : BaseState
		{
			private static readonly float contactDistance = 0.2f; // 対象に接触したとみなす距離
			private static readonly float speed = 20.0f; // 対象へ向かうスピード
			private Vector3 moveVector;
			private Transform target;
			public DashToTargetState(Player player)
				: base(player)
			{
				target = player.lockOnTarget.transform;
			}

			public override void Move()
			{
				Vector3 oldPosition = player.transform.position;
				// 移動ベクトルを計算し、プレイヤーを移動させる
				moveVector = target.position - player.transform.position;
				moveVector.Normalize();
				moveVector *= Time.deltaTime * speed;
				player.Move(moveVector);
				// 対象に十分接触している、もしくは移動量がmoveVectorで指定した値よりも少なかったら(何かにぶつかって移動できなかった)
				if ((target.position - player.transform.position).magnitude < contactDistance ||
					(player.transform.position - oldPosition).magnitude < moveVector.magnitude * 0.9f)
				{
					// ジャンプさせる
					player.Jumping();
					player.isAirDashPossible = true;
					player.animator.SetBool(player.isTrampledID, true);
					player.currentState = new DepressionState(player);
					player.powerPointCreator.Create(player.lockOnTarget.transform.position);
					player.LockOnRelease();
					return;
				}
			}
		}

		/// <summary>
		/// 攻撃状態
		/// </summary>
		private class AttackState : BaseState
		{
			private Vector3 moveVector;
			public AttackState(Player player)
				: base(player)
			{
			}

			public override void Move()
			{
				player.attackFlow.Move();
				if (!player.attackFlow.isAction())
				{
					if (player.isGrounded)
					{
						player.currentState = new NormalState(player);
					}
					else
					{
						player.currentState = new AirState(player);
					}

					return;
				}
				player.FallGravity();
				// マジックナンバーがなんぼのもんじゃーい
				// 攻撃中の落下速度調整用数値
				moveVector.y = player.jumpVY * 0.15f * Time.deltaTime;
				player.Move(moveVector);
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
		public LockOnState lockOnState
		{
			get { return _lockOnState; }
			private set { _lockOnState = value; }
		}

		// ロックオンアイコン画像
		private Sprite autoLockOnIconSprite;
		private Sprite manualLockOnIconSprite;

		private static readonly float autoLockOnLimitDistance = 6.0f; // オートロックオン限界距離
		private static readonly float manualLockOnLimitDistance = 30.0f; // マニュアルロックオン限界距離

		private PlayerAttackFlow attackFlow;
		private GameObject powerPointPrefab; // 結界ポイントエフェクト
		private PowerPointCreator powerPointCreator; // 結界の点を生成するためのクラス

		#endregion

		void Start()
		{
			Physics.gravity = new Vector3(0.0f, Physics.gravity.y * 2.0f, 0.0f);

			// リソースロード
			autoLockOnIconSprite = Resources.Load<Sprite>("Sprites/AutoLockOnIcon");
			manualLockOnIconSprite = Resources.Load<Sprite>("Sprites/ManualRockOnIcon");

			characterController = GetComponent<CharacterController>();
			cameraController = GameObject.FindGameObjectWithTag(TagName.CameraController).GetComponent<CameraController>();
			//playerTrampled = GetComponentInChildren<PlayerTrampled>();
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
			isGroundedID = Animator.StringToHash("IsGrounded");
			isTrampledID = Animator.StringToHash("IsTrampled");

			attackFlow = new PlayerAttackFlow(animator);

			// 初期状態へ
			currentState = new NormalState(this);

			// ロックオンアイコン用のCanvas
			UICanvasGenerator.FollowIconCanvasGenerate();
			GameObject lockOnIconPrefab = Resources.Load<GameObject>("Prefabs/UI/LockOnIcon");
			lockOnIcon = Instantiate(lockOnIconPrefab).GetSafeComponent<FollowIcon>();
			lockOnIcon.Hide();

			powerPointPrefab = Resources.Load<GameObject>("Prefabs/Effects/PowerPoint");

			// 結界の点を生成するためのスクリプトをアタッチ
			powerPointCreator = gameObject.AddComponent<PowerPointCreator>();
		}

		Vector3 cameraFront = new Vector3(0.0f, -0.2f, 1.0f);

		void FixedUpdate()
		{
			if (lockOnState == LockOnState.Manual)
			{
				cameraController.LookAt(lockOnTarget.transform, 1.0f, CameraController.InterpolationMode.Curve);
			}

			if (playerInput.cameraToFront)
			{
				cameraController.SetRotation(Quaternion.LookRotation(meshRoot.rotation * cameraFront), 0.6f, CameraController.InterpolationMode.Curve);
			}
			checkGrounded();

			// プレイヤーが移動する前の「カメラ→プレイヤー」ベクトルを保持
			BaseState oldState;
			do
			{
				oldState = currentState;
				// 現在の状態の動作を実行
				currentState.Move();
			} while (currentState != oldState);
			// 移動後の「カメラ→プレイヤー」ベクトル
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
			else if (lockOnIcon.isShow)
			{
				lockOnIcon.Hide();
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
			playerCamera.EndLockOn();
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
		/// 重力計算(落下時)
		/// </summary>
		private void FallGravity()
		{
			jumpVY += Physics.gravity.y * Time.deltaTime * 0.2f;
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
			Move(new Vector3(vx, vy, vz));
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

		/// <summary>
		/// 攻撃判定を発生
		/// </summary>
		private void Attack()
		{
			// 空中攻撃
			attackFlow.Move();
			if (attackFlow.isAction())
			{
				currentState = new AttackState(this);
			}

			foreach (Transform enemy in containedObjects)
			{
				// todo : 技倍率は仮
				enemy.GetComponent<Actor>().Damage(attack, 1.0f);
			}
		}

		/// <summary>
		/// プレイヤーにジャンプさせる
		/// </summary>
		private void Jumping()
		{
			jumpVY = jumpPower;
			currentJumpState = JumpState.Jumping;
			// 正しくアニメーションを遷移させるために接地フラグをfalseに
			animator.SetBool(isGroundedID, false);
			currentState = new DepressionState(this);
		}

		/// <summary>
		/// プレイヤーにハイジャンプさせる
		/// </summary>
		private void HighJumping()
		{
			jumpVY = highJumpPower;
			currentJumpState = JumpState.HighJumping;
			// 正しくアニメーションを遷移させるために接地フラグをfalseに
			animator.SetBool(isGroundedID, false);
			currentState = new DepressionState(this);
		}

		/// <summary>
		/// 踏みつけジャンプ入力処理
		/// 入力されたら対象へダッシュ状態へ移行
		/// </summary>
		private void JumpTrampledInput()
		{
			currentState = new DashToTargetState(this);
		}


	}
}