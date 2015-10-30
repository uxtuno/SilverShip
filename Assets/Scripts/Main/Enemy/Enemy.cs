using UnityEngine;
using System.Collections;
using Uxtuno;

namespace Kuvo
{
	/// <summary>
	/// 敵の共通動作を規定した抽象クラス
	/// </summary>
	abstract public class Enemy : MyMonoBehaviour
	{
		private CameraController cameraController;

		protected virtual void Start()
		{
			cameraController = GameObject.FindGameObjectWithTag(TagName.CameraController).GetComponent<CameraController>();
		}

		protected virtual void Update()
		{
			// エネミーとカメラの距離に応じて描画状態を切り替える
			Vector3 cameraToVector = cameraController.cameraTransform.position - transform.position;
			if (cameraToVector.magnitude < 2.0f)
			{
				isShow = false;
			}
			else
			{
				isShow = true;
			}
		}

		/// <summary>
		/// 近接攻撃
		/// </summary>
		abstract public void ShortRangeAttack();
	}
}