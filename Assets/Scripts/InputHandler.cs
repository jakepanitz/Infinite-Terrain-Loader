using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{

    public float moveSpeed;
    public float turnSpeed;
    public float scrollSpeed;
    public Camera cam;
    public Transform Plane;
    public bool autoMove;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (autoMove)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
        } else {
            transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed);
        }
        
        float roll = Plane.eulerAngles.x;
        if (roll > 90) roll = -(360 - roll);
        transform.Rotate(Vector3.up, roll * 0.05f * Time.deltaTime * turnSpeed);
        Plane.eulerAngles = new Vector3(
            Plane.eulerAngles.x + Input.GetAxis("Horizontal"),
            Plane.eulerAngles.y,
            Plane.eulerAngles.z
        );
        cam.transform.Translate(Vector3.forward* Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * scrollSpeed);

        if(Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }
    }
}
