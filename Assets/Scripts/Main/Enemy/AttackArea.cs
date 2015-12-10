using UnityEngine;

namespace Kuvo
{
	public class AttackArea : MonoBehaviour
	{
		public int? power { get; set; }
		public float? magnification { get; set; }

		public void OnTriggerEnter(Collider other)
		{
			if (power == null || magnification == null)
			{
				return;
			}
			GameManager.instance.player.Damage((int)power, (float)magnification);
		}

		public void Set(int power, float magnification)
		{
			this.power = power;
			this.magnification = magnification;
		}
	}
}
