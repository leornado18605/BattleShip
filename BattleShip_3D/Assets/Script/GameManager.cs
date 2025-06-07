using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private void Awake()
    {
        instance = this;
    }

    [System.Serializable]
    public class Player
    {
        public enum PlayerType
        {
            HUMAN,
            NPC
        }

        public PlayerType playerType;
        public Tile[,] myGrid = new Tile[10, 10];
        public bool[,] revealeGrid = new bool[10, 10];
        public PlayField playField;
        public LayerMask layerMask;

        [Space]
        public GameObject camPosition;
        public GameObject placePanel;
        public GameObject shootPanel;
        //Panels to show and hide ships
        public Player()
        {
            for (int x = 0; x < 10; x++)
            {
                for(int y = 0; y < 10; y ++)
                {
                    OccupationType type = OccupationType.EMPTY;
                    myGrid[x, y] = new Tile(type, null);
                    revealeGrid[x, y] = false;
                }
            }
        }
        public List<GameObject> placedShipList = new List<GameObject>();
    }
    int activePlayer;
    public Player[] players = new Player[2];

    //State machine
    public enum GameStates
    {
        P1_PLACE_SHIPS,
        P2_PLACE_SHIPS,
        SHOOTING,
        IDLE
    }
    public GameStates gameStates;
    public GameObject battleCamPosition;
    bool camIsMoving;
    public GameObject placingCanvas;

    bool isShooting; //protectfor coroutine 

    //Rocket
    public GameObject rocketPrefab;
    float amplitude = 3f;
    float cTime;
    private void Start()
    {
        PlacingManager.instance.SetPlayer(players[activePlayer].playField, players[activePlayer].playerType.ToString());
        HideAllPanels();

        //active place panel from first player
        players[activePlayer].placePanel.SetActive(true);
        gameStates = GameStates.IDLE;
    }
    public void AddShipToList(GameObject placedShip)
    {
        players[activePlayer].placedShipList.Add(placedShip);
    }

    public void UpdateGrid(Transform shipTransform, ShipBehaviour ship, GameObject placedShip)
    {
        foreach (Transform child in shipTransform)
        {
            TileInfo tileInfo = child.GetComponent<GhostBehaviour>().GetTileInfo();

            players[activePlayer].myGrid[tileInfo.xPos, tileInfo.zPos] = new Tile(ship.type, ship);
        }

        AddShipToList(placedShip);
    }

    public bool CheckIfOccupied(int xPos, int zPos)
    {
        return players[activePlayer].myGrid[xPos, zPos].IsOccupied();
    }

    public void DeleteAllShipFromList()
    {
        foreach(GameObject ship in players[activePlayer].placedShipList)
        {
            Destroy(ship);  
        }
        players[activePlayer].placedShipList.Clear();

        InitGrid();
    }

    public void InitGrid()
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                OccupationType type = OccupationType.EMPTY;
                players[activePlayer].myGrid[x, y] = new Tile(type, null);
               
            }
        }
    }

    private bool player1Ready = false;
    private bool player2Ready = false;

    public void Update()
    {
        switch (gameStates)
        {
            case GameStates.IDLE:
                {
                    //Waiting here
                }
                break;

            case GameStates.P1_PLACE_SHIPS:
                {
                    players[activePlayer].placePanel.SetActive(false);
                    PlacingManager.instance.SetPlayer(players[activePlayer].playField, players[activePlayer].playerType.ToString());
                    StartCoroutine(MoveCamera(players[activePlayer].camPosition));
                    gameStates = GameStates.IDLE;
                }
                break;
            case GameStates.P2_PLACE_SHIPS:
                {
                    players[activePlayer].placePanel.SetActive(false);
                    PlacingManager.instance.SetPlayer(players[activePlayer].playField, players[activePlayer].playerType.ToString());
                    gameStates = GameStates.IDLE;
                }
                break;
            case GameStates.SHOOTING:
                {
                    HideAllPanels(); // hide
                    players[activePlayer].shootPanel.SetActive(true); // display shoot panel

                    // hide shoot panel of player not work
                    int otherPlayer = (activePlayer + 1) % 2;
                    players[otherPlayer].shootPanel.SetActive(false);

                    // hide ship of player is shooted
                    if (activePlayer == 0) // wwhen 1 is shoooted, hide 2
                    {
                        foreach (var ship in players[1].placedShipList)
                        {
                            if (ship != null && ship.GetComponent<MeshRenderer>() != null)
                            {
                                ship.GetComponent<MeshRenderer>().enabled = false;
                            }
                        }
                    }
                    else if (activePlayer == 1) // when 2 , hide 1
                    {
                        foreach (var ship in players[0].placedShipList)
                        {
                            if (ship != null && ship.GetComponent<MeshRenderer>() != null)
                            {
                                ship.GetComponent<MeshRenderer>().enabled = false;
                            }
                        }
                    }

                    StartCoroutine(MoveCamera(battleCamPosition));
                    
                }
                break;
        }
    }

    public void HideAllPanels()
    {
        players[0].placePanel.SetActive(false);
        players[0].shootPanel.SetActive(false);

        players[1].placePanel.SetActive(false);
        players[1].shootPanel.SetActive(false);
    }

    //PLACE PANEL BUTTON P1
    public void P1PlaceShip()
    {
        gameStates = GameStates.P1_PLACE_SHIPS;
    }

    //PLACE PANEL BUTTON P1
    public void P2PlaceShip()
    {
        gameStates = GameStates.P2_PLACE_SHIPS;
    }

    //READY BUTTON
    public void PlacingReady()
    {
        if(activePlayer == 0)
        {
            HideAllMyShips();
            player1Ready = true;
            SwitchPlayer();

            //NPC PLAYER 2
            if (players[activePlayer].playerType == Player.PlayerType.NPC)
            {
                gameStates = GameStates.P2_PLACE_SHIPS;

                //move the cam
                StartCoroutine(MoveCamera(players[activePlayer].camPosition));
                return;
            }
            
            players[activePlayer].placePanel.SetActive(true);
            //UnHideAllMyShips() ;
            return;
        }

        if (activePlayer == 1)
        {
            HideAllMyShips();
            player2Ready = true;
           

            if (player1Ready && player2Ready) 
            {
                SwitchPlayer(); 
                StartCoroutine(MoveCamera(battleCamPosition));
                gameStates = GameStates.SHOOTING; 
                placingCanvas.SetActive(false); 

            }
        else
        {
            SwitchPlayer();
            StartCoroutine(MoveCamera(battleCamPosition));
            players[activePlayer].placePanel.SetActive(true);
            placingCanvas.SetActive(false);
        }
        }
    }
    public void HideAllMyShips()
    {
        foreach(var ship in players[activePlayer].placedShipList)
        {
            ship.GetComponent<MeshRenderer>().enabled = false;
        }
    }
    //samsame
    void UnHideAllMyShips()
    {
        foreach (var ship in players[activePlayer].placedShipList)
        {
            ship.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    void SwitchPlayer()
    {
        activePlayer++;
        activePlayer %= 2;
    }

    IEnumerator MoveCamera(GameObject camObj)
    {
        if(camIsMoving)
        {
            yield break;
        }
        camIsMoving = true;

        float t = 0;
        float duration = 0.5f;
        Vector3 startPos = Camera.main.transform.position;
        Quaternion startRot = Camera.main.transform.rotation;

        Vector3 toPos = camObj.transform.position;
        Quaternion toRot = camObj.transform.rotation;

        while(t < duration)
        {
            t += Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(startPos, toPos, t/duration);
            Camera.main.transform.rotation = Quaternion.Lerp(startRot, toRot, t/duration);

            yield return null;
        }

        camIsMoving=false;
    }

    public void ShotButton()
    {
        UnHideAllMyShips();
        players[activePlayer].shootPanel.SetActive(false);
        gameStates = GameStates.SHOOTING;
    }

   int Opponent()
    {
        int me = activePlayer;
        me++;
        me %= 2;
        int opponent = me;
        return opponent;
    }

    public void CheckShot(int x, int z, TileInfo info)
    {
        StartCoroutine(CheckCoordinate(x, z, info));
    }

    IEnumerator CheckCoordinate(int x, int z, TileInfo info)
    {
        if (isShooting)
        {
            yield break;
        }
        int opponent = Opponent();

        if (!players[opponent].playField.RequestTile(info))
        {
            Debug.Log("No");
            isShooting = false;
            yield break;
            
        }

        Vector3 starPos = Vector3.zero;
        Vector3 goalPos = info.gameObject.transform.position;

        GameObject rocket = Instantiate(rocketPrefab, starPos, Quaternion.identity);

        while(MoveInArcToTile(starPos, goalPos, 0.5f, rocket))
        {
            yield return null;
        }
        Destroy(rocket);
        cTime = 0;
        //if is occupied
        if (players[opponent].myGrid[x, z].IsOccupied())
        {
            //Damage to ship
            bool sunk = players[opponent].myGrid[x, z].placedShip.TakeDamage();
            if(sunk)
            {
                players[opponent].placedShipList.Remove(players[opponent].myGrid[x, z].placedShip.gameObject);
            }
            //hihgilighty the tile different
            info.ActivateHightLight(3, true);
        }

        else
        {
            info.ActivateHightLight(2, true);
        }

        //reveal tile
       if (players[opponent].revealeGrid[x, z] == true)
       {
            print("u shoted aready");
            yield break;
       }

        //check win
        if (players[opponent].placedShipList.Count == 0)
        {
            print("You win!");
            yield break ;
        }
        yield return new WaitForSeconds(3f);
        //Hide my ships
        HideAllMyShips();
        //Switch Player
        SwitchPlayer();

        if (players[activePlayer].playerType == Player.PlayerType.NPC)
        {
            isShooting = false;
            gameStates = GameStates.IDLE;
            NPCshot();
            yield break;
        }
        //Activate the correct panel
        players[activePlayer].shootPanel.SetActive(true);
        //GameState To IDLE
        gameStates = GameStates.IDLE;
        isShooting = false;

    }

    bool MoveInArcToTile(Vector3 starPos, Vector3 goalPos, float speed, GameObject rocket)
    {
        cTime += speed * Time.deltaTime;
        Vector3 myPos = Vector3.Lerp(starPos, goalPos, cTime);
        myPos.y = amplitude * Mathf.Sin(Mathf.Clamp01(cTime) * Mathf.PI);
        rocket.transform.LookAt(myPos);

        return goalPos != (rocket.transform.position = Vector3.Lerp(rocket.transform.position, myPos, cTime));

    }

    //NPC MODE
    void NPCshot()
    {
        int index = 0;
        int x = 0;
        int z = 0;

        TileInfo info = null;
        int opponent = Opponent();

        List<int[]> partiallyRevealedTiles = new List<int[]>();
        //Partitially revealed ships
        for(int i = 0; i<10; i++)
        {
            for(int j = 0; j < 10; j++)
            {
                //got hit already
                if (players[opponent].revealeGrid[i, j])
                {
                    //is that occupied
                    if (players[opponent].myGrid[i, j].IsOccupied())
                    {
                        //IS THAT DESTROY ALREDY
                        if (players[opponent].myGrid[i, j].placedShip.IsHit())
                        {
                            partiallyRevealedTiles.Add(new int[2] { i, j });
                        }
                    }
                }
            }
        }

        //Store all neighbours
        List<int[]> neighbourList = new List<int[]>();
        if(partiallyRevealedTiles.Count > 0)
        {
            for(int i = 0; i < partiallyRevealedTiles.Count; i++)
            {
                neighbourList.AddRange(GetNeighBours(partiallyRevealedTiles[i]));
            }
            index = Random.Range(0, neighbourList.Count);

            x = neighbourList[index][0];
            z = neighbourList[index][1];
            info = players[opponent].playField.GetTileInfo(x, z);

            CheckShot(x, z, info);
            return;
        }

        //Shoot any other random
        List<int[]> randomShootList = new List<int[]>();
        for (int i = 0; i < 10; i++)
        { 
            for(int j = 0; j < 10; j++)
            {
                if (players[opponent].revealeGrid[i, j] != true)
                {
                    randomShootList.Add(new int[2] { i, j });
                }
            }
        }
        index = Random.Range(0, randomShootList.Count);

        x = randomShootList[index][0];
        z = randomShootList[index][1];
       info = players[opponent].playField.GetTileInfo(x, z);

        CheckShot(x, z, info);

    }
    List<int[]> GetNeighBours(int[] originalCoords)
    {
        List<int[]> neighbours = new List<int[]>();
        //Find All NEIGHBOURS IN A 3X3 Grid Around the original coords
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                //IGNORE DIAGNOAL AND CENTER
                if (x == 0 && z == 0)
                { continue; }
                //Top Left
                if (x == -1 && z == 1)
                {
                    continue;
                }
                //TOP RIGHT
                if (x == 1 && z == 1)
                {
                    continue;
                }
                //BOTTOM LEFT
                if (x == -1 && z == -1)
                {
                    continue;
                }
                //BOTTOM RIGHT
                if (x == 1 && z == -1)
                {
                    continue;
                }

                int checkX = originalCoords[0] + x;
                int checkZ = originalCoords[1] + z;

                //Check IF  INSIDE GRID
                if (checkX >= 0 && checkX < 10 && checkZ >= 0 && checkZ < 10 && players[Opponent()].revealeGrid[checkX, checkZ] == false)
                {
                    neighbours.Add(new int[2] { checkX, checkZ });
                }
            }
        }
        return neighbours;
    }
}
