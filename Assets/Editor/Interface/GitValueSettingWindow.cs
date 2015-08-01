using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

public class GitValueSettingWindow : EditorWindow
{
	public static GitValueSettingWindow Instance { get; private set; }

	string label = "";
	string command = "";
	string configValue = "";
	bool unset = false;
	bool isGlobal = false;


	public static void Init (string label, string command)
	{
		// Get existing open window or if none, make a new one:
		Instance = EditorWindow.GetWindow<GitValueSettingWindow>(true, label);

		Instance.label = label;
		Instance.command = command;
	}


	void OnGUI()
	{
		isGlobal = GUILayout.Toggle(isGlobal, "Global");
		unset = GUILayout.Toggle(unset, "Unset value");

		EditorGUILayout.BeginHorizontal();

		if ( !unset )
		{
			GUILayout.Label(label + ": ");
			configValue = EditorGUILayout.TextField(configValue);
		}
		else
			GUILayout.Label("");

		EditorGUILayout.EndHorizontal();

		if ( GUILayout.Button("Okay", GUILayout.MaxWidth(100)) )
		{
			string globalString = isGlobal ? "--global " : "";
			string unsetString = unset ? "--unset " : "";

			GitSystem.RunGitCmd("config " + globalString + unsetString + command + " " + configValue);
			Close();
		}
	}
}