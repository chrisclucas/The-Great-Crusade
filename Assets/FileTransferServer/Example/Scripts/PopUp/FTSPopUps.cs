//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class FTSPopUps : MonoBehaviour {

//    public GameObject popupPrefab;  // Drag the PopUp prefab here (in editor).
//    FileTransferServer fts;

//	// Use this for initialization
//	void Start () {
//        fts = gameObject.GetComponent<FileTransferServer>();
//	}
	
//	// Update is called once per frame
//	void Update () {
		
//	}

//    public void RX_PopUp()
//    {
//        GameObject popup = GameObject.Instantiate(popupPrefab);
//        popup.transform.SetParent(transform.root, false);
//        popup.GetComponent<PopUp>().SetMessage("Received: " + fts.GetCurrentFile());
//    }
//}
