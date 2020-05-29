using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform targetToFollow;
    public float minXClamp;
    public float maxXClamp;
    public float minZClamp;
    public float maxZClamp;

    // Start is called before the first frame update
    void Start()
    {
        //transform.rotation = Quaternion.Euler(90,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        var targetPosition = targetToFollow.position;
        float clampedX = Mathf.Clamp(targetPosition.x, minXClamp, maxXClamp);
        float clampedZ = Mathf.Clamp(targetPosition.z, minZClamp, maxZClamp);
        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
    }
}
