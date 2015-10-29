using UnityEngine;
using System.Collections;

namespace Uxtuno
{
	public class PlayerCamera : MyMonoBehaviour
	{
		[Tooltip("水平方向のカメラ移動速度"), SerializeField]
		private float horizontalRotationSpeed = 100.0f; // 水平方向へのカメラ移動速度
		[Tooltip("垂直方向のカメラ移動速度"), SerializeField]
		private float verticaltalRotationSpeed = 40.0f; // 垂直方向へのカメラ移動速度
		[Tooltip("水平方向のカメラ回転閾値"), SerializeField]
		private float horizontalRotationThreshold = 0.2f; // 水平方向のカメラ回転閾値
		[Tooltip("垂直方向のカメラ回転閾値"), SerializeField]
		private float verticalRotationThreshold = 0.2f; // 垂直方向のカメラ回転閾値
		[Tooltip("ドラッグの増幅倍率"), SerializeField]
		private float cameraDragAmplification = 4.0f;

		private Vector2 beginCameraDragPosition; // カメラ回転のためのドラッグ開始地点
		private CameraController cameraController;

		void Start()
		{
			cameraController = GetComponentInChildren<CameraController>();
			if (cameraController == null)
			{
				Debug.LogError("プレイヤーにカメラがありません");
			}
		}

		void Update()
		{
			if (cameraController.targetToDistance < 1.0f)
			{
				isShow = false;
			}
			else if (!isShow)
			{
				isShow = true;
			}

			if (Input.GetMouseButtonDown(1))
			{
				beginCameraDragPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
				// 中央を(0, 0)にする
				beginCameraDragPosition.x -= 0.5f;
				beginCameraDragPosition.y -= 0.5f;
			}

			if (Input.GetMouseButton(1))
			{
				Vector2 position = Camera.main.ScreenToViewportPoint(Input.mousePosition);
				// 中央を(0, 0)にする
				position.x -= 0.5f;
				position.y -= 0.5f;
				position *= cameraDragAmplification; // 移動量を増幅させる
				if (Mathf.Abs(position.y) < verticalRotationThreshold)
				{
					position.y = 0.0f;
				}

				if (Mathf.Abs(position.x) < horizontalRotationThreshold)
				{
					position.x = 0.0f;
				}

				if (position.sqrMagnitude > 1.0f)
				{
					position.Normalize();
				}

				cameraController.CameraMove(position.x * horizontalRotationSpeed * Time.deltaTime, position.y * verticaltalRotationSpeed * Time.deltaTime);
			}
		}
	}
}