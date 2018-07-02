using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUp : MonoBehaviour {

    Animator anim;
    Text msg;

	// Use this for initialization
	void Awake ()
    {
        anim = gameObject.GetComponent<Animator>();
        msg = transform.Find("Message").GetComponent<Text>();
	}

    /// <summary>Sets the message and shows the PopUp</summary>
    public void SetMessage(string message)
    {
        msg.text = message;
        anim.Play("FadeIn");
    }
	
    public void Close()
    {
        anim.Play("FadeOut");
    }

    public void Destroy()
    {
        GameObject.Destroy(gameObject);
    }
}
