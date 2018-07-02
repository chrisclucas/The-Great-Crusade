using UnityEngine;
using System.Collections;

public class GoToAssetStore : MonoBehaviour {

	// Use this for initialization
	public void GoToTheAssetStore()
    {
        Application.OpenURL("https://www.assetstore.unity3d.com/#!/content/73518");
    }
}
