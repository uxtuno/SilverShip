using UnityEngine;
using System.Collections.Generic;

namespace Uxtuno
{
	/// <summary>
	/// プレイヤーのコンボ攻撃などを管理
	/// </summary>
	public class PlayerAttackFlow
	{
		private class Motion
		{
			private readonly string name;
			private readonly float _motionSeconds; // モーション時間
			private float motionCount;

			/// <summary>
			/// モーション時間
			/// </summary>
			public float motionSeconds { get { return _motionSeconds; } }

			private readonly string nextInputName;
			private readonly float inputReceptionSeconds;
			private readonly int motionID;
			//private Motion currenMotion;
			private Motion changeMotion;

			/// <summary>
			/// コンストラクタ
			/// </summary>
			public Motion()
				: this("")
			{
			}

			/// <summary>
			/// コンストラクタ(こんなコメントはいらないんだよー)
			/// </summary>
			/// <param name="name">攻撃モーションの名前</param>
			/// <param name="motionSeconds">攻撃モーションの時間</param>
			/// <param name="motionID">攻撃モーションのID</param>
			/// <param name="nextInputName">攻撃モーションを出すための入力ボタン</param>
			/// <param name="inputReceptionSeconds">入力受付時間</param>
			public Motion(string name, float motionSeconds = 0.0f, int motionID = 0, string nextInputName = "", float inputReceptionSeconds = 0.0f)
			{
				//currenMotion = this;
				this.name = name;
				_motionSeconds = motionSeconds;
				this.motionID = motionID;
				this.nextInputName = nextInputName;
				this.inputReceptionSeconds = inputReceptionSeconds;
			}

			private List<Motion> nextMotions = new List<Motion>();
			/// <summary>
			/// 次に遷移する攻撃モーションを設定
			/// </summary>
			public Motion AddNextMotion(Motion motion)
			{
				nextMotions.Add(motion);
				return motion;
			}

			/// <summary>
			/// モーション制御
			/// </summary>
			/// <returns>別のモーションへの遷移フラグ</returns>
			public bool MotionControl(out Motion newMotion, out int motionID)
			{
				foreach (Motion motion in nextMotions)
				{
					if (Input.GetButtonDown(motion.nextInputName) &&
						motionCount >= motionSeconds - inputReceptionSeconds) // 入力受付時間内なら
					{
						changeMotion = motion;
					}
				}

				// 攻撃モーション後に次のモーションへ移行
				motionCount += Time.deltaTime;
				if (motionCount > motionSeconds)
				{
					newMotion = changeMotion;
					if (changeMotion == null)
					{
						motionID = 0;
					}
					else
					{
						motionID = changeMotion.motionID;
					}

					motionCount = 0.0f;
					changeMotion = null;
					return true;
				}

				newMotion = this;
				motionID = 0;
				return false;
			}

			/// <summary>
			/// 現在のモーション再生経過時間を返す
			/// </summary>
			/// <returns></returns>
			public float GetMotionCount()
			{
				return motionCount;
			}

			/// <summary>
			/// 次のモーションに移行予定か
			/// </summary>
			/// <returns></returns>
			public bool isChange()
			{
				return changeMotion != null;
			}

			/// <summary>
			/// モーション番号を返す
			/// 0なら何も再生していない
			/// </summary>
			/// <returns></returns>
			public int GetMotionID()
			{
				return motionID;
			}

			/// <summary>
			/// モーション名を返す
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				return name;
			}
		}

		/// <summary>
		/// 現在のモード
		/// </summary>
		public enum Mode
		{
			Air,
			Ground,
		}
		private Mode currentMode = Mode.Ground;

		private Motion airRootMotion; // 空中時のルートモーション
		private Motion groundRootMotion; // 地上時のルートモーション
		private readonly Motion rootMotion; // モーション管理
		private Motion currentMotion; // 現在のモーション
		private Animator animator;

		private static readonly int motionParamID;
		/// <summary>
		/// 静的コンストラクタ
		/// </summary>
		static PlayerAttackFlow()
		{
			motionParamID = Animator.StringToHash("Combo");
		}

		/// <summary>
		/// コンボ攻撃などを制御
		/// </summary>
		public PlayerAttackFlow(Animator animator)
		{
			this.animator = animator;
			groundRootMotion = new Motion();
			Motion attack1 = new Motion("GroundAttack1", 1.3f, 1, InputName.Attack, 0.5f);
			Motion attack2 = new Motion("GroundAttack2", 1.3f, 2, InputName.Attack, 0.5f);
			Motion attack3 = new Motion("GroundAttack3", 1.3f, 3, InputName.Attack, 0.5f);
			// コンボ攻撃を追加
			groundRootMotion.AddNextMotion(attack1);
			attack1.AddNextMotion(attack2);
			attack2.AddNextMotion(attack3);

			airRootMotion = new Motion();
			attack1 = new Motion("AirAttack1", 1.3f, 1, InputName.Attack, 0.5f);
			attack2 = new Motion("AirAttack2", 1.3f, 2, InputName.Attack, 0.5f);
			attack3 = new Motion("AirAttack3", 1.3f, 3, InputName.Attack, 0.5f);

			// コンボ攻撃を追加
			airRootMotion.AddNextMotion(attack1);
			attack1.AddNextMotion(attack2);
			attack2.AddNextMotion(attack3);

			currentMotion = groundRootMotion;
		}

		public void ChangeMode(Mode mode)
		{
			switch (mode)
			{
				case Mode.Air:
					currentMotion = airRootMotion;
					break;
				case Mode.Ground:
					currentMotion = groundRootMotion;
					break;
			}
			currentMode = mode;
		}

		/// <summary>
		/// 攻撃中か
		/// </summary>
		public bool isAction()
		{
			return currentMotion.GetMotionID() != 0;
		}

		/// <summary>
		/// 攻撃動作
		/// </summary>
		public void Move()
		{
			int motionID;
			bool changed = currentMotion.MotionControl(out currentMotion, out motionID);
			if (changed)
			{
				animator.SetInteger(motionParamID, motionID);

				if (currentMotion == null)
				{
					ChangeMode(currentMode);
				}
			}
		}
	}
}
