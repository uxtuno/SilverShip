using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Kuvo
{
	[RequireComponent(typeof(BaseEnemyAI))]
	public class TeamAI : BaseAI
	{
		List<BaseEnemyAI> members = new List<BaseEnemyAI>();
		private static readonly float radius = 30.0f;

		protected void Start()
		{
			BaseEnemyAI[] enemies = Physics.OverlapSphere(transform.position, radius)
				.Select((obj) => obj.GetComponent<BaseEnemyAI>())
				.Where((obj) => obj != null && transform != obj.transform)
				.ToArray();

			members.AddRange(enemies);
			print(members.Count);
		}
	}
}
