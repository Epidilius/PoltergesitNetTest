using UnityEngine;

namespace hebertsystems.AVK
{
	//  The base class for all Turrets.  Provides an abstraction layer
	//  between the turrets and player input allowing for a
	//  modular system where turrets can be swapped in and out with ease.
	//
	public abstract class Turret : MonoBehaviour
	{
		public abstract TurretInput input  { set; }
	}
}
