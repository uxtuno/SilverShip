using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace Uxtuno
{
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(Image))]

	/// <summary>
	/// 対象を追従するアイコン
	/// </summary>
	public class FollowIcon : MyMonoBehaviour
	{

		private Image image; // 自分自身の画像
		[Tooltip("追従対象"), SerializeField]
		private Transform target; // 追従対象

		void Awake()
		{
			// 生成後のタイミングでも使用できるようにAwake()で初期化
			image = this.GetSafeComponent<Image>();

			// 親をキャンバスに
			transform.SetParent(UICanvasGenerator.followIconCanvas.transform, false);
		}

		void LateUpdate()
		{
			if (target != null && isShow)
			{
				// 対象の移動後に位置を反映する必要があるためLateUpdateで処理
				transform.position = Camera.main.WorldToScreenPoint(target.position);
				if (transform.position.z < 0.0f)
				{
					if (image.enabled)
					{
						image.enabled = false;
					}
				}
				else
				{
					if (!image.enabled)
					{
						image.enabled = true;
					}
				}
			}
		}

		/// <summary>
		/// 追従アイコンの設定
		/// </summary>
		/// <param name="target">追従対象</param>
		/// <param name="sprite">表示スプライト</param>
		public void Set(Transform target, Sprite sprite)
		{
			SetTarget(target);
			SetSprite(sprite);
		}

		/// <summary>
		/// 追従対象を設定
		/// </summary>
		/// <param name="target">追従対象</param>
		public void SetTarget(Transform target)
		{
			this.target = target;
		}

		/// <summary>
		/// 表示スプライトを変更
		/// 同時に非表示であっても表示状態にする
		/// </summary>
		/// <param name="sprite">表示スプライト</param>
		public void SetSprite(Sprite sprite)
		{
			image.sprite = sprite;
			isShow = true;
		}

		/// <summary>
		/// 非表示にする
		/// </summary>
		public void Hide()
		{
			isShow = false;
		}

		/// <summary>
		/// 表示する
		/// </summary>
		public void Show()
		{
			isShow = true;
		}

		private bool _isShow = true; // 表示フラグ

		/// <summary>
		/// 表示状態を変更
		/// </summary>
		public override bool isShow
		{
			get
			{
				return _isShow;
			}

			set
			{
				image.enabled = value;
				_isShow = value;
			}
		}
	}
}