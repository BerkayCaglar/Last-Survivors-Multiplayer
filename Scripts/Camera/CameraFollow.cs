using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    Vector3 CameraPos = new Vector3(0f, 9f, -3f);
    [SerializeField]
    Quaternion CameraRot;
    public static CameraFollow Instance;
    private void Start() {
        Instance = this;
    }
    public void FollowPlayer(GameObject Player)
    {
        transform.position = Vector3.Slerp(transform.position, Player.transform.position + CameraPos,speed);
        transform.rotation = CameraRot;
    }
}