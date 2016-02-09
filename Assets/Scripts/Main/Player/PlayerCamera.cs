using UnityEngine;
using System.Collections;

namespace Uxtuno
{
	/// <summary>
	/// プレイヤーのカメラ動作を実装
	/// </summary>
	public class PlayerCamera
	{
		private CameraController controller;
		private Transform cameraTransform;
		private Transform target; // ロックオン対象
		private Player player;
		private static readonly float playerMoveToCameraRotationSpeed = 10.0f; // プレイヤーが動いた時のカメラ回転速度
		private readonly float horizontalRotationSpeed;
		private readonly float verticalRotationSpeed;
		private static readonly float verticalMoveRotationSpeed = 0.5f; // 上下移動時のカメラ回転補正値

		/// <summary>
		/// プレイヤーカメラの動作
		/// </summary>
		/// <param name="controller">カメラを制御するためのクラス</param>
		/// <param name="horizontalRotationSpeed">プレイヤーがカメラを操作するときの水平方向移動速度</param>
		/// <param name="verticalRotationSpeed">プレイヤーがカメラを操作するときの垂直方向移動速度</param>
		public PlayerCamera(CameraController controller, float horizontalRotationSpeed, float verticalRotationSpeed)
		{
			this.controller = controller;
			cameraTransform = controller.cameraTransform;
			this.horizontalRotationSpeed = horizontalRotationSpeed;
			this.verticalRotationSpeed = verticalRotationSpeed;
			player = GameManager.instance.player;
		}

		/// <summary>
		/// ロックオン時のカメラ動作
		/// </summary>
		/// <param name="target"></param>
		public void BeginLockOn(Transform target)
		{
			this.target = target;
			//controller.SetPovot((target.position + player.transform.position) / 2.0f);
			Quaternion q = Quaternion.LookRotation(target.position - cameraTransform.position);
			controller.transform.SetParent(null);
		}

		/// <summary>
		/// ロックオン終了時のカメラ動作
		/// </summary>
		public void EndLockOn()
		{
			//Vector3 lookPoint = player.transform.Find("LookPoint").position;
			//controller.SetPovot(lookPoint);
			//controller.transform.position = lookPoint;
			//controller.transform.SetParent(player.transform);
			//controller.DefaultLocalCameraPosition();
		}

		/// <summary>
		///プレイヤーの移動にともなうカメラの回転
		/// </summary>
		/// <param name="moveVector">プレイヤーの移動ベクトル</param>
		public void PlayerMoveToCameraRotation(Vector3 moveVector)
		{
			// カメラの回転入力
			Vector2 cameraMove = Vector3.zero;
			cameraMove.x = PlayerInput.cameraHorizontal;
			cameraMove.y = PlayerInput.cameraVertical;

			// これは適当な値
			// また、moveVectorを変換する前の値が必要なのでここで取得
			float cameraVertical = -moveVector.y * verticalMoveRotationSpeed;
			// 下を向くのは上昇中のみなのでそれ以外は値を0に
			if(cameraVertical > 0.0f)
			{
				cameraVertical = 0.0f;
			}

			// 地面に着いている状態のみ前方を向く処理
			if ((player.isGrounded || player.currentState.GetType() == typeof(Player.AirDashState)) &&
				controller.xAngle > 0.0f)
			{
				cameraVertical = Vector3.Scale(moveVector, new Vector3(1.0f, 0.0f, 1.0f)).magnitude * verticalMoveRotationSpeed * 0.25f;
			}
			// プレイヤーが移動した時のY軸カメラ回転量を計算
			moveVector = controller.cameraTransform.InverseTransformDirection(moveVector);
			Vector2 moveVectorXZ = new Vector2(moveVector.x, moveVector.z);

			float moveAngleXZ = Mathf.Atan2(moveVectorXZ.y, moveVectorXZ.x);
			float cameraHorizontal = Mathf.Cos(moveAngleXZ) * moveVectorXZ.magnitude;

			if (cameraMove != Vector2.zero)
			{
				controller.CameraMove(cameraMove.x * horizontalRotationSpeed * Time.deltaTime, cameraMove.y * verticalRotationSpeed * Time.deltaTime, 0.2f);
			}
			else if (Mathf.Abs(cameraHorizontal) > float.Epsilon || Mathf.Abs(cameraVertical) > float.Epsilon)
			{
				controller.CameraMove(cameraHorizontal * playerMoveToCameraRotationSpeed, cameraVertical * playerMoveToCameraRotationSpeed, 0.1f);
			}
		}

		/// <summary>
		/// プレイヤーの入力によりカメラを制御
		/// </summary>
		public void CameraInput()
		{
			// カメラの回転入力
			Vector2 cameraMove = Vector3.zero;
			cameraMove.x = PlayerInput.cameraHorizontal;
			cameraMove.y = PlayerInput.cameraVertical;

			if (cameraMove != Vector2.zero)
			{
				controller.CameraMove(cameraMove.x * horizontalRotationSpeed * Time.deltaTime, cameraMove.y * verticalRotationSpeed * Time.deltaTime, 0.1f);
			}
		}
	}
}

