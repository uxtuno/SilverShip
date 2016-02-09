using UnityEngine;

namespace Kuvo
{
	public class AttackArea : MonoBehaviour
	{
		private int power = 1;
		private float magnification = 1.0f;

		public void OnTriggerEnter(Collider other)
		{
			if (other.tag == TagName.Player)
			{
				GameManager.instance.player.Damage(power, magnification);
			}
		}

		public void Set(int power, float magnification)
		{
			this.power = power;
			this.magnification = magnification;
		}
	}
}
