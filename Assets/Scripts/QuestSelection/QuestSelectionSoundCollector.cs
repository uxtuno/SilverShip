using UnityEngine;

namespace Kuvo
{
	/// <summary>
	/// 特定の名前から
	/// 対応するAudioClipを取得する
	/// (QuestSelectionシーンで使用されるAudioClipを保持している)
	/// </summary>
	public class QuestSelectionSoundCollector : BaseSoundCollector<QuestSelectionSoundCollector.SoundName>
	{
		public enum SoundName
		{
			NONE,
			BGM,                // BGM
			CursorSelect,       // カーソルの移動音
			Submit,             // 決定音
			Cancel,             // キャンセル音
		}

		protected override void Awake()
		{
			sounds.Add(SoundName.NONE, null);
			sounds.Add(SoundName.BGM, Resources.Load<AudioClip>("Sounds/Bureikou") as AudioClip);
			sounds.Add(SoundName.CursorSelect, Resources.Load<AudioClip>("Sounds/Cursor") as AudioClip);
			sounds.Add(SoundName.Submit, Resources.Load<AudioClip>("Sounds/Submit") as AudioClip);
			sounds.Add(SoundName.Cancel, Resources.Load<AudioClip>("Sounds/Cancel") as AudioClip);
		}
	}
}
