/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SceneChanger.cs"
 * 
 *	This script handles the changing of the scene, and stores
 *	which scene was previously loaded, for use by PlayerStart.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class SceneChanger : MonoBehaviour
	{

		public int previousScene = -1;
		public string previousSceneName = "";

		private SettingsManager settingsManager;


		private void Awake ()
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;
		}
		
		
		public void ChangeScene (string sceneName, int sceneNumber, bool saveRoomData)
		{
			bool useLoadingScreen = false;
			if (settingsManager != null && settingsManager.useLoadingScreen)
			{
				useLoadingScreen = true;
			}

			KickStarter.mainCamera.FadeOut (0f);

			if (KickStarter.player)
			{
				KickStarter.player.Halt ();
			}

			Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
			foreach (Sound sound in sounds)
			{
				if (sound.canDestroy)
				{
					Destroy (sound);
				}
			}

			Dialog dialog = GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>();
			dialog.KillDialog (true);

			LevelStorage levelStorage = this.GetComponent <LevelStorage>();
			if (saveRoomData)
			{
				levelStorage.StoreCurrentLevelData ();
				previousScene = Application.loadedLevel;
				previousSceneName = Application.loadedLevelName;
			}
			
			StateHandler stateHandler = this.GetComponent <StateHandler>();
			stateHandler.gameState = GameState.Normal;
			
			LoadLevel (sceneName, sceneNumber, useLoadingScreen);
		}


		private void LoadLevel (string sceneName, int sceneNumber)
		{
			if (settingsManager != null && settingsManager.useLoadingScreen)
			{
				LoadLevel (sceneName, sceneNumber, true);
			}
			else
			{
				LoadLevel (sceneName, sceneNumber, false);
			}
		}


		private void LoadLevel (string sceneName, int sceneNumber, bool useLoadingScreen)
		{
			if (useLoadingScreen)
			{
				GameObject go = new GameObject ("LevelManager");
				LoadingScreen loadingScreen = go.AddComponent <LoadingScreen>();
				loadingScreen.StartCoroutine (loadingScreen.InnerLoad (sceneName, sceneNumber, AdvGame.GetSceneName (settingsManager.loadingSceneIs, settingsManager.loadingSceneName), settingsManager.loadingScene));
			}
			else
			{
				if (sceneName != "")
				{
					Application.LoadLevel (sceneName);
				}
				else
				{
					Application.LoadLevel (sceneNumber);
				}
			}
		}

	}

}