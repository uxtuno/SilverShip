using UnityEngine;

namespace Kuvo
{
	public class MainSceneController : MonoBehaviour
	{
		private MainSoundCollector soundCollector { get { return this.GetSafeComponent<MainSoundCollector>(); } }
		[SerializeField]
		private GameObject seaObject = null;

		private void Start()
		{
			if(seaObject)
			{
				if (!SoundPlayerSingleton.instance.PlaySE(seaObject, soundCollector[MainSoundCollector.SoundName.SeaWave], true, SoundPlayerSingleton.FadeMode.NONE, 0, 0.2f)) 
				{
					Debug.LogError("SeaWaveBGMの再生に失敗しました");
				}
			}

			if (!SoundPlayerSingleton.instance.PlayBGM(soundCollector[MainSoundCollector.SoundName.BGM], true, SoundPlayerSingleton.FadeMode.FadeIn))
			{
				Debug.LogError("BGMの再生に失敗しました");
			}
		}
	}
}
