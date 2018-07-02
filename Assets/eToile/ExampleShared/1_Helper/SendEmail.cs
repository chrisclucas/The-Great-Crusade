using UnityEngine;
using System.Collections;

/*
 * Opens the default email application depending on the platform.
 */

public class SendEmail : MonoBehaviour {

    public void SendAutoEmail()
    {
        string email = "jmonsuarez@gmail.com";
        string subject = "FileManagementAsset - eToile";
        string body = "";
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }
}
