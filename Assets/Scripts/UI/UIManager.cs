using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject townPanel;
    public GameObject TownPanel { get { return townPanel; } set { townPanel = value; } }

    [SerializeField]
    private TerrainSlot centerSlot;

    [SerializeField]
    private TerrainSlot[] areaSlots;

    [SerializeField]
    private TerrainSlot currentSlot; //current Slot a player right click to change yield

    [SerializeField]
    private GameObject unitDragPrefab;

    [SerializeField]
    private GameObject outsideTownParent;

    [SerializeField]
    private GameObject yieldIconPrefab;
    public GameObject YieldIconPrefab { get { return yieldIconPrefab; } }

    [SerializeField]
    private List<GameObject> allUnitDrags;
    public List<GameObject> AllUnitDrags { get { return allUnitDrags; } set { allUnitDrags = value; } }

    [SerializeField]
    private GameObject blockImage;

    [SerializeField]
    private GameObject professionPanel;

    [SerializeField]
    private TMP_Text labelQuestionText;

    [SerializeField]
    private TMP_Text[] btnYieldTexts;

    [SerializeField]
    private Transform foodParent;

    [SerializeField]
    private TMP_Text foodText;

    [SerializeField]
    private List<GameObject> foodIconList = new List<GameObject>();

    [SerializeField]
    private GameObject shipInPortPrefab;

    [SerializeField]
    private GameObject portShipsParent;

    [SerializeField]
    private List<GameObject> allShipIcons;
    public List<GameObject> AllShipIcons { get { return allShipIcons; } set { allShipIcons = value; } }

    [SerializeField]
    private ShipInPort currentShipIcon; //current ship icon a player has selected

    [SerializeField]
    private StockSlot[] stockSlots; //town's warehouse slot

    [SerializeField]
    private CargoSlot[] cargoSlots; //ship's cargo hold
    public CargoSlot[] CargoSlots { get { return cargoSlots; } }

    [SerializeField]
    private GameObject stockDragPrefab; //icon dragged from town's stock
    public GameObject StockDragPrefab { get { return stockDragPrefab; } }

    [SerializeField]
    private GameObject cargoDragPrefab; //icon dragged from ship's cargo

    [SerializeField]
    private TMP_Text moneyText;

    [SerializeField]
    private TMP_Text moneyEuropeText;

    [Header("Europe")]
    [SerializeField]
    private GameObject europePanel;

    [SerializeField]
    private GameObject toEuropeShipsParent;

    [SerializeField]
    private GameObject fromEuropeShipsParent;

    [SerializeField]
    private List<GameObject> allShipToEuropeIcons;

    [SerializeField]
    private List<GameObject> allShipFromEuropeIcons;

    [SerializeField]
    private List<GameObject> allShipInEuropeIcons;
    public List<GameObject> AllShipInEuropeIcons { get { return allShipInEuropeIcons; } }

    [SerializeField]
    private GameObject europePortShipsParent;

    [SerializeField]
    private CargoSlot[] cargoSlotsEurope; //ship's cargo hold (Europe)
    public CargoSlot[] CargoSlotsEurope { get { return cargoSlotsEurope; } }

    [SerializeField]
    private StockSlot[] stockSlotsEurope; //Europe's port slot

    [SerializeField]
    private bool inEurope;
    public bool InEurope { get { return inEurope; } }

    [SerializeField]
    private GameObject shipDragPrefab; //ship icon drag
    public GameObject ShipDragPrefab { get { return shipDragPrefab; } }

    [SerializeField]
    private GameObject purchaseShipPanel;
    public GameObject PurchaseShipPanel { get { return purchaseShipPanel; } }

    [SerializeField]
    private LandUnit curLandUnit;
    public LandUnit CurLandUnit { get { return curLandUnit; } set { curLandUnit = value; } }

    [SerializeField]
    private GameObject orderPanel;
    public GameObject OrderPanel { get { return orderPanel; } set { orderPanel = value; } }

    [SerializeField]
    private GameObject trainUnitPanel;
    public GameObject TrainUnitPanel { get { return trainUnitPanel; } }

    [SerializeField]
    private GameObject europePortUnitParent;

    [SerializeField]
    private List<GameObject> waitingLandUnitsInEuropeIcons; //icons of people waiting at the port
    public List<GameObject> WaitingLandUnitsInEuropeIcons { get { return waitingLandUnitsInEuropeIcons; } }

    [SerializeField]
    private UnitDrag curUnitIcon; //current land Unit icon a player has selected
    public UnitDrag CurUnitIcon { get { return curUnitIcon; } set { curUnitIcon = value; } }

    public static UIManager instance;

    void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EndTurn()
    {
        GameManager.instance.Endturn();
    }

    public void ToggleTownPanel(bool show)
    {
        if (show == false)
        {
            DestroyOldUnitDrag();
            RemoveAllYieldIcons();
            HideUnitWorkInTown(GameManager.instance.CurTown.CurHex);
            DeleteAllCargoDragsInSlots(cargoSlots);
            DestroyAllShipIcons();
            DisableAllCargoSlots(cargoSlots);
        }
        townPanel.SetActive(show);
    }

    public void SetupHexSlots(Hex centerHex, Hex[] aroundHexes)
    {
        centerSlot.HexSlotInit(centerHex);

        //setup auto Food production on CenterSlot
        //Debug.Log("Setup Center Slot");

        if (centerSlot.Hex.YieldID == -1)
            centerSlot.SelectYield(0);//Change to Food (Adjust Actual Yield and Accumulate
        else
            centerSlot.AdjustActualYield();//Already Food (Adjust Actual Yield)

        for (int i = 0; i < areaSlots.Length; i++)
        {
            areaSlots[i].HexSlotInit(aroundHexes[i]);
        }
    }

    public void DestroyOldUnitDrag()
    {
        if (inEurope)
        {
            foreach (GameObject obj in waitingLandUnitsInEuropeIcons)
            {
                Destroy(obj);
            }
            waitingLandUnitsInEuropeIcons.Clear();
        }
        else
        {
            foreach (GameObject obj in allUnitDrags)
            {
                Destroy(obj);
            }
            allUnitDrags.Clear();
        }
    }

    private void HideUnitWorkInTown(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitStatus == UnitStatus.WorkInField)
                unit.gameObject.SetActive(false);
        }
    }

    private void DestroyAllShipIcons()
    {
        foreach (GameObject obj in allShipIcons)
        {
            Destroy(obj);
        }
        allShipIcons.Clear();
    }

    public void SetupUnitDragOutsideTown(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitType == UnitType.Land) 
            {
                if (unit.UnitStatus == UnitStatus.None || unit.UnitStatus == UnitStatus.ToBoard)
                {
                    GameObject unitObj = Instantiate(unitDragPrefab, outsideTownParent.transform);
                    allUnitDrags.Add(unitObj);

                    UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
                    unitDrag.UnitInit((LandUnit)unit);
                }
            }
        }
    }

    public void SetupUnitDragWorkingInTerrain()
    {
        foreach (TerrainSlot terrainSlot in areaSlots)
        {
            if (terrainSlot.Hex == null)
                continue;

            if (terrainSlot.Hex.Labor != null)
            {
                GameObject unitObj = Instantiate(unitDragPrefab, terrainSlot.LaborParent);
                allUnitDrags.Add(unitObj);

                UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
                unitDrag.UnitInit((LandUnit)terrainSlot.Hex.Labor);

                unitDrag.IconParent = terrainSlot.LaborParent;
                unitDrag.WorkAtNewTerrainSlot(terrainSlot);
            }
        }
    }

    public void RemoveAllYieldIcons()
    {
        centerSlot.RemoveYieldIcons();

        foreach (TerrainSlot terrainSlot in areaSlots)
        {
            if (terrainSlot == null)
                continue;

            terrainSlot.RemoveYieldIcons();
        }
    }

    public void SetupCurrentTown(Hex curHex, Hex[] aroundHexes)//open Town Panel
    {
        //Terrain
        SetupHexSlots(curHex, aroundHexes);

        //Check passenger units arrived
        CheckShipArriveWithPassenger(curHex);

        //Available Workers
        SetupUnitDragOutsideTown(curHex);
        
        //Working Workers
        SetupUnitDragWorkingInTerrain();
        SetupYieldInTerrain();

        //Town Info
        UpdateTotalFoodIcons();
        SetupStockSlots(curHex);

        //Ship
        SetupShipsInPort(curHex);
        SetupShipsCargoSlot(curHex);

        //Text
        UpdateMoneyText();
    }

    private void SetupYieldInTerrain()
    {
        //Debug.Log("Setup Around Slot");

        foreach (TerrainSlot terrainSlot in areaSlots)
        {
            if (terrainSlot.Hex == null)
                continue;

            if (terrainSlot.Hex.Labor != null && terrainSlot.Hex.YieldID != -1)
            {
                terrainSlot.AdjustActualYield();
            }
        }
    }

    public void SelectProfession(TerrainSlot slot)
    {
        currentSlot = slot;

        UpdateLabelQuestionText();
        UpdateButtonTextsYield();

        blockImage.SetActive(true);
        professionPanel.SetActive(true);
    }

    public void SelectYield(int i)//Link to Select Profession Button on UI
    {
        //Debug.Log($"Select: {i}");

        if (currentSlot != null)
            currentSlot.SelectYield(i);

        blockImage.SetActive(false);
        professionPanel.SetActive(false);

        if (currentSlot.Hex.YieldID == 0)
            UpdateTotalFoodIcons();
    }

    public void UpdateLabelQuestionText()
    {
        if (currentSlot == null)
            return;

        string s = string.Format("Select a profession for {0}", currentSlot.Hex.Labor.UnitName);
        labelQuestionText.text = s;
    }

    public void UpdateButtonTextsYield()
    {
        if (currentSlot == null)
            return;

        for (int i = 0; i < btnYieldTexts.Length; i++)
        {
            string s = string.Format("{0} {1}", 
                currentSlot.NormalYield[i], GameManager.instance.ProductData[i].productName);

            btnYieldTexts[i].text = s;
        }
    }

    public void SetupParentSpacing(int n, Transform parent, int iconWidth, int parentWidth)
    {
        if (n <= 1)
            return;

        HorizontalLayoutGroup layout = parent.GetComponent<HorizontalLayoutGroup>();
        int totalWidth = iconWidth * n;
        int excessWidth = totalWidth - parentWidth;

        if (excessWidth <= 0)
            return;

        int result = excessWidth / (n - 1);
        layout.spacing = -result;
    }

    public GameObject GenerateFoodIcon()
    {
        GameObject foodObj = Instantiate(yieldIconPrefab, foodParent);
        Image iconImg = foodObj.GetComponent<Image>();

        iconImg.sprite = GameManager.instance.ProductData[0].icons[0];

        return foodObj;
    }

    public void UpdateTotalFoodIcons()
    {
        foreach (GameObject obj in foodIconList)
            Destroy(obj);

        foodIconList.Clear();
        //Debug.Log($"Food This Turn:{GameManager.instance.CurTown.TotalYieldThisTurn[0]}");
        foodText.text = GameManager.instance.CurTown.TotalYieldThisTurn[0].ToString();
        //Debug.Log($"Update Food Text:{foodText.text}");
        foodText.gameObject.SetActive(true);

        for (int i = 0; i < GameManager.instance.CurTown.TotalYieldThisTurn[0]; i++)
        {
            GameObject iconobj = GenerateFoodIcon();
            foodIconList.Add(iconobj);
        }
        SetupParentSpacing(
            GameManager.instance.CurTown.TotalYieldThisTurn[0], foodParent, 64, 300);
    }

    private void SetupShipsInPort(Hex hex) //in New World
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitType == UnitType.Naval)
            {
                GameObject unitObj = Instantiate(shipInPortPrefab, portShipsParent.transform);
                allShipIcons.Add(unitObj);

                ShipInPort shipIcon = unitObj.GetComponent<ShipInPort>();
                shipIcon.UnitInit((NavalUnit)unit, false);
            }
        }
    }

    public void SetupStockSlots(Hex hex)
    {
        for (int i = 0; i < stockSlots.Length; i++)
        {
            stockSlots[i].stockInit(i, hex.Town, inEurope);
        }
    }

    public void CheckActiveUIPanel()
    {
        if (townPanel.activeInHierarchy)
            ToggleTownPanel(false);

        if (europePanel.activeInHierarchy)
            ToggleEuropePanel(false);
    }

    public void SetupCargoSlot(NavalUnit ship, CargoSlot[] cargoSlots)
    {
        for (int i = 0; i < cargoSlots.Length; i++)
        {
            if (i < ship.CargoHoldNum)
            {
                cargoSlots[i].gameObject.SetActive(true);
                cargoSlots[i].SlotInit(ship, i);
            }
            else
                cargoSlots[i].gameObject.SetActive(false);
        }
    }

    private void SetupShipsCargoSlot(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitType == UnitType.Naval)
            {
                //first ship
                NavalUnit firstShip = unit.gameObject.GetComponent<NavalUnit>();
                SetupCargoSlot(firstShip, cargoSlots);
                UpdateCargoSlots(firstShip, cargoSlots);

                break;
            }
        }
    }

    private void DeleteAllCargoDragsInSlots(CargoSlot[] cargoSlots)
    {
        foreach (CargoSlot slot in cargoSlots)
        {
            if (slot.CargoDrag != null)
            {
                Destroy(slot.CargoDrag.gameObject);
                slot.CargoDrag = null;
            }
        }
    }

    public void UpdateCargoSlots(NavalUnit ship, CargoSlot[] cargoSlots) //Update icon in ship's hold
    {
        DeleteAllCargoDragsInSlots(cargoSlots);

        for (int i = 0; i < ship.CargoList.Count; i++)
        {
            GameObject cargoDragObj =
                Instantiate(cargoDragPrefab, cargoSlots[i].transform.position, Quaternion.identity, cargoSlots[i].transform);

            CargoDrag cargoDrag = cargoDragObj.GetComponent<CargoDrag>();
            cargoSlots[i].CargoDrag = cargoDrag;

            int productId = ship.CargoList[i].ProductID;
            Sprite icon = GameManager.instance.ProductData[productId].icons[0];

            cargoDrag.CargoDragInit(ship, icon, i);
        }
    }

    public void ToggleStockDragRaycast(bool flag)
    {
        //Debug.Log($"All raycast:{flag}");

        StockSlot[] slots;

        if (inEurope)
            slots = stockSlotsEurope;
        else
            slots = stockSlots;

        foreach (StockSlot slot in slots)
        {
            slot.ToggleRayCastStockDrag(flag);
        }
    }

    public void CheckEuropePanel()
    {
        if (europePanel.activeInHierarchy)
            ToggleEuropePanel(false);
        else
            ToggleEuropePanel(true);
    }

    public void SetupShipsInEurope(List<NavalUnit> ships)
    {
        foreach (Unit unit in ships)
        {
            if (unit.UnitType == UnitType.Naval)
            {
                GameObject unitObj = Instantiate(shipInPortPrefab, europePortShipsParent.transform);
                allShipInEuropeIcons.Add(unitObj);

                ShipInPort shipIcon = unitObj.GetComponent<ShipInPort>();
                shipIcon.UnitInit((NavalUnit)unit, true);
            }
        }
    }

    public void DestroyShipIcons(List<GameObject> shipList)
    {
        foreach (GameObject obj in shipList)
        {
            Destroy(obj);
        }
        shipList.Clear();
    }

    private void DestroyAllShipsEuropeIcons()
    {
        DestroyShipIcons(allShipToEuropeIcons);
        DestroyShipIcons(allShipFromEuropeIcons);
        DestroyShipIcons(allShipInEuropeIcons);
    }

    //Setup first ship's cargo slot in Europe
    private void SetupShipsCargoSlotEurope(List<NavalUnit> ships)
    {
        foreach (Unit unit in ships)
        {
            if (unit.UnitType == UnitType.Naval)
            {
                //first ship
                NavalUnit firstShip = unit.gameObject.GetComponent<NavalUnit>();
                SetupCargoSlot(firstShip, cargoSlotsEurope);
                UpdateCargoSlots(firstShip, cargoSlotsEurope);

                break;
            }
        }
    }

    public void ToggleEuropePanel(bool flag)
    {
        if (flag == false)
        {
            DestroyAllShipsEuropeIcons();
            DisableAllCargoSlots(cargoSlotsEurope);
            europePanel.SetActive(false);
            inEurope = false;
        }
        else
        {
            townPanel.SetActive(false);
            europePanel.SetActive(true);
            inEurope = true;

            SetupShipsToEurope(EuropeManager.instance.ShipsToEurope);
            SetupShipsFromEurope(EuropeManager.instance.ShipsFromEurope);
            SetupShipsInEurope(EuropeManager.instance.ShipsInEurope);
            SetupShipsCargoSlotEurope(EuropeManager.instance.ShipsInEurope);
            DestroyOldUnitDrag();
            SetupUnitDragInEuropePort();
            SetupStockSlotsEurope();
            UpdateMoneyEuropeText();
        }
    }

    public void SetupStockSlotsEurope()
    {
        for (int i = 0; i < stockSlotsEurope.Length; i++)
        {
            stockSlotsEurope[i].stockInitEurope(i);
        }
    }

    private void SetupShipsToEurope(List<ShipInTransit> shipsToEU)
    {
        foreach (ShipInTransit shipInTransit in shipsToEU)
        {
            GameObject unitObj = Instantiate(shipInPortPrefab, toEuropeShipsParent.transform);
            allShipToEuropeIcons.Add(unitObj);

            ShipInPort shipIcon = unitObj.GetComponent<ShipInPort>();
            shipIcon.UnitInit(shipInTransit.Ship, false);
            shipIcon.UpdateTurnText(shipInTransit.TurnLeft);
        }
    }

    private void SetupShipsFromEurope(List<ShipInTransit> shipsToEU)
    {
        foreach (ShipInTransit shipInTransit in shipsToEU)
        {
            GameObject unitObj = Instantiate(shipInPortPrefab, fromEuropeShipsParent.transform);
            allShipFromEuropeIcons.Add(unitObj);

            ShipInPort shipIcon = unitObj.GetComponent<ShipInPort>();
            shipIcon.UnitInit(shipInTransit.Ship, false);
            shipIcon.UpdateTurnText(shipInTransit.TurnLeft);
        }
    }

    public void UpdateIconsFromEuropeToNewWorld()
    {
        DestroyShipIcons(allShipFromEuropeIcons);
        SetupShipsFromEurope(EuropeManager.instance.ShipsFromEurope);

        DestroyShipIcons(AllShipInEuropeIcons);
        SetupShipsInEurope(EuropeManager.instance.ShipsInEurope);

        DeleteAllCargoDragsInSlots(cargoSlotsEurope);
        DisableAllCargoSlots(cargoSlotsEurope);
        SetupShipsCargoSlotEurope(EuropeManager.instance.ShipsInEurope);
    }

    private void DisableAllCargoSlots(CargoSlot[] cargoSlots)
    {
        foreach (CargoSlot slot in cargoSlots)
        {
            slot.gameObject.SetActive(false);
        }
    }

    public void UpdateMoneyText()
    {
        moneyText.text = $"{GameManager.instance.PlayerFaction.Money}";
    }

    public void UpdateMoneyEuropeText()
    {
        moneyEuropeText.text = $"{GameManager.instance.PlayerFaction.Money}";
    }

    public void ShowColonyClearLandText()
    {
        DialogManager.instance.ShowColonyClearLandText();
    }

    public void ShowColonyNotEnoughToolsText(int n)
    {
        DialogManager.instance.ShowColonyNotEnoughToolsText(n);
    }

    public void ShowColonyHasOtherTownAroundText()
    {
        DialogManager.instance.ShowColonyHasOtherTownAround();
    }

    public void AddNewShipInEurope(int i)
    {
        if (!GameManager.instance.CheckShipPriceAndMoney(i))
            return;

        NavalUnit ship = GameManager.instance.GenerateHiddenShip(i);
        EuropeManager.instance.ShipsInEurope.Add(ship);

        //Hide ship game object in map01
        ship.gameObject.SetActive(false);

        GameObject unitObj = Instantiate(shipInPortPrefab, europePortShipsParent.transform);
        allShipInEuropeIcons.Add(unitObj);

        ShipInPort shipIcon = unitObj.GetComponent<ShipInPort>();
        shipIcon.UnitInit(ship, true);

        GameManager.instance.PayShipPrice(i);
        UpdateMoneyEuropeText();
        TogglePurchasePanel(false);

        SetupSelectedShipsCargoSlotEurope(ship);
    }

    public void TogglePurchasePanel(bool flag)
    {
        purchaseShipPanel.SetActive(flag);
    }

    //Setup selected ship's cargo slot in New World
    public void SetupSelectedShipsCargoSlot(NavalUnit ship)
    {
        SetupCargoSlot(ship, cargoSlots);
        UpdateCargoSlots(ship, cargoSlots);
    }

    //Setup selected ship's cargo slot in Europe
    public void SetupSelectedShipsCargoSlotEurope(NavalUnit ship)
    {
        SetupCargoSlot(ship, cargoSlotsEurope);
        UpdateCargoSlots(ship, cargoSlotsEurope);
    }

    public void CheckShipArriveWithPassenger(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitType == UnitType.Naval)
            {
                NavalUnit ship = (NavalUnit)unit;
                GameManager.instance.CheckPassengerArriving(ship, hex, ship.Faction);
            }
        }
    }

    public void ToggleOrderPanel(bool flag)
    {
        blockImage.SetActive(flag);
        orderPanel.SetActive(flag);
    }

    private void SwapUnitMemberInTownToTheFront(List<Unit> unitList)
    {
        if (curLandUnit == null)
            return;

        int i = unitList.IndexOf(curLandUnit);

        if (i == -1)
            return;

        Unit unit = unitList[i];

        unitList.RemoveAt(i);
        unitList.Insert(0, unit);
    }

    private void SwapUnitMemberInEuropeToTheFront(List<LandUnit> landUnitList)
    {
        if (curLandUnit == null)
            return;

        int i = landUnitList.IndexOf(curLandUnit);

        if (i == -1)
            return;

        LandUnit landUnit = landUnitList[i];

        landUnitList.RemoveAt(i);
        landUnitList.Insert(0, landUnit);
    }

    private void MoveToTheFront()
    {
        if (inEurope)
            SwapUnitMemberInEuropeToTheFront(EuropeManager.instance.LandUnitsInEurope);
        else
            SwapUnitMemberInTownToTheFront(GameManager.instance.CurTown.CurHex.UnitsInHex);
    }

    public void SetOrderForLandUnit(int i) //Map with Order Button
    {
        if (curLandUnit == null)
            return;

        switch (i)
        {
            case 0:
                curLandUnit.UnitStatus = UnitStatus.None;
                curUnitIcon.CheckStatusIcon();
                break;
            case 1:
                curLandUnit.UnitStatus = UnitStatus.ToBoard;
                curUnitIcon.CheckStatusIcon();
                break;
            case 2:
                MoveToTheFront();
                DestroyOldUnitDrag();

                if (inEurope)
                    SetupUnitDragInEuropePort();
                else
                {
                    SetupUnitDragOutsideTown(GameManager.instance.CurTown.CurHex);
                    SetupUnitDragWorkingInTerrain();
                }
                break;
        }
        ToggleOrderPanel(false);
    }

    public void ToggleTrainPanel(bool flag)
    {
        trainUnitPanel.SetActive(flag);
    }

    public void CreateUnitDragInEuropePort(int i) //Map with Train Each Unit Button
    {
        if (!GameManager.instance.CheckLandUnitPriceAndMoney(i))
            return;

        LandUnit landUnit = GameManager.instance.GenerateHiddenLandUnit(i);
        EuropeManager.instance.LandUnitsInEurope.Add(landUnit);

        //Hide unit's game object in map01
        landUnit.gameObject.SetActive(false);

        GameObject unitObj = Instantiate(unitDragPrefab, europePortUnitParent.transform);
        waitingLandUnitsInEuropeIcons.Add(unitObj);

        UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
        unitDrag.UnitInit(landUnit);

        GameManager.instance.PayUnitPrice(i);
        UpdateMoneyEuropeText();
        ToggleTrainPanel(false);
    }

    public void SetupUnitDragInEuropePort()
    {
        foreach (LandUnit unit in EuropeManager.instance.LandUnitsInEurope)
        {
            if (unit.UnitType == UnitType.Land)
            {
                if (unit.UnitStatus == UnitStatus.None || unit.UnitStatus == UnitStatus.ToBoard
                    || unit.UnitStatus == UnitStatus.Hidden)
                {
                    GameObject unitObj = Instantiate(unitDragPrefab, europePortUnitParent.transform);
                    waitingLandUnitsInEuropeIcons.Add(unitObj);

                    UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
                    unitDrag.UnitInit(unit);
                    unitDrag.CheckStatusIcon();
                }
            }
        }
    }
}
