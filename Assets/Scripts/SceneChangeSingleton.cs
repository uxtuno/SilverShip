using UnityEngine;
using UnityEngine.UI;

namespace Kuvo
{
	/// <summary>
	/// グラフィカルなシーン切り替えを行いたいクラス
	/// </summary>
	public class SceneChangerSingleton : MonoBehaviour
	{
		#region - シングルトンを実現させるための処理 -
		// 唯一のインスタンス
		private static SceneChangerSingleton _instance;

		/// <summary>
		/// プライベートコンストラクタ―
		/// </summary>
		private SceneChangerSingleton()
		{
		}

		public static SceneChangerSingleton instance
		{
			get
			{
				if (!_instance)
				{
					if (!(_instance = FindObjectOfType<SceneChangerSingleton>()))
					{
						GameObject go = new GameObject("SceneChangerSingleton");
						_instance = go.AddComponent<SceneChangerSingleton>();
					}
				}

				return _instance;
			}
		}
		#endregion

		private enum FadeState
		{
			NONE,
			FadeIn,
			FadeOut,
		}

		private static readonly int forGround = 30000;

		private string sceneName { get; set; }
		private float fadeTime { get; set; }
		private GameObject canvasObject { get; set; }
		private Image maskImage { get; set; }
		private FadeState fadeState { get; set; }

		private void Awake()
		{
			// 複数生成の禁止
			if (this != instance)
			{
				Destroy(gameObject);
			}

			DontDestroyOnLoad(this);
			sceneName = string.Empty;
			fadeTime = float.NaN;
			fadeState = FadeState.NONE;
			maskImage = null;
		}

		public void OnLevelWasLoaded(int level)
		{
			if (fadeState != FadeState.FadeIn)
			{
				return;
			}

			if (fadeTime == float.NaN)
			{
				Debug.LogWarning("fadeTimeの値が正常に与えられていません\n初期値の1.0fを使用します");
				fadeTime = 1.0f;
			}

			maskImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

			iTween.ValueTo(gameObject, iTween.Hash("from", 1f, "to", 0f, "time", fadeTime, "onupdate", "FadeAlpha"));
		}

		/// <summary>
		/// フェードアウト⇒フェードインでシーンを切り替える
		/// </summary>
		public void FadeChange(string sceneName, float fadeTime = 1.0f)
		{
			if (fadeState != FadeState.NONE)
			{
				Debug.LogError("シーン遷移実行中に不正にシーンを切り替えようとしました");
				return;
			}

			this.sceneName = sceneName;
			this.fadeTime = fadeTime;

			// Canvasを生成
			Canvas canvas;
			canvasObject = new GameObject();
			canvasObject.name = "FadeMaskCanvas";
			canvas = canvasObject.AddComponent<Canvas>();
			canvas.pixelPerfect = true;
			canvas.renderMode = RenderMode.ScreenSpaceCamera;
			canvas.sortingOrder = forGround;
			canvas.gameObject.AddComponent<CanvasScaler>();

			// 最前面に表示するマスクを生成
			GameObject imageObject = new GameObject();
			imageObject.name = "MaskImage";
			imageObject.transform.SetParent(canvas.transform);
			imageObject.GetSafeComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
			imageObject.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
			maskImage = imageObject.AddComponent<Image>();
			maskImage.sprite = Resources.Load<Sprite>("Sprites/FadeFilter");
			maskImage.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
			fadeState = FadeState.FadeOut;

			iTween.ValueTo(gameObject, iTween.Hash("from", 0.0f, "to", 1.0f, "time", this.fadeTime, "onupdate", "FadeAlpha"));
		}

		void FadeAlpha(float alpha)
		{
			maskImage.color = new Color(0.0f, 0.0f, 0.0f, alpha);
			if (alpha >= 1.0f && fadeState == FadeState.FadeOut)
			{
				fadeState = FadeState.FadeIn;
				DontDestroyOnLoad(canvasObject);
				Application.LoadLevel(sceneName);
				return;
			}

			if (alpha <= 0.0f && fadeState == FadeState.FadeIn)
			{
				fadeState = FadeState.NONE;
				Destroy(canvasObject.gameObject);
			}
		}
	}
}
