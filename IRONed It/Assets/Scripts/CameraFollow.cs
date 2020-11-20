using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //[Tooltip("camera movement speed; smaller number is faster movement")]
    //public float dampTime = .15f; // time it takes camera to reach its destination <-- so smaller is faster
    [Tooltip("camera's horizontal offset from player when moving")]
    public float horizontalOffset = 1.4f;

    Transform player; // player

    //Vector3 cameraTarget; // camera's target position
    //Vector3 velocity = Vector3.zero;

    public static CameraFollow instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        player = Player.instance.transform;
        transform.position = new Vector3(player.position.x + horizontalOffset, player.position.y, -10); // set camera to player's location
    }

    //private void LateUpdate()
    //{
    //    cameraTarget = new Vector3(player.position.x + horizontalOffset, transform.position.y, transform.position.z); // position camera is moving towards
    //    transform.position = Vector3.SmoothDamp(transform.position, cameraTarget, ref velocity, dampTime); // move camera toward target
    //}
}
