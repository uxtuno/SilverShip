using System.Collections.Generic;
using UnityEngine;

namespace Kuvo
{
	/// <summary>
	/// 音の再生等を行うクラス
	/// </summary>
	public class SoundPlayerSingleton : MonoBehaviour
	{
		#region - シングルトンを実現させるための処理 -
		// 唯一のインスタンス
		private static SoundPlayerSingleton _instance;

		/// <summary>
		/// プライベートコンストラクタ―
		/// </summary>
		private SoundPlayerSingleton()
		{
		}

		public static SoundPlayerSingleton instance
		{
			get
			{
				if (!_instance)
				{
					if (!(_instance = FindObjectOfType<SoundPlayerSingleton>()))
					{
						GameObject go = new GameObject("SoundPlayerSingleton");
						_instance = go.AddComponent<SoundPlayerSingleton>();
					}
				}

				return _instance;
			}
		}
		#endregion

		private AudioSource fadeSource { get; set; }
		private AudioSource bgmSource { get; set; }
		private List<AudioSource> seSources { get; set; }

		public enum FadeMode
		{
			NONE,
			FadeIn,
			FadeOut,
		}

		public void Awake()
		{
			// 複数生成の禁止
			if (this != instance)
			{
				Destroy(gameObject);
			}

			fadeSource = null;
			bgmSource = null;
			seSources = new List<AudioSource>();
		}

		/// <summary>
		/// BGMを再生する
		/// </summary>
		/// <param name="target"> BGMの発信源</param>
		/// <param name="audioClip"> 音源</param>
		/// <param name="loop"> ループ再生するかどうか</param>
		/// <param name="mode"> フェードの種類</param>
		/// <param name="fadeTime"> フェードにかける時間</param>
		/// <param name="maxVolume"> 最大音量(通常時の音量でもある)</param>
		/// <param name="minVolume"> 最小音量</param>
		public bool PlayBGM(AudioClip audioClip, bool loop = false
			, FadeMode mode = FadeMode.NONE, float fadeTime = 5.0f
			, float maxVolume = 1.0f, float minVolume = 0.0f)
		{
			if (!audioClip)
			{
				Debug.LogError("audioClipがnullです。");
				return false;
			}

			if (bgmSource && bgmSource.isPlaying)
			{
				bgmSource.Stop();
				bgmSource = null;
			}

			bgmSource = Play(gameObject, audioClip, loop, mode, fadeTime, maxVolume, minVolume);
			return true;
		}

		/// <summary>
		/// SEを再生する
		/// </summary>
		/// <param name="target"> SEの発信源</param>
		/// <param name="audioClip"> 音源</param>
		/// <param name="loop"> ループ再生するかどうか</param>
		/// <param name="mode"> フェードの種類</param>
		/// <param name="fadeTime"> フェードにかける時間</param>
		/// <param name="maxVolume"> 最大音量(通常時の音量でもある)</param>
		/// <param name="minVolume"> 最小音量</param>
		public bool PlaySE(GameObject target, AudioClip audioClip, bool loop = false
		, FadeMode mode = FadeMode.NONE, float fadeTime = 5.0f
		, float maxVolume = 1.0f, float minVolume = 0.0f)
		{
			if (!target)
			{
				Debug.LogError("targetがnullです。");
				return false;
			}

			if (!audioClip)
			{
				Debug.LogError("audioClipがnullです。");
				return false;
			}

			AudioSource audioSource = Play(target, audioClip, loop, mode, fadeTime, maxVolume, minVolume);
			foreach (AudioSource seSource in seSources)
			{
				if (seSource == audioSource)
				{
					return true;
				}
			}
			seSources.Add(audioSource);

			return true;
		}

		private AudioSource Play(GameObject target, AudioClip audioClip, bool loop
		, FadeMode mode, float fadeTime
		, float maxVolume, float minVolume)
		{
			AudioSource audioSource = null;
			if (!(audioSource = target.GetComponent<AudioSource>() as AudioSource))
			{
				audioSource = target.AddComponent<AudioSource>();
			}
			else if (audioSource.isPlaying)
			{
				audioSource.Stop();
				iTween.Stop(gameObject);
			}

			if (audioSource.playOnAwake)
			{
				audioSource.playOnAwake = false;
			}

			audioSource.loop = loop;
			audioSource.clip = audioClip;

			if (mode != FadeMode.NONE)
			{
				fadeSource = audioSource;
			}

			switch (mode)
			{
				case FadeMode.NONE:
					audioSource.volume = maxVolume;
					break;

				case FadeMode.FadeIn:
					iTween.ValueTo(gameObject, iTween.Hash("from", minVolume, "to", maxVolume, "time", fadeTime, "onUpdate", "Fade"));
					break;

				case FadeMode.FadeOut:
					iTween.ValueTo(gameObject, iTween.Hash("from", maxVolume, "to", minVolume, "time", fadeTime, "onUpdate", "Fade"));
					break;
			}

			audioSource.Play();
			return audioSource;
		}

		void Fade(float value)
		{
			fadeSource.volume = value;
		}
	}
}
