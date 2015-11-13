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

	protected override IEnumerator AirStagger()
	{
		yield break;
	}

	protected override IEnumerator GroundStagger()
	{
		yield break;
	}

	public override IEnumerator ShortRangeAttack()
	{
		yield break;
	}
}
