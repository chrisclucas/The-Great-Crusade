//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class DownloadImage : MonoBehaviour {

//    FileTransferServer fts;

//    Dropdown validServerList;
//    float listUpdateTimer = 0f;

//    RawImage rImage;
//    bool iRequestedTheFile = false;     // Just remembers who has requested the last download.
//    Image bar;
//    Text percent;

//    // Use this for initialization
//    void Start () {
//        fts = GameObject.Find("FileTransferServer").GetComponent<FileTransferServer>();
//        validServerList = transform.Find("DropdownServers").GetComponent<Dropdown>();
//        rImage = transform.Find("RawImage").GetComponent<RawImage>();

//        bar = transform.Find("DownloadBar").Find("Bar").GetComponent<Image>();
//        bar.fillAmount = 0f;
//        percent = transform.Find("DownloadBar").Find("Text").GetComponent<Text>();
//        percent.text = "";
//    }

//    // Update is called once per frame
//    void Update () {
//        listUpdateTimer += Time.deltaTime;
//        if (listUpdateTimer >= 1f)
//        {
//            // Updates the available server list:
//            validServerList.ClearOptions();
//            validServerList.AddOptions(fts.GetServerList());
//        }

//        // Update the progress bar:
//        if (iRequestedTheFile && fts.GetCurrentFile() == "capture.jpg")
//        {
//            bar.fillAmount = fts.GetCurrentPartialProgress();
//            percent.text = Mathf.FloorToInt(bar.fillAmount * 100f) + "%";
//        }
//    }

//    public void DownloadImageFile()
//    {
//        fts.RequestFile(validServerList.value, "capture.jpg");
//        iRequestedTheFile = true;
//    }

//    public void Delete()
//    {
//        rImage.texture = null;
//        bar.fillAmount = 0f;
//        percent.text = "";
//    }

//    public void ShowDownload()
//    {
//        if(iRequestedTheFile)
//        {
//            rImage.texture = FileManagement.ImportTexture("capture.jpg");
//            iRequestedTheFile = false;

//            bar.fillAmount = 1f;
//            percent.text = "100%";
//        }
//    }

//    public void FileNotFound()
//    {
//        if(iRequestedTheFile)
//        {
//            Delete();
//            percent.text = "File not found";
//            iRequestedTheFile = false;
//        }
//    }

//    public void FileTimeout()
//    {
//        if (iRequestedTheFile)
//            print("File timeout");
//    }

//}
