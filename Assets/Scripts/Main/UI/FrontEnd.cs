using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Uxtuno
{
	// 必須コンポーネント
	[RequireComponent(typeof(Canvas))]

	/// <summary>
	/// ゲーム画面前面に表示するUIを管理する
	/// </summary>
	public class FrontEnd : MyMonoBehaviour
	{
		[SerializeField, Tooltip("残り時間表示用")]
		private Text timeLeft;
		[SerializeField, Tooltip("スコア表示用")]
		private Text score;

		private Canvas canvas;
		void Start()
		{
			canvas = this.GetSafeComponent<Canvas>();
		}

		void Update()
		{
			timeLeft.text = string.Format("{0:000}", (int)GameManager.instance.timeLeft);
			score.text = string.Format("{0:0,000,000}", GameManager.instance.score);
        }
	}
}