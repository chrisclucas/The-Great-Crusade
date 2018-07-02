using UnityEngine;
using System.Collections;

/*
 * Allows quit the application with the "escape" key (also back in Android and Windows Phone).
 */

public class ExitApp : MonoBehaviour {

	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            print("Quit app");
        }
    }
}
