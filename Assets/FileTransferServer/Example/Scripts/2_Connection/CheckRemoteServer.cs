//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class CheckRemoteServer : MonoBehaviour {

//    InputField serverIP;
//    Dropdown validServerList;
//    FileTransferServer fts;

//    // Use this for initialization
//    void Start () {
//        fts = GameObject.Find("FileTransferServer").GetComponent<FileTransferServer>();
//	    serverIP = transform.Find("InputField").GetComponent<InputField>();
//        validServerList = transform.Find("DropdownServers").GetComponent<Dropdown>();
//        // Default broadcast IP:
//        string[] ipParts = TransportScript.localComputerIPAddress.Split('.');
//        ipParts[3] = "255";
//        serverIP.text = ipParts[0] + "."+ ipParts[1] + "."+ ipParts[2] + "." + ipParts[3];
//    }
	
//	public void CheckStatus()
//    {
//        if(serverIP.text != "")
//            fts.CheckServerStatus(serverIP.text);
//    }

//    public void ResetStatus()
//    {
//        serverIP.image.color = new Color32(237,240,211,255);
//    }
//    public void SetOKStatus()
//    {
//        // One or more servers are responding:
//        serverIP.image.color = Color.green;
//        validServerList.ClearOptions();
//        validServerList.AddOptions(fts.GetServerList());
//    }
//    public void SetTimeOutStatus()
//    {
//        // Ther in no response:
//        serverIP.image.color = Color.red;
//    }
//    public void ResetServerIP()
//    {
//        // Clear the server list:
//        fts.ResetServerList();
//        validServerList.ClearOptions();
//        validServerList.AddOptions(fts.GetServerList());
//        ResetStatus();
//    }
//}
