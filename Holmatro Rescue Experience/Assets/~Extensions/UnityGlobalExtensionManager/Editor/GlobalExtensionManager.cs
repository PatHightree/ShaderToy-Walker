using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GlobalExtensionManager : EditorWindow
{
    public static string GlobalFolder  = "UnityGlobalExtensions";
    public static string AssetFolder   = "~Global";
    public static string ThisExtension = "UnityGlobalExtensionManager";

    [MenuItem( "Window/Global extensions", false )]
    static void ShowWindow()
    {
        GlobalExtensionManager globalExtensionManager = GetWindow<GlobalExtensionManager>();
        globalExtensionManager.title = "Extensions";
        globalExtensionManager.Show();
    }

	private List<Extension> _extensions;	
    private string _globalRoot;
    private string _localRoot;
    private bool _compiling;

	public static GlobalExtensionManager Instance;

	private void OnEnable()
	{
	    Instance = this;
        Refresh();
	}

	private void OnDisable()
	{
		Instance = null;
	}

    private void Update()
    {
        if( _compiling != EditorApplication.isCompiling )
        {
            _compiling = EditorApplication.isCompiling;
            Repaint();
        }
    }

    private void GetPaths()
    {
        System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame( true );

        string projectFolder = Application.dataPath;
        string scriptPath = sf.GetFileName().Substring( projectFolder.Length + 1 ).Replace( '\\', '/' );

        string[] parts = scriptPath.Split( '/' );

        bool root = false;
        for( int i = parts.Length - 1; i > 0; i-- )
        {
            if( parts[i].EndsWith( ".cs" ) ) {}
            else if( parts[ i ].ToLower() == "editor" )
                root = true;
            else if( root )
            {
                AssetFolder = string.Join( "/", parts, 0, i );
                ThisExtension = parts[ i ];
                break;
            }
        }
    }

    public void Refresh()
    {
        GetPaths();

        _extensions = new List<Extension>();

        _globalRoot = System.Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
        _globalRoot = Path.Combine( _globalRoot, GlobalFolder );
        Directory.CreateDirectory( _globalRoot );

        _localRoot = Application.dataPath;
        _localRoot = Path.Combine( _localRoot, AssetFolder );


        // find all subfolders of the global folder

        string[] assetFolders = Directory.GetDirectories( _globalRoot );
        foreach( string assetFolder in assetFolders )
        {
            string assetName = Path.GetFileName( assetFolder );

            if( assetName.ToLower() == ThisExtension.ToLower() )
                continue;

            Extension ext = new Extension(assetName)
            {
                Name = assetName,
                GlobalPath = assetFolder,
                LocalPath = Path.Combine( _localRoot, assetName )
                            .Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar ),
                InGlobal = true
            };

            if( Directory.Exists( ext.LocalPath ) )
            {
                DirectoryInfo d = new DirectoryInfo( ext.LocalPath );

                ext.IsLink = (d.Attributes & FileAttributes.ReparsePoint) != 0;
                ext.IsCopy = !ext.IsLink;
            }

            _extensions.Add( ext );
        }


        // find all subfolders of the local folder which aren't also in the global one

        if( Directory.Exists( _localRoot ) )
        {
            assetFolders = Directory.GetDirectories( _localRoot );
            foreach( string assetFolder in assetFolders )
            {
                string assetName = Path.GetFileName( assetFolder );

                if( assetName.ToLower() == ThisExtension.ToLower() )
                    continue;

                if( !Directory.Exists( Path.Combine( _globalRoot, assetName ) ) )
                {
                    Extension ext = new Extension( assetName )
                        {
                            Name = assetName,
                            GlobalPath = Path.Combine( _globalRoot, assetName )
                                             .Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar ),
                            LocalPath = assetFolder.Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar ),
                            InGlobal = false
                        };

                    _extensions.Add( ext );
                }
            }
        }

        _extensions.Sort(( extLeft, extRight ) => extLeft.Name.CompareTo( extRight.Name ) );
    }

    private void FullRefresh()
    {
        AssetDatabase.Refresh();
        GlobalExtensionSettings.Save();
        Refresh();
    }

    bool IsSymLink( string pFolder )
    {
        DirectoryInfo info = new DirectoryInfo( pFolder );
        return (info.Attributes & FileAttributes.ReparsePoint) != 0;
    }

    private bool StdButton( bool bValue, string pOn, string pTooltipOn, string pOff, string pTooltipOff, GUIStyle pStyle )
    {
        return GUILayout.Toggle( bValue, 
            new GUIContent( 
                bValue ? pOn : pOff, 
                bValue ? pTooltipOn : pTooltipOff ), 
            pStyle,
            GUILayout.Width( 64 ), GUILayout.Height( 20 ) );
    }

    private Vector3 _scroll;
    void OnGUI()
    {
        bool refresh = false;

        GUILayout.BeginArea( new Rect( 0, 0, position.width, position.height - 24 ) );

        _scroll = GUILayout.BeginScrollView( _scroll );

        foreach (Extension ext in _extensions)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label( ext.Name, GUILayout.ExpandWidth( true ) );

            bool disabled = ext.InGlobal && !ext.IsLink && !ext.IsCopy;
            bool newDisabled = disabled;

            string moveToGlobal = !ext.InGlobal ? "copy this item to the global folder and " : "";
            bool newLink = ext.IsLink;
            bool newCopy = ext.IsCopy;

            string copyMode = "";

            if( !ext.IsCopy && (ext.IsLink || ext.Settings.PreferSymlink) )
                copyMode = " (link)";
            else if( ext.IsCopy && ext.Settings.LocalIsMaster )
                copyMode = " (dev)";

            GUI.enabled = !_compiling;

            if( ext.InGlobal )
            {
                newDisabled = StdButton( disabled, 
                    "Off", "This item is not included in this project.",
                    "Off", "Click to " + moveToGlobal + "remove from this project",
                    EditorStyles.miniButtonLeft );

                if( ext.IsLink || ( !ext.IsLink && !ext.IsCopy && ext.Settings.PreferSymlink ) )
                    newLink = StdButton( ext.IsLink,
                                         "On" + copyMode, "This item is included in this project via a symlink to the global folder",
                                         "On" + copyMode, "Click to " + moveToGlobal + "include in this project via a symlink",
                                         EditorStyles.miniButtonMid );
                else if( ext.IsCopy || ( !ext.IsLink && !ext.IsCopy && !ext.Settings.PreferSymlink ) )
                    newCopy = StdButton( ext.IsCopy,
                                         "On" + copyMode, "This item has been copied into this project",
                                         "On" + copyMode, "Click to " + moveToGlobal + "include in this project by making a copy",
                                         EditorStyles.miniButtonMid );

                if( GUILayout.Button(
                        new GUIContent( EditorGUIUtility.FindTexture( "d_icon dropdown" ) ),
                        EditorStyles.miniButtonRight,
                        GUILayout.Width( 18 ), GUILayout.Height( 20 ) ) 
                  )
                {
                    ShowItemMenu( ext );
                }
            }
            else
            {
                if( GUILayout.Button( "Copy to global",
                                      GUILayout.Width( 64 + 64 + 18 ), GUILayout.Height( 20 ) )
                  )
                {
                    ext.CopyToGlobal();
                    refresh = true;
                }
            }

            GUI.enabled = true;
            
            GUILayout.EndHorizontal();

            if( newDisabled != disabled && newDisabled )
            {
                // remove local link/copy

                if( !ext.InGlobal ) ext.CopyToGlobal();
                ext.RemoveLocal();
                refresh = true;
            }
            else if (newLink != ext.IsLink && newLink)
            {
                // create new local link

                if( !ext.InGlobal ) ext.CopyToGlobal();
                ext.RemoveLocal();
                ext.Link();
                refresh = true;
            }
            else if( newCopy != ext.IsCopy && newCopy )
            {
                // create new local copy

                if( !ext.InGlobal ) ext.CopyToGlobal();
                ext.RemoveLocal();
                ext.CopyToLocal();
                refresh = true;
            }

            if( refresh )
                break;
        }

        GUILayout.EndScrollView();

        GUILayout.EndArea();

        Rect r = new Rect( 0, position.height - 24, position.width, 24 );
        GUILayout.BeginArea( r );
        GUILayout.BeginHorizontal();

        GUILayout.Space( 3.0f );

		if (GUILayout.Button(
				new GUIContent("Refresh", "Refresh the global folder\nNote: local folders are automatically refreshed"),
				EditorStyles.miniButton,
				GUILayout.Width(64), GUILayout.Height(20)
			)
		)
			refresh = true;

        GUILayout.FlexibleSpace();

        GUIStyle link = new GUIStyle(EditorStyles.miniButton);
        if( GUILayout.Button( 
                new GUIContent("Open global folder", EditorGUIUtility.FindTexture( "FolderEmpty Icon" ), _globalRoot ), 
                link,
                GUILayout.Height(20)
            ) 
        )
            System.Diagnostics.Process.Start( _globalRoot );


		if (GUILayout.Button(
				new GUIContent(EditorGUIUtility.FindTexture("d_icon dropdown")),
				EditorStyles.miniButton,
				GUILayout.Width(18), GUILayout.Height(20)))
		{
			ShowGlobalMenu();
		}


        GUILayout.Space( 3.0f );

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUI.enabled = true;

        if (refresh)
        {
            AssetDatabase.Refresh();
            Refresh();
            Repaint();
        }
    }

	private void ShowItemMenu(Extension pExt)
	{
		GenericMenu menu =  new GenericMenu();

#pragma warning disable 162
	    if( GlobalExtensionLinkManager.IsSupported )
	        menu.AddItem( new GUIContent( "Settings/Use symlink" ), pExt.Settings.PreferSymlink, OnItemMenuToggleSymlink, pExt );
	    else
            menu.AddItem( new GUIContent( "Settings/Use symlink (not supported)" ), pExt.Settings.PreferSymlink, null );
#pragma warning restore 162

        menu.AddItem( new GUIContent( "Settings/Developer mode (local is master)" ), pExt.Settings.LocalIsMaster, OnItemMenuToggleDeveloper, pExt );

        if( pExt.IsCopy )
        {
            if( !pExt.Settings.LocalIsMaster )
            {
                menu.AddSeparator( "" );
                menu.AddItem( new GUIContent( "Update local copy" ), false, OnItemMenuUpdateLocal, pExt );
            }
            else
            {
                menu.AddSeparator( "" );
                menu.AddItem( new GUIContent( "Update global copy" ), false, OnItemMenuUpdateGlobal, pExt );
            }
        }

        if( pExt.IsLink || pExt.IsCopy )
        {
            menu.AddSeparator( "" );
            menu.AddItem( new GUIContent( "Remove from global folder" ), false, OnItemMenuRemoveFromGlobal, pExt );
        }

	    menu.ShowAsContext();
	}

	private void OnItemMenuToggleSymlink(object pExt)
	{
		Extension ext = (Extension) pExt;

        if( ext.IsLink )
        {
            if( !EditorUtility.DisplayDialog(
                  "Confirm",
                  "Do you really wish to convert this extension to a copy instead of a symlink?",
                  "Yes, convert to a copy",
                  "No"
                ) )
                return;

            ext.RemoveLocal();
            ext.CopyToLocal();
        }
        else if( ext.IsCopy )
        {
            if( !EditorUtility.DisplayDialog(
                  "Confirm",
                  "Do you really wish to convert this extension to a symlink instead of a copy?",
                  "Yes, convert to a symlink",
                  "No"
                ) )
                return;

            ext.RemoveLocal();
            ext.Link();
        }

        ext.Settings.PreferSymlink = !ext.Settings.PreferSymlink;

		GlobalExtensionSettings.Save();
		FullRefresh();
	}

    private void OnItemMenuToggleDeveloper( object pExt )
    {
        Extension ext = (Extension)pExt;
        ext.Settings.LocalIsMaster = !ext.Settings.LocalIsMaster;
        FullRefresh();
    }

    private void OnItemMenuUpdateLocal( object pExt )
    {
        if( !EditorUtility.DisplayDialog(
              "Confirm",
              "Do you really wish to overwrite your local files?",
              "Yes, please update the local folder",
              "No"
            ) )
            return;

        Extension ext = (Extension)pExt;
        ext.RemoveLocal();
        ext.CopyToLocal();
        FullRefresh();
    }

    private void OnItemMenuUpdateGlobal( object pExt )
    {
        if( !EditorUtility.DisplayDialog(
              "Confirm",
              "Do you really wish to overwrite the global files?",
              "Yes, please update the global folder",
              "No"
            ) )
            return;

        Extension ext = (Extension)pExt;
        ext.RemoveGlobal();
        ext.CopyToGlobal();
        FullRefresh();
    }

	private void OnItemMenuRemoveFromGlobal(object pExt)
	{
	    if( !EditorUtility.DisplayDialog(
              "Confirm",
              "Removing this extension from the global folder may break other projects!",
              "Yes, please remove it",
              "No"
            ) ) 
            return;

		Extension ext = (Extension)pExt;

	    if( ext.IsLink )
	    {
	        ext.RemoveLocal();
            ext.CopyToLocal();
	    }

        ext.RemoveGlobal();

        FullRefresh();
    }

	private void ShowGlobalMenu()
	{
		GenericMenu menu = new GenericMenu();

#pragma warning disable 162
        if( GlobalExtensionLinkManager.IsSupported )
            menu.AddItem( new GUIContent( "Default include method/Use symlink" ), GlobalExtensionSettings.Instance.PreferSymlink, OnGlobalMenuToggleSymlink );
        else
            menu.AddItem( new GUIContent( "Default include method/Use symlink (not supported)" ), GlobalExtensionSettings.Instance.PreferSymlink, null );
#pragma warning restore 162

		menu.ShowAsContext();
	}

	private void OnGlobalMenuToggleSymlink()
	{
        GlobalExtensionSettings.Instance.PreferSymlink = !GlobalExtensionSettings.Instance.PreferSymlink;

		foreach (GlobalExtensionSettings.ExtensionSettings asset in GlobalExtensionSettings.Instance.Assets)
            asset.PreferSymlink = GlobalExtensionSettings.Instance.PreferSymlink;

		GlobalExtensionSettings.Save();
		Refresh();
	}

	// from http://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
    private static void DirectoryCopy( string sourceDirName, string destDirName, bool copySubDirs )
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo( sourceDirName );
        DirectoryInfo[] dirs = dir.GetDirectories();

        if( !dir.Exists )
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName );
        }

        // If the destination directory doesn't exist, create it. 
        if( !Directory.Exists( destDirName ) )
        {
            Directory.CreateDirectory( destDirName );
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach( FileInfo file in files )
        {
            string temppath = Path.Combine( destDirName, file.Name );
            file.CopyTo( temppath, false );
        }

        // If copying subdirectories, copy them and their contents to new location. 
        if( copySubDirs )
        {
            foreach( DirectoryInfo subdir in dirs )
            {
                string temppath = Path.Combine( destDirName, subdir.Name );
                DirectoryCopy( subdir.FullName, temppath, copySubDirs );
            }
        }
    }

    private class Extension
    {
        public string Name;
        public string GlobalPath;
        public string LocalPath;
        public bool InGlobal;
        public bool IsLink;
        public bool IsCopy;

	    public GlobalExtensionSettings.ExtensionSettings Settings;

	    public Extension(string pName)
	    {
		    Name = pName;
			Settings = GlobalExtensionSettings.Instance[pName];
	    }

        public void RemoveLocal()
        {
            if( IsLink )
            {
                GlobalExtensionLinkManager.Delete( LocalPath );
                AssetDatabase.Refresh();
            }
            
            if( Directory.Exists( LocalPath ) )
            {
                AssetDatabase.DeleteAsset( "Assets/" + AssetFolder + "/" + Name );
                AssetDatabase.Refresh();
            }
        }

        public void RemoveGlobal()
        {
            Directory.Delete( GlobalPath, true );
        }

        public void CopyToLocal()
        {
            DirectoryCopy( GlobalPath, LocalPath, true );
        }

        public void CopyToGlobal()
        {
            DirectoryCopy( LocalPath, GlobalPath, true );
        }

        public void Link()
        {
            GlobalExtensionLinkManager.Create( LocalPath, GlobalPath, true );
        }
    }
}
