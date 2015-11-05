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
		//[Tooltip("水平方向のカメラ回転閾値"), SerializeField]
		//private float horizontalRotationThreshold = 0.2f; // 水平方向のカメラ回転閾値
		//[Tooltip("垂直方向のカメラ回転閾値"), SerializeField]
		//private float verticalRotationThreshold = 0.2f; // 垂直方向のカメラ回転閾値
		//[Tooltip("ドラッグの増幅倍率"), SerializeField]
		//private float cameraDragAmplification = 4.0f;

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

			PlayerInput input = PlayerInput.instance;
			Vector3 position = Vector3.zero;
			position.x = input.cameraHorizontal;
			position.y = input.cameraVertical;

			cameraController.CameraMove(position.x * horizontalRotationSpeed * Time.deltaTime, position.y * verticaltalRotationSpeed * Time.deltaTime);
		}
	}
}