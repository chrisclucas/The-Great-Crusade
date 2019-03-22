using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LocalServerData : MonoBehaviour {

    Text localIP;
    Toggle localServer;
    InputField inputChunk;
    FileTransferServer fts;

    // Use this for initialization
    void Start () {
        fts = GameObject.Find("FileTransferServer").GetComponent<FileTransferServer>();
        localServer = transform.Find("ToggleLocalServer").GetComponent<Toggle>();
        localIP = transform.Find("LabelLocalIP").Find("Text").GetComponent<Text>();
        localIP.text = TransportScript.localComputerIPAddress;
        inputChunk = transform.Find("InputChunk").GetComponent<InputField>();
        inputChunk.text = fts.GetMaxChunkSize().ToString();
    }

    public void SetLocalServerStatus()
    {
        fts.SetLocalServerStatus(localServer.isOn);
    }
    
    public void SetServerChunkSize()
    {
        if(inputChunk.text != "")
        {
            fts.SetMaxChunkSize(int.Parse(inputChunk.text));
            inputChunk.text = fts.GetMaxChunkSize().ToString();
        }
    }
}
