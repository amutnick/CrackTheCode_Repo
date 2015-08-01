/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionScene.cs"
 * 
 *	This action loads a new scene.
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
	public class ActionScene : Action
	{
		
		public ChooseSceneBy chooseSceneBy = ChooseSceneBy.Number;
		public int sceneNumber;
		public string sceneName;
		public bool assignScreenOverlay;
				
		public ActionScene ()
		{
			this.isDisplayed = true;
			title = "Engine: Change scene";
			numSockets = 0;
		}
		
		
		override public float Run ()
		{
			if (!assignScreenOverlay)
			{
				ChangeScene ();
				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;
				KickStarter.mainCamera._ExitSceneWithOverlay ();
				return defaultPauseTime;
			}
			else
			{
				ChangeScene ();
				isRunning = false;
				return 0f;
			}
		}


		private void ChangeScene ()
		{
			if (sceneNumber > -1 || chooseSceneBy == ChooseSceneBy.Name)
			{
				SceneChanger sceneChanger = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SceneChanger>();
				sceneChanger.ChangeScene (AdvGame.GetSceneName (chooseSceneBy, sceneName), sceneNumber, true);
			}
		}


		override public int End (List<AC.Action> actions)
		{
			return -1;
		}
		

		#if UNITY_EDITOR

		override public void ShowGUI ()
		{
			chooseSceneBy = (ChooseSceneBy) EditorGUILayout.EnumPopup ("Choose scene by:", chooseSceneBy);
			if (chooseSceneBy == ChooseSceneBy.Name)
			{
				sceneName = EditorGUILayout.TextField ("Scene name:", sceneName);
			}
			else
			{
				sceneNumber = EditorGUILayout.IntField ("Scene number:", sceneNumber);
			}
			assignScreenOverlay = EditorGUILayout.Toggle ("Overlay current screen?", assignScreenOverlay);
		}
		
		
		override public string SetLabel ()
		{
			if (chooseSceneBy == ChooseSceneBy.Name)
			{
				return (" (" + sceneName + ")");
			}
			return (" (" + sceneNumber + ")");
		}

		#endif
		
	}

}