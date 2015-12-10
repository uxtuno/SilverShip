using System.Collections;
using UnityEngine;

namespace Kuvo
{
	/// <summary>
	/// タイトルシーン管理クラス
	/// </summary>
	public class TitleSceneController : MonoBehaviour
	{
		[SerializeField]
		private GameObject flashingGameObject = null;          // 点滅対象のゲームオブジェクト
		[SerializeField]
		private string nextSceneName = string.Empty;    // 遷移先のシーン名
		private TitleSoundCollector soundCollector { get; set; }

		private void Awake()
		{
			soundCollector = gameObject.AddComponent<TitleSoundCollector>();
			SoundPlayerSingleton.instance.PlayBGM(soundCollector[TitleSoundCollector.SoundName.BGM], true, SoundPlayerSingleton.FadeMode.FadeIn, 10.0f);
		}

		private IEnumerator Start()
		{
			if (!flashingGameObject)
			{
				Debug.LogError("点滅させるGameObjectがnullです");
				yield break;
			}

			if (nextSceneName == string.Empty)
			{
				nextSceneName = "questSelection";
			}

			// 一定間隔でゲームオブジェクトを点滅させる
			while (true)
			{
				flashingGameObject.SetActive(!flashingGameObject.activeInHierarchy);
				yield return new WaitForSeconds(0.5f);
			}
		}

		private void Update()
		{
			// いずれかのキーが入力されたらシーンを切り替える
			if (Input.anyKeyDown)
			{
				SceneChangerSingleton.instance.FadeChange(nextSceneName);
			}
		}
	}
}
