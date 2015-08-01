/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionInventoryCheckSelected.cs"
 * 
 *	This action is used to check the currently-selected item.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionInventoryCheckSelected : ActionCheck
	{
		
		public int parameterID = -1;
		public int invID;
		public bool checkNothing = false;

		private InventoryManager inventoryManager;
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			invID = AssignInvItemID (parameters, parameterID, invID);
		}
		
		
		override public bool CheckCondition ()
		{
			RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();		
			if (runtimeInventory)
			{
				if (checkNothing)
				{
					if (runtimeInventory.selectedItem == null)
					{
						return true;
					}
				}
				else
				{
					if (runtimeInventory.selectedItem != null && runtimeInventory.selectedItem.id == invID)
					{
						return true;
					}
				}
			}
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		public ActionInventoryCheckSelected ()
		{
			this.isDisplayed = true;
			title = "Inventory: Check selected";
		}
		
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			checkNothing = EditorGUILayout.Toggle ("Check for none selected?", checkNothing);
			if (!checkNothing)
			{
				if (!inventoryManager)
				{
					inventoryManager = AdvGame.GetReferences ().inventoryManager;
				}
				
				if (inventoryManager)
				{
					// Create a string List of the field's names (for the PopUp box)
					List<string> labelList = new List<string>();
					
					int i = 0;
					int invNumber = 0;
					if (parameterID == -1)
					{
						invNumber = -1;
					}
					
					if (inventoryManager.items.Count > 0)
					{
						foreach (InvItem _item in inventoryManager.items)
						{
							labelList.Add (_item.label);
							
							// If an item has been removed, make sure selected variable is still valid
							if (_item.id == invID)
							{
								invNumber = i;
							}
							
							i++;
						}
						
						if (invNumber == -1)
						{
							Debug.LogWarning ("Previously chosen item no longer exists!");
							invID = 0;
						}
						
						parameterID = Action.ChooseParameterGUI ("Inventory item:", parameters, parameterID, ParameterType.InventoryItem);
						if (parameterID >= 0)
						{
							invNumber = Mathf.Min (invNumber, inventoryManager.items.Count-1);
							invID = -1;
						}
						else
						{
							invNumber = EditorGUILayout.Popup ("Inventory item:", invNumber, labelList.ToArray());
							invID = inventoryManager.items[invNumber].id;
						}

						AfterRunningOption ();
					}
					else
					{
						EditorGUILayout.HelpBox ("No inventory items exist!", MessageType.Info);
						invID = -1;
					}
				}
			}
		}
		
		
		override public string SetLabel ()
		{
			if (checkNothing)
			{
				return (" (Nothing)");
			}

			if (inventoryManager)
			{
				return (" (" + inventoryManager.GetLabel (invID) + ")");
			}
			return "";
		}
		
		#endif
		
	}
	
}