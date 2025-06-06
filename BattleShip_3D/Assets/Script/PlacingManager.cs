using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class PlacingManager : MonoBehaviour
{
    public static PlacingManager instance;
    public bool isPlacing;
    bool canPlace;

    public PlayField playeField;

    public LayerMask layerToCheck;

    [System.Serializable]

    public class ShipToplace
    {
        public GameObject shipGhost;
        public GameObject shipPrefabs;
        public int amountToPlace = 1;
        public Text amountText;
        [HideInInspector]
        public int placedAmount = 0;
    }

    public List<ShipToplace> shipList = new List<ShipToplace>();

    public Button buttonReady;

    int currentShip = 3;

    RaycastHit hit;
    Vector3 hitPoint;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        buttonReady.interactable = false;
        ActivateShipGhost(-1);
        UpdateAmountText();
    }

    public void SetPlayer(PlayField _playField, string playerType)
    {
        playeField =  _playField;
        buttonReady.interactable = false;

        ClearAllShips();
        
        //NPC
        if(playerType == "NPC")
        {
            //AUTO PLACE SHIPS
            AutoPlaceShip();
            //UPDATE GAME MANAGER THAN THE TURN IS COMPLETE
            GameManager.instance.PlacingReady();
        }
    }
    private void Update()
    {
        if (isPlacing)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerToCheck))
            {
                //if tile is not the oppenent tile
                if (!playeField.RequestTile(hit.collider.GetComponent<TileInfo>()))
                {
                    return;
                }
                hitPoint = hit.point;
            }

            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                PlaceShip();
            }

            if (Input.GetMouseButtonDown(1))
            {
                RotateShipGod();
            }
            PlaceGhost();
        }

    }

    void ActivateShipGhost(int index)
    {
        if (index != -1)
        {
            if (shipList[index].shipGhost.activeInHierarchy)
            {
                return;
            }
        }
        //DEACTIVATE ALL GHOST
        for (int i = 0; i < shipList.Count; i++)
        {
            shipList[i].shipGhost.SetActive(false);
        }

        if (index == -1)
        {
            return;
        }
        shipList[index].shipGhost.SetActive(true);
    }

    void PlaceGhost()
    {
        if (isPlacing)
        {
            canPlace = CheckForOtherShip();
            shipList[currentShip].shipGhost.transform.position = new Vector3(Mathf.Round(hitPoint.x), 0, Mathf.Round(hitPoint.z));
        }

        else
        {
            //deativate all ghost
            ActivateShipGhost(-1);
        }
    }

    void RotateShipGod()
    {
        shipList[currentShip].shipGhost.transform.localEulerAngles += new Vector3(0f, 90f, 0f);
    }
    bool CheckForOtherShip()
    {
        foreach(Transform child in shipList[currentShip].shipGhost.transform)
        {
            GhostBehaviour ghostBehaviour = child.GetComponent<GhostBehaviour>();
            if(!ghostBehaviour.OverTile())
            {
                child.GetComponent<MeshRenderer>().material.color = new Color32(255, 0, 0, 125);
                return false;
            }
            child.GetComponent<MeshRenderer>().material.color = new Color32(0, 0, 0, 100);
        }
        return true;
    }

    bool CheckForOtherShip(Transform tr)
    {
        foreach (Transform child in tr.transform)
        {
            GhostBehaviour ghostBehaviour = child.GetComponent<GhostBehaviour>();
            if (!ghostBehaviour.OverTile())
            {
                //child.GetComponent<MeshRenderer>().material.color = new Color32(255, 0, 0, 125);
                return false;
            }
            //child.GetComponent<MeshRenderer>().material.color = new Color32(0, 0, 0, 100);
        }
        return true;
    }


    void PlaceShip()
    {
        Vector3 pos = new Vector3(Mathf.Round(hitPoint.x), 0, Mathf.Round(hitPoint.z));
        Quaternion rot = shipList[currentShip].shipGhost.transform.rotation;

        GameObject newShip = Instantiate(shipList[currentShip].shipPrefabs, pos, rot);

        //Update Grid
        GameManager.instance.UpdateGrid(shipList[currentShip].shipGhost.transform, newShip.GetComponent<ShipBehaviour>(), newShip);

        //Fix overload
        shipList[currentShip].placedAmount++;
        //DEACTIVATE IS PLACING
        isPlacing = false;
        //DEACTIVATE ALL GHOST
        ActivateShipGhost(-1);
        //CHECK IF ALL SHIPS HAS BEEN PLACED
        CheckIfAllShipPlaced();
        //UPDATE TEXT AMOUNT TO PLACE
        UpdateAmountText();
    }


    //Buttons
    public void PlaceShipButton(int index)
    {
        if(CheckIfAllShipPlaced(index))
        {
            print("You have placed enough already");
            return;
        }

        //we can activate ship ghost
        currentShip = index;
        ActivateShipGhost(currentShip);
        isPlacing = true;
    }
    bool CheckIfAllShipPlaced(int index)
    {
        return shipList[index].placedAmount == shipList[index].amountToPlace;
    } //specific ship
    bool CheckIfAllShipPlaced() // all ship
    {
        foreach(var ship in shipList)
        {
            if(ship.placedAmount != ship.amountToPlace)
            {
                return false;
            }
        }
        buttonReady.interactable = true;
        return true;
    }


    void UpdateAmountText()
    {
        for (int i = 0; i < shipList.Count; i++)
        {
            shipList[i].amountText.text = (shipList[i].amountToPlace - shipList[i].placedAmount).ToString();
        }
    }

    public void ClearAllShips()
    {
        GameManager.instance.DeleteAllShipFromList();
        foreach(var ship in shipList)
        {
            ship.placedAmount = 0;
        }
        UpdateAmountText ();
        
        buttonReady.interactable = false;
    }

    public void AutoPlaceShip()
    {
        ClearAllShips();

        bool posFound = false;

        //Loop through all ships
        for(int i = 0; i < shipList.Count;i++)
        {
            for (int j = 0; j < shipList[i].amountToPlace; j++)
            {
                if (shipList[i].amountToPlace <= 0)
                {
                    Debug.Log("ERROR, no or negative ship amount");
                    return;
                }

                posFound = false;

                while (!posFound)
                {
                    //stay here until we find  a possible pos
                    currentShip = i;
                    int xPos = Random.Range(0, 10);
                    int zPos = Random.Range(0, 10);

                    //Pick Rand on start pos
                    GameObject tempGhost = Instantiate(shipList[currentShip].shipGhost);
                    tempGhost.SetActive(true);
                    //set ghost playfield

                    //set pos of ghost
                    tempGhost.transform.position = new Vector3(playeField.transform.position.x + xPos, 0, playeField.transform.position.z + zPos);

                    Vector3[] rot = { Vector3.zero, new Vector3(0, 90, 0), new Vector3(0, 180, 0), new Vector3(0, 270, 0) };
                    //check for all rotation
                    for(int r = 0; r < rot.Length; r++)
                    {
                        List<int> indexList = new List<int> { 0, 1, 2, 3 };
                        int rr = indexList[Random.Range(0, indexList.Count)];

                        tempGhost.transform.rotation = Quaternion.Euler(rot[rr]);

                        if(CheckForOtherShip(tempGhost.transform))
                        {
                            PlaceAutoShip(tempGhost);
                            posFound = true;
                        }
                        else
                        {
                            Destroy(tempGhost);
                            indexList.Remove(r);
                        }
                    } 
                }
            }
        }
        //buttonReady.interactable = true;
        CheckIfAllShipPlaced();
    }
    public void PlaceAutoShip(GameObject temp)
    {
        GameObject newShip = Instantiate(shipList[currentShip].shipPrefabs, temp.transform.position, temp.transform.rotation);
        GameManager.instance.UpdateGrid(temp.transform, newShip.GetComponent<ShipBehaviour>(), newShip);
        shipList[currentShip].placedAmount++;

        Destroy(temp);
        UpdateAmountText();
    }
}
