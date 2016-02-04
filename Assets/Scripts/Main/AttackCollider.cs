using UnityEngine;
using System.Collections.Generic;

namespace Uxtuno
{
	[RequireComponent(typeof(Collider))]
	public class AttackCollider : MyMonoBehaviour
	{
		private Collider myCollider;
		[SerializeField]
		private List<string> tags;
		private Actor owner; // オーナー
		public float magnification { get; private set; } // 倍率

		void Start()
		{
			myCollider = GetComponent<Collider>();
			myCollider.enabled = false;
			// 親に一つしかActorがいないこと前提
			owner = GetComponentInParent<Actor>();
			magnification = 1.0f;
		}

		/// <summary>
		/// このColliderを持つActorを設定する
		/// </summary>
		/// <param name="owner"></param>
		public void SetOwner(Actor owner)
		{
			this.owner = owner;
		}

		/// <summary>
		/// あたり判定を発生させる
		/// </summary>
		/// <param name="magnification"></param>
		public void BeginCollision()
		{
			myCollider.enabled = true;
		}

		/// <summary>
		/// あたり判定を消す
		/// </summary>
		public void EndCollision()
		{
			myCollider.enabled = false;
		}

		void OnTriggerEnter(Collider other)
		{
			foreach (var tag in tags)
			{
				if (other.tag == tag)
				{
					other.GetComponent<Actor>().Damage(/*Owner.attack =*/ 10, magnification);
				}
			}
		}
	}
}