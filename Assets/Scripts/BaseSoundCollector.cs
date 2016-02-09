using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーンごとに使用する音楽を管理するクラスは
/// このクラスを継承する
/// </summary>
abstract public class BaseSoundCollector<T> : MonoBehaviour
{
    // 派生クラス側で定義
    //public enum SoundName
    //{
    //    // BGM
    //    // SE
    //}

    protected virtual void Awake()
    {
        // ここで読み込み
        //sounds.Add(SoundName.Title, Resources.Load<AudioClip>("Sounds/BGM"));
        //sounds.Add(SoundName.Explosion, Resources.Load<AudioClip>("Sounds/SE"));
    }

    /// <summary>
    /// サウンド名からAudioClipを取得
    /// </summary>
    public AudioClip this[T index]
    {
        get
        {
            if (!sounds.ContainsKey(index))
            {
                return null;
            }

            return sounds[index];
        }
    }

    protected Dictionary<T, AudioClip> sounds = new Dictionary<T, AudioClip>();
}
