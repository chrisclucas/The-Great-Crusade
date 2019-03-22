using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SendFile : MonoBehaviour {

    FileTransferServer fts;

    Dropdown validServerList;
    float listUpdateTimer = 0f;
    SelectFile sf;

    // Use this for initialization
    void Start ()
    {
        fts = GameObject.Find("FileTransferServer").GetComponent<FileTransferServer>();
        sf = transform.parent.Find("PanelSelect").GetComponent<SelectFile>();
        validServerList = transform.Find("DropdownServers").GetComponent<Dropdown>();
    }

    // Update is called once per frame
    void Update ()
    {
        listUpdateTimer += Time.deltaTime;
        if (listUpdateTimer >= 1f)
        {
            validServerList.ClearOptions();
            // Add possibility of broadcast:
            string[] ipParts = GlobalDefinitions.localComputerIPAddress.Split('.');
            ipParts[3] = "255";
            validServerList.options.Add(new Dropdown.OptionData() { text = ipParts[0] + "." + ipParts[1] + "." + ipParts[2] + "." + ipParts[3] });
            // Updates the available server list:
            validServerList.AddOptions(fts.GetServerList());
        }
    }

    public void SendSelectedFile()
    {
        fts.SendFile(validServerList.captionText.text, sf.GetSelection());
    }

    public void SendUpdate()
    {
        fts.SendUpdate(validServerList.captionText.text, "list.txt");
    }
}
