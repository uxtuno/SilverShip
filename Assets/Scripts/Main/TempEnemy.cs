using UnityEngine;
using System.Collections;
using Kuvo;
using System;

public class TempEnemy : BaseEnemy {
	protected override float sight { get; set; }

	protected override void Start()
	{
		base.Start();
		sight = 2f;
		hp = 1;
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

	public override IEnumerator LongRangeAttack()
	{
		yield break;
	}
}
