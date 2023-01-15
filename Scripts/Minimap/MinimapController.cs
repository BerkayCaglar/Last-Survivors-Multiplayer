using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    Vector3 CameraPos;
    public static MinimapController Instance;
    private void Start() {
        Instance = this;
    }
    public void FollowPlayer(GameObject Player)
    {
        transform.position = Vector3.Slerp(transform.position, Player.transform.position + CameraPos,speed);
    }
}