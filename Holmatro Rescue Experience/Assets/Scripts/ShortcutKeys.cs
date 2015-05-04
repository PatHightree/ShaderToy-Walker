using UnityEngine;
using System.Collections;

public class ShortcutKeys : MonoBehaviour {
	public KeyCode Quit = KeyCode.Escape;
	public KeyCode Restart = KeyCode.Backspace;

	void Update () {
		if (Input.GetKey(Quit))
			Application.Quit();
		if (Input.GetKeyDown(Restart))
			Application.LoadLevel(0);
	}
}
