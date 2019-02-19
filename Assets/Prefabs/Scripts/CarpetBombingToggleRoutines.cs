using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarpetBombingToggleRoutines : MonoBehaviour
{
    public GameObject hex;

    /// <summary>
    /// Selects a hex for carpet bombing
    /// </summary>
    public void SelectHex()
    {
        if (GetComponent<Toggle>().isOn)
        {
            GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.CARPETBOMBINGSELECTIONKEYWORD + " " + name);

            foreach (Transform childTransform in transform.parent.transform)
                if (childTransform.gameObject.name == "BombingToggle")
                    if ((childTransform.gameObject != gameObject) && (childTransform.gameObject.GetComponent<Toggle>().isOn))
                        childTransform.gameObject.GetComponent<Toggle>().isOn = false;
        }
    }

    public void LocateCarpetBombingHex()
    {
        GlobalDefinitions.WriteToCommandFile(GlobalDefinitions.CARPETBOMBINGLOCATIONKEYWORD + " " + name);

        Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        // This centers the camera on the unit
        mainCamera.transform.position = new Vector3(hex.transform.position.x, hex.transform.position.y, mainCamera.transform.position.z);
        // This then moves the camera over to the left so that the gui doesn't cover the unit
        mainCamera.transform.position = new Vector3(
            mainCamera.ViewportToWorldPoint(new Vector2(0.25f, 0.5f)).x,
            hex.transform.position.y,
            mainCamera.transform.position.z);
    }
}
