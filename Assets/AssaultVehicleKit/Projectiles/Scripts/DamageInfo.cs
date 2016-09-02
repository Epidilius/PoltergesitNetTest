using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Structure to hold damage info to be used by various behaviors.
	//
	public struct DamageInfo
	{
		public int damage;				// Damage amount.
		public bool critical;			// Whether the damage is a critical hit.
		public Vector3? point;			// The point of the damage (optional).
		public Vector3? normal;			// The normal of the damage (optional).
		public Entity source;			// The Entity source of the damage (optional).

		public DamageInfo(int damage, bool critical, Vector3? point = null, Vector3? normal = null, Entity source = null)
		{
			this.damage = damage;
			this.critical = critical;
			this.point = point;
			this.normal = normal;
			this.source = source;
		}
	}
}
