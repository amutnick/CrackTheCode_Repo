

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionSetActive : Action
	{

        public GameObject objToAffect;
        public bool turnOn;
        public enum ActiveState { Active, InActive };
        public int parameterID = -1;
        public int constantID = 0;
        public ActiveState activeState = 0;

        public ActionSetActive()
		{
			this.isDisplayed = true;
			title = "Object: SetActive";
		}

        override public void AssignValues(List<ActionParameter> parameters)
        {
            objToAffect = AssignFile(parameters, parameterID, constantID, objToAffect);
        }

		override public float Run ()
		{

            if (activeState == ActiveState.Active)
            {
                objToAffect.SetActive(true);
            }
            else
            {
                objToAffect.SetActive(false);
            }

            return 0f;
		}
		
		
		#if UNITY_EDITOR

        override public void ShowGUI(List<ActionParameter> parameters)
		{
            parameterID = Action.ChooseParameterGUI("Object to affect:", parameters, parameterID, ParameterType.GameObject);
            if (parameterID >= 0)
            {
                constantID = 0;
                objToAffect = null;
            }
            else
            {
                objToAffect = (GameObject)EditorGUILayout.ObjectField("Object to affect:", objToAffect, typeof(GameObject), true);

                constantID = FieldToID(objToAffect, constantID);
                objToAffect = IDToField(objToAffect, constantID, false);
            }

            activeState = (ActiveState)EditorGUILayout.EnumPopup("Active State", activeState);
          
			
			
			AfterRunningOption ();
		}
		

		public override string SetLabel ()
		{
			// Return a string used to describe the specific action's job.
			
			string labelAdd = "";
			return labelAdd;
		}

		#endif
		
	}

}