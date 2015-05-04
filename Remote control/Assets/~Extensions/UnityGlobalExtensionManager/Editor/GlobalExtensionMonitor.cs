using UnityEngine;
using UnityEditor;
using System.Collections;

public class GlobalExtensionMonitor : AssetPostprocessor
{

    static void OnPostprocessAllAssets(
        string[] importedAssets,
		string[] deletedAssets,
		string[] movedAssets,
		string[] movedFromAssetPaths
        )
    {
	    if (GlobalExtensionManager.Instance != null)
	    {
		    bool refresh = false;

			foreach (string path in importedAssets)
				refresh |= Check(path);

			foreach (string path in deletedAssets)
				refresh |= Check(path);

			foreach (string path in movedAssets)
				refresh |= Check(path);

			foreach (string path in movedFromAssetPaths)
				refresh |= Check(path);

			if (refresh)
			{
				GlobalExtensionManager.Instance.Refresh();
				GlobalExtensionManager.Instance.Repaint();
			}
	    }
    }

    static bool Check( string pPath )
    {
        return pPath.StartsWith( "Assets/~Global/" );
    }
}
