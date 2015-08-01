/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberShapeable.cs"
 * 
 *	This script is attached to shapeable scripts in the scene
 *	with shapekey values we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class RememberShapeable : ConstantID
	{
		
		public ShapeableData SaveData ()
		{
			ShapeableData shapeableData = new ShapeableData();
			shapeableData.objectID = constantID;
			
			if (GetComponent <Shapeable>())
			{
				Shapeable shapeable = GetComponent <Shapeable>();
				shapeableData.activeKeyIDs = new List<int>();
				shapeableData.values = new List<float>();
				
				foreach (ShapeGroup shapeGroup in shapeable.shapeGroups)
				{
					shapeableData.activeKeyIDs.Add (shapeGroup.GetActiveKeyID ());
					shapeableData.values.Add (shapeGroup.GetActiveKeyValue ());
				}
			}

			return (shapeableData);
		}


		public void LoadData (ShapeableData data)
		{
			if (GetComponent <Shapeable>())
			{
				Shapeable shapeable = GetComponent <Shapeable>();

				for (int i=0; i<data.activeKeyIDs.Count; i++)
				{
					shapeable.shapeGroups[i].SetActive (data.activeKeyIDs[i], data.values[i], 0f, MoveMethod.Linear, null);
				}
			}
		}
	
	}


	[System.Serializable]
	public class ShapeableData
	{

		public int objectID;
		public List<int> activeKeyIDs;
		public List<float> values;
		
		public ShapeableData () { }
		
	}

}
