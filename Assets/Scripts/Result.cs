using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kuvo
{
	public class Result : MonoBehaviour
	{
		// 結果表示エリア
		// Textコンポーネントを持つValueという名前の子が必要
		[SerializeField]
		private GameObject score = null;
		[SerializeField]
		private GameObject timeLeft = null;
		[SerializeField]
		private GameObject clearHP = null;
		[SerializeField]
		private GameObject evaluationSymbol = null;
		[SerializeField]
		private GameObject totalScore = null;

		// 結果を表示するコンポーネント
		private Text scoreValue;
		private Text timeLeftValue;
		private Text clearHPValue;
		private Text totalScoreValue;
		private Text evaluationSymbolValue;

		// 項目ごとの情報
		private ResultItem scoreItem;
		private ResultItem timeLeftItem;
		private ResultItem clearHPItem;

		// フォーマット文字列
		private static readonly string scoreFormat = "{0:#,0}";
		private static readonly string timeFormat = "{0:0:00:00}";
		private static readonly string hpFormat = "{0}";

		// それぞれの項目に乗算する倍率
		private static readonly int scoreMagnification = 1;
		private static readonly int timeLeftMagnification = 100;
		private static readonly int clearHPMagnification = 100;

		[SerializeField, Tooltip("次のシーンへ移行するためのボタン")]
		private GameObject nextButton;
		private GameData data;

		private static readonly string valueString = "Value"; // 名前で子を探すための定数
		private static readonly float waitSeconds = 1.0f; // 項目表示後の待機時間
		private static readonly float showCompleteSeconds = 1.0f; // 項目の表示完了までにかかる時間

		private enum Evaluation
		{
			None,
			S,
			A,
			B,
			C
		}

		// それぞれの評価の得点範囲
		private Range<int> evaluationRangeS = new Range<int>(160000, int.MaxValue);
		private Range<int> evaluationRangeA = new Range<int>(100000, 159999);
		private Range<int> evaluationRangeB = new Range<int>(60000, 99999);
		private Range<int> evaluationRangeC = new Range<int>(0, 59999);

		void Start()
		{
			scoreValue = score.transform.FindDeepGameObject(valueString).GetSafeComponent<Text>();
			timeLeftValue = timeLeft.transform.FindDeepGameObject(valueString).GetSafeComponent<Text>();
			clearHPValue = clearHP.transform.FindDeepGameObject(valueString).GetSafeComponent<Text>();
			totalScoreValue = totalScore.transform.FindDeepGameObject(valueString).GetSafeComponent<Text>();
			evaluationSymbolValue = evaluationSymbol.transform.FindDeepGameObject(valueString).GetSafeComponent<Text>();

			score.SetActive(false);
			timeLeft.SetActive(false);
			clearHP.SetActive(false);
			evaluationSymbol.SetActive(false);

			data = GameManager.instance.GetGameData();

			scoreItem = new ResultItem(score, scoreValue, data.score, scoreFormat, scoreMagnification);
			timeLeftItem = new ResultItem(timeLeft, timeLeftValue, (int)data.timeLeft, timeFormat, timeLeftMagnification);
			clearHPItem = new ResultItem(clearHP, clearHPValue, data.clearHP, hpFormat, clearHPMagnification);

			StartCoroutine(ShowResult());
		}

		void Update()
		{
			// いずれかのキー入力を検知
			if (Input.anyKeyDown)
			{
				if (nextButton.activeInHierarchy)
				{
					SceneChangerSingleton.instance.FadeChange(SceneName.title);
				}
			}
		}

		/// <summary>
		/// リザルト画面を表示
		/// </summary>
		/// <returns></returns>
		private IEnumerator ShowResult()
		{
			nextButton.SetActive(false);
			yield return new WaitForSeconds(1.0f);
			yield return StartCoroutine(ShowResultItem(scoreItem));
			yield return StartCoroutine(ShowResultItem(timeLeftItem));
			yield return StartCoroutine(ShowResultItem(clearHPItem));
			Evaluation eval = CheckEvaluation(totalScoreUpdate);
			string evalText = "F";
			switch (eval)
			{
				case Evaluation.S:
					evalText = "S";
					break;
				case Evaluation.A:
					evalText = "A";
					break;
				case Evaluation.B:
					evalText = "B";
					break;
				case Evaluation.C:
					evalText = "C";
					break;
			}

			evaluationSymbolValue.text = evalText;
			evaluationSymbol.SetActive(true);
			yield return StartCoroutine(ShowEvaluation());

			nextButton.SetActive(true);
		}

		private IEnumerator ShowEvaluation()
		{
			float scale = 0.7f;
			float scalingSpeed = 4.0f;
			while (scale < 1.0f)
			{
				evaluationSymbol.transform.localScale = new Vector3(scale, scale, scale);
				scale += scalingSpeed * Time.deltaTime;
				yield return new WaitForSeconds(Time.deltaTime);
			}
			evaluationSymbol.transform.localScale = Vector3.one;
		}

		/// <summary>
		/// スコアに対する評価を返す
		/// </summary>
		/// <param name="score"></param>
		/// <returns></returns>
		private Evaluation CheckEvaluation(int score)
		{
			if (evaluationRangeS.IsInRange(score))
				return Evaluation.S;
			if (evaluationRangeA.IsInRange(score))
				return Evaluation.A;
			if (evaluationRangeB.IsInRange(score))
				return Evaluation.B;

			return Evaluation.C;
		}

		private void TotalScoreUpdate(float value)
		{
			// 誤差を軽減するため、別に加算したものをキャストしている
			totalScoreValue.text = string.Format(scoreFormat, (int)value);
		}

		private int totalScoreUpdate = 0;
		/// <summary>
		/// 項目を一つ表示
		/// </summary>
		/// <param name="item">項目</param>
		/// <param name="textField">表示エリア</param>
		/// <param name="waitSconds">表示直後の待機時間</param>
		/// <param name="value">値</param>
		/// <param name="format">値を表示する際のフォーマット文字列(string.Formatに準拠)</param>
		/// <param name="magnification">倍率</param>
		/// <returns></returns>
		private IEnumerator ShowResultItem(ResultItem item)
		{
			item.item.SetActive(true);
			item.textField.text = string.Format(item.format, 0);

			// 1秒あたりの増加量
			float incrementValue = (item.value / showCompleteSeconds);
			float showValue = 0.0f;
			float total = (float)totalScoreUpdate;
			while (showValue < item.value)
			{
				item.textField.text = string.Format(item.format, (int)showValue);
				TotalScoreUpdate(total + showValue * item.magnification);
				showValue += incrementValue * Time.deltaTime;
				yield return new WaitForSeconds(Time.deltaTime);
			}
			// 暫定結果
			totalScoreUpdate += item.value * item.magnification;
			TotalScoreUpdate(totalScoreUpdate);
			item.textField.text = string.Format(item.format, item.value);

			yield return new WaitForSeconds(waitSeconds);
		}

		/// <summary>
		/// 一定間隔でゲームオブジェクトを点滅させる
		/// </summary>
		/// <returns></returns>
		private IEnumerator Flashing()
		{
			if (!nextButton)
			{
				Debug.LogError("nextButtonがnullです");
				yield break;
			}

			while (true)
			{
				nextButton.SetActive(!nextButton.activeInHierarchy);
				yield return new WaitForSeconds(1f);
			}
		}

		private class ResultItem
		{
			public ResultItem(GameObject item, Text textField, int value, string format, int magnification)
			{
				_item = item;
				_textField = textField;
				_value = value;
				_format = format;
				_magnification = magnification;
			}

			private readonly GameObject _item;
			public readonly Text _textField;
			public readonly int _value;
			public readonly string _format;
			public readonly int _magnification;

			/// <summary>
			/// 項目オブジェクト
			/// </summary>
			public GameObject item { get { return _item; } }
			/// <summary>
			/// テキスト表示エリア
			/// </summary>
			public Text textField { get { return _textField; } }
			/// <summary>
			/// 数値
			/// </summary>
			public int value { get { return _value; } }
			/// <summary>
			/// フォーマット情報
			/// </summary>
			public string format { get { return _format; } }
			/// <summary>
			/// スコア倍率
			/// </summary>
			public int magnification { get { return _magnification; } }
		}
	}
}
