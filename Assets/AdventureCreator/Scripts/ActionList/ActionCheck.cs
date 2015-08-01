/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCheck.cs"
 * 
 *	This is an intermediate class for "checking" Actions,
 *	that have TRUE and FALSE endings.
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
	public class ActionCheck : Action
	{

		public ResultAction resultActionTrue;
		public int skipActionTrue = -1;
		public AC.Action skipActionTrueActual;
		public Cutscene linkedCutsceneTrue;
		public ActionListAsset linkedAssetTrue;
		
		public ResultAction resultActionFail = ResultAction.Stop;
		public int skipActionFail = -1;
		public AC.Action skipActionFailActual;
		public Cutscene linkedCutsceneFail;
		public ActionListAsset linkedAssetFail;


		public ActionCheck ()
		{
			numSockets = 2;
		}


		public override int End (List<Action> actions)
		{
			return ProcessResult (CheckCondition (), actions);
		}


		protected int ProcessResult (bool result, List<Action> actions)
		{
			if (result)
			{
				if (resultActionTrue == ResultAction.Continue)
				{
					return -3;
				}
				
				else if (resultActionTrue == ResultAction.Stop)
				{
					return -1;
				}
				
				else if (resultActionTrue == ResultAction.Skip)
				{
					int skip = skipActionTrue;
					if (skipActionTrueActual && actions.Contains (skipActionTrueActual))
					{
						skip = actions.IndexOf (skipActionTrueActual);
					}
					else if (skip == -1)
					{
						skip = 0;
					}
					return (skip);
				}
				
				else if (resultActionTrue == ResultAction.RunCutscene)
				{
					if (isAssetFile && linkedAssetTrue != null)
					{
						AdvGame.RunActionListAsset (linkedAssetTrue);
					}
					else if (!isAssetFile && linkedCutsceneTrue != null)
					{
						linkedCutsceneTrue.SendMessage ("Interact");
					}
					return -2;
				}
			}
			else
			{
				if (resultActionFail == ResultAction.Continue)
				{
					return -3;
				}
				
				else if (resultActionFail == ResultAction.Stop)
				{
					return -1;
				}
				
				else if  (resultActionFail == ResultAction.Skip)
				{
					int skip = skipActionFail;
					if (skipActionFailActual && actions.Contains (skipActionFailActual))
					{
						skip = actions.IndexOf (skipActionFailActual);
					}
					else if (skip == -1)
					{
						skip = 0;
					}
					return (skip);						
				}
				
				else if (resultActionFail == ResultAction.RunCutscene)
				{
					if (isAssetFile && linkedAssetFail != null)
					{
						AdvGame.RunActionListAsset (linkedAssetFail);
					}
					else if (!isAssetFile && linkedCutsceneFail != null)
					{
						linkedCutsceneFail.SendMessage ("Interact");
					}
					return -2;
				}
			}

			return 0;
		}


		public virtual bool CheckCondition ()
		{
			return false;
		}


		#if UNITY_EDITOR
		

		override public void SkipActionGUI (List<Action> actions, bool showGUI)
		{
			if (showGUI)
			{
				EditorGUILayout.Space ();
				resultActionTrue = (ResultAction) EditorGUILayout.EnumPopup("If condition is met:", (ResultAction) resultActionTrue);
			}
			if (resultActionTrue == ResultAction.RunCutscene && showGUI)
			{
				if (isAssetFile)
				{
					linkedAssetTrue = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList to run:", linkedAssetTrue, typeof (ActionListAsset), false);
				}
				else
				{
					linkedCutsceneTrue = (Cutscene) EditorGUILayout.ObjectField ("Cutscene to run:", linkedCutsceneTrue, typeof (Cutscene), true);
				}
			}
			else if (resultActionTrue == ResultAction.Skip)
			{
				SkipActionTrueGUI (actions, showGUI);
			}
			
			if (showGUI)
			{
				resultActionFail = (ResultAction) EditorGUILayout.EnumPopup("If condition is not met:", (ResultAction) resultActionFail);
			}
			if (resultActionFail == ResultAction.RunCutscene && showGUI)
			{
				if (isAssetFile)
				{
					linkedAssetFail = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList to run:", linkedAssetFail, typeof (ActionListAsset), false);
				}
				else
				{
					linkedCutsceneFail = (Cutscene) EditorGUILayout.ObjectField ("Cutscene to run:", linkedCutsceneFail, typeof (Cutscene), true);
				}
			}
			else if (resultActionFail == ResultAction.Skip)
			{
				SkipActionFailGUI (actions, showGUI);
			}
		}
		
		
		private void SkipActionTrueGUI (List<Action> actions, bool showGUI)
		{
			if (skipActionTrue == -1)
			{
				// Set default
				int i = actions.IndexOf (this);
				if (actions.Count > i+1)
				{
					skipActionTrue = i+1;
				}
				else
				{
					skipActionTrue = i;
				}
			}

			int tempSkipAction = skipActionTrue;
			List<string> labelList = new List<string>();
			
			if (skipActionTrueActual)
			{
				bool found = false;
				
				for (int i = 0; i < actions.Count; i++)
				{
					labelList.Add (i.ToString () + ": " + actions [i].title);
					
					if (skipActionTrueActual == actions [i])
					{
						skipActionTrue = i;
						found = true;
					}
				}
				
				if (!found)
				{
					skipActionTrue = tempSkipAction;
				}
			}
			
			if (skipActionTrue >= actions.Count)
			{
				skipActionTrue = actions.Count - 1;
			}
			
			if (showGUI)
			{
				if (actions.Count > 1)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField ("  Action to skip to:");
					tempSkipAction = EditorGUILayout.Popup (skipActionTrue, labelList.ToArray());
					skipActionTrue = tempSkipAction;
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					EditorGUILayout.HelpBox ("Cannot skip action - no further Actions available", MessageType.Warning);
					return;
				}
			}
			
			skipActionTrueActual = actions [skipActionTrue];
		}
		
		
		private void SkipActionFailGUI (List<Action> actions, bool showGUI)
		{
			if (skipActionFail == -1)
			{
				// Set default
				int i = actions.IndexOf (this);
				if (actions.Count > i+1)
				{
					skipActionFail = i+1;
				}
				else
				{
					skipActionFail = i;
				}
			}

			int tempSkipAction = skipActionFail;
			List<string> labelList = new List<string>();
			
			if (skipActionFailActual)
			{
				bool found = false;
				
				for (int i = 0; i < actions.Count; i++)
				{
					labelList.Add (i.ToString () + ": " + actions [i].title);
					
					if (skipActionFailActual == actions [i])
					{
						skipActionFail = i;
						found = true;
					}
				}
				
				if (!found)
				{
					skipActionFail = tempSkipAction;
				}
			}
			
			if (skipActionFail >= actions.Count)
			{
				skipActionFail = actions.Count - 1;
			}
			
			if (showGUI)
			{
				if (actions.Count > 1)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField ("  Action to skip to:");
					tempSkipAction = EditorGUILayout.Popup (skipActionFail, labelList.ToArray());
					skipActionFail = tempSkipAction;
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					EditorGUILayout.HelpBox ("Cannot skip action - no further Actions available", MessageType.Warning);
					return;
				}
			}
			
			skipActionFailActual = actions [skipActionFail];
		}


		override public void DrawOutWires (List<Action> actions, int i, int offset)
		{
			if (resultActionTrue == ResultAction.Continue)
			{
				if (actions.Count > i+1)
				{
					AdvGame.DrawNodeCurve (nodeRect, actions[i+1].nodeRect, Color.green, 27 + offset);
				}
			}
			else if (resultActionTrue == ResultAction.Skip)
			{
				if (actions.Contains (skipActionTrueActual))
				{
					AdvGame.DrawNodeCurve (nodeRect, skipActionTrueActual.nodeRect, Color.green, 27 + offset);
				}
			}
			
			if (resultActionFail == ResultAction.Continue)
			{
				if (actions.Count > i+1)
				{
					AdvGame.DrawNodeCurve (nodeRect, actions[i+1].nodeRect, Color.red, 10);
				}
			}
			else if (resultActionFail == ResultAction.Skip)
			{
				if (actions.Contains (skipActionFailActual))
				{
					AdvGame.DrawNodeCurve (nodeRect, skipActionFailActual.nodeRect, Color.red, 10);
				}
			}
		}


		public override void FixLinkAfterDeleting (Action actionToDelete, Action targetAction, List<Action> actionList)
		{
			if ((resultActionFail == ResultAction.Skip && skipActionFailActual == actionToDelete) || (resultActionFail == ResultAction.Continue && actionList.IndexOf (actionToDelete) == (actionList.IndexOf (this) + 1)))
			{
				if (targetAction == null)
				{
					resultActionFail = ResultAction.Stop;
				}
				else
				{
					resultActionFail = ResultAction.Skip;
					skipActionFailActual = targetAction;
				}
			}
			if ((resultActionTrue == ResultAction.Skip && skipActionTrueActual == actionToDelete) || (resultActionTrue == ResultAction.Continue && actionList.IndexOf (actionToDelete) == (actionList.IndexOf (this) + 1)))
			{
				if (targetAction == null)
				{
					resultActionTrue = ResultAction.Stop;
				}
				else
				{
					resultActionTrue = ResultAction.Skip;
					skipActionTrueActual = targetAction;
				}
			}
		}


		public override void PrepareToCopy (int originalIndex, List<Action> actionList)
		{
			if (resultActionFail == ResultAction.Continue)
			{
				if (originalIndex == actionList.Count - 1)
				{
					resultActionFail = ResultAction.Stop;
				}
				else if (actionList [originalIndex + 1].isMarked)
				{
					resultActionFail = ResultAction.Skip;
					skipActionFailActual = actionList [originalIndex + 1];
				}
				else
				{
					resultActionFail = ResultAction.Stop;
				}
			}
			if (resultActionFail == ResultAction.Skip)
			{
				if (skipActionFailActual.isMarked)
				{
					int place = 0;
					foreach (Action _action in actionList)
					{
						if (_action.isMarked)
						{
							if (_action == skipActionFailActual)
							{
								skipActionFailActual = null;
								skipActionFail = place;
								break;
							}
							place ++;
						}
					}
				}
				else
				{
					resultActionFail = ResultAction.Stop;
				}
			}

			if (resultActionTrue == ResultAction.Continue)
			{
				if (originalIndex == actionList.Count - 1)
				{
					resultActionTrue = ResultAction.Stop;
				}
				else if (actionList [originalIndex + 1].isMarked)
				{
					resultActionTrue = ResultAction.Skip;
					skipActionTrueActual = actionList [originalIndex + 1];
				}
				else
				{
					resultActionTrue = ResultAction.Stop;
				}
			}
			if (resultActionTrue == ResultAction.Skip)
			{
				if (skipActionTrueActual.isMarked)
				{
					int place = 0;
					foreach (Action _action in actionList)
					{
						if (_action.isMarked)
						{
							if (_action == skipActionTrueActual)
							{
								skipActionTrueActual = null;
								skipActionTrue = place;
								break;
							}
							place ++;
						}
					}
				}
				else
				{
					resultActionTrue = ResultAction.Stop;
				}
			}
		}


		public override void AfterCopy (List<Action> copyList)
		{
			if (resultActionFail == ResultAction.Skip && skipActionFailActual == null && copyList.Count > skipActionFail)
			{
				skipActionFailActual = copyList[skipActionFail];
			}
			if (resultActionTrue == ResultAction.Skip && skipActionTrueActual == null && copyList.Count > skipActionTrue)
			{
				skipActionTrueActual = copyList[skipActionTrue];
			}
		}
		
		#endif

	}

}