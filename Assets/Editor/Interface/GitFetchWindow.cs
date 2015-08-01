using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

public class GitFetchWindow : EditorWindow
{
	public static GitFetchWindow Instance { get; private set; }

	int remoteSelection = 0;
	string[] remotes;

	public static void Init ()
	{
		// Get existing open window or if none, make a new one:
		Instance = EditorWindow.GetWindow<GitFetchWindow>(true, "Git Fetch");

		Instance.remotes = GitSystem.GetRemotesList();

		for ( int i = 0; i < Instance.remotes.Length; i++ )
		{
			if ( Instance.remotes[i] == GitSystem.currentRemote )
			{
				Instance.remoteSelection = i;
			}
		}
	}


	void OnGUI()
	{
		remoteSelection = EditorGUILayout.Popup(remoteSelection, remotes);
		GitSystem.currentRemote = remotes[remoteSelection];

		if ( GUILayout.Button("Fetch", GUILayout.MaxWidth(100)) )
		{
			GitSystem.Fetch(remotes[remoteSelection]);
			Close();
		}
	}
}