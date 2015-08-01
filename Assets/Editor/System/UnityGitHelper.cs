using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections;

public class UnityGitHelper : MonoBehaviour
{
	public static void CleanupUntracked()
	{
		Debug.Log(GitSystem.RunGitCmd("clean -d -f"));

		UnityGitHelper.CleanupUntracked(GitSystem.GetUntrackedFilesList(false));
	}


	static void CleanupUntracked(string[] untrackedFiles)
	{
		foreach ( string path in untrackedFiles )
		{
			Debug.Log(path);
			AssetDatabase.DeleteAsset(path);
		}

		AssetDatabase.Refresh();
	}


	public static void CreateUnityGitIgnores()
	{
		string libraryPath = Application.dataPath + "/../Library/.gitignore";
		string projectPath = Application.dataPath + "/../.gitignore";
		string[] libraryContentsArray =
		{
			"*",
			"!.gitignore",
			"!EditorBuildSettings.asset",
			"!InputManager.asset",
			"!ProjectSettings.asset",
			"!QualitySettings.asset",
			"!TagManager.asset",
			"!TimeManager.asset",
			"!AudioManager.asset",
			"!DynamicsManager.asset",
			"!NetworkManager.asset"
		};
		string[] projectContentsArray =
		{
			"Temp",
			"*.csproj",
			"*.pidb",
			"*.sln",
			"*.userprefs"
		};

		File.WriteAllLines(libraryPath, libraryContentsArray);
		File.WriteAllLines(projectPath, projectContentsArray);
	}
}