using UnityEngine;

namespace Kuvo
{
	/// <summary>
	/// 特定の名前から
	/// 対応するAudioClipを取得する
	/// Titleシーンで使用されるAudioClipを保持している
	/// </summary>
	public class TitleSoundCollector : BaseSoundCollector<TitleSoundCollector.SoundName>
	{
		public enum SoundName
		{
			NONE,
			BGM,             // BGM
		}

		protected override void Awake()
		{
			sounds.Add(SoundName.NONE, null);
			sounds.Add(SoundName.BGM, Resources.Load<AudioClip>("Sounds/Yoimatsurinokaze"));
		}
	}
}
