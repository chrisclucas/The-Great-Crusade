#define ENABLEPAN

using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public int speed = 1;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var d = Input.GetAxis("Mouse ScrollWheel");
        Vector2 viewportPostion = Camera.main.ScreenToViewportPoint(Input.mousePosition);
#if ENABLEPAN
        if (Input.GetKey(KeyCode.RightArrow) || (viewportPostion.x >= 0.99))
            transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
        if (Input.GetKey(KeyCode.LeftArrow) || (viewportPostion.x <= 0))
            transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
        if (Input.GetKey(KeyCode.DownArrow) || (viewportPostion.y <= 0))
            transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        if (Input.GetKey(KeyCode.UpArrow) || (viewportPostion.y >= 0.99))
            transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
#else
        // Disable mouse panning for editor 
        if (Input.GetKey(KeyCode.RightArrow))
            transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
        if (Input.GetKey(KeyCode.DownArrow))
            transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        if (Input.GetKey(KeyCode.UpArrow))
            transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
#endif
        if (Input.GetKey(KeyCode.PageDown) || (d > 0f))
            GetComponent<Camera>().orthographicSize--;
        if ((Input.GetKey(KeyCode.PageUp) || (d < 0f)) && (GetComponent<Camera>().orthographicSize > 1f))
            GetComponent<Camera>().orthographicSize++;

        // Right mouse click - center to the location clicked
        if (Input.GetMouseButtonDown(1))
            Camera.main.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
