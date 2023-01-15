using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerRaycastHit : NetworkBehaviour
{
    Ray ray;
    RaycastHit hit;
    private Vector3 rayPosition = new Vector3(0,2f,0f);
    private Material hitObjectMaterial;
    private Color previousHitObjectMaterialColor;
    private bool isHit = false, isHitObjectMaterialColorReset = false;
    private void Update() 
    {
        if(!IsOwner) return;
        
        Ray ray = new Ray(transform.position + rayPosition, transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Roof")))
        {
            if(!isHit)
            {
                isHit = true;
                hitObjectMaterial = hit.transform.GetComponent<Renderer>().material;
                previousHitObjectMaterialColor = hitObjectMaterial.color;
                hitObjectMaterial.color = new Color(hitObjectMaterial.color.r, hitObjectMaterial.color.g, hitObjectMaterial.color.b, 0f);
            }
        }
        else
        {
            if (hitObjectMaterial != null && !isHitObjectMaterialColorReset)
            {
                isHitObjectMaterialColorReset = true;
                StartCoroutine(ResetHitObjectMaterialColor());
            } 
        }

        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 10f, Color.red);
    }
    private IEnumerator ResetHitObjectMaterialColor()
    {
        yield return new WaitForSeconds(0.5f);
        hitObjectMaterial.color = previousHitObjectMaterialColor;
        isHit = false;
        isHitObjectMaterialColorReset = false;
    }
}