using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBehaviour : MonoBehaviour
{
    public int shipLength;
    int hitAmount;
    public OccupationType type;

    private void Start()
    {
        hitAmount = shipLength;
    }

    bool IsSunk()
    {
        return hitAmount <= 0;
    }

    public bool IsHit()
    {
        return hitAmount < shipLength && hitAmount > 0;
    }

    public bool TakeDamage()
    {
        hitAmount--;
        if (IsSunk())
        {
            //report that to game manager

            //mesherenderer -unhide ship
            GetComponent<MeshRenderer>().enabled = true;
            return true;
        }
        return false;
    }
}
