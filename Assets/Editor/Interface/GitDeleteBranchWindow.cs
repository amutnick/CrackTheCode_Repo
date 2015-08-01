using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GitDeleteBranchWindow : EditorWindow
{
	public static GitDeleteBranchWindow Instance { get; private set; }

	int selection = 0;
	bool deleteOnlyIfMerged = true;
	string[] branches = null;


	public static void Init ()
	{
		// Get existing open window or if none, make a new one:
		Instance = EditorWindow.GetWindow<GitDeleteBranchWindow>(true, "Git Delete Branch");

		Instance.branches = GitSystem.GetBranchList(false);
	}


	void OnGUI()
	{
		if ( branches.Length > 0 )
		{
			selection = EditorGUILayout.Popup(selection, branches);

			if ( GUILayout.Button("Delete Branch", GUILayout.MaxWidth(100)) )
			{
				GitSystem.DeleteBranch(branches[selection], deleteOnlyIfMerged);
				Close();
			}
		}
		else
			GUILayout.Label("No existing branches to delete...");
	}
}