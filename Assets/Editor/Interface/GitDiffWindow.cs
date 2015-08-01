using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

public class GitDiffWindow : EditorWindow
{
	public static GitDiffWindow Instance { get; private set; }

	class DiffData
	{
		public List<string> oursList = new List<string>();
		public List<string> theirsList = new List<string>();
	}
	DiffData diffData = new DiffData();


	public static GitDiffWindow Init (string diffString)
	{
		if ( Instance != null )
			Instance.Close();

		Instance = EditorWindow.GetWindow<GitDiffWindow>(true, "Git Diff");

		Instance.ShowDiff(diffString);

		return Instance;
	}


	Vector2 scrollPos = Vector2.zero;

	void OnGUI()
	{
		GUILayout.BeginHorizontal();
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos);
			{
				for ( int i = 0; i < diffData.oursList.Count; i++ )
				{
					try
					{
						if ( diffData.oursList[i] != diffData.theirsList[i] )
							GUI.contentColor = Color.yellow;
						else
							GUI.contentColor = Color.white;

						GUILayout.Label(diffData.oursList[i]);
					}
					catch
					{
						Debug.Log(i + " : " + diffData.theirsList.Count);
					}
				}
			}
			GUILayout.EndScrollView();

			scrollPos = GUILayout.BeginScrollView(scrollPos);
			{
				GUI.contentColor = Color.red;

				for ( int i = 0; i < diffData.theirsList.Count; i++ )
				{
					if ( diffData.oursList[i] != diffData.theirsList[i] )
						GUI.contentColor = Color.red;
					else
						GUI.contentColor = Color.white;

					GUILayout.Label(diffData.theirsList[i]);
				}
			}
			GUILayout.EndScrollView();
		}
		GUILayout.EndHorizontal();
	}


	void ShowDiff(string diff)
	{
		string[] lines = diff.Split('\n');
		int showingFlag = -1; // -1 = None, 0 = Both, 1 = Ours, 2 = Theirs

		Debug.Log(diff);

		for ( int i = 0; i < lines.Length-1; i++ )
		{
			if ( lines[i].StartsWith("@@@") )
				showingFlag = 0;
			else if ( lines[i].StartsWith("++<<<<<<<") )
				showingFlag = 1;
			else if ( lines[i].StartsWith("++=======") )
				showingFlag = 2;
			else if ( lines[i].StartsWith("++>>>>>>>") )
				showingFlag = 0;
			else if ( showingFlag == 1 )
				diffData.oursList.Add(lines[i].Substring(lines[i].Length - (lines[i].Length-2)));
			else if ( showingFlag == 2 )
				diffData.theirsList.Add(lines[i].Substring(lines[i].Length - (lines[i].Length-2)));
			else if ( showingFlag == 0 )
			{
				diffData.oursList.Add(lines[i].Substring(lines[i].Length - (lines[i].Length-2)));
				diffData.theirsList.Add(lines[i].Substring(lines[i].Length - (lines[i].Length-2)));
			}
		}
	}
}