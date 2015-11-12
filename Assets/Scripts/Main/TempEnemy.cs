using UnityEngine;
using System.Collections;
using Kuvo;


public class TempEnemy : Enemy {
	protected override float sight
	{
		get
		{
			return 2.0f;
		}
		set
		{

		}
	}

	public override IEnumerator ShortRangeAttack()
	{
		yield break;
	}
}
