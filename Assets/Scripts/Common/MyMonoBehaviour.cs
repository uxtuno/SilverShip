using UnityEngine;
using Uxtuno;

namespace Uxtuno
{
	/// <summary>
	/// MonoBehaviourに独自の機能を追加する
	/// </summary>
	public class MyMonoBehaviour : MonoBehaviour
	{
		private Renderer _renderer; // rendererプロパティの実態

		/// <summary>
		/// 自分自身のRendererを取得する
		/// </summary>
		public new virtual Renderer renderer
		{
			get
			{
				if (_renderer == null)
				{
					_renderer = GetComponent<Renderer>();
				}

				return _renderer;
			}
		}

		private Collider _collider; // colliderプロパティの実態

		/// <summary>
		/// 自分自身のcollideを取得する
		/// </summary>
		public new virtual Collider collider
		{
			get
			{
				if (_collider == null)
				{
					_collider = GetComponent<Collider>();
				}

				return _collider;
			}
		}

		private Rigidbody _rigidbody;   // rigidbodyプロパティの実態

		/// <summary>
		/// 自分自身のrigidbodyを取得する
		/// </summary>
		public new Rigidbody rigidbody
		{
			get
			{
				if (_rigidbody == null)
				{
					_rigidbody = GetComponent<Rigidbody>();
				}

				return _rigidbody;
			}
		}

		/// <summary>
		/// 描画状態
		/// 子も含めた全ての描画状態を変更する
		/// </summary>
		public virtual bool isShow
		{
			// 初めに見つかったRendererの状態を返す
			// そのため全ての子のRenderer.enebledが等しい事を期待する
			get
			{
				return GetComponentInChildren<Renderer>().enabled;
			}

			// 子も含めて全てのRendererの表示状態を変更する
			set
			{
				foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
				{
					renderer.enabled = value;
				}

				foreach (Light light in GetComponentsInChildren<Light>())
				{
					light.enabled = value;
				}
			}
		}
	}
}
