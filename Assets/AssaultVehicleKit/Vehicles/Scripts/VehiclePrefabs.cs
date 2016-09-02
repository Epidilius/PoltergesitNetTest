using UnityEngine;
using System.Collections;

namespace hebertsystems.AVK
{
	//  Base class for containing a list of vehicle prefabs in the scene.
	//  Can be used by other behaviors for selecting player vehicle prefabs
	//  or AI vehicle prefabs shared within the scene.
	//
	public class VehiclePrefabs : MonoBehaviour 
	{
		[SerializeField]
		protected Vehicle[] vehiclePrefabs;							// The list of vehicle prefabs for use in the scene.
		
		public int count	{get {return vehiclePrefabs.Length;} }
		
		public int GetPrefabNameIndex(string prefabName)
		{
			int index = -1;
			
			// Search the array for the name
			for(int i=0; i<vehiclePrefabs.Length; i++)
			{
				// If we have a match, set index and break.
				if(vehiclePrefabs[i].name.Equals(prefabName))
				{
					index = i;
					break;
				}
			}
			
			return index;
		}
		
		public Vehicle GetPrefabAtIndex(int index)
		{
			if(vehiclePrefabs.Length == 0) return null;
			
			index = index % vehiclePrefabs.Length;
			if(index < 0) index = 0;
			
			return vehiclePrefabs[index];
		}
	}
}
