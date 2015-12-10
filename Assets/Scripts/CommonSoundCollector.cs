namespace Kuvo
{
	/// <summary>
	/// 特定の名前から
	/// 対応するAudioClipを取得する
	/// (全般的に使用されるAudioClipを保持している)
	/// </summary>
	public class CommonSoundCollector : BaseSoundCollector<CommonSoundCollector.SoundName>
	{
		public enum SoundName
		{
			NONE,
		}

		protected override void Awake()
		{
			sounds.Add(SoundName.NONE, null);
		}
	}
}
