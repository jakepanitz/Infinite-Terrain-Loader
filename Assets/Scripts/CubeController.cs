using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    private Vector3 TargetPosition;
    private Vector3 velocity;

    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, TargetPosition, ref velocity, 0.3f);
    }

    public void SetNewPosition(Vector3 pos)
    {
        // if (pos.x == TargetPosition.x && pos.z == TargetPosition.z) return;
        transform.position = new Vector3(
            pos.x, 0, pos.z
        );
        TargetPosition = pos;
    }
}
