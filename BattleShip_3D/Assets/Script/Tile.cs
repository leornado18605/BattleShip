using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OccupationType
{
    EMPTY,
    CRUISER,
    DESTROYER,
    SUBMARINE,
    BATTLESHIP,
    CARRIER 
}
public class Tile
{
    public OccupationType type;
    public ShipBehaviour placedShip;

    //constructor
    public Tile(OccupationType _type, ShipBehaviour _placedShip)
    {
        type = _type;
        placedShip = _placedShip;
    }

    public bool IsOccupied()
    {
        return type == OccupationType.BATTLESHIP ||
            type == OccupationType.CARRIER ||
            type == OccupationType.SUBMARINE ||
            type == OccupationType.CRUISER ||
            type == OccupationType.DESTROYER;
    }

}
