/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionSceneCheck.cs"
 * 
 *	This action checks the player's last-visited scene,
 *	useful for running specific "player enters the room" cutscenes.
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
	public class ActionSceneCheck : ActionCheck
	{
		
		public enum IntCondition { EqualTo, NotEqualTo };
		public enum SceneToCheck { Current, Previous };
		public ChooseSceneBy chooseSceneBy = ChooseSceneBy.Number;
		public SceneToCheck sceneToCheck = SceneToCheck.Current;
		public int sceneNumber;
		public string sceneName;
		public IntCondition intCondition;


		public ActionSceneCheck ()
		{
			this.isDisplayed = true;
			title = "Engine: Check scene";
		}

		
		override public bool CheckCondition ()
		{
			int actualSceneNumber = 0;
			string actualSceneName = "";

			if (sceneToCheck == SceneToCheck.Previous)
			{
				SceneChanger sceneChanger = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SceneChanger>();
				actualSceneNumber = sceneChanger.previousScene;
				actualSceneName = sceneChanger.previousSceneName;
			}
			else
			{
				actualSceneNumber = Application.loadedLevel;
				actualSceneName = Application.loadedLevelName;
			}

			if (intCondition == IntCondition.EqualTo)
			{
				if (chooseSceneBy == ChooseSceneBy.Name && actualSceneName == sceneName)
				{
					return true;
				}

				if (chooseSceneBy == ChooseSceneBy.Number && actualSceneNumber == sceneNumber)
				{
					return true;
				}
			}
			
			else if (intCondition == IntCondition.NotEqualTo)
			{
				if (chooseSceneBy == ChooseSceneBy.Name && actualSceneName != sceneName)
				{
					return true;
				}

				if (chooseSceneBy == ChooseSceneBy.Number && actualSceneNumber != sceneNumber)
				{
					return true;
				}
			}
			
			return false;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI ()
		{
			chooseSceneBy = (ChooseSceneBy) EditorGUILayout.EnumPopup ("Choose scene by:", chooseSceneBy);

			EditorGUILayout.BeginHorizontal();
				sceneToCheck = (SceneToCheck) EditorGUILayout.EnumPopup (sceneToCheck);
				if (chooseSceneBy == ChooseSceneBy.Name)
				{
					EditorGUILayout.LabelField ("scene name is:", GUILayout.Width (100f));
					intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
					sceneName = EditorGUILayout.TextField (sceneName);
				}
				else
				{
					EditorGUILayout.LabelField ("scene number is:", GUILayout.Width (100f));
					intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
					sceneNumber = EditorGUILayout.IntField (sceneNumber);
				}
			EditorGUILayout.EndHorizontal();
		}

		#endif

	}

}