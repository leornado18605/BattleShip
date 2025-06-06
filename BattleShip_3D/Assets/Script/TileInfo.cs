using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;

public class TileInfo : MonoBehaviour
{
    public int xPos, zPos;
    bool shot;

    public SpriteRenderer spriteRenderer;
    public Sprite[] titleHighLights;

    public void ActivateHightLight(int index, bool _shot)
    {
        spriteRenderer.sprite = titleHighLights[index];

        //Color the sprite
        shot = _shot;
    }

    private void OnMouseOver()
    {

        if(GameManager.instance.gameStates == GameManager.GameStates.SHOOTING)
        {
            if(!shot)
            {
                ActivateHightLight(1, false);
            }

            if(Input.GetMouseButtonDown(0))
            {
                //Game manager check
                GameManager.instance.CheckShot(xPos, zPos, this);
            }
        }
    }

    private void OnMouseExit()
    {
        if(!shot)
        {
            ActivateHightLight(0, false);
        }
    }

    public void SetTileInfo(int _xPos, int _zPos)
    {
        xPos = _xPos;
        zPos = _zPos;
    }
}
