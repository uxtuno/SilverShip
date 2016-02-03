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
		private static readonly float playerMoveToCameraRotationSpeed = 5.0f; // プレイヤーが動いた時のカメラ回転速度
		private readonly float horizontalRotationSpeed;
		private readonly float verticalRotationSpeed;
		private PlayerInput input = PlayerInput.instance;

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
			controller.SetRotation(q, 0.5f, CameraController.InterpolationMode.Curve);
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
			cameraMove.x = input.cameraHorizontal;
			cameraMove.y = input.cameraVertical;

			// プレイヤーが移動した時のY軸カメラ回転量を計算
			//float cameraRotateY = Mathf.Atan2(now.x * old.z - now.z * old.x, now.x * old.x + now.z * old.z) * Mathf.Rad2Deg;
			moveVector = controller.cameraTransform.InverseTransformDirection(moveVector);
			Vector2 moveVectorXZ = new Vector2(moveVector.x, moveVector.z);
			float moveAngleXZ = Mathf.Atan2(moveVectorXZ.y, moveVectorXZ.x);
			float cameraHorizontal = Mathf.Cos(moveAngleXZ) * moveVectorXZ.magnitude;

			if (cameraMove != Vector2.zero)
			{
				controller.CameraMove(cameraMove.x * horizontalRotationSpeed * Time.deltaTime, cameraMove.y * verticalRotationSpeed * Time.deltaTime, 0.2f);
			}
			else if (cameraHorizontal != 0.0f)
			{
				controller.CameraMove(cameraHorizontal * playerMoveToCameraRotationSpeed, 0.0f, 0.1f);
			}
		}

		/// <summary>
		/// プレイヤーの入力によりカメラを制御
		/// </summary>
		public void CameraInput()
		{
			// カメラの回転入力
			Vector2 cameraMove = Vector3.zero;
			cameraMove.x = input.cameraHorizontal;
			cameraMove.y = input.cameraVertical;

			if (cameraMove != Vector2.zero)
			{
				controller.CameraMove(cameraMove.x * horizontalRotationSpeed * Time.deltaTime, cameraMove.y * verticalRotationSpeed * Time.deltaTime, 0.2f);
			}
		}

		public void LockOnCamera()
		{
			if(target == null)
			{
				return;
			}
			controller.SetPovot((target.position + player.transform.position) / 2.0f);

			Vector3 playerPosition = new Vector3(player.transform.position.x, 0.0f, player.transform.position.z);
			Vector3 cameraPosition = new Vector3(cameraTransform.position.x, 0.0f, cameraTransform.position.z);
			Vector3 cameraToPlayer = playerPosition - cameraPosition;
			Vector3 cameraVertical = new Vector3(cameraTransform.right.x, 0.0f, cameraTransform.right.z);
			cameraVertical.Normalize();

			// カメラの視線方向と垂直となるベクトルとプレイヤーの座標との直近点を求める
			// 線分 と 点 の直近座標を求める式を利用
			Vector3 nearPositionOnCameraVertical = cameraPosition + cameraVertical * Vector3.Dot(cameraToPlayer, cameraVertical);

			// カメラの水平ベクトルとプレイヤーとの距離
			float cameraVerticalToPlayerDistance = (playerPosition - nearPositionOnCameraVertical).magnitude;

			cameraToPlayer.Normalize();
			Vector3 cameraFrontXZ = new Vector3(cameraTransform.forward.x, 0.0f, cameraTransform.forward.z);
			cameraFrontXZ.Normalize();

			if (cameraVerticalToPlayerDistance > 5.0f)
			{
				cameraFrontXZ *= cameraVerticalToPlayerDistance - 5.0f;
				cameraTransform.position += cameraFrontXZ;
			}
			else if (cameraVerticalToPlayerDistance < 3.0f)
			{
				cameraFrontXZ *= 3.0f - cameraVerticalToPlayerDistance;
				cameraTransform.position -= cameraFrontXZ;
			}
			//float cameraToPlayerAngle = Mathf.Acos(Vector3.Dot(cameraFrontXZ, cameraToPlayer.normalized)) * Mathf.Rad2Deg;
			//if (cameraToPlayerAngle > 30.0f)
			//{
			//	if (cameraFrontXZ.x * cameraToPlayer.z - cameraFrontXZ.z * cameraToPlayer.x > 0.0f)
			//	{
			//		controller.CameraMove((cameraToPlayerAngle - 30.0f), 0.0f, 0.0f);
			//	}
			//	else
			//	{
			//		controller.CameraMove(-(cameraToPlayerAngle - 30.0f), 0.0f, 0.0f);
			//	}
			//}

			//// マニュアルロックオン時カメラ動作
			//// Todo : 何度もアクセスするためcameraTransformをフィールドとして保持したい
			//Vector2 v1 = new Vector2(controller.cameraTransform.forward.x, controller.cameraTransform.forward.z).normalized;
			//Vector3 cameraToPlayer = (target.position - controller.cameraTransform.position).normalized;
			//Vector3 cameraToLockOnEnemy = (target.position - controller.cameraTransform.position).normalized;
			//Vector2 v3 = new Vector2(cameraToPlayer.x, cameraToPlayer.z).normalized;
			//Vector2 v2 = new Vector2(cameraToLockOnEnemy.x, cameraToLockOnEnemy.z).normalized;
			//float rotateY = Mathf.Acos(Vector2.Dot(v1, v2)) * Mathf.Rad2Deg - 30.0f;
			//bool isLookEnemy = false;
			//bool isLookPlayer = false;
			//if (rotateY > 0.0f)
			//{
			//	isLookEnemy = true;
			//	if ((v1.x * v2.y - v1.y * v2.x > 0.0f))
			//	{
			//		//cameraController.CameraActualMove(-(rotateY), 0.0f);
			//		controller.CameraMove(-(rotateY) * Time.deltaTime * 30.0f, 0.0f, 0.0f);
			//	}
			//	else
			//	{
			//		controller.CameraMove((rotateY) * Time.deltaTime * 30.0f, 0.0f, 0.0f);
			//	}
			//	//Quaternion q = Quaternion.LookRotation(lockOnTarget.lockOnPoint.position - cameraController.transform.position);
			//	//cameraController.SetRotation(q, 0.5f, CameraController.InterpolationMode.Curve);
			//}
			//else
			//{
			//	v1 = new Vector2(controller.cameraTransform.forward.x, controller.cameraTransform.forward.z).normalized;
			//	rotateY = Mathf.Acos(Vector2.Dot(v1, v3)) * Mathf.Rad2Deg - 30.0f;
			//	if (rotateY > 0.0f)
			//	{
			//		Quaternion q = Quaternion.LookRotation(target.position - controller.transform.position);
			//		controller.SetRotation(q, 0.5f, CameraController.InterpolationMode.Curve);
			//	}
			//}
		}
	}
}

