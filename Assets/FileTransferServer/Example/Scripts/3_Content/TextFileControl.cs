using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextFileControl : MonoBehaviour {

    InputField input;

	// Use this for initialization
	void Start ()
    {
	    input = transform.Find("InputField").GetComponent<InputField>();
    }
	
	public void SaveTextFile()
    {
        FileManagement.SaveFile("text.txt", input.text);
    }

    public void LoadTextFile()
    {
        if(FileManagement.FileExists("text.txt"))
            input.text = FileManagement.ReadFile<string>("text.txt");
    }

    public void Delete()
    {
        FileManagement.DeleteFile("text.txt");
        input.text = "";
    }
}
