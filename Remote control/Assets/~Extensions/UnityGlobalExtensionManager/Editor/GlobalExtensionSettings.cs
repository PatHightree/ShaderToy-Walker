using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[Serializable]
public class GlobalExtensionSettings : ScriptableObject
{
	[SerializeField] public bool PreferSymlink;
	[SerializeField] public List<ExtensionSettings> Assets;

	public ExtensionSettings this[string pName]
	{
		get
		{
			foreach (ExtensionSettings asset in Assets)
				if (asset.Name == pName)
					return asset;

			ExtensionSettings newExtension = new ExtensionSettings() {Name = pName};
			Assets.Add(newExtension);
			return newExtension;
		}
	}

	[NonSerialized]
	private static GlobalExtensionSettings _settings;
	public static GlobalExtensionSettings Instance
	{
		get
		{
			string path = "Assets/" + GlobalExtensionManager.AssetFolder + "/settings.asset";

            if( _settings == null )
                _settings = (GlobalExtensionSettings)AssetDatabase.LoadAssetAtPath( path, typeof( GlobalExtensionSettings ) );

            if( _settings == null )
            {
                string folder = Path.Combine( Application.dataPath, GlobalExtensionManager.AssetFolder ).Replace( '/', Path.DirectorySeparatorChar );

                if( Directory.Exists( folder ) )
                {
                    _settings = ScriptableObject.CreateInstance<GlobalExtensionSettings>();
                    AssetDatabase.CreateAsset( _settings, path );
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            if( _settings != null )
			    return _settings;

            // temporarily use fake setting
            return ScriptableObject.CreateInstance<GlobalExtensionSettings>();
		}
	}

	public static void Save()
	{
		EditorUtility.SetDirty(Instance);
		AssetDatabase.SaveAssets();
	}

	[Serializable]
	public class ExtensionSettings
	{
		public string Name;
		public bool PreferSymlink;
	    public bool LocalIsMaster;

		public ExtensionSettings()
		{
			PreferSymlink = Instance.PreferSymlink;
		}
	}
}

