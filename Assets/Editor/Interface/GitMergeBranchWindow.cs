using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GitMergeBranchWindow : EditorWindow
{
	public static GitMergeBranchWindow Instance { get; private set; }

	int fromSelection = 0;
	string[] branches = null;


	public static void Init ()
	{
		// Get existing open window or if none, make a new one:
		Instance = EditorWindow.GetWindow<GitMergeBranchWindow>(true, "Git Merge Branch");

		Instance.branches = GitSystem.GetBranchList(true);
	}


	void OnGUI()
	{
		if ( branches.Length > 0 )
		{
			fromSelection = EditorGUILayout.Popup(fromSelection, branches);

			if ( GUILayout.Button("Merge Branch", GUILayout.MaxWidth(100)) )
			{
				GitSystem.MergeBranch(branches[fromSelection]);
				Close();
			}
		}
		else
			GUILayout.Label("No existing branches to merge...");
	}
}