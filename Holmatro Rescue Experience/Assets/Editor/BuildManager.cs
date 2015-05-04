using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor {
	public class BuildManager : MonoBehaviour {
		private const string BuildPath = "Builds";
		// PC settings
		private static readonly string[] LevelsPC = new[] { "Assets/+Scenes/Holmatro Rescue Experience.unity" };
		private const string FilenamePC = "Holmatro Rescue Experience.exe";
		// Android settings
		private static readonly string[] LevelsAndroid = new[] { "Assets/+Scenes/Holmatro Rescue Experience.unity" };
		private const string FilenameAndroid = "Holmatro Rescue Experience.apk";

		[MenuItem("Build/Build PC")]
		static void BuildPC() {
			//EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows64);
			string buildPathFilename = Path.Combine(BuildPath, FilenamePC);
			
			BuildPipeline.BuildPlayer(
				LevelsPC,
				buildPathFilename,
				BuildTarget.StandaloneWindows64, 
				BuildOptions.None
			);
		}

		[MenuItem("Build/Build Remote Control")]
		static void BuildRemoteControl() {
			//EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);

			string buildPathFilename = Path.Combine(BuildPath, FilenameAndroid);
			
			BuildPipeline.BuildPlayer(
				LevelsAndroid,
				buildPathFilename,
				BuildTarget.Android, 
				BuildOptions.None
			);
		}
	}
}
