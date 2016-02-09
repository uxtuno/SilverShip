using System.Collections;
using UnityEngine;

namespace Kuvo
{
	/// <summary>
	/// 弾丸クラス
	/// 弾の移動処理以外の弾の状態を規定する
	/// </summary>
	public class BulletEntity : MonoBehaviour
	{
		//float delta = 10;
		IEnumerator Start()
		{
			float n = 0.0f;
			Vector3 v = Vector3.zero;
			while (true)
			{
				n += Time.deltaTime;
				//transform.Rotate(new Vector3(0, theta, 0));
				v.x = Mathf.Cos(n) * 360.0f;
				v.y = Mathf.Sin(n) * 360.0f;
				transform.eulerAngles = v;
				yield return new WaitForFixedUpdate();
			}
		}
	}
}
