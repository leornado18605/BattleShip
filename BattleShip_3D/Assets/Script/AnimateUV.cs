using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateUV : MonoBehaviour
{
    public float scrollSpeed = 0.5f;

    float offset;
    public bool U = false;
    public bool V = false;

    Material material;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        offset = Time.time * scrollSpeed % 1;

        if( U && V)
        {
            material.mainTextureOffset = new Vector2(offset, offset);
        }

        else if( U )
        {
            material.mainTextureOffset = new Vector2(offset, 0f);
        }

        else if( V )
        {
            material.mainTextureOffset = new Vector2(0f, offset);
        }
    }
}
