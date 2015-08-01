using UnityEngine;
using UnityEditor;

using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Debug = UnityEngine.Debug;

public class GitSystem : Editor
{
	public static string currentRemote = "";
	public delegate void CommandOutput(string data, bool isDone);

	public static void InitNewRepo ()
	{
		string repoPath = GetRepoPath ();

		if (repoPath == "")
		{
			repoPath = EditorUtility.OpenFolderPanel ("Choose a repo directory", "", "");
			
			if (repoPath == "" || repoPath == null)
				return;
			
			repoPath = repoPath.Replace (@"\", "/");
			Debug.Log (repoPath);
		}
		else
		{
			Debug.Log ("Repo already exists at: " + repoPath);
			return;
		}
		
		Debug.Log (RunGitCmd ("init " + repoPath));
		UnityGitHelper.CreateUnityGitIgnores();
	}


	static string GetRepoPath ()
	{
		Object selection = Selection.activeObject;
		string startPath = Application.dataPath;
		string selectionPath = Selection.activeObject == null ? "" : AssetDatabase.GetAssetPath(selection).Substring(6);
		string[] locationParts = (startPath + selectionPath).Split ('/');

		for (int o = 0; o < locationParts.Length; o++)
		{
			string tryPath = "";
			
			for (int i = 0; i < locationParts.Length - o; i++)
			{
				tryPath += locationParts[i] + "/";
			}

			if (Directory.Exists (tryPath + ".git"))
			{
				tryPath = tryPath.Remove(tryPath.Length-1);
				return tryPath.Replace (@"\", @"/");
			}
		}

		return "";
	}


	/* **** Commit All **** */

	public static void CommitAll ()
	{
		string[] modifiedFiles = GetModifiedFilesList ();
		string[] untrackedFiles = GetUntrackedFilesList ();
		string[] deletedFiles = GetDeletedFilesList ();
		string commitMessage = "Commit all from Unity:\n\n";

		modifiedFiles = ConformModifyListToDeletionList(modifiedFiles, deletedFiles);

		foreach (string path in modifiedFiles)
		{
			RunGitCmd ("add \"" + path + "\"");
			commitMessage += "modified: " + path + " \n";
		}
		
		foreach (string path in untrackedFiles)
		{
			RunGitCmd ("add \"" + path + "\"");
			commitMessage += "added: " + path + " \n";
		}
		
		foreach (string path in deletedFiles)
		{
			RunGitCmd ("rm \"" + path + "\"");
			commitMessage += "removed: " + path + " \n";
		}
		
		Debug.Log (RunGitCmd ("commit -m \"" + commitMessage + "\""));
		Debug.Log(commitMessage);
	}


	/* **** Commit **** */

	public static void Commit(string commitMessage, string[] addFiles, string[] removeFiles)
	{
		string feedback = "";

		foreach (string path in addFiles)
			RunGitCmd ("add \"" + path + "\"");

		foreach (string path in removeFiles)
			RunGitCmd ("rm \"" + path + "\"");

		feedback = RunGitCmd ("commit -m \"" + commitMessage + "\"");

		if ( feedback.Contains(commitMessage) )
		{
//			Debug.Log(feedback);
		}

		Debug.Log(feedback);
	}


	public static void ResolveConflict(string file, bool useMine)
	{
		string commitMessage = "\"Resolved conflict for file: " + file;

		if ( useMine )
		{
			commitMessage += " using ours.\"";
			RunGitCmd("checkout --ours " + file);
		}
		else
		{
			commitMessage += " using theirs.\"";
			RunGitCmd("checkout --theirs " + file);
		}

		RunGitCmd("add " + file);

		Debug.Log(RunGitCmd("commit -m " + commitMessage));

		AssetDatabase.Refresh();
	}


	/* **** Push **** */

	public static void Push(string remoteName, CommandOutput outputDelegate)
	{
		if ( IsRemoteLocal(remoteName) )
		{
			string feedback = RunGitCmd("push -v --progress --porcelain " + remoteName + " " + GetCurrentBranch(), outputDelegate);

			if ( feedback.Contains("[rejected]") )
				Debug.LogError("Push error: " + feedback + "\n\nTry fetch or pull first.");
		}
		else
			Debug.Log("Sorry, UnityGit can only push to a local git repo for now.");
	}


	/* **** Pull **** */

	public static void Pull(string remoteName)
	{
		Pull(remoteName, null);
	}

	public static void Pull(string remoteName, CommandOutput outputDelegate)
	{
		if ( IsRemoteLocal(remoteName) )
		{
			string feedback = RunGitCmd("pull " + remoteName + " " + GetCurrentBranch(), true, outputDelegate);

			if ( feedback.Contains("Aborting") )
			{
				Debug.LogError(feedback);
				Debug.LogError("Error pulling!");
			}
		}
		else
			Debug.Log("Sorry, UnityGit can only pull from a local git repo for now.");
	}


	public static void PostPull()
	{
		string[] unmergedFiles = GetUnmergedFilesList();

		if ( unmergedFiles.Length > 0 )
			GitConflictsWindow.Init(unmergedFiles);

		AssetDatabase.Refresh();
	}


	/* **** Fetch **** */

	public static void Fetch(string remoteName)
	{
		if ( IsRemoteLocal(remoteName) )
		{
			string feedback = RunGitCmd("fetch --all --verbose --progress " + remoteName);

			if ( feedback.Contains("Aborting") )
			{
				Debug.LogError(feedback);
				Debug.LogError("Error fetching!");
			}
			else
				Debug.Log(feedback);
		}
		else
			Debug.Log("Sorry, UnityGit can only fetch from a local git repo for now.");
	}


	/* **** GetModifiedFilesList **** */

	public static string[] GetModifiedFilesList ()
	{
		return GetModifiedFilesList(true);
	}


	public static string[] GetModifiedFilesList (bool filterUsingSelection)
	{
		string filesString = RunGitCmd ("ls-files --modified --exclude-standard");
		string[] filesList = RemoveEmptyListEntries(filesString);
		
		if ( filterUsingSelection )
			return FilterUsingSelection(filesList);
		else
			return filesList;
	}


	/* **** GetUntrackedFilesList **** */

	public static string[] GetUntrackedFilesList ()
	{
		return GetUntrackedFilesList(true);
	}

	public static string[] GetUntrackedFilesList (bool filterUsingSelection)
	{
		string filesString = RunGitCmd ("ls-files --other --exclude-standard");
		string[] filesList = RemoveEmptyListEntries(filesString);

		if ( filterUsingSelection )
			return FilterUsingSelection(filesList);
		else
			return filesList;
	}


	/* **** GetDeletedFilesList **** */

	public static string[] GetDeletedFilesList ()
	{
		return GetDeletedFilesList(true);
	}

	public static string[] GetDeletedFilesList (bool filterUsingSelection)
	{
		string filesString = RunGitCmd ("ls-files --deleted --exclude-standard");
		string[] filesList = RemoveEmptyListEntries(filesString);
		
		if ( filterUsingSelection )
			return FilterUsingSelection(filesList);
		else
			return filesList;
	}

	/* **** GetUnmergedFilesList **** */

	public static string[] GetUnmergedFilesList ()
	{
		return GetUnmergedFilesList(true);
	}

	public static string[] GetUnmergedFilesList (bool filterUsingSelection)
	{
		string filesString = RunGitCmd ("ls-files --unmerged --exclude-standard");
		string[] filesList = RemoveEmptyListEntries(filesString);
		
		if ( filterUsingSelection )
			return FilterUsingSelection(filesList);
		else
			return filesList;
	}


	/* **** ConformModifyListToDeletionList **** */

	public static string[] ConformModifyListToDeletionList(string[] modifiedFiles, string[] deletedFiles)
	{
		List<string> newModifiedList = new List<string>();

		foreach ( string modFile in modifiedFiles )
		{
			bool addFile = true;

			foreach ( string deletedFile in deletedFiles )
			{
				if ( modFile == deletedFile )
				{
					addFile = false;
					break;
				}
			}

			if ( addFile )
				newModifiedList.Add(modFile);
		}

		return newModifiedList.ToArray();
	}


	/* **** GetRemotesList **** */

	public static string[] GetRemotesList ()
	{
		return RemoveEmptyListEntries(RunGitCmd("remote"));
	}


	/* **** Checks to see if the remote is local or a web address **** */

	public static bool IsRemoteLocal(string remoteName)
	{
		string[] results = RemoveEmptyListEntries(RunGitCmd("remote -v"));
		string[] webUrlTypes = new string[] { "git://", "http://", "https://", "ssh://" };

		foreach ( string result in results )
		{
			string[] splitData = result.Split('\t');

			if ( splitData.Length > 1 )
			{
				string remote = splitData[0];
				string address = splitData[1];

				if ( remote == remoteName )
				{
					bool isURL = false;

					foreach ( string url in webUrlTypes )
					{
						if ( address.StartsWith(url) )
						{
							isURL = true;
							break;
						}
					}

					if ( address.Contains("@") || isURL )
					{
						return false;
					}
				}
			}
		}

		return true;
	}


	/* **** Filters files based on a selected directory **** */

	static string[] FilterUsingSelection(string[] files)
	{
		Object[] objects = Selection.objects;

		if ( Selection.activeObject != null )
		{
			List<string> filteredFiles = new List<string>();

			foreach ( Object obj in objects )
			{
				string baseDirectory = AssetDatabase.GetAssetPath(obj);

				foreach ( string file in files )
				{
					if ( file.StartsWith(baseDirectory) )
					{
						filteredFiles.Add(file);
					}
				}
			}

			return filteredFiles.ToArray();
		}

		return files;
	}


	/* **** Removes any empty strings (typically found at the end of the array) **** */

	static string[] RemoveEmptyListEntries (string listString)
	{
		string[] items = listString.Split ('\n');
		List<string> itemsList = new List<string> ();
		
		for (int i = 0; i < items.Length; i++)
			if (Regex.Replace (items[i], "\\s+", "") != "")
				itemsList.Add (items[i]);
		
		return itemsList.ToArray ();
	}


	/* **** Branching **** */

	public static string GetCurrentBranch()
	{
		string[] branches = RemoveEmptyListEntries (RunGitCmd ("branch"));
		
		foreach (string branch in branches)
		{
			if (branch.Contains ("*"))
			{
				return branch.Replace("* ", "");
			}
		}

		return "";
	}


	public static string[] GetBranchList()
	{
		return GetBranchList(true);
	}


	public static string[] GetBranchList(bool includeCurrent)
	{
		string[] branches = RemoveEmptyListEntries (RunGitCmd ("branch")); // -r will give remote and -a will give both
		List<string> modifiedBranchList = new List<string>();
		string currentBranch = GetCurrentBranch();

		foreach ( string branch in branches )
		{
			string branchName = branch.Replace("*", "");
			bool isCurrent;

			branchName = branchName.Replace(" ", "");

			isCurrent = branchName == currentBranch;

			if ( isCurrent && !includeCurrent )
				continue;
			else
				modifiedBranchList.Add(branchName);
		}

		return modifiedBranchList.ToArray();
	}


	public static void CreateBranch(string branchName)
	{
		CreateBranch(branchName, true);
	}


	public static void CreateBranch(string branchName, bool checkoutAfterCreation)
	{
		if ( !DoesBranchExist(branchName) )
		{
			RunGitCmd("branch " + branchName);

			if ( checkoutAfterCreation )
				CheckoutBranch(branchName);
		}
	}


	public static void CheckoutBranch(string branchName)
	{
		if ( DoesBranchExist(branchName) )
		{
			string result = RunGitCmd("checkout " + branchName);

			if (result.Contains ("Aborting"))
			{
				Debug.LogError ("Branch checkout has been aborted.  Make sure you commit or stash your changes before checking out another branch.");
				return;
			}
		}

		AssetDatabase.Refresh();
	}


	public static void MergeBranch(string branchName)
	{
		Debug.Log(RunGitCmd("merge " + branchName));
		UnityGitHelper.CleanupUntracked();

		AssetDatabase.Refresh();
	}


	public static void DeleteBranch(string branchName, bool mustBeMerged)
	{
		string removeFlag = mustBeMerged ? "-d" : "-D";

		if ( DoesBranchExist(branchName) )
		{
			string result;

			result = RunGitCmd("branch --verbose " + removeFlag + " " + branchName);
			Debug.Log(result);
		}
	}


	public static bool DoesBranchExist(string newBranchName)
	{
		string[] branches = GetBranchList();

		foreach ( string branch in branches )
		{
			if ( branch == newBranchName || branch == "* " + newBranchName )
				return true;
		}

		return false;
	}


	/* **** GC **** */

	public static void GC()
	{
		RunGitCmd("gc");
	}


	/* **** RunGitCmd **** */

	public static string RunGitCmd (string command)
	{
		return RunGitCmd(command, true);
	}


	public static string RunGitCmd (string command, bool includeGitDir)
	{
		return RunGitCmd(command, includeGitDir, null);
	}


	public static string RunGitCmd (string command, CommandOutput outputDelegate)
	{
		return RunGitCmd(command, true, outputDelegate);
	}


	static Process proc = null;
		
	public static string RunGitCmd (string command, bool includeGitDir, CommandOutput outputDelegate)
	{
		string cmd = GetGitExePath();
		string repoPath = GetRepoPath();

		if ( proc != null )
		{
			Debug.LogWarning("You must wait for previous processes to finish!");
			return "";
		}

		if ( cmd != "" )
		{
			ProcessStartInfo startInfo = new ProcessStartInfo (cmd);
			string result;

			proc = new Process();

			if ( includeGitDir )
				command = "--git-dir=\"" + repoPath + "/.git\" --work-tree=\"" + repoPath + "\" " + command;

//			startInfo.Arguments = "cd.. && cd.. && " + command;
			startInfo.Arguments = command;

			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.CreateNoWindow = true;

			proc.StartInfo = startInfo;

			proc.Start ();

			if ( outputDelegate == null )
			{
				StreamReader streamReader = proc.StandardOutput;

				while (!proc.HasExited)
				{
					Thread.Sleep (0);

					result = streamReader.ReadToEnd ();

					proc.Close();
					proc = null;

					return result;
				}
			}
			else
			{
				ThreadPool.QueueUserWorkItem(ThreadedUpdateProcess, outputDelegate);
				return "Threaded Process";
			}
		}

		return "No Git.exe path defined!";
	}


	static void ThreadedUpdateProcess(object outputDelegateObj)
	{
		CommandOutput outputDelegate = (CommandOutput)outputDelegateObj;
		StreamReader streamReader = proc.StandardOutput;
		int count = 0;

		while (!proc.HasExited)
		{
			Thread.Sleep (0);

			count = streamReader.Peek();

			if ( count > 0 )
			{
				char[] bytes = new char[count];
				streamReader.ReadBlock(bytes, 0, count);
				outputDelegate(new string(bytes), false);
			}
		}

		count = streamReader.Peek();

		if ( count > 0 )
		{
			char[] bytes = new char[count];
			streamReader.ReadBlock(bytes, 0, count);
			outputDelegate(new string(bytes), true);
		}
		else
			outputDelegate("", true);

		proc.Close();
		proc = null;
	}


	const string git32 =  @"C:\Program Files\Git\bin\git.exe";
	const string git64 = @"C:\Program Files (x86)\Git\bin\git.exe";

	static string GetGitExePath()
	{
		string locationKey = "GitLocation";
		string location;

		if ( EditorPrefs.HasKey(locationKey) )
		{
			string loc = EditorPrefs.GetString(locationKey);

			if ( File.Exists(loc) )
			{
				return loc;
			}
		}

		if ( File.Exists(git32) )
		{
			EditorPrefs.SetString(locationKey, git64);
			return git64;
		}

		if ( File.Exists(git64) )
		{
			EditorPrefs.SetString(locationKey, git32);
			return git32;
		}

		location = EditorUtility.OpenFilePanel("Where is Git.exe?", "C:\\Program Files\\", "exe");

		if ( File.Exists(location) )
		{
			EditorPrefs.SetString(locationKey, location);
			return location;
		}

		return "";
	}
}