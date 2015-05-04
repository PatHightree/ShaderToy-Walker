using UnityEngine;
using UnityEditor;

//Source from http://answers.unity3d.com/questions/179775/game-window-size-from-editor-window-in-editor-mode.html
//Modified by seieibob for use at the Virtual Environment and Multimodal Interaction Lab at the University of Maine.
//Use however you'd like!

/// <summary>
/// Displays a popup window that undocks, repositions and resizes the game window according to
/// what is specified by the user in the popup. Offsets are applied to ensure screen borders are not shown.
/// </summary>
[InitializeOnLoad]
public class GameWindowMover : EditorWindow {
	
 	//The size of the toolbar above the game view, excluding the OS border.
	private static int tabHeight = 22;
	
	//Get the size of the window borders. Changes depending on the OS.
	#if UNITY_STANDALONE_WIN
	//Windows settings
	private static int osBorderWidth = 5;
	#elif UNITY_STANDALONE_OSX
	//Mac settings (untested)
	private int osBorderWidth = 0; //OSX windows are borderless.
	#else
	//Linux / other platform; sizes change depending on the variant you're running
	private int osBorderWidth = 5;
	#endif
	
	//Desired window resolution
	private static Vector2 gameSize = new Vector2(1280, 800);
	//Desired window position
	private static Vector2 gamePosition = new Vector2(0, 0);
	
	//Tells the script to use the default resolution specified in the player settings.
	private bool usePlayerSettingsResolution = true;
 
	static GameWindowMover() {
	    gameSize = new Vector2(EditorPrefs.GetInt("gameSizeWidth", 1280), EditorPrefs.GetInt("gameSizeHeight", 800));
		gamePosition = new Vector2(EditorPrefs.GetInt("gamePositionX", 0), EditorPrefs.GetInt("gamePositionY", 0));
		//MoveGameWindow();
	}

	//Shows the popup
    [MenuItem ("Window/Set Game Window Position...")]
    static void OpenPopup() {
		// TODO Reading back these values from registry only works when the window is opened for the first time. 
		// TODO When Unity starts with the window open, this function is not called!
	    gameSize = new Vector2(EditorPrefs.GetInt("gameSizeWidth", 1280), EditorPrefs.GetInt("gameSizeHeight", 800));
		gamePosition = new Vector2(EditorPrefs.GetInt("gamePositionX", 0), EditorPrefs.GetInt("gamePositionY", 0));

		GameWindowMover window = (GameWindowMover)(EditorWindow.GetWindow(typeof(GameWindowMover)));
		//Set popup window properties
		Vector2 popupSize = new Vector2(300, 140);
		//When minSize and maxSize are the same, no OS border is applied to the window.
		window.minSize = popupSize;
		window.maxSize = popupSize;
		window.title = "Game Window Mover";
		window.ShowPopup();
    }
 
	//Returns the current game view as an EditorWindow object.
    public static EditorWindow GetMainGameView(){
		//Creates a game window. Only works if there isn't one already.
		EditorApplication.ExecuteMenuItem("Window/Game");
		
		System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
		System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
		System.Object Res = GetMainGameView.Invoke(null,null);
		return (EditorWindow)Res;
    }
 
    void OnGUI(){
		
		EditorGUILayout.Space();
		
		usePlayerSettingsResolution = GUILayout.Toggle(usePlayerSettingsResolution, " Use Player Settings Resolution");
		if(usePlayerSettingsResolution){
			gameSize = new Vector2(PlayerSettings.defaultScreenWidth, PlayerSettings.defaultScreenHeight);
		}
		//Gray out the resolution field if we're overriding with the target resolution.
		GUI.enabled = !usePlayerSettingsResolution;
		//Constrain fields to ints
		Vector2 newGameSize = EditorGUILayout.Vector2Field("Window Size (Pixels)", new Vector2((int)gameSize.x, (int)gameSize.y));
		if(Mathf.Abs(newGameSize.x - gameSize.x) >= 1 || Mathf.Abs(newGameSize.y - gameSize.y) >= 1){
			gameSize = new Vector2((int)newGameSize.x, (int)newGameSize.y);
		}
		GUI.enabled = true;
		
		//Constrain fields to ints
		Vector2 newGamePosition = EditorGUILayout.Vector2Field("Window Position", new Vector2((int)gamePosition.x, (int)gamePosition.y));
		if(Mathf.Abs(newGamePosition.x - gamePosition.x) >= 1 || Mathf.Abs(newGamePosition.y - gamePosition.y) >= 1){
			gamePosition = new Vector2((int)newGamePosition.x, (int)newGamePosition.y);
		}
		
		EditorGUILayout.Space();

		if (GUILayout.Button("Apply")) {
			MoveGameWindow();
			EditorPrefs.SetInt("gameSizeWidth", (int)gameSize.x);
			EditorPrefs.SetInt("gameSizeHeight", (int)gameSize.y);
			EditorPrefs.SetInt("gamePositionX", (int)gamePosition.x);
			EditorPrefs.SetInt("gamePositionY", (int)gamePosition.y);
		}

		if (GUILayout.Button("Close")) {
			EditorWindow gameView = GetMainGameView();
			gameView.Close();
		}
	}
	
	static void MoveGameWindow(){
		EditorWindow gameView = GetMainGameView();
		gameView.title = "Game (Stereo)";
		//When minSize and maxSize are the same, no OS border is applied to the window.
		gameView.minSize = new Vector2(gameSize.x, gameSize.y + tabHeight - osBorderWidth);
		gameView.maxSize = gameView.minSize;
		Rect newPos = new Rect(gamePosition.x, gamePosition.y - tabHeight, gameSize.x, gameSize.y + tabHeight - osBorderWidth);
		gameView.position = newPos;	
		gameView.ShowPopup();
	}
}