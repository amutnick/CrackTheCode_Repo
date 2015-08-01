/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionVarCopy.cs"
 * 
 *	This action is used to transfer the value of one Variable to another
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
	public class ActionVarCopy : Action
	{
		
		public int oldParameterID = -1;
		public int oldVariableID;
		public VariableLocation oldLocation;

		public int newParameterID = -1;
		public int newVariableID;
		public VariableLocation newLocation;

		#if UNITY_EDITOR
		private VariableType oldVarType = VariableType.Boolean;
		private VariableType newVarType = VariableType.Boolean;
		#endif

		private LocalVariables localVariables;
		private VariablesManager variablesManager;


		public ActionVarCopy ()
		{
			this.isDisplayed = true;
			title = "Variable: Copy";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			oldVariableID = AssignVariableID (parameters, oldParameterID, oldVariableID);
			newVariableID = AssignVariableID (parameters, newParameterID, newVariableID);
		}


		override public float Run ()
		{
			if (newVariableID != -1 && oldVariableID != -1)
			{
				GVar oldVar;
				if (oldLocation == VariableLocation.Global)
				{
					oldVar = RuntimeVariables.GetVariable (oldVariableID);
				}
				else
				{
					oldVar = LocalVariables.GetVariable (oldVariableID);
				}

				if (newLocation == VariableLocation.Local && !isAssetFile)
				{
					SetVariable (LocalVariables.GetVariable (newVariableID), VariableLocation.Local, oldVar);
				}
				else
				{
					SetVariable (RuntimeVariables.GetVariable (newVariableID), VariableLocation.Global, oldVar);
				}
			}

			return 0f;
		}

		
		private void SetVariable (GVar newVar, VariableLocation _newLocation, GVar oldVar)
		{
			if (newVar == null || oldVar == null)
			{
				return;
			}

			if (oldLocation == VariableLocation.Global)
			{
				oldVar.Download ();
			}

			if (newVar.type == VariableType.Integer || newVar.type == VariableType.Boolean)
			{
				int oldValue = oldVar.val;
				newVar.SetValue (oldValue, SetVarMethod.SetValue);
			}
			else if (newVar.type == VariableType.Float)
			{
				float oldValue = oldVar.floatVal;
				newVar.SetValue (oldValue, AC.SetVarMethod.SetValue);
			}
			else if (newVar.type == VariableType.String)
			{
				string oldValue = oldVar.textVal;
				newVar.SetValue (oldValue);
			}

			if (_newLocation == VariableLocation.Global)
			{
				newVar.Upload ();
			}

			GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>().VariableChanged ();
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			// OLD

			if (isAssetFile)
			{
				oldLocation = VariableLocation.Global;
			}
			else
			{
				oldLocation = (VariableLocation) EditorGUILayout.EnumPopup ("'From' source:", oldLocation);
			}
			
			if (oldLocation == VariableLocation.Global)
			{
				if (!variablesManager)
				{
					variablesManager = AdvGame.GetReferences ().variablesManager;
				}
				
				if (variablesManager)
				{
					oldVariableID = ShowVarGUI (variablesManager.vars, parameters, ParameterType.GlobalVariable, oldVariableID, oldParameterID, false);
				}
			}
			else if (oldLocation == VariableLocation.Local)
			{
				if (!localVariables && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent<LocalVariables>())
				{
					localVariables = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>();
				}
				
				if (localVariables)
				{
					oldVariableID = ShowVarGUI (localVariables.localVars, parameters, ParameterType.LocalVariable, oldVariableID, oldParameterID, false);
				}
			}

			EditorGUILayout.Space ();

			// NEW

			if (isAssetFile)
			{
				newLocation = VariableLocation.Global;
			}
			else
			{
				newLocation = (VariableLocation) EditorGUILayout.EnumPopup ("'To' source:", newLocation);
			}
			
			if (newLocation == VariableLocation.Global)
			{
				if (!variablesManager)
				{
					variablesManager = AdvGame.GetReferences ().variablesManager;
				}
				
				if (variablesManager)
				{
					newVariableID = ShowVarGUI (variablesManager.vars, parameters, ParameterType.GlobalVariable, newVariableID, newParameterID, true);
				}
			}
			else if (newLocation == VariableLocation.Local)
			{
				if (!localVariables && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent<LocalVariables>())
				{
					localVariables = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>();
				}
				
				if (localVariables)
				{
					newVariableID = ShowVarGUI (localVariables.localVars, parameters, ParameterType.LocalVariable, newVariableID, newParameterID, true);
				}
			}

			// Types match?
			if (oldParameterID == -1 && newParameterID == -1 && newVarType != oldVarType)
			{
				EditorGUILayout.HelpBox ("The chosen Variables do not share the same Type", MessageType.Warning);
			}

			AfterRunningOption ();
		}


		private int ShowVarGUI (List<GVar> vars, List<ActionParameter> parameters, ParameterType parameterType, int variableID, int parameterID, bool isNew)
		{
			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			int i = 0;
			int variableNumber = -1;

			if (vars.Count > 0)
			{
				foreach (GVar _var in vars)
				{
					labelList.Add (_var.label);
					
					// If a GlobalVar variable has been removed, make sure selected variable is still valid
					if (_var.id == variableID)
					{
						variableNumber = i;
					}
					
					i ++;
				}
				
				if (variableNumber == -1 && (parameters == null || parameters.Count == 0 || parameterID == -1))
				{
					// Wasn't found (variable was deleted?), so revert to zero
					Debug.LogWarning ("Previously chosen variable no longer exists!");
					variableNumber = 0;
					variableID = 0;
				}

				string label = "'From' variable:";
				if (isNew)
				{
					label = "'To' variable:";
				}

				parameterID = Action.ChooseParameterGUI (label, parameters, parameterID, parameterType);
				if (parameterID >= 0)
				{
					//variableNumber = 0;
					variableNumber = Mathf.Min (variableNumber, vars.Count-1);
					variableID = -1;
				}
				else
				{
					variableNumber = EditorGUILayout.Popup (label, variableNumber, labelList.ToArray());
					variableID = vars [variableNumber].id;
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("No variables exist!", MessageType.Info);
				variableID = -1;
				variableNumber = -1;
			}

			if (isNew)
			{
				newParameterID = parameterID;

				if (variableNumber >= 0)
				{
					newVarType = vars[variableNumber].type;
				}
			}
			else
			{
				oldParameterID = parameterID;

				if (variableNumber >= 0)
				{
					oldVarType = vars[variableNumber].type;
				}
			}

			return variableID;
		}


		override public string SetLabel ()
		{
			if (newLocation == VariableLocation.Local && !isAssetFile)
			{
				if (!localVariables && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent<LocalVariables>())
				{
					localVariables = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>();
				}
				
				if (localVariables)
				{
					return GetLabelString (localVariables.localVars, newVariableID);
				}
			}
			else
			{
				if (!variablesManager)
				{
					variablesManager = AdvGame.GetReferences ().variablesManager;
				}
				
				if (variablesManager)
				{
					return GetLabelString (variablesManager.vars, newVariableID);
				}
			}
			
			return "";
		}


		private string GetLabelString (List<GVar> vars, int variableNumber)
		{
			string labelAdd = "";

			if (vars.Count > 0 && variableNumber > -1 && vars.Count > variableNumber)
			{
				labelAdd = " (" + vars [variableNumber].label + ")";
			}

			return labelAdd;
		}

		#endif

	}

}