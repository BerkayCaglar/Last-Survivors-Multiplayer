using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCaseManager : MonoBehaviour
{
    private void OnCollisionEnter(Collision other) 
    {
        Destroy(gameObject,2f);
    }
}