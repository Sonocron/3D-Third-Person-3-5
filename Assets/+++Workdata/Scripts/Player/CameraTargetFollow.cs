using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetFollow : MonoBehaviour
{
    public Transform player;
    public float height;
    private void LateUpdate()
    {
        transform.position = player.position + new Vector3(0, height, 0);
    }
}
