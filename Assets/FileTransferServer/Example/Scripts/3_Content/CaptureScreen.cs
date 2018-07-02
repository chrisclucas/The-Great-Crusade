using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CaptureScreen : MonoBehaviour {

    RawImage rImage;
    Texture2D capture;

    // Use this for initialization
    void Start () {
        rImage = transform.Find("RawImage").GetComponent<RawImage>();
        capture = new Texture2D(Screen.width, Screen.height);
    }

    public void Capture()
    {
        StartCoroutine(TakeScreenshot());
    }

    IEnumerator TakeScreenshot()
    {
        yield return new WaitForEndOfFrame();
        // Screenshot:
        capture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        capture.Apply();
        // Show capture on screen:
        rImage.texture = capture;
        // Save screenshot picking from a texture:
        if (Application.platform != RuntimePlatform.WebGLPlayer)    // Images are too big to be saved into cookies.
            FileManagement.SaveJpgTexture("capture.jpg", rImage.texture, 100);
    }

    public void Load()
    {
        if (FileManagement.FileExists("capture.jpg"))
            rImage.texture = FileManagement.ImportTexture("capture.jpg");
    }

    public void Delete()
    {
        FileManagement.DeleteFile("capture.jpg");
        rImage.texture = null;
    }
}
