using UnityEngine;
using System.Collections;

/// <summary>
/// カメラからの距離を一定にする
/// </summary>
public class LockOnIcon : MonoBehaviour
{

	[SerializeField]
	private float cameraToDistance = 5.0f; // カメラまでの距離
	private bool isCreate = false;

	/// <summary>
	/// ロックオンターゲット
	/// ロックオンアイコン生成時に設定しておくこと
	/// </summary>
	public Transform lockOnPoint { get; set; }

	void Start()
	{
		// 未設定の場合でも動作するように
		if (lockOnPoint == null)
		{
			lockOnPoint = new GameObject("lockOnTarget").transform;
			lockOnPoint.position = transform.position;
			lockOnPoint.parent = transform.parent;
			isCreate = true;
		}
	}

	void LateUpdate()
	{
		Vector3 viewPortPoint = Camera.main.WorldToViewportPoint(lockOnPoint.position);
		if (Vector3.Angle(Camera.main.transform.forward, transform.position - Camera.main.transform.position) < 90.0f)
		{
			viewPortPoint.z = cameraToDistance;
		}
		else
		{
			viewPortPoint.z = -10.0f;
		}
		transform.position = Camera.main.ViewportToWorldPoint(viewPortPoint);
	}

	void OnDestroy()
	{
		if (isCreate)
		{
			Destroy(lockOnPoint.gameObject);
		}
	}
}
