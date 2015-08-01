/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionHotspotEnable.cs"
 * 
 *	This Action can enable and disable a Hotspot.
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
	public class ActionHotspotEnable : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public Hotspot hotspot;

		public ChangeType changeType = ChangeType.Enable;

		
		public ActionHotspotEnable ()
		{
			this.isDisplayed = true;
			title = "Hotspot: Enable or disable";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			hotspot = AssignFile <Hotspot> (parameters, parameterID, constantID, hotspot);
		}

		
		override public float Run ()
		{
			if (hotspot == null)
			{
				return 0f;
			}

			if (changeType == ChangeType.Enable)
			{
				hotspot.TurnOn ();
			}
			else
			{
				hotspot.TurnOff ();
			}

			return 0f;
		}

		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Hotspot to affect:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				hotspot = null;
			}
			else
			{
				hotspot = (Hotspot) EditorGUILayout.ObjectField ("Hotspot to affect:", hotspot, typeof (Hotspot), true);
				
				constantID = FieldToID <Hotspot> (hotspot, constantID);
				hotspot = IDToField <Hotspot> (hotspot, constantID, false);
			}

			changeType = (ChangeType) EditorGUILayout.EnumPopup ("Change to make:", changeType);

			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			string labelAdd = "";
			if (hotspot != null)
			{
				labelAdd = " (" + hotspot.name + " - " + changeType + ")";
			}
			return labelAdd;
		}
		
		#endif
		
	}

}