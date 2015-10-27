using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kuvo
{
	public class QuestSelactionSceneController : MonoBehaviour
	{
		/// <summary>
		/// 以前選択状態にあったゲームオブジェクト(切り替え確認用)
		/// </summary>
		private GameObject oldSelectedGameObject { get; set; }
		private SoundCollector soundCollector { get; set; }

		private void Awake()
		{
			oldSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			soundCollector = gameObject.AddComponent<SoundCollector>();
		}

		void Update()
		{
			// 選択状態のゲームオブジェクトが切り替わった直後に1度実行する
			if (oldSelectedGameObject != EventSystem.current.currentSelectedGameObject)
			{
				ChangeSelectedGameObject();
				oldSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			}
		}

		private void ChangeSelectedGameObject()
		{
			// 選択状態のゲームオブジェクトが切り替わった直後の処理
			if (!SoundPlayerSingleton.Instance.PlaySE(gameObject, soundCollector[SoundCollector.SoundName.CursorSelect]))
			{
				Debug.LogError("SEが再生できませんでした。");
			}
		}
	}
}
