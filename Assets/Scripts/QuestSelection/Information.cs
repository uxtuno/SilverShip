using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kuvo
{
	/// <summary>
	/// 選択時に対象のテキストを変更するクラス
	/// </summary>
	[RequireComponent(typeof(Selectable))]
	public class Information : MonoBehaviour, ISelectHandler
	{
		[Serializable]
		private struct TargetAndMessage
		{
			public Text target;
			public string message;
		}

		[SerializeField]
		private List<TargetAndMessage> targetsAndMessages;

		/// <summary>
		/// 選択状態になると呼び出される
		/// </summary>
		void ISelectHandler.OnSelect(BaseEventData eventData)
		{
			foreach (TargetAndMessage targetAndMessage in targetsAndMessages)
			{
				targetAndMessage.target.text = targetAndMessage.message;
			}
		}
	}
}