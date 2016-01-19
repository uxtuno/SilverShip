using UnityEngine;
using System.Collections;

namespace Uxtuno
{
	/// <summary>
	/// カメラに近づきすぎたものを非表示にする
	/// </summary>
	public class CameraNearHide : MyMonoBehaviour
	{
		[SerializeField, Tooltip("非表示にする対象オブジェクト")]
		private MyMonoBehaviour hideTarget;

		void OnTriggerEnter(Collider other)
		{
			if(other.tag == TagName.MainCamera)
			{
				hideTarget.isShow = false;
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (other.tag == TagName.MainCamera)
			{
				hideTarget.isShow = true;
			}
		}

	}
}