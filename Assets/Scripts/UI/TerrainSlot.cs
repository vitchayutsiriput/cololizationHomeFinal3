using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class TerrainSlot : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private Hex hex;
    public Hex Hex { get { return hex; } }

    [SerializeField]
    private Image terrainImage;

    [SerializeField]
    private Image forestImage;

    [SerializeField]
    private Image townImage;

    [SerializeField]
    private Transform yieldParent;

    [SerializeField]
    private Transform laborParent;
    public Transform LaborParent { get { return laborParent; } }

    //0-food, 1-sugar, 2-tobacco 3-cotton 4-fur 5-lumber 6-ore 7-silver
    [SerializeField]
    private int[] normalYield = new int[8];
    public int[] NormalYield { get { return normalYield; } set { normalYield = value; } }

    [SerializeField]
    private int[] actualYield = new int[8]; //Actual Yield labor is currently working
    public int[] ActualYield { get { return actualYield; } set { actualYield = value; } }

    [SerializeField]
    private bool centerHex;

    [SerializeField]
    private TMP_Text yieldText;

    [SerializeField]
    private List<GameObject> yieldIconList = new List<GameObject>();
    public List<GameObject> YieldIconList { get { return yieldIconList; } set { yieldIconList = value; } }

    [SerializeField]
    private GameManager gameMgr;
    [SerializeField]
    private UIManager uiMgr;

    void Awake()
    {
        gameMgr = GameManager.instance;
        uiMgr = UIManager.instance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (centerHex || hex.Labor != null)
            return;

        //Debug.Log($"On Drop:{hex.X},{hex.Y}:{hex.HexType}");
        GameObject unitObj = eventData.pointerDrag;
        UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();

        if (unitDrag == null)
            return;

        unitDrag.QuitOldTerrainSlot(); //old slot remove this labor
        unitDrag.WorkAtNewTerrainSlot(this); //this slot is remembered in this labor

        unitDrag.IconParent = laborParent;
        unitObj.transform.position = laborParent.position;

        unitDrag.LandUnit.UnitStatus = UnitStatus.WorkInField;
        hex.Labor = unitDrag.LandUnit;

        uiMgr.SelectProfession(this);
    }

    public void SelectYield(int i)
    {
        hex.YieldID = i;
        AdjustActualYield();
        Accumulate();
    }

    private void Accumulate()
    {
        gameMgr.CurTown.TotalYieldThisTurn[hex.YieldID] += actualYield[hex.YieldID];
    }

    private void SetupNormalYield()
    {
        for (int i = 0; i < 8; i++)
        {
            normalYield[i] = hex.ResourceYield[i]; //Data from Scriptable Object

            if (i == 4 || i == 5) //Fur or Lumber
            {
                if (!hex.HasForest) //No forest
                    normalYield[i] = 0;
            }
            else //Not Fur nor Lumber
            {
                if (hex.HasForest)
                    normalYield[i] -= 2; //Reduce agriculture
            }

            if (hex.HasTown)
                normalYield[i] = Mathf.CeilToInt(normalYield[i]/2f);

            if (normalYield[i] < 0)
                normalYield[i] = 0;
        }
    }

    public void HexSlotInit(Hex hexData)
    {
        if (hexData != null)
        {
            hex = hexData;
            terrainImage.sprite = hex.TerrainSprite.sprite;
            SetupNormalYield();

            if (hex.HasForest)
            {
                forestImage.sprite = hex.ForestSprite.sprite;
                forestImage.gameObject.SetActive(true);
            }

            if (hex.HasTown)
            {
                townImage.sprite = hex.Town.TownSprite.sprite;
                townImage.gameObject.SetActive(true);
            }
        }
    }

    public void ReduceTownYield()
    {
        if (hex.YieldID != -1)
        {
            gameMgr.CurTown.TotalYieldThisTurn[hex.YieldID] -= actualYield[hex.YieldID];
            actualYield[hex.YieldID] = 0;
        }
    }

    public void RemoveYieldIcons()
    {
        foreach (GameObject obj in yieldIconList)
        {
            Destroy(obj);
        }
        yieldIconList.Clear();
        yieldText.gameObject.SetActive(false);

        //ReduceTownYield();

        if (hex.YieldID == 0)
            uiMgr.UpdateTotalFoodIcons();
    }

    public GameObject GenerateYieldIcon(int i)
    {
        GameObject yieldObj = Instantiate(uiMgr.YieldIconPrefab, yieldParent);
        Image iconImg = yieldObj.GetComponent<Image>();

        iconImg.sprite = gameMgr.ProductData[i].icons[0];

        if (i == 0)//Food
        {
            if (hex.HexType == HexType.Ocean)
                iconImg.sprite = gameMgr.ProductData[0].icons[1];//Fish
            else
                iconImg.sprite = gameMgr.ProductData[0].icons[0];//Corn
        }

        return yieldObj;
    }

    public void GenerateYieldIcons(int id)
    {
        //Debug.Log($"Gen Icon of id:{id}, quantiy:{actualYield[id]}");

        yieldText.text = actualYield[id].ToString();
        yieldText.gameObject.SetActive(true);

        for (int i = 0; i < actualYield[id]; i++)
        {
            GameObject iconobj = GenerateYieldIcon(id);
            yieldIconList.Add(iconobj);
        }
        uiMgr.SetupParentSpacing(actualYield[id], yieldParent, 64, 250);
    }

    public void AdjustActualYield()
    {
        ConvertNormalToActualYield(hex.YieldID);

        //accumulate actual yield to total production
        //Debug.Log($"Yield ID:{hex.YieldID}");
        //Debug.Log($"Normal Yield:{normalYield[hex.YieldID]}");
        //Debug.Log($"ID:{hex.YieldID}-Actual Yield:{actualYield[hex.YieldID]} plus to {gameMgr.CurTown.TotalYieldThisTurn[hex.YieldID]}");

        //gameMgr.CurTown.TotalYieldThisTurn[hex.YieldID] += actualYield[hex.YieldID];

        //Debug.Log($"Total Yield this turn:{gameMgr.CurTown.TotalYieldThisTurn[hex.YieldID]}");

        //generate all yield icons in one hex
        GenerateYieldIcons(hex.YieldID);
    }

    public void ConvertNormalToActualYield(int id)
    {
        if (hex.YieldID == -1)
            return;

        //Formula to Adjust NormalYield ***


        //convert normal Yield to actual yield
        actualYield[id] = normalYield[id];
    }
}
