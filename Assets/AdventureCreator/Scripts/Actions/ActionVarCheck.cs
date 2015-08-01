/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionVarCheck.cs"
 * 
 *	This action checks to see if a Variable has been assigned a certain value,
 *	and performs something accordingly.
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
	public class ActionVarCheck : ActionCheck
	{

		public int parameterID = -1;
		public int variableID;
		public int variableNumber;

		public int checkParameterID = -1;

		public GetVarMethod getVarMethod = GetVarMethod.EnteredValue;
		public int compareVariableID;

		public int intValue;
		public float floatValue;
		public IntCondition intCondition;
		public bool isAdditive = false;
		
		public BoolValue boolValue = BoolValue.True;
		public enum BoolCondition { EqualTo, NotEqualTo };
		public BoolCondition boolCondition;

		public string stringValue;

		public VariableLocation location = VariableLocation.Global;

		private LocalVariables localVariables;
		private VariablesManager variablesManager;
		
		
		public ActionVarCheck ()
		{
			this.isDisplayed = true;
			title = "Variable: Check";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			variableID = AssignVariableID (parameters, parameterID, variableID);

			intValue = AssignInteger (parameters, checkParameterID, intValue);
			boolValue = AssignBoolean (parameters, checkParameterID, boolValue);
			floatValue = AssignFloat (parameters, checkParameterID, floatValue);
			stringValue = AssignString (parameters, checkParameterID, stringValue);
			compareVariableID = AssignVariableID (parameters, checkParameterID, compareVariableID);
		}

		
		override public int End (List<AC.Action> actions)
		{
			if (variableID == -1)
			{
				return 0;
			}

			GVar compareVar = null;

			if (getVarMethod == GetVarMethod.GlobalVariable || getVarMethod == GetVarMethod.LocalVariable)
			{
				if (compareVariableID == -1)
				{
					return 0;
				}

				if (getVarMethod == GetVarMethod.GlobalVariable)
				{
					compareVar = RuntimeVariables.GetVariable (compareVariableID);
					compareVar.Download ();
				}
				else if (getVarMethod == GetVarMethod.LocalVariable && !isAssetFile)
				{
					compareVar = LocalVariables.GetVariable (compareVariableID);
				}
			}

			if (location == VariableLocation.Local && !isAssetFile)
			{
				return ProcessResult (CheckCondition (LocalVariables.GetVariable (variableID), compareVar), actions);
			}

			else
			{
				GVar var = RuntimeVariables.GetVariable (variableID);
				if (var != null)
				{
					var.Download ();
					return ProcessResult (CheckCondition (var, compareVar), actions);
				}
				return -1;
			}
		}
		
		
		private bool CheckCondition (GVar _var, GVar _compareVar)
		{
			if (_compareVar != null && _var != null && _compareVar.type != _var.type)
			{
				Debug.LogWarning ("Cannot compare " + _var.label + " and " + _compareVar.label + " as they are not the same type!");
				return false;
			}

			if (_var.type == VariableType.Boolean)
			{
				int fieldValue = _var.val;
				int compareValue = (int) boolValue;
				if (_compareVar != null)
				{
					compareValue = _compareVar.val;
				}

				if (boolCondition == BoolCondition.EqualTo)
				{
					if (fieldValue == compareValue)
					{
						return true;
					}
				}
				else
				{
					if (fieldValue != compareValue)
					{
						return true;
					}
				}
			}

			else if (_var.type == VariableType.Integer || _var.type == VariableType.PopUp)
			{
				int fieldValue = _var.val;
				int compareValue = intValue;
				if (_compareVar != null)
				{
					compareValue = _compareVar.val;
				}

				if (intCondition == IntCondition.EqualTo)
				{
					if (fieldValue == compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.NotEqualTo)
				{
					if (fieldValue != compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.LessThan)
				{
					if (fieldValue < compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.MoreThan)
				{
					if (fieldValue > compareValue)
					{
						return true;
					}
				}
			}

			else if (_var.type == VariableType.Float)
			{
				float fieldValue = _var.floatVal;
				float compareValue = floatValue;
				if (_compareVar != null)
				{
					compareValue = _compareVar.floatVal;
				}

				if (intCondition == IntCondition.EqualTo)
				{
					if (fieldValue == compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.NotEqualTo)
				{
					if (fieldValue != compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.LessThan)
				{
					if (fieldValue < compareValue)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.MoreThan)
				{
					if (fieldValue > compareValue)
					{
						return true;
					}
				}
			}

			else if (_var.type == VariableType.String)
			{
				string fieldValue = _var.textVal;
				string compareValue = AdvGame.ConvertTokens (stringValue);
				if (_compareVar != null)
				{
					compareValue = _compareVar.textVal;
				}

				if (boolCondition == BoolCondition.EqualTo)
				{
					if (fieldValue == compareValue)
					{
						return true;
					}
				}
				else
				{
					if (fieldValue != compareValue)
					{
						return true;
					}
				}
			}
			
			return false;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (isAssetFile)
			{
				location = VariableLocation.Global;
			}
			else
			{
				location = (VariableLocation) EditorGUILayout.EnumPopup ("Source:", location);
			}

			if (isAssetFile && getVarMethod == GetVarMethod.LocalVariable)
			{
				EditorGUILayout.HelpBox ("Local Variables cannot be referenced by Asset-based Actions.", MessageType.Warning);
			}

			if (location == VariableLocation.Global)
			{
				if (!variablesManager)
				{
					variablesManager = AdvGame.GetReferences ().variablesManager;
				}
				
				if (variablesManager)
				{
					parameterID = Action.ChooseParameterGUI ("Variable:", parameters, parameterID, ParameterType.GlobalVariable);
					if (parameterID >= 0)
					{
						variableID = ShowVarGUI (parameters, variablesManager.vars, variableID, false);
					}
					else
					{
						variableID = ShowVarGUI (parameters, variablesManager.vars, variableID, true);
					}
				}
			}

			else if (location == VariableLocation.Local)
			{
				if (!localVariables && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent<LocalVariables>())
				{
					localVariables = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>();
				}
				
				if (localVariables)
				{
					parameterID = Action.ChooseParameterGUI ("Variable:", parameters, parameterID, ParameterType.LocalVariable);
					if (parameterID >= 0)
					{
						variableID = ShowVarGUI (parameters, localVariables.localVars, variableID, false);
					}
					else
					{
						variableID = ShowVarGUI (parameters, localVariables.localVars, variableID, true);
					}
				}
			}

		}


		private int ShowVarSelectorGUI (List<GVar> vars, int ID)
		{
			variableNumber = -1;
			
			List<string> labelList = new List<string>();
			foreach (GVar _var in vars)
			{
				labelList.Add (_var.label);
			}
			
			variableNumber = GetVarNumber (vars, ID);
			
			if (variableNumber == -1)
			{
				// Wasn't found (variable was deleted?), so revert to zero
				Debug.LogWarning ("Previously chosen variable no longer exists!");
				variableNumber = 0;
				ID = 0;
			}

			variableNumber = EditorGUILayout.Popup ("Variable:", variableNumber, labelList.ToArray());
			ID = vars[variableNumber].id;

			return ID;
		}


		private int ShowVarGUI (List<ActionParameter> parameters, List<GVar> vars, int ID, bool changeID)
		{
			if (vars.Count > 0)
			{
				if (changeID)
				{
					ID = ShowVarSelectorGUI (vars, ID);
				}
				variableNumber = Mathf.Min (variableNumber, vars.Count-1);
				getVarMethod = (GetVarMethod) EditorGUILayout.EnumPopup ("Compare with:", getVarMethod);

				if (parameters == null || parameters.Count == 0)
				{
					EditorGUILayout.BeginHorizontal ();
				}

				if (vars [variableNumber].type == VariableType.Boolean)
				{
					boolCondition = (BoolCondition) EditorGUILayout.EnumPopup (boolCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						checkParameterID = Action.ChooseParameterGUI ("Boolean:", parameters, checkParameterID, ParameterType.Boolean);
						if (checkParameterID < 0)
						{
							boolValue = (BoolValue) EditorGUILayout.EnumPopup ("Boolean:", boolValue);
						}
					}
				}
				else if (vars [variableNumber].type == VariableType.Integer)
				{
					intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						checkParameterID = Action.ChooseParameterGUI ("Integer:", parameters, checkParameterID, ParameterType.Integer);
						if (checkParameterID < 0)
						{
							intValue = EditorGUILayout.IntField ("Integer:", intValue);
						}
					}
				}
				else if (vars [variableNumber].type == VariableType.PopUp)
				{
					intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						checkParameterID = Action.ChooseParameterGUI ("Value:", parameters, checkParameterID, ParameterType.Integer);
						if (checkParameterID < 0)
						{
							intValue = EditorGUILayout.Popup ("Value:", intValue, vars [variableNumber].popUps);
						}
					}
				}
				else if (vars [variableNumber].type == VariableType.Float)
				{
					intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						checkParameterID = Action.ChooseParameterGUI ("Float:", parameters, checkParameterID, ParameterType.Float);
						if (checkParameterID < 0)
						{
							floatValue = EditorGUILayout.FloatField ("Float:", floatValue);
						}
					}
				}
				else if (vars [variableNumber].type == VariableType.String)
				{
					boolCondition = (BoolCondition) EditorGUILayout.EnumPopup (boolCondition);
					if (getVarMethod == GetVarMethod.EnteredValue)
					{
						checkParameterID = Action.ChooseParameterGUI ("String:", parameters, checkParameterID, ParameterType.String);
						if (checkParameterID < 0)
						{
							stringValue = EditorGUILayout.TextField ("String:", stringValue);
						}
					}
				}

				if (getVarMethod == GetVarMethod.GlobalVariable)
				{
					if (!variablesManager)
					{
						variablesManager = AdvGame.GetReferences ().variablesManager;
					}

					if (variablesManager == null || variablesManager.vars == null || variablesManager.vars.Count == 0)
					{
						EditorGUILayout.HelpBox ("No Global variables exist!", MessageType.Info);
					}
					else
					{
						checkParameterID = Action.ChooseParameterGUI ("Global variable:", parameters, checkParameterID, ParameterType.GlobalVariable);
						if (checkParameterID < 0)
						{
							compareVariableID = ShowVarSelectorGUI (variablesManager.vars, compareVariableID);
						}
					}
				}
				else if (getVarMethod == GetVarMethod.LocalVariable)
				{
					if (!localVariables && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent<LocalVariables>())
					{
						localVariables = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>();
					}
					
					if (localVariables == null || localVariables.localVars == null || localVariables.localVars.Count == 0)
					{
						EditorGUILayout.HelpBox ("No Local variables exist!", MessageType.Info);
					}
					else
					{
						checkParameterID = Action.ChooseParameterGUI ("Local variable:", parameters, checkParameterID, ParameterType.LocalVariable);
						if (checkParameterID < 0)
						{
							compareVariableID = ShowVarSelectorGUI (localVariables.localVars, compareVariableID);
						}
					}
				}

				if (parameters == null || parameters.Count == 0)
				{
					EditorGUILayout.EndHorizontal ();
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("No variables exist!", MessageType.Info);
				ID = -1;
				variableNumber = -1;
			}

			return ID;
		}


		override public string SetLabel ()
		{
			if (location == VariableLocation.Local && !isAssetFile)
			{
				if (!localVariables && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent<LocalVariables>())
				{
					localVariables = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>();
				}
				
				if (localVariables)
				{
					return GetLabelString (localVariables.localVars);
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
					return GetLabelString (variablesManager.vars);
				}
			}

			return "";
		}


		private string GetLabelString (List<GVar> vars)
		{
			string labelAdd = "";

			if (vars.Count > 0 && vars.Count > variableNumber && variableNumber > -1)
			{
				labelAdd = " (" + vars[variableNumber].label;
				
				if (vars [variableNumber].type == VariableType.Boolean)
				{
					labelAdd += " " + boolCondition.ToString () + " " + boolValue.ToString ();
				}
				else if (vars [variableNumber].type == VariableType.Integer)
				{
					labelAdd += " " + intCondition.ToString () + " " + intValue.ToString ();
				}
				else if (vars [variableNumber].type == VariableType.Float)
				{
					labelAdd += " " + intCondition.ToString () + " " + floatValue.ToString ();
				}
				else if (vars [variableNumber].type == VariableType.String)
				{
					labelAdd += " " + boolCondition.ToString () + " " + stringValue;
				}
				else if (vars [variableNumber].type == VariableType.PopUp)
				{
					labelAdd += " " + intCondition.ToString () + " " + vars[variableNumber].popUps[intValue];
				}
				
				labelAdd += ")";
			}

			return labelAdd;
		}
		
		#endif


		private int GetVarNumber (List<GVar> vars, int ID)
		{
			int i = 0;
			foreach (GVar _var in vars)
			{
				if (_var.id == ID)
				{
					return i;
				}
				i++;
			}
			return -1;
		}

	}

}