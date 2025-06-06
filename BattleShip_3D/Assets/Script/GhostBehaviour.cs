using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GhostBehaviour : MonoBehaviour
{
    //Raycast 
    public LayerMask layerToCheck;
    RaycastHit hit;
    TileInfo tileInfo;

    PlayField playField;

    public void SetPlayField(PlayField _playField)
    {
        playField = _playField;
    }

    public bool OverTile()
    {
        tileInfo = GetTileInfo();

        if (tileInfo != null && !GameManager.instance.CheckIfOccupied(tileInfo.xPos, tileInfo.zPos))
        {
            return true;
        }

        tileInfo = null;
        return false;
    }

    public TileInfo GetTileInfo()
    {
        Ray ray = new Ray(transform.position, -transform.up);

        if(Physics.Raycast(ray, out hit, 1f, layerToCheck))
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.red);
            return hit.collider.GetComponent<TileInfo>();
        }

        return null;
    }
}
