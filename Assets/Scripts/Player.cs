using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

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
		[Tooltip("オートロックオンエリア"), SerializeField]
		private ContainedObjects autoLockOnArea;
		[SerializeField]
		private int _attack = 5;
		public override int attack
		{
			get
			{
				return _attack;
			}
			protected set
			{
				_attack = value;
			}
		}

		private CharacterController characterController; // キャラクターコントローラー
		private GameObject cameraRigPrefab; // カメラコントローラー
		private CameraController cameraController; // カメラコントローラー
		private Transform meshRoot; // プレイヤーメッシュのルート
		private Animator animator; // アニメーションのコントロール用
		private PlayerCamera playerCamera; // カメラ動作を委譲?

		// アニメーション用ID
		private int speedID;
		private int isJumpID;
		private int isGroundedID;
		private int isTrampledID;
		private int isFallID;
		private int isDamageID;

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

		public abstract class BaseState
		{
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

		/// <summary>
		/// 現在のState
		/// </summary>
		public BaseState currentState { get; private set; }

		#region - フィールド
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

		private static readonly float autoLockOnLimitDistance = 12.0f; // オートロックオン限界距離
		private static readonly float manualLockOnLimitDistance = 30.0f; // マニュアルロックオン限界距離

		private PlayerAttackFlow attackFlow;
		private GameObject powerPointPrefab; // 結界ポイントエフェクト
		private PowerPointCreator powerPointCreator; // 結界の点を生成するためのクラス
		private static readonly int barrierPointNumber = 5; // 結界を発生させる事ができる点の数
		private GameObject barrierPrefab;
		[SerializeField, Tooltip("足のCollider")]
		private ContainedObjects _footContained; // 足付近の壁、敵を検知
		[SerializeField, Tooltip("武器Collider")]
		private AttackCollider _weaponCollider;

		#endregion

		void Start()
		{
			// リソースロード
			autoLockOnIconSprite = Resources.Load<Sprite>("Sprites/AutoLockOnIcon");
			manualLockOnIconSprite = Resources.Load<Sprite>("Sprites/ManualRockOnIcon");
			barrierPrefab = Resources.Load<GameObject>("Prefabs/Effects/Barrier/Barrier");
			cameraRigPrefab = Resources.Load<GameObject>("Prefabs/Player/CameraRig");

			characterController = GetComponent<CharacterController>();
			GameObject cameraRig = GameObject.FindGameObjectWithTag(TagName.CameraController);
			if (cameraRig == null)
			{
				cameraRig = Instantiate(cameraRigPrefab);
			}
			cameraController = cameraRig.GetComponent<CameraController>();
			cameraController.target = transform;

			//playerTrampled = GetComponentInChildren<PlayerTrampled>();
			playerCamera = new PlayerCamera(cameraController, horizontalRotationSpeed, verticalRotationSpeed);
			animator = GetComponentInChildren<Animator>(); // アニメーションをコントロールするためのAnimatorを子から取得
			meshRoot = animator.transform; // Animatorがアタッチされているのがメッシュのはずだから

			// ジャンプできる高さから初速を計算(√2gh)
			jumpPower = Mathf.Sqrt(2.0f * -Physics.gravity.y * jumpHeight);
			highJumpPower = Mathf.Sqrt(2.0f * -Physics.gravity.y * highJumpHeight);

			if (autoLockOnArea == null)
			{
				Debug.Log(typeof(ContainedObjects) + " が見つかりません");
			}

			speedID = Animator.StringToHash("Speed");
			isJumpID = Animator.StringToHash("IsJump");
			isGroundedID = Animator.StringToHash("IsGrounded");
			isTrampledID = Animator.StringToHash("IsTrampled");
			isFallID = Animator.StringToHash("IsFall");
			isDamageID = Animator.StringToHash("IsDamage");

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
			if (lockOnState == LockOnState.Manual && lockOnTarget)
			{
				// 敵とプレイヤーの中心点を求め境界球とする
				Vector3 lookPosition = lockOnTarget.lockOnPoint.position;
				// 地上に近すぎないように限界値を決める
				float limitHeightDistance = 2.0f;
				RaycastHit hit;
				Ray ray = new Ray(lookPosition, Vector3.down);
				if (Physics.Raycast(ray, out hit, 2.0f, LayerName.Obstacle.maskValue))
				{
					if (hit.distance < limitHeightDistance)
					{
						lookPosition.y = hit.point.y + limitHeightDistance;
					}
				}
				Vector3 halfToTarget = (lookPosition - lockOnPoint.position) * 0.5f;
				Vector3 center = lockOnPoint.position + halfToTarget;
				halfToTarget.y *= Camera.main.aspect;
				float adjustment = 5.0f; // プレイヤーが視界に入るように適切な値を調整
				float radius = halfToTarget.magnitude + adjustment;
				cameraController.SetPivot(center);
				// d = r / sinθ : ※θはfieldOfView
				cameraController.distance = -(radius / Mathf.Sin(Camera.main.fieldOfView * 0.5f));
				cameraController.LookAt(center, 1.0f, CameraController.InterpolationMode.Curve);
			}

			if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.CameraToFront))
			{
				cameraController.LookDirection(meshRoot.TransformDirection(cameraFront), 1.0f, CameraController.InterpolationMode.Curve);
			}
			checkGrounded();

			UnityEngine.Debug.Assert(currentState != null);

			// 現在の状態の動作を実行
			currentState.Move();
			CommonState();

			LockOnControl();

			StartCoroutine(CameraControl());
		}

		/// <summary>
		/// 各Stateの共通処理
		/// </summary>
		private void CommonState()
		{
			if (powerPointCreator.count == barrierPointNumber &&
				PlayerInput.GetButtonDownInFixedUpdate(ButtonName.Barrier))
			{
				Vector3 powerPointCenter = Vector3.zero;

				// 全ての点の位置を加算
				foreach (Transform powerPoint in powerPointCreator.GetPowerPoints())
				{
					powerPointCenter += powerPoint.position;
				}

				// それを点の個数で割ることで中心点を算出
				powerPointCenter /= powerPointCreator.count;

				GameObject barrier = Instantiate(barrierPrefab);
				barrier.transform.position = powerPointCenter;
			}
		}

		IEnumerator CameraControl()
		{
			yield return new WaitForFixedUpdate();

			if (lockOnState != LockOnState.Manual)
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
		private void LockOnControl()
		{
			// マニュアルロックオン
			if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.LockOn))
			{
				if (lockOnState != LockOnState.Manual)
				{
					BeginManualLockOn();
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
			else
			{
				ManualLockOn();
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
				// ロックオン対象がいなくなったので解除
				if (lockOnTarget == null)
				{
					LockOnRelease();
				}
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

		private static readonly float blockLockOnReleaseSeconds = 1.5f; // ロックオンが解除されるまでの遮蔽時間
		private float blockLockOnReleaseCount;
		/// <summary>
		/// マニュアルロックオン時動作
		/// </summary>
		private void ManualLockOn()
		{
			if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.LeftLockOnChanged))
			{
				LockOnChange(FindNextLockOnEnemy(true));
			}

			if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.RightLockOnChanged))
			{
				LockOnChange(FindNextLockOnEnemy(false));
			}

			// 間に遮るものがあればカウントを増加
			if (CheckLockOnObstacle())
			{
				blockLockOnReleaseCount += Time.deltaTime;
				if (blockLockOnReleaseCount > blockLockOnReleaseSeconds)
				{
					LockOnRelease();
				}
			}
			else
			{
				blockLockOnReleaseCount = 0.0f;
			}
		}

		/// <summary>
		/// ロックオン対象との間に障害物があるか
		/// </summary>
		/// <returns></returns>
		private bool CheckLockOnObstacle()
		{
			// ロックオン対象からプレイヤーへレイを飛ばす
			Vector3 origin = lockOnTarget.lockOnPoint.position;
			Vector3 direction = lockOnPoint.position - origin;
			return Physics.Raycast(origin, direction, direction.magnitude, LayerName.Obstacle.maskValue);
		}

		/// <summary>
		/// 次にロックオンする敵を見つける
		/// </summary>
		/// <param name="isLeft">左側から探す(falseなら右から)</param>
		private Actor FindNextLockOnEnemy(bool isLeft)
		{
			float minAngle = float.PositiveInfinity;
			Transform lockOn = null;
			foreach (Transform enemy in autoLockOnArea)
			{
				// プレイヤーからエネミーへのベクトル
				var toEnemy = enemy.position - transform.position;
				var front = cameraController.cameraTransform.forward;
				front.y = 0.0f;
				float cosTheta = Vector3.Dot(toEnemy, front) / (toEnemy.magnitude * front.magnitude);
				float angle = Mathf.Acos(cosTheta) * Mathf.Rad2Deg;
				float rotateDirection = front.x * toEnemy.z - front.z * toEnemy.x;
				if (isLeft)
				{
					if (rotateDirection < 0.0f)
						continue;
				}
				else
				{
					if (rotateDirection >= 0.0f)
						continue;
				}

				if (enemy == lockOnTarget.transform)
					continue;

				if (angle < minAngle)
				{
					minAngle = angle;
					lockOn = enemy;
				}
			}

			return lockOn != null ? lockOn.GetComponent<Actor>() : null;
		}

		/// <summary>
		/// マニュアルロックオン開始
		/// </summary>
		private void BeginManualLockOn()
		{
			// ロックオン
			if (lockOnTarget != null && !CheckLockOnObstacle())
			{

				playerCamera.BeginLockOn(lockOnTarget.lockOnPoint);
				lockOnState = LockOnState.Manual;
				lockOnIcon.Set(lockOnTarget.lockOnPoint, manualLockOnIconSprite);
			}
		}

		/// <summary>
		/// ロックオン対象切り替え
		/// </summary>
		/// <param name="newTarget">新しいロックオン対象</param>
		private void LockOnChange(Actor newTarget)
		{
			if (newTarget != null)
			{
				lockOnTarget = newTarget;
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
			foreach (Transform enemy in autoLockOnArea)
			{
				if (enemy == null)
				{
					continue;
				}

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
			cameraController.ResetPivot();
			cameraController.ResetDistance();
			lockOnTarget = null;
			lockOnIcon.Hide();
			lockOnState = LockOnState.None;
			playerCamera.EndLockOn();
		}

		private const float unGroundedSeconds = 0.08f; // 地面から離れたとみなす時間
		private float ungroundedCount = 0.0f; // characterController.isGroundedがfalseを返してからの時間

		/// <summary>
		/// // 実際に地面に接触しているか
		/// </summary>
		public bool isGrounded { get; private set; }

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

			animator.SetBool(isGroundedID, isGrounded);
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
			input.x = PlayerInput.horizontal;
			input.z = PlayerInput.vertical;
			Vector3 direction = cameraController.cameraTransform.rotation * input;
			direction.y = 0.0f;
			direction.Normalize();
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
			if (PlayerInput.cameraHorizontal == 0.0f &&
				PlayerInput.cameraVertical == 0.0f &&
				lockOnState != LockOnState.Manual
				)
			{
				playerCamera.PlayerMoveToCameraRotation(moveVector);
			}
		}

		/// <summary>
		/// 攻撃判定を発生
		/// </summary>
		private IEnumerator Attack()
		{
			// 空中攻撃
			attackFlow.Move();
			if (attackFlow.isAction())
			{
				currentState = new AttackState(this);
			}

			//foreach (Transform enemy in autoLockOnArea)
			//{
			//	// todo : 技倍率は仮
			//	enemy.GetComponent<Actor>().Damage(attack, 1.0f);
			//}

			_weaponCollider.BeginCollision();
			yield return new WaitForSeconds(1);
			_weaponCollider.EndCollision();
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
			animator.SetTrigger(isJumpID);
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
			currentState = new DashToTargetState(this, lockOnTarget.lockOnPoint.position);
		}

		/// <summary>
		/// 踏みつけジャンプを実行
		/// 点作成攻撃
		/// </summary>
		private void JumpTrampled()
		{
			Jumping();
			isAirDashPossible = true;
			animator.SetBool(isTrampledID, true);
			currentState = new DepressionState(this);
			powerPointCreator.Create(transform.position);
			LockOnRelease();
		}

		public override void Damage(int attackPower, float magnification)
		{
			base.Damage(attackPower, magnification);
			animator.SetTrigger(isDamageID);
			currentState = new DamageState(this);
		}

		// プレーヤーの各状態をそれぞれクラスとしてあらわすStateパターンで実装
		// todo : 状態遷移の際にインスタンスの生成を伴うので、リアルタイムな動作を要求されるゲームには向いてないと思われる
		// 解決法として、状態クラスをSingletonとして実装し、一つのインスタンスを再利用する方法が考えられる
		// また、Singletonとして実装するのであれば内部変数は持つべきでないかもしれない
		// xxx : 状態遷移の実行箇所がバラバラで、可読性の面からも書き直す必要あり
		#region - 状態クラス

		/// <summary>
		/// 通常時(地上)
		/// </summary>
		public class NormalState : BaseState
		{
			public NormalState(Player player)
				: base(player)
			{
				// 接地しているのでジャンプ状態を解除
				player.currentJumpState = JumpState.None;
				player.jumpVY = 0.0f;
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

				player.attackFlow.Move();

				if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.Jump))
				{
					player.Jumping();
					player.isAirDashPossible = true;
					return;
				}
				else if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.Attack))
				{
					player.StartCoroutine(player.Attack());
				}

				// ハイジャンプ入力
				if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.JumpTrampled))
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
		public class AirState : BaseState
		{
			private static readonly float trampledJumpInputSeconds = 0.1f; // 踏みつけジャンプ入力同時押し猶予時間
			private float trampledJumpInputCount; // 踏みつけジャンプ入力受付カウンタ

			private static readonly float movementRestriction = 0.5f; // この値を掛けることで移動速度を制限
			private static readonly float airDashPossibleSeconds = 0.4f; // 空中ダッシュが可能になる時間
			private static readonly float airDashDisableSeconds = 1.4f; // 空中ダッシュが可能になる時間
			private float airDashPossibleCount; // 空中ダッシュが可能になる時間のカウンタ
			private static readonly float airCommandLimitHeight = 3.0f; // 空中動作が可能な高度
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

				if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.Jump))
				{
					if (player.isAirDashPossible &&
						airDashPossibleCount > airDashPossibleSeconds &&
						airDashPossibleCount < airDashDisableSeconds)
					{
						player.currentState = new AirDashState(player);
						return;
					}
				}
				else if (player.jumpVY < 0.0f && PlayerInput.GetButtonDownInFixedUpdate(ButtonName.Attack))
				{
					player.StartCoroutine(player.Attack());
				}

				// 踏みつけジャンプ入力成功
				if (PlayerInput.GetButtonDownInFixedUpdate(ButtonName.JumpTrampled))
				{
					RaycastHit hit;
					Ray ray = new Ray(player.transform.position, player.meshRoot.forward);
					//if (player.__footContained.GetContainedObjects().Count != 0)
					if (Physics.Raycast(ray, out hit, 0.5f))
					{
						// 壁付近ならジャンプ
						if (hit.transform.tag == "Wall")
						{
							player.Jumping();
							player.currentState = new WallKick(player);
							Vector3 angles = Vector3.zero;
							angles.y = Mathf.Atan2(hit.normal.x, hit.normal.z) * Mathf.Rad2Deg;
							player.meshRoot.eulerAngles = angles;
							return;
						}
					}

					if (player.lockOnTarget != null)
					{
						ray = new Ray(player.transform.position, Vector3.down);
						if (!Physics.Raycast(ray, out hit, airCommandLimitHeight, LayerName.Obstacle.maskValue))
						{
							player.currentState = new DashToTargetState(player, player.lockOnTarget.lockOnPoint.position);
						}
					}
					return;
				}

				// 接地しているので通常状態に
				if (player.isGrounded && player.jumpVY <= 0.0f)
				{
					player.currentState = new NormalState(player);
					player.animator.SetBool(player.isFallID, false);
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
					player.animator.SetBool(player.isFallID, true);
					// 落下中は速度を落とす
					player.Gravity();
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
		public class AirDashState : BaseState
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
		public class DepressionState : BaseState
		{
			private float transitionCount; // 次の状態へ遷移するまでの時間をカウント
			private static readonly float transitionSeconds = 0.3f; // 次の状態へ遷移するまでの時間
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
		public class DashToTargetState : BaseState
		{
			private static readonly float contactDistance = 0.2f; // 対象に接触したとみなす距離
			private static readonly float speed = 20.0f; // 対象へ向かうスピード
			private Vector3 moveVector;
			private Vector3 target;
			public DashToTargetState(Player player, Vector3 position)
				: base(player)
			{
				target = position;
			}

			public override void Move()
			{
				Vector3 oldPosition = player.transform.position;
				// 移動ベクトルを計算し、プレイヤーを移動させる
				moveVector = target - player.transform.position;
				moveVector.Normalize();
				moveVector *= Time.deltaTime * speed;
				player.Move(moveVector);
				// 対象に十分接触している、もしくは移動量がmoveVectorで指定した値よりも少なかったら(何かにぶつかって移動できなかった)
				if ((target - player.transform.position).magnitude < contactDistance ||
					(player.transform.position - oldPosition).magnitude < moveVector.magnitude * 0.9f)
				{
					foreach (var enemy in player._footContained)
					{
						// 踏みつけジャンプさせる
						if (enemy.tag == TagName.Enemy || player.lockOnTarget != null)
						{
							// 倍率は適当な数値(マジックナンバー)
							enemy.GetComponent<Actor>().Damage(player.attack, 1.0f);
						}
					}

					if(player.lockOnTarget != null)
					{
						player.JumpTrampled();
						return;
					}
					return;
				}
			}
		}

		/// <summary>
		/// 攻撃状態
		/// </summary>
		public class AttackState : BaseState
		{
			private static readonly float speed = 4.0f;
			public AttackState(Player player)
				: base(player)
			{
				player.attackFlow.OnActionChanged += new Action(OnActionChanged);
			}

			public override void Move()
			{
				player.attackFlow.Move();
				if (!player.attackFlow.isAction())
				{
					player.attackFlow.OnActionChanged -= new Action(OnActionChanged);
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
				Vector3 inputVec = player.calclateMoveDirection();
				Vector3 moveVector = Vector3.zero;
				if (inputVec.sqrMagnitude > float.Epsilon)
				{
					moveVector = inputVec * speed;
					Vector3 newAngles = Vector3.zero;
					newAngles.y = Mathf.Atan2(moveVector.x, moveVector.z) * Mathf.Rad2Deg;
					player.meshRoot.eulerAngles = newAngles;
				}
				player.FallGravity();
				moveVector.y = player.jumpVY;
				player.Move(moveVector * Time.deltaTime);
			}

			private void OnActionChanged()
			{

			}
		}

		/// <summary>
		/// 壁キック
		/// </summary>
		public class WallKick : BaseState
		{
			// 初速
			private static readonly float primarySpeed = 12.0f;
			private float speed;
			private float wallKickCount;
			private static readonly float wallKickSeconds = 0.4f;
			private static readonly float deceleration = 0.36f; // 減速量
			public WallKick(Player player)
				: base(player)
			{
				speed = primarySpeed;
				player.isAirDashPossible = true;
				player.animator.SetBool(player.isTrampledID, true);
			}

			public override void Move()
			{
				player.Gravity();
				Vector3 moveVector = player.meshRoot.forward * speed;
				moveVector.y = player.jumpVY;
				player.Move(moveVector * Time.deltaTime);
				speed -= deceleration;

				wallKickCount += Time.deltaTime;
				if (wallKickCount >= wallKickSeconds)
				{
					player.currentState = new AirState(player);
				}
			}
		}

		/// <summary>
		/// ダメージ判定中
		/// </summary>
		public class DamageState : BaseState
		{
			private readonly float damegeSeconds;
			public DamageState(Player player)
				: base(player)
			{
				damegeSeconds = player.animator.GetCurrentAnimatorStateInfo(0).length;
				player.StartCoroutine(MoveCoroutine());
				player.jumpVY = 0.0f;
			}

			public override void Move()
			{
			}

			IEnumerator MoveCoroutine()
			{
				float count = 0.0f;
				while (count <= damegeSeconds)
				{
					count += Time.deltaTime;
					player.FallGravity();
					Vector3 moveVector = Vector3.zero;
					moveVector.y = player.jumpVY;
					player.Move(moveVector * Time.deltaTime);
					yield return null;
				}
				player.currentState = new NormalState(player);
			}
		}

		#endregion
	}
}