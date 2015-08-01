/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"LoadingScreen.cs"
 * 
 *	This script temporarily loads an "in-between" scene that acts
 *	as a loading screen.  Code adapted from work by Robert Utter
 *	at https://chicounity3d.wordpress.com/2014/01/25/loading-screen-tutorial
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class LoadingScreen : MonoBehaviour
	{

		public IEnumerator InnerLoad (string sceneName, int sceneNumber, string loadingSceneName, int loadingScene)
		{
			Object.DontDestroyOnLoad (this.gameObject);
			if (loadingSceneName != "")
			{
				Application.LoadLevel (loadingSceneName);
			}
			else
			{
				Application.LoadLevel (loadingScene);
			}

			yield return null;

			if (sceneName != "")
			{
				Application.LoadLevel (sceneName);
			}
			else
			{
				Application.LoadLevel (sceneNumber);
			}
			Destroy (this.gameObject);
		}

	}

}