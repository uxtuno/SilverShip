using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kuvo
{
	public class Result : MonoBehaviour
	{
		[SerializeField]
		private GameObject nextButton = null;   // "次へ"のGameObject
		[SerializeField]
		private Text timeField = null;          // 時間の表示枠
		private IEnumerator showResult = null;  // ShowResult()のコルーチン
		private int h, m, s = 0;                // 時,分,秒
		private int clearSeconds = 120;         // クリアタイム(秒)

		void Start()
		{
			nextButton.SetActive(false);

			// クリアタイムを時,分,秒に換算
			s = clearSeconds % 60;
			clearSeconds -= s;
			clearSeconds /= 60;
			m = clearSeconds % 60;
			clearSeconds -= m;
			clearSeconds /= 60;
			h = clearSeconds;

			StartCoroutine(showResult = ShowResult(Resources.Load<GameObject>("Prefabs/Result/Complete") as GameObject, Vector3.zero));
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
				else
				{
					StopCoroutine(showResult);
					StartCoroutine(ShowResult(Resources.Load<GameObject>("Prefabs/Result/Complete") as GameObject, Vector3.zero, 0f));
				}
			}
		}

		/// <summary>
		/// 結果を表示する
		/// </summary>
		/// <param name="result"> 評価</param>
		/// <param name="position"> 評価の表示位置</param>
		/// <param name="waitTime"> 待機時間</param>
		private IEnumerator ShowResult(GameObject result, Vector3 position, float waitTime = 1f)
		{
			yield return new WaitForSeconds(waitTime);

			if (!GameObject.Find(result.name + "(Clone)"))
			{
				Debug.Log(result.name);
				GameObject obj = Instantiate(result);
				obj.transform.SetParent(FindObjectOfType<Canvas>().transform);
				obj.GetSafeComponent<RectTransform>().anchoredPosition3D = position;
				obj.GetComponent<RectTransform>().localScale = Vector3.one;

				yield return new WaitForSeconds(waitTime);
			}

			if (timeField.text != h.ToString() + ":" + m.ToString().PadLeft(2, '0') + ":" + s.ToString().PadLeft(2, '0'))
			{
				timeField.text = h.ToString() + ":" + m.ToString().PadLeft(2, '0') + ":" + s.ToString().PadLeft(2, '0');

				yield return new WaitForSeconds(waitTime);
			}

			nextButton.SetActive(true);
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
	}
}
