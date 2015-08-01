using UnityEngine;
using UnityEditor;

using System.Collections;

public class GitMenuCommands : MonoBehaviour
{
	[MenuItem("Git/Init New Repo")]
	static void InitNewRepo()
	{
		GitSystem.InitNewRepo();
	}


	[MenuItem ("Git/Commit &#c")]
	static void GitCommit()
	{
		GitCommitWindow.Init();
	}


	[MenuItem("Git/Quick Commit All %#&c")]
	static void CommitAll()
	{
		GitSystem.CommitAll();
	}


	[MenuItem ("Git/Pull %#&p")]
	static void GitPull()
	{
		GitPullWindow.Init();
	}


	[MenuItem ("Git/Push #&p")]
	static void GitPush()
	{
		GitPushWindow.Init();
	}


	[MenuItem ("Git/Fetch")]
	static void GitFetch()
	{
		GitFetchWindow.Init();
	}


	[MenuItem ("Git/Resolve Conflicts")]
	static void GitResolveConflicts()
	{
		GitConflictsWindow.Init();
	}


	/* **** Branching **** */

	[MenuItem ("Git/Branching/Print Current")]
	static void GitCurrentBranch()
	{
		Debug.Log("Current branch: " + GitSystem.GetCurrentBranch() + "\n");
	}


	[MenuItem ("Git/Branching/Checkout")]
	static void GitCheckoutBranch()
	{
		GitCheckoutBranchWindow.Init();
	}


	[MenuItem ("Git/Branching/Create")]
	static void GitCreateBranch()
	{
		GitCreateBranchWindow.Init();
	}


	[MenuItem ("Git/Branching/Merge")]
	static void GitMergeBranch()
	{
		GitMergeBranchWindow.Init();
	}


	[MenuItem ("Git/Branching/Delete")]
	static void GitDeleteBranch()
	{
		GitDeleteBranchWindow.Init();
	}


	[MenuItem ("Git/GC/Execute")]
	static void GCNormal()
	{
		Debug.Log(GitSystem.RunGitCmd("gc"));
	}


	[MenuItem ("Git/GC/Auto")]
	static void GCAuto()
	{
		Debug.Log(GitSystem.RunGitCmd("gc --auto"));
	}


	[MenuItem ("Git/Cleanup/Untracked")]
	static void CleanupUntracked()
	{
		UnityGitHelper.CleanupUntracked();
	}


	[MenuItem ("Git/Cleanup/Untracked And Ignored")]
	static void CleanupUntrackedAndIgnored()
	{
		Debug.LogWarning("This feature partially implemented, it only cleared untracked, but not ignored files...");
		UnityGitHelper.CleanupUntracked();
	}


	[MenuItem ("Git/Cleanup/Ignored")]
	static void CleanupIgnored()
	{
		Debug.Log(GitSystem.RunGitCmd("clean -d -X -f"));

		Debug.LogWarning("This feature not yet implemented...");
//		UnityGitHelper.UnityCleanupUntracked(GitSystem.GetUntrackedFilesList(false));
	}


	/* **** Standard ignore files **** */

	[MenuItem("Git/Ignore/Create Unity Ignore Files")]
	static void CreateIgnoreFiles()
	{
		UnityGitHelper.CreateUnityGitIgnores();
	}


	/* **** Settings **** */

	// === GC (Garbage Collection) === //

	[MenuItem("Git/Settings/GC/Set gc auto value")]
	static void AutoGCValue ()
	{
		GitValueSettingWindow.Init("GC Auto Value", "gc.auto");
	}


	// === Pack Size Limit === //

	[MenuItem("Git/Settings/Pack/Set pack size limit")]
	static void SetPackSize ()
	{
		GitValueSettingWindow.Init("Pack size limit", "pack.packSizeLimit");
	}


	[MenuItem("Git/Settings/Pack/Get pack size limit")]
	static void GetPackSize ()
	{
		Debug.Log("Pack size limit: " + GitSystem.RunGitCmd ("config pack.packSizeLimit"));
	}


	[MenuItem("Git/Settings/GC/Revert auto value to 6700")]
	static void Auto6700 ()
	{
		GitSystem.RunGitCmd ("config gc.auto 6700");
		Debug.Log ("Auto value set to 6700");
	}


	// === CRLF (Carriage Return / Line Feed) === //

	[MenuItem("Git/Settings/CRLF/Show current mode")]
	static void ShowCRLF ()
	{
		Debug.Log ("Auto CRLF mode: " + GitSystem.RunGitCmd ("config --get core.autocrlf"));
	}


	[MenuItem("Git/Settings/CRLF/Auto CRLF On")]
	static void AutoCRLFOn ()
	{
		GitSystem.RunGitCmd ("config core.autocrlf true");
		ShowCRLF();
	}


	[MenuItem("Git/Settings/CRLF/Auto CRLF Off")]
	static void AutoCRLFOff ()
	{
		GitSystem.RunGitCmd ("config core.autocrlf false");
		ShowCRLF();
	}


	[MenuItem("Git/Settings/CRLF/Auto CRLF On (Global)")]
	static void GlobalAutoCRLFOn ()
	{
		GitSystem.RunGitCmd ("config --global core.autocrlf true");
		ShowCRLF();
	}


	[MenuItem("Git/Settings/CRLF/Auto CRLF Off (Global)")]
	static void GlobalAutoCRLFOff ()
	{
		GitSystem.RunGitCmd ("config --global core.autocrlf false");
		ShowCRLF();
	}


	/* **** Git Help **** */

	[MenuItem("Git/Help")]
	static void ShowGitHelp ()
	{
		GitSystem.RunGitCmd ("help git");
	}
}