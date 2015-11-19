using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Kuvo
{
	public class Result : MonoBehaviour
	{
		[SerializeField]
		private GameObject nextButton = null;
		[SerializeField]
		private Text timeField = null;

		private IEnumerator showResult = null;

		int clearSeconds = 120;
		int h, m, s = 0;

		void Start()
		{
			nextButton.SetActive(false);

			s = clearSeconds % 60;
			clearSeconds -= s;
			clearSeconds /= 60;
			m = clearSeconds % 60;
			clearSeconds -= m;
			clearSeconds /= 60;
			h = clearSeconds;

			StartCoroutine(showResult = ShowResult(Resources.Load<GameObject>("Prefabs/Result/Complete") as GameObject, Vector3.zero));
		}
		int score = 0;
		void Update()
		{
			if (Input.anyKeyDown)
			{
				if (nextButton.activeInHierarchy)
				{
					SceneChangerSingleton.Instance.FadeChange(SceneName.title);
				}
				else
				{
					StopCoroutine(showResult);
					StartCoroutine(ShowResult(Resources.Load<GameObject>("Prefabs/Result/Complete") as GameObject, Vector3.zero, 0f));
				}
			}
		}

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

		private IEnumerator Flashing()
		{
			if (!nextButton)
			{
				Debug.LogError("nextButtonがnullです");
				yield break;
			}

			// 一定間隔でゲームオブジェクトを点滅させる
			while (true)
			{
				nextButton.SetActive(!nextButton.activeInHierarchy);
				yield return new WaitForSeconds(1f);
			}
		}
	}
}
