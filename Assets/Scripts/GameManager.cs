using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Transform hexParent;

    [SerializeField]
    private GameObject hexPrefab;

    public const int WIDTH = 50; //no. of Column in this map
    public const int HEIGHT = 60; //no. of Row in this map

    [SerializeField]
    private Hex[,] allHexes = new Hex[WIDTH, HEIGHT];
    public Hex[,] AllHexes { get { return allHexes; } }

    [SerializeField]
    private HexData[] hexData;
    public HexData[] HexData { get { return hexData; } }

    [SerializeField]
    private bool showingText;

    [SerializeField]
    private int oceanEdgeIndex;

    [SerializeField]
    private GameObject landUnitPrefab;

    [SerializeField]
    private GameObject navalUnitPrefab;

    [SerializeField]
    private GameObject townPrefab;

    [SerializeField]
    private LandUnitData[] landUnitData;
    public LandUnitData[] LandUnitData { get { return landUnitData; } }

    [SerializeField]
    private NavalUnitData[] navalUnitData;
    public NavalUnitData[] NavalUnitData { get { return navalUnitData; } }

    [SerializeField]
    private Faction playerFaction;
    public Faction PlayerFaction { get { return playerFaction; } }

    [SerializeField]
    private Faction[] factions; //England, France, Spain, Netherland, Portugal
    public Faction[] Factions { get { return factions; } }

    [SerializeField]
    private FactionData[] factionData;
    public FactionData[] FactionData { get { return factionData; } }

    [SerializeField]
    private Unit curUnit;
    public Unit CurUnit { get { return curUnit; } set { curUnit = value; } }

    [SerializeField]
    private Unit curAiUnit;
    public Unit CurAiUnit { get { return curAiUnit; } set { curAiUnit = value; } }

    [SerializeField]
    private Town curTown;
    public Town CurTown { get { return curTown; } set { curTown = value; } }

    [SerializeField]
    private bool playerTurn = true;
    public bool PlayerTurn { get { return playerTurn; } set { playerTurn = value; } }

    [SerializeField]
    private int gameTurn = 1;
    public int GameTurn { get { return gameTurn; } set { gameTurn = value; } }

    [SerializeField]
    private int nativeTownNum;

    public const int ARCTICNORTH = HEIGHT - 2; //58
    public const int ARCTICSOUTH = 2;

    public const int TUNDRANORTH = HEIGHT - 6; //54
    public const int TUNDRASOUTH = 6;

    public const int GRASSNORTH = HEIGHT - 12; //48
    public const int GRASSSOUTH = 12;

    public const int PRAIRIENORTH = HEIGHT - 20; //40
    public const int PRAIRIESOUTH = 20;

    public const int SAVANNANORTH = HEIGHT - 24; //36
    public const int SAVANNASOUTH = 24;

    public const int TROPICALNORTH = HEIGHT - 30; //30
    public const int TROPICALSOUTH = 30;

    [SerializeField]
    private ProductData[] productData;
    public ProductData[] ProductData { get { return productData; } }

    [SerializeField]
    private int year = 1600;
    public int Year { get { return year; } set { year = value; } }

    [SerializeField]
    private Unit winner;

    [SerializeField]
    private Unit loser;

    [SerializeField]
    private Faction curForeignFaction; //cur Faction that player's meeting
    public Faction CurForeignFaction { get { return curForeignFaction; } set { curForeignFaction = value; } }

    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetUpFaction();
        SelectPlayerFaction();
        DetermineOcean();
        GenerateAllHexes();

        GenerateAllEuropeanShips();
        GenerateAllNativeTowns();
        GenerateAllNativeUnits();
        GenerateAllEuropeanExplorerUnits();

        ShowFirstKingDialogText();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            ToggleHexText();

        if (Input.GetKeyDown(KeyCode.Tab))
            SelectNextPlayerUnit();

        if (Input.GetKeyDown(KeyCode.Backspace))
            Endturn();

        if (Input.GetKeyDown(KeyCode.E))
            EuropePanel();
    }

    private void SetUpFaction()
    {
        for (int i = 0; i < factions.Length; i++)
        {
            factions[i].FactionInit(factionData[i]);
        }
    }

    private void GenerateAllHexes()
    {
        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                Vector3 hexPos = grid.GetCellCenterWorld(new Vector3Int(x, y));
                //Debug.Log(hexPos);

                GameObject hexObj = Instantiate(hexPrefab, hexPos, Quaternion.identity, hexParent);
                Hex hex = hexObj.GetComponent<Hex>();

                int n = Random.Range(oceanEdgeIndex - 4, oceanEdgeIndex + 5);

                if (x >= n)
                    hex.HexInit(x, y, hexPos, this, 0);//Ocean
                else
                {
                    /*int i = Random.Range(1, hexData.Length);
                    hex.HexInit(x, y, hexPos, this, i);//Land*/
                    GenerateAllBiomes(x, y, hex, hexPos);
                }
                //Debug.Log($"{x}:{y}");
                allHexes[x, y] = hex;
            }
        }
    }

    public void SelectPlayerFaction()
    {
        int i = Settings.playerNationId;
        playerFaction = factions[i];
    }

    private void ToggleHexText()
    {
        foreach (Hex hex in allHexes)
            hex.ToggleAllBasicText(!showingText);

        showingText = !showingText;
    }

    private void DetermineOcean()
    {
        oceanEdgeIndex = WIDTH - Random.Range(7, 10);
        //Debug.Log($"min:{oceanIndexMin}");
    }

    private Hex FindNearestLandHexToLand(int y)
    {
        int min = oceanEdgeIndex - 3;
        int max = oceanEdgeIndex + 4;

        Hex destHex = null;

        for (int x = min; x <= max; x++)
        {
            //Debug.Log($"checking:{x},{y}");
            if (allHexes[x, y].HexType != HexType.Ocean)
            {
                //Debug.Log($"found:{x},{y}");
                destHex = allHexes[x, y];
            }
        }
        return destHex;
    }

    private void GenerateEuropeanShip(Faction faction, int yPos, int i)
    {
        int x = WIDTH - 2; //near right edge of a map
        int y = yPos;
        Hex hex = allHexes[x, y];

        GameObject obj = Instantiate(navalUnitPrefab, hex.Pos, Quaternion.identity, faction.UnitParent);
        NavalUnit ship = obj.GetComponent<NavalUnit>();

        ship.UnitInit(this, faction, navalUnitData[i]);
        ship.SetupPosition(hex);
        faction.Units.Add(ship); //First Unit of European nations is a ship

        if (faction == playerFaction)
        {
            ClearDarkFogAroundUnit(ship);
            SelectPlayerUnit(ship);
            CameraController.instance.MoveCamera(ship.CurPos);
            ship.Visible = true;
        }
        else
        {
            Hex destinatioHex = FindNearestLandHexToLand(yPos);
            if (destinatioHex != null)
                ship.SetUnitDestination(destinatioHex);
        }
    }

    private void GenerateAllEuropeanShips()
    {
        int interval = HEIGHT / 5;
        int n = HEIGHT;

        List<int> spots = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            n -= Random.Range(5, interval);
            spots.Add(n);
        }

        for (int i = 0; i < 5; i++)
        {
            int index = Random.Range(0, spots.Count);
            int yPosition = spots[index];
            spots.RemoveAt(index);

            GenerateEuropeanShip(factions[i], yPosition, 0); //0 is caravel
        }
    }

    public void ShowToggleBorder(Unit unit)
    {
        if (unit.Faction == playerFaction)
            unit.ToggleBorder(true, Color.green);
        else
            unit.ToggleBorder(true, Color.red);
    }

    public void ClearToggleBorder(Unit unit)
    {
        unit.ToggleBorder(false, Color.green);
    }

    public void FocusPlayerUnit(Unit unit)
    {
        //ShowBorderOnOurTroops(Color.blue);
        ShowToggleBorder(unit);
        //Debug.Log("Show");
    }

    public void SelectPlayerUnit(Unit unit)
    {
        //Debug.Log(curUnit);

        //Unselect Old Current Unit
        if (curUnit != null)
        {
            ClearToggleBorder(curUnit);
            curUnit.SetUnitToNormalLayerOrder();

            if (curUnit.UnitStatus == UnitStatus.OnBoard)
                curUnit.gameObject.SetActive(false);
        }

        //Set New Next Unit
        unit.gameObject.SetActive(true);
        unit.SetUnitToFrontLayerOrder();

        curUnit = unit;
        CameraController.instance.MoveCamera(curUnit.transform.position);

        HideOtherLandUnits(curUnit);
        FocusPlayerUnit(curUnit);
    }

    public void SelectAiUnit(Unit unit)
    {
        //Debug.Log($"{unit.Faction}:{unit.UnitName}");

        if (curUnit != null)
            ClearToggleBorder(curUnit);

        curAiUnit = unit;
        //UpdateCanGoHex();

        FocusPlayerUnit(curAiUnit);
        //Debug.Log($"{curAiUnit.Faction}:{curAiUnit.UnitName}");
    }

    public void ClearDarkFogAroundUnit(Unit unit)
    {
        //Debug.Log($"Center: {unit.CurHex.X},{unit.CurHex.Y}");
        if (unit.CurHex == null)
            return;

        unit.CurHex.DiscoverHex();

        List<Hex> adjHexes = HexCalculator.GetHexAround(allHexes, unit.CurHex);

        //Debug.Log(adjHexes.Count);

        foreach (Hex hex in adjHexes)
        {
            hex.DiscoverHex();
        }
    }

    public void ClearDarkFogAroundEveryUnit(Faction faction)
    {
        foreach (Unit unit in faction.Units)
        {
            //Debug.Log($"{unit.UnitName} discovers:");
            ClearDarkFogAroundUnit(unit);
        }
    }

    public void LeaveSeenFogAroundUnit(Unit unit)
    {
        unit.CurHex.SeenHex();

        List<Hex> adjHexes = HexCalculator.GetHexAround(allHexes, unit.CurHex);

        //Debug.Log(adjHexes.Count);

        foreach (Hex hex in adjHexes)
        {
            hex.SeenHex();
        }
    }

    public bool CheckIfHexIsAdjacent(Hex centerHex, Hex targetHex)
    {
        List<Hex> adjHexes = HexCalculator.GetHexAround(allHexes, centerHex);

        return (adjHexes.Contains(targetHex)) ? true : false;
    }

    private int FindIndexOfCurUnit()
    {
        if (playerFaction.Units.Contains(curUnit))
        {
            for (int i = 0; i < playerFaction.Units.Count; i++)
            {
                if (curUnit == playerFaction.Units[i])
                    return i;
            }
            return -1;
        }
        else
            return -1;
    }

    private bool CheckNextUnit(Unit unit)
    {
        //Next Unit that should not be selected will return false

        //Workers in field
        if (unit.UnitType == UnitType.Land && unit.UnitStatus == UnitStatus.WorkInField)
            return false;

        //Hidden ship
        if (unit.UnitType == UnitType.Naval && unit.UnitStatus == UnitStatus.Hidden)
            return false;

        //Passengers in hidden ship
        if (unit.UnitType == UnitType.Land && unit.UnitStatus == UnitStatus.OnBoard)
        {
            LandUnit landUnit = (LandUnit)unit;

            if (landUnit.TransportShip.UnitStatus == UnitStatus.Hidden)
                return false;
        }
        return true;
    }

    private void SelectNextPlayerUnit()
    {
        int curId = FindIndexOfCurUnit();
        
        if (curId == -1)
            return;

        for (int i = 0; i < playerFaction.Units.Count; i++)
        {
            curId++;

            if (curId >= playerFaction.Units.Count)
                curId = 0;

            //Debug.Log($"Select Unit:{i}");
            bool success = CheckNextUnit(playerFaction.Units[curId]);

            if (success)
            {
                SelectPlayerUnit(playerFaction.Units[curId]);
                Debug.Log($"Cur Unit:{curUnit.UnitName}-{curUnit.UnitStatus}");
                break;
            }
        }
    }

    public void ResetAllUnits(Faction faction)
    {
        foreach (Unit unit in faction.Units)
        {
            CheckUnitClearingLand(faction, unit);
            CheckUnitBuildingSettlement(faction, unit);
            unit.MovePoint = unit.MovePointMax;
        }
    }

    public void Endturn()
    {
        if (playerTurn == false)
            return;

        if (curUnit != null)
            curUnit.ToggleBorder(false, Color.green);

        Debug.Log("End Turn");

        year++;
        DialogManager.instance.ToggleAiMoveDialog(true);

        UIManager.instance.CheckActiveUIPanel();
        EuropeManager.instance.UpdateShipInTransitTurn();

        playerTurn = false;
        AIManager.instance.StartAITurn();
    }

    public void GenerateTown(Faction faction, Hex curHex)
    {
        GameObject obj = Instantiate(townPrefab, curHex.Pos, Quaternion.identity, faction.TownParent);
        Town town = obj.GetComponent<Town>();

        town.TownInit(this, faction);
        town.CurHex = curHex;
        town.CurPos = town.CurHex.Pos;
        faction.Towns.Add(town);

        curHex.HasTown = true;
        curHex.Town = town;

        curTown = town;
    }

    private void GenerateAllNativeTowns()
    {
        for (int i = 5; i < factions.Length; i++)
        {
            nativeTownNum = Random.Range(5, 10);

            for (int j = 0; j < nativeTownNum; j++)
            {
                int landEdge = oceanEdgeIndex - 1;

                int x = Random.Range(0, landEdge);
                int y = Random.Range(0, HEIGHT);
                Hex hex = allHexes[x, y];

                if (HexCalculator.CheckIfHexAroundHasTown(allHexes, hex))
                    continue;

                if (hex.HexType != HexType.Ocean)
                    GenerateTown(factions[i], hex);
            }
        }
    }

    private void GenerateLandUnit(Faction faction, Hex hex, int unitId, bool show)//normal land units
    {
        GameObject obj = Instantiate(landUnitPrefab, hex.Pos, Quaternion.identity, faction.UnitParent);
        LandUnit unit = obj.GetComponent<LandUnit>();

        unit.UnitInit(this, UIManager.instance, faction, landUnitData[unitId]);
        unit.SetupPosition(hex);
        unit.ShowHideSprite(show);

        faction.Units.Add(unit);
    }

    private void GeneratePassengerUnit(Faction faction, Hex hex, int unitId, bool show, NavalUnit ship)//ship passengers
    {
        GameObject obj = Instantiate(landUnitPrefab, hex.Pos, Quaternion.identity, ship.PassengerParent.transform);
        LandUnit unit = obj.GetComponent<LandUnit>();

        unit.UnitInit(this, UIManager.instance, faction, landUnitData[unitId]);
        unit.SetupPosition(hex);

        unit.UnitStatus = UnitStatus.OnBoard;
        unit.TransportShip = ship;
        obj.SetActive(false);

        faction.Units.Add(unit);
        ship.Passengers.Add(unit);
    }

    private void GenerateAllNativeUnits()
    {
        for (int i = 5; i < factions.Length; i++)
        {
            foreach (Town town in factions[i].Towns)
            {
                GenerateLandUnit(factions[i], town.CurHex, 3, false); //Tropical Indian
            }
        }
    }

    private void GenerateAllEuropeanExplorerUnits()
    {
        for (int i = 0; i < 5; i++)
        {
            NavalUnit firstShip = factions[i].Units[0].gameObject.GetComponent<NavalUnit>();

            GeneratePassengerUnit(factions[i], firstShip.CurHex, 1, false, firstShip); //Veteran Soldiers
            GeneratePassengerUnit(factions[i], firstShip.CurHex, 2, false, firstShip); //Hardy Pioneers
        }
    }

    public void CheckUnitBuildingSettlement(Faction faction, Unit unit)
    {
        if (unit.UnitStatus == UnitStatus.Building)
        {
            GenerateTown(faction, unit.CurHex);
            unit.UnitStatus = UnitStatus.None;
            unit.ChangeStatusIcon();
        }
    }

    public void CheckUnitClearingLand(Faction faction, Unit unit)
    {
        if (unit.UnitStatus == UnitStatus.Clearing)
        {
            unit.CurHex.ClearForest();
            unit.UnitStatus = UnitStatus.None;
            unit.ChangeStatusIcon();
        }
    }

    private void GenerateBiome(int x, int y, Hex hex, Vector3 hexPos, int defaultTerrain, List<int> otherTerrain)
    {
        int n = Random.Range(1, 101);

        if (n <= 50)
            hex.HexInit(x, y, hexPos, this, defaultTerrain);//Main Biome Land
        else
        {
            int i = Random.Range(0, otherTerrain.Count);
            hex.HexInit(x, y, hexPos, this, otherTerrain[i]);//Other Biome Land
        }
    }

    private void GenerateAllBiomes(int x, int y, Hex hex, Vector3 hexPos)
    {
        int n = Random.Range(1, 101);

        //Arctic
        if ((y >= 0 && y < ARCTICSOUTH) || (y >= ARCTICNORTH && y < HEIGHT))
        {
            GenerateBiome(x, y, hex, hexPos, 8, new List<int> { 5 });//Tundra
        }

        //Tundra
        else if ((y >= ARCTICSOUTH && y < TUNDRASOUTH) || (y >= TUNDRANORTH && y < ARCTICNORTH))
        {
            GenerateBiome(x, y, hex, hexPos, 5, new List<int> { 1 });//Grassland
        }

        //Grassland
        else if ((y >= TUNDRASOUTH && y < GRASSSOUTH) || (y >= GRASSNORTH && y < TUNDRANORTH))
        {
            GenerateBiome(x, y, hex, hexPos, 1, new List<int> { 2, 5 });//Prairie, Tundra
        }

        //Prairie
        else if ((y >= GRASSSOUTH && y < PRAIRIESOUTH) || (y >= PRAIRIENORTH && y < GRASSNORTH))
        {
            GenerateBiome(x, y, hex, hexPos, 2, new List<int> { 1, 3, 4 });//Grassland, Savanna, Plain
        }

        //Savanna
        else if ((y >= PRAIRIESOUTH && y < SAVANNASOUTH) || (y >= SAVANNANORTH && y < PRAIRIENORTH))
        {
            GenerateBiome(x, y, hex, hexPos, 3, new List<int> { 1, 2, 4, 6 });//Grassland, Prairie, Plain, Desert
        }

        //Tropical
        else if ((y >= SAVANNASOUTH && y < TROPICALSOUTH) || (y >= 30 && y < SAVANNANORTH))
        {
            GenerateBiome(x, y, hex, hexPos, 4, new List<int> { 1, 7 });//Plain, Swamp
        }

        //**Special Conditions**
        //Hills
        if (n > 80)
        {
            hex.ClearForest();
            GenerateBiome(x, y, hex, hexPos, 9, new List<int> { 10 });//Mountains
        }
    }

    public void SelectPlayerFirstUnit()
    {
        if (playerFaction.Units.Count > 0)
        {
            Unit firstUnit = playerFaction.Units[0];

            if (firstUnit.UnitStatus == UnitStatus.Hidden)
                return;

            SelectPlayerUnit(firstUnit);
            CameraController.instance.MoveCamera(firstUnit.CurPos);
        }
    }

    public void SetupCurrentTown(Town town)//setup before open town panel
    {
        if (UIManager.instance.TownPanel.activeInHierarchy)
            return;

        curTown = town;
        Hex[] aroundHexes = HexCalculator.GetHexAroundToArray(allHexes, curTown.CurHex);

        UIManager.instance.ToggleTownPanel(true);
        UIManager.instance.SetupCurrentTown(curTown.CurHex, aroundHexes);
    }

    public void HideOtherLandUnits(Unit thisUnit)
    {
        if (thisUnit.CurHex == null)
            return;

        foreach(Unit other in thisUnit.CurHex.UnitsInHex)
        {
            if (other.UnitType == UnitType.Land && other.Faction == playerFaction)
            {
                if (other != thisUnit)
                    other.gameObject.SetActive(false);
            }
        }
    }

    public void ShowFirstOfOtherUnit(Hex hex)
    {
        if (hex.UnitsInHex.Count == 0)
            return;

        foreach (Unit other in hex.UnitsInHex)
        {
            if (other.UnitStatus != UnitStatus.WorkInField)
            {
                other.gameObject.SetActive(true);
                break;
            }
        }
    }

    public NavalUnit CheckIfHexHasOurShipToBoard(Hex hex, Faction faction)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitType == UnitType.Naval && unit.Faction == faction)
            {
                NavalUnit ship = (NavalUnit)unit;
                if ((ship.Passengers.Count + ship.CargoList.Count) < ship.CargoHoldNum)
                    return ship;
            }
        }
        return null;
    }

    public void AccumulateStockAllTowns()
    {
        for (int i = 0; i < factions.Length; i++)
        {
            for (int j = 0; j < factions[i].Towns.Count; j++)
            {
                factions[i].Towns[j].AccumulateToWarehouse();
            }
        }
    }

    public void StartNewTurn()
    {
        playerTurn = true;
        SelectPlayerFirstUnit();
        AccumulateStockAllTowns();
        DialogManager.instance.ToggleAiMoveDialog(false);
    }

    public void EuropePanel()
    {
        UIManager.instance.CheckEuropePanel();
    }

    public void CheckShipToEurope()
    {
        //Debug.Log("Checking Ship");
        foreach (Unit unit in playerFaction.Units)
        {
            if (unit.UnitType == UnitType.Naval && unit.CurHex != null)
            {
                if (unit.CurHex.X == WIDTH - 1)
                {
                    NavalUnit ship = (NavalUnit)unit;
                    CameraController.instance.MoveCamera(unit.CurPos);

                    //Debug.Log("Go to Europe?");
                    EuropeManager.instance.QuestionGoToEurope(ship);
                }
            }
        }
    }

    public void CheckToGenerateShipFromEurope(NavalUnit ship)
    {
        int y = Random.Range(0, HEIGHT);

        if (!playerFaction.Units.Contains(ship))
        {
            Debug.Log("New Ship");
            //GenerateEuropeanShip(playerFaction, y, (int)ship.NavalUnitType);
        }
        else
        {
            Debug.Log("Old Ship");
            MoveEuropeanShip(ship);
        }
    }

    private void MoveEuropeanShip(NavalUnit ship)
    {
        int x = WIDTH - 2; //near right edge of a map
        int y = Random.Range(0, HEIGHT);
        Hex hex = allHexes[x, y];

        Debug.Log($"Position: {hex.Pos}");

        ship.SetupPosition(hex);
        ship.gameObject.SetActive(true);
        ship.gameObject.transform.position = ship.CurPos;

        ClearDarkFogAroundUnit(ship);
        SelectPlayerUnit(ship);
        CameraController.instance.MoveCamera(ship.CurPos);
        ship.Visible = true;
        ship.UnitStatus = UnitStatus.None;
    }

    private IEnumerator ShowCasualtyReportWait(float seconds)
    {
        //attacking animation
        yield return new WaitForSeconds(seconds);
        Debug.Log("Show Combat Dialog");
        DialogManager.instance.ToggleCombatPanel(true);
    }

    private void CalculateCombatLogic(Unit ourUnit, Unit enemyUnit)
    {
        if (ourUnit.Strength > enemyUnit.Strength)
        {
            winner = ourUnit;
            loser = enemyUnit;
        }
        else if (ourUnit.Strength < enemyUnit.Strength)
        {
            winner = enemyUnit;
            loser = ourUnit;
        }
        else//draw
        {
            int n = Random.Range(0, 2);

            if (n == 0)
            {
                winner = ourUnit;
                loser = enemyUnit;
            }
            else
            {
                winner = enemyUnit;
                loser = ourUnit;
            }
        }
    }

    public void SetupDeadUnit(Unit deadUnit)
    {
        Faction faction = deadUnit.Faction;
        Hex hex = deadUnit.CurHex;

        if (deadUnit.UnitType == UnitType.Naval)
        {
            NavalUnit ship = (NavalUnit)deadUnit;

            foreach (LandUnit passenger in ship.Passengers)
            {
                if (hex.UnitsInHex.Contains(passenger))
                    hex.UnitsInHex.Remove(passenger);

                faction.Units.Remove(passenger);
                Destroy(passenger.gameObject);
            }
        }

        if (hex.UnitsInHex.Contains(deadUnit))
            hex.UnitsInHex.Remove(deadUnit);

        faction.Units.Remove(deadUnit);
        Destroy(deadUnit.gameObject);
    }

    private IEnumerator ShowUnitDies(float seconds, Unit winner, Unit loser)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Hide Combat Dialog");
        DialogManager.instance.ToggleCombatPanel(false);
        
        //loser dies animation
        Debug.Log("Animation");
        
        SetupDeadUnit(loser);
        ShowMilitaryResultDialog();
    }

    private void ShowMilitaryResultDialog()
    {
        DialogManager.instance.ShowCombatResult(winner, loser);
    }

    public void StartCombat(Unit attacker, Unit defender)
    {
        DialogManager.instance.CombatAnalysisInit(attacker, defender);
        StartCoroutine(ShowCasualtyReportWait(0.5f));

        CalculateCombatLogic(attacker, defender);

        StartCoroutine(ShowUnitDies(4f, winner, loser));
    }

    public void CheckUnmetFaction(Unit unit)
    {
        Faction faction = HexCalculator.CheckNeverMetFaction(allHexes, unit);

        if (faction != null)
        {
            curForeignFaction = faction;
            DialogManager.instance.ToggleForeignDialog(true);
        }
    }

    public void ShowFirstKingDialogText()
    {
        DialogManager.instance.ShowFirstKingQuestDialogText();
    }

    //Ship bought from Europe in Map01 as hidden ship at x, y (0, 0)
    public NavalUnit GenerateHiddenShip(int id)
    {
        Hex hex = allHexes[0, 0];

        GameObject obj = Instantiate(navalUnitPrefab, hex.Pos, Quaternion.identity, playerFaction.UnitParent);
        NavalUnit ship = obj.GetComponent<NavalUnit>();

        ship.UnitInit(this, playerFaction, navalUnitData[id]);
        ship.SetupPosition(hex);
        playerFaction.Units.Add(ship);
        ship.UnitStatus = UnitStatus.Hidden;

        return ship;
    }

    public bool CheckShipPriceAndMoney(int i)
    {
        if (playerFaction.Money < navalUnitData[i].price)
            return false;
        else
            return true;
    }

    public void PayShipPrice(int i)
    {
        playerFaction.Money -= navalUnitData[i].price;
    }

    public void CheckIfPassengerReadyToBoardShip(NavalUnit ship, Hex hex, Faction faction)
    {
        List<Unit> unitsToRemove = new List<Unit>();

        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitType == UnitType.Land && unit.UnitStatus == UnitStatus.ToBoard)
            {
                if ((ship.Passengers.Count + ship.CargoList.Count) < ship.CargoHoldNum)
                {
                    LandUnit landUnit = (LandUnit)unit;
                    landUnit.BoardingShip(ship);
                    landUnit.gameObject.SetActive(false);
                    unitsToRemove.Add(unit);
                }
            }
        }

        foreach (Unit unit in unitsToRemove)
            hex.UnitsInHex.Remove(unit);
    }

    public void CheckPassengerArriving(NavalUnit ship, Hex hex, Faction faction)
    {
        foreach (LandUnit unit in ship.Passengers)
        {
            //ArriveAtPort
            unit.UnitStatus = UnitStatus.None;
            unit.ChangeStatusIcon();
            gameObject.transform.parent = faction.UnitParent.transform;
            unit.TransportShip = null;
        }
        ship.Passengers.Clear();
    }

    public bool CheckIfAroundHexHasTown(Hex hex)
    {
        return HexCalculator.CheckIfHexAroundHasTown(allHexes, hex);
    }

    public bool CheckLandUnitPriceAndMoney(int i)
    {
        if (playerFaction.Money < landUnitData[i].price)
            return false;
        else
            return true;
    }

    //Land Unit bought from Europe in Map01 as hidden land unit at x, y (0, 0)
    public LandUnit GenerateHiddenLandUnit(int id)
    {
        Hex hex = allHexes[0, 0];

        GameObject obj = Instantiate(landUnitPrefab, hex.Pos, Quaternion.identity, playerFaction.UnitParent);
        LandUnit unit = obj.GetComponent<LandUnit>();

        unit.UnitInit(this, UIManager.instance, playerFaction, landUnitData[id]);
        unit.SetupPosition(hex);
        playerFaction.Units.Add(unit);
        unit.UnitStatus = UnitStatus.Hidden;

        return unit;
    }

    public void PayUnitPrice(int i)
    {
        playerFaction.Money -= landUnitData[i].price;
    }
}