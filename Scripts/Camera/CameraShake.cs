using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    [SerializeField]
    private float ShakeDuration,shakeAmount;
    private Camera MainCamera;
    public static CameraShake Instance;
    private void Start() {
        MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        Instance = this;
    }
    public IEnumerator CameraShaker(int shakeDowner = 1)
    {
        float elapsedTime = 0f;

        while (elapsedTime<ShakeDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Slerp(transform.position,transform.position + Random.insideUnitSphere,shakeAmount / shakeDowner);
            yield return null;
        }
    }
}