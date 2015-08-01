using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

public class GitCommitWindow : EditorWindow
{
	public static GitCommitWindow Instance { get; private set; }

	string[] modifiedFiles = GitSystem.GetModifiedFilesList();
	bool[] commitModifiedFiles;
	string[] untrackedFiles = GitSystem.GetUntrackedFilesList();
	bool[] commitUntrackedFiles;
	string[] deletedFiles = GitSystem.GetDeletedFilesList();
	bool[] commitDeletedFiles;

	bool somethingToCommit = false;
	string commitMessage = "";


	public static void Init ()
	{
		if ( Instance != null )
			Instance.Close();

		// Get existing open window or if none, make a new one:
		Instance = EditorWindow.GetWindow<GitCommitWindow>(true, "Git Commit");

		Instance.InitFiles(true);
	}


	void InitFiles(bool initSelect)
	{
		int fileCount = 0;

		Instance.modifiedFiles = GitSystem.GetModifiedFilesList();
		Instance.untrackedFiles = GitSystem.GetUntrackedFilesList();
		Instance.deletedFiles = GitSystem.GetDeletedFilesList();

		Instance.commitModifiedFiles = new bool[Instance.modifiedFiles.Length];
		Instance.commitUntrackedFiles = new bool[Instance.untrackedFiles.Length];
		Instance.commitDeletedFiles = new bool[Instance.deletedFiles.Length];

		if ( initSelect )
		{
			for ( int i = 0; i < Instance.commitModifiedFiles.Length; i++ )
				Instance.commitModifiedFiles[i] = true;

			for ( int i = 0; i < Instance.commitDeletedFiles.Length; i++ )
				Instance.commitDeletedFiles[i] = true;
		}

		fileCount = Instance.modifiedFiles.Length;
		fileCount += Instance.untrackedFiles.Length;
		fileCount += Instance.deletedFiles.Length;

		Instance.somethingToCommit = fileCount > 0;
		Instance.modifiedFiles = GitSystem.ConformModifyListToDeletionList(Instance.modifiedFiles, Instance.deletedFiles);
	}


	Vector2 scrollPosition = Vector2.zero;
	int lastSelectedIndex = 0;
	bool lastSelectedValue = true;

	void OnGUI()
	{
		if ( somethingToCommit )
		{
			Color baseContentColor = GUI.contentColor;
			bool shiftDown = Event.current.shift;
			bool valueChanged = false;
			int selectedIndex = 0;

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			// === Modified Files === //

			GUI.contentColor = Color.cyan;
			for ( int i = 0; i < modifiedFiles.Length; i++ )
			{
				bool prevValue = commitModifiedFiles[i];

				commitModifiedFiles[i] = GUILayout.Toggle(commitModifiedFiles[i], modifiedFiles[i]);

				// Was this box just checked?
				if ( prevValue != commitModifiedFiles[i] )
				{
					valueChanged = true;

					if ( shiftDown )
					{
						selectedIndex = i;
					}
					else
					{
						lastSelectedIndex = i;
						lastSelectedValue = commitModifiedFiles[i];
					}
				}
			}

			// === Untracked Files === //

			GUI.contentColor = baseContentColor;
			for ( int i = 0; i < untrackedFiles.Length; i++ )
			{
				bool prevValue = commitUntrackedFiles[i];

				commitUntrackedFiles[i] = GUILayout.Toggle(commitUntrackedFiles[i], untrackedFiles[i]);

				// Was this box just checked?
				if ( prevValue != commitUntrackedFiles[i] )
				{
					valueChanged = true;

					// Should we multi select / deselect?
					if ( shiftDown )
					{
						selectedIndex = i + modifiedFiles.Length;
					}
					else
					{
						lastSelectedIndex = i + modifiedFiles.Length;
						lastSelectedValue = commitUntrackedFiles[i];
					}
				}
			}

			// === Deleted Files === //

			GUI.contentColor = Color.red;
			for ( int i = 0; i < deletedFiles.Length; i++ )
			{
				bool prevValue = commitDeletedFiles[i];

				commitDeletedFiles[i] = GUILayout.Toggle(commitDeletedFiles[i], deletedFiles[i]);

				if ( prevValue != commitDeletedFiles[i] )
				{
					valueChanged = true;

					// Should we multi select / deselect?
					if ( shiftDown )
					{
						selectedIndex = i + modifiedFiles.Length + commitUntrackedFiles.Length;
					}
					else
					{
						lastSelectedIndex = i + modifiedFiles.Length + commitUntrackedFiles.Length;
						lastSelectedValue = commitDeletedFiles[i];
					}
				}
			}
			GUILayout.EndScrollView();

			if ( valueChanged )
			{
				// Change the modified files
				// Should we multi select / deselect?
				if ( shiftDown )
				{
					int start = (lastSelectedIndex < selectedIndex) ? lastSelectedIndex : selectedIndex;
					int end = (lastSelectedIndex < selectedIndex) ? selectedIndex : lastSelectedIndex;

					start = Mathf.Clamp(start, 0, modifiedFiles.Length);
					end = Mathf.Clamp(end, 0, modifiedFiles.Length);

					for ( int cur = start; cur < end; cur++ )
					{
						commitModifiedFiles[cur] = lastSelectedValue;
					}
				}

				// Change the untracked files
				// Should we multi select / deselect?
				if ( shiftDown )
				{
					int start = (lastSelectedIndex < selectedIndex) ? lastSelectedIndex : selectedIndex;
					int end = (lastSelectedIndex < selectedIndex) ? selectedIndex : lastSelectedIndex;

					start -= modifiedFiles.Length;
					end -= modifiedFiles.Length;

					start = Mathf.Clamp(start, 0, commitUntrackedFiles.Length);
					end = Mathf.Clamp(end, 0, commitUntrackedFiles.Length);

					for ( int cur = start; cur < end; cur++ )
					{
						commitUntrackedFiles[cur] = lastSelectedValue;
					}
				}

				// Change the deleted files
				// Should we multi select / deselect?
				if ( shiftDown )
				{
					int start = (lastSelectedIndex < selectedIndex) ? lastSelectedIndex : selectedIndex;
					int end = (lastSelectedIndex < selectedIndex) ? selectedIndex : lastSelectedIndex;

					start -= modifiedFiles.Length + untrackedFiles.Length;
					end -= modifiedFiles.Length + untrackedFiles.Length;

					start = Mathf.Clamp(start, 0, deletedFiles.Length);
					end = Mathf.Clamp(end, 0, deletedFiles.Length);

					for ( int cur = start; cur < end; cur++ )
					{
						commitDeletedFiles[cur] = lastSelectedValue;
					}
				}
			}

			// Select All and None
			GUI.contentColor = baseContentColor;
			GUILayout.BeginHorizontal();
			if ( GUILayout.Button("Select All", GUILayout.MaxWidth(100)) )
			{
				for ( int i = 0; i < commitModifiedFiles.Length; i++ )
					commitModifiedFiles[i] = true;

				for ( int i = 0; i < commitUntrackedFiles.Length; i++ )
					commitUntrackedFiles[i] = true;

				for ( int i = 0; i < commitDeletedFiles.Length; i++ )
					commitDeletedFiles[i] = true;
			}
			else if ( GUILayout.Button("Select None", GUILayout.MaxWidth(100)) )
			{
				for ( int i = 0; i < commitModifiedFiles.Length; i++ )
					commitModifiedFiles[i] = false;

				for ( int i = 0; i < commitUntrackedFiles.Length; i++ )
					commitUntrackedFiles[i] = false;

				for ( int i = 0; i < commitDeletedFiles.Length; i++ )
					commitDeletedFiles[i] = false;
			}
			GUILayout.EndHorizontal();

			GUILayout.Label("");
			GUILayout.Label("Commit message:");
			commitMessage = GUILayout.TextArea(commitMessage, GUILayout.MinHeight(100));
			commitMessage = commitMessage.Replace("\"", "");
			commitMessage = commitMessage.Replace("\'", "");

			// Commit and Cancel
			if ( commitMessage != "" )
			{
				GUILayout.Label("");
				GUILayout.BeginHorizontal();
				if ( GUILayout.Button("Cancel", GUILayout.MaxWidth(100)) )
				{
					Close();
				}
				else if ( GUILayout.Button("Commit", GUILayout.MaxWidth(100)) )
				{
					DoCommit();
					InitFiles(false);
					commitMessage = "";
				}
				GUILayout.EndHorizontal();
			}
		}
		else
		{
			GUILayout.Label("Nothing to commit...");
		}
	}


	void DoCommit()
	{
		List<string> addFiles = new List<string>();
		List<string> removeFiles = new List<string>();

		for ( int i = 0; i < modifiedFiles.Length; i++ )
		{
			if ( commitModifiedFiles[i] )
			{
				addFiles.Add(modifiedFiles[i]);
			}
		}

		for ( int i = 0; i < untrackedFiles.Length; i++ )
		{
			if ( commitUntrackedFiles[i] )
			{
				addFiles.Add(untrackedFiles[i]);
			}
		}

		for ( int i = 0; i < deletedFiles.Length; i++ )
		{
			if ( commitDeletedFiles[i] )
			{
				removeFiles.Add(deletedFiles[i]);
			}
		}

		GitSystem.Commit(commitMessage, addFiles.ToArray(), removeFiles.ToArray());
	}
}