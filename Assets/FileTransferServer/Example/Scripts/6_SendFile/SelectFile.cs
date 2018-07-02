using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectFile : MonoBehaviour {

    public GameObject fileBrowser;
    Text selection;
    Toggle fullPath;

    // Use this for initialization
    void Start () {
        selection = transform.Find("Selection").Find("Text").GetComponent<Text>();
        fullPath = transform.Find("ToggleUnrestricted").GetComponent<Toggle>();
    }

    // Create a FileBrowser window (with default options):
    public void OpenFileBrowser()
    {
        GameObject browserInstance = GameObject.Instantiate(fileBrowser);
        string ini = "";
        if(fullPath.isOn)
            ini = Application.persistentDataPath;
        browserInstance.GetComponent<FileBrowser>().SetBrowserWindow(OnPathSelected, ini, fullPath.isOn);
    }

    void OnPathSelected(string path)
    {
        selection.text = path;
    }

    public string GetSelection()
    {
        return selection.text;
    }
}
