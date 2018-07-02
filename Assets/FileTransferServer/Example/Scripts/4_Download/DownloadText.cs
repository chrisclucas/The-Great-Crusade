using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DownloadText : MonoBehaviour {

    FileTransferServer fts;

    Dropdown validServerList;
    bool iRequestedTheFile = false;     // Just remembers who has requested the last download.

    Text input;
    float listUpdateTimer = 0f;
    Image bar;
    Text percent;

    // Use this for initialization
    void Start ()
    {
        fts = GameObject.Find("FileTransferServer").GetComponent<FileTransferServer>();
        validServerList = transform.Find("DropdownServers").GetComponent<Dropdown>();
        input = transform.Find("ReceivedText").Find("Text").GetComponent<Text>();

        bar = transform.Find("DownloadBar").Find("Bar").GetComponent<Image>();
        bar.fillAmount = 0f;
        percent = transform.Find("DownloadBar").Find("Text").GetComponent<Text>();
        percent.text = "";
    }

    // Update is called once per frame
    void Update ()
    {
        listUpdateTimer += Time.deltaTime;
        if (listUpdateTimer >= 1f)
        {
            // Updates the available server list:
            validServerList.ClearOptions();
            validServerList.AddOptions(fts.GetServerList());
        }

        // Update the progress bar:
        if (iRequestedTheFile && fts.GetCurrentFile() == "text.txt")
        {
            bar.fillAmount = fts.GetCurrentPartialProgress();
            percent.text = Mathf.FloorToInt(bar.fillAmount * 100f) + "%";
        }
    }

    public void DownloadTextFile()
    {
        fts.RequestFile(validServerList.value, "text.txt");
        iRequestedTheFile = true;
    }

    public void Delete()
    {
        input.text = "";
        bar.fillAmount = 0f;
        percent.text = "";
    }

    public void ShowDownload()
    {
        if (iRequestedTheFile)
        {
            input.text = FileManagement.ReadFile<string>("text.txt");
            iRequestedTheFile = false;

            bar.fillAmount = 1f;
            percent.text = "100%";
        }
    }

    public void FileNotFound()
    {
        if (iRequestedTheFile)
        {
            Delete();
            percent.text = "File not found";
            iRequestedTheFile = false;
        }
    }

    public void FileTimeout()
    {
        if (iRequestedTheFile)
            print("File timeout");
    }
}
