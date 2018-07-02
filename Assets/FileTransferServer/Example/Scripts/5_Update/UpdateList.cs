using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UpdateList : MonoBehaviour {

    FileTransferServer fts;

    Dropdown validServerList;
    float listUpdateTimer = 0f;

    bool iRequestedTheList = false;     // Just remembers who has requested the last download.
    Transform statusPanel;
    // First item:
    RawImage rImage1;
    Image bar1;
    Text percent1;
    // Second item:
    RawImage rImage2;
    Image bar2;
    Text percent2;

    // Use this for initialization
    void Start () {
        fts = GameObject.Find("FileTransferServer").GetComponent<FileTransferServer>();
        validServerList = transform.Find("DropdownServers").GetComponent<Dropdown>();
        statusPanel = transform.parent.Find("PanelStatus");
        // First item:
        rImage1 = statusPanel.Find("RawImage1").GetComponent<RawImage>();
        bar1 = transform.Find("DownloadBar1").Find("Bar").GetComponent<Image>();
        percent1 = transform.Find("DownloadBar1").Find("Text").GetComponent<Text>();
        bar1.fillAmount = 0f;
        percent1.text = "";
        // Second item:
        rImage2 = statusPanel.Find("RawImage2").GetComponent<RawImage>();
        bar2 = transform.Find("DownloadBar2").Find("Bar").GetComponent<Image>();
        percent2 = transform.Find("DownloadBar2").Find("Text").GetComponent<Text>();
        bar2.fillAmount = 0f;
        percent2.text = "";
    }

    // Update is called once per frame
    void Update () {
        listUpdateTimer += Time.deltaTime;
        if (listUpdateTimer >= 1f)
        {
            // Updates the available server list:
            validServerList.ClearOptions();
            validServerList.AddOptions(fts.GetServerList());
        }
        // Update the progress bar 1:
        if (iRequestedTheList && fts.GetCurrentFile() == "example.png")
        {
            bar1.fillAmount = fts.GetCurrentPartialProgress();
            percent1.text = Mathf.FloorToInt(bar1.fillAmount * 100f) + "%";
        }
        // Update the progress bar 2:
        if (iRequestedTheList && fts.GetCurrentFile() == "capture.jpg")
        {
            bar2.fillAmount = fts.GetCurrentPartialProgress();
            percent2.text = Mathf.FloorToInt(bar2.fillAmount * 100f) + "%";
        }
    }

    public void StartUpdate()
    {
        iRequestedTheList = true;
        fts.RequestUpdateList(validServerList.value, "list.txt");
    }

    public void Delete()
    {
        // Clear the view window:
        rImage1.texture = null;
        bar1.fillAmount = 0f;
        percent1.text = "";

        rImage2.texture = null;
        bar2.fillAmount = 0f;
        percent2.text = "";
    }

    public void ShowDownload()
    {
        // This function is called from the "OnFileDownload" event:
        if (iRequestedTheList && fts.GetCurrentFile().Contains("example.png"))
        {
            rImage1.texture = FileManagement.ImportTexture("example.png");
            bar1.fillAmount = 1f;
            percent1.text = "100%";
        }
        if (iRequestedTheList && fts.GetCurrentFile().Contains("capture.jpg"))
        {
            rImage2.texture = FileManagement.ImportTexture("capture.jpg");
            bar2.fillAmount = 1f;
            percent2.text = "100%";
        }
    }

    public void FileNotFound()
    {
        // This function is called from the "OnFileDownload" event:
        if (iRequestedTheList && fts.GetCurrentFile() == "example.png")
        {
            percent1.text = "File not found";
            rImage1.texture = null;
        }
        if (iRequestedTheList && fts.GetCurrentFile() == "capture.jpg")
        {
            percent2.text = "File not found";
            rImage2.texture = null;
        }
    }

    public void ListNotFound()
    {
        if (iRequestedTheList)
        {
            Delete();
            percent1.text = "List not found";
            percent2.text = "List not found";
            iRequestedTheList = false;
        }
    }

    public void ListTimeout()
    {
        if (iRequestedTheList)
        {
            Delete();
            percent1.text = "Server not responding";
            percent2.text = "Server not responding";
        }
    }

    public void ListIsComplete()
    {
        if(iRequestedTheList)
        {
            iRequestedTheList = false;
            print("[UpdateList.cs] The requested list was downloaded.");
        }
    }

    public void AbortUpdate()
    {
        fts.AbortDownloadList();
        iRequestedTheList = false;
        Delete();
    }
}
