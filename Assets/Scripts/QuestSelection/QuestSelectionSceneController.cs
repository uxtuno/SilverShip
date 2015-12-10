using UnityEngine;
using UnityEngine.EventSystems;

namespace Kuvo
{
	public class QuestSelectionSceneController : MonoBehaviour
	{
		/// <summary>
		/// 以前選択状態にあったゲームオブジェクト(切り替え確認用)
		/// </summary>
		private GameObject oldSelectedGameObject { get; set; }
		private QuestSelectionSoundCollector soundCollector { get; set; }
		private bool isFirstSelected { get; set; }

		private void Start()
		{
			oldSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			soundCollector = gameObject.AddComponent<QuestSelectionSoundCollector>();
			isFirstSelected = true;

			if (!SoundPlayerSingleton.instance.PlayBGM(soundCollector[QuestSelectionSoundCollector.SoundName.BGM], true, SoundPlayerSingleton.FadeMode.FadeIn))
			{
				Debug.LogError("BGMの再生に失敗しました");
			}
		}

		void Update()
		{
			if (Input.GetButtonDown("Submit"))
			{
				Debug.Log("はいった");
				oldSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			}
			else if (Input.GetButtonDown("Cancel"))
			{

				oldSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			}
			else if (oldSelectedGameObject != EventSystem.current.currentSelectedGameObject)
			{
				ChangeSelectedGameObject();
				oldSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			}
		}

		private void ChangeSelectedGameObject()
		{
			if (isFirstSelected)
			{
				isFirstSelected = false;
				return;
			}

			if (!SoundPlayerSingleton.instance.PlaySE(gameObject, soundCollector[QuestSelectionSoundCollector.SoundName.CursorSelect]))
			{
				Debug.LogError("SEが再生できませんでした。");
			}
		}
	}
}
