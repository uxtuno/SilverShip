using UnityEngine;
using System.Collections;

namespace Kuvo
{
	/// <summary>
	/// 弾丸クラス
	/// 弾の移動処理以外の弾の状態を規定する
	/// </summary>
	public class BulletEntity : MonoBehaviour
	{
		float delta = 50;
		IEnumerator Start()
		{
			while (true)
			{
				transform.Rotate(new Vector3(0, delta, 0));
				yield return new WaitForFixedUpdate();
			}
		}
	}
}
