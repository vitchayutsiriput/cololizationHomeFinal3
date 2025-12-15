using TMPro;
using UnityEngine;
using System.Collections.Generic;


public enum HexType
{
    Ocean,
    Grassland,
    Prairie,
    Savanna,
    Plain,
    Tundra,
    Desert,
    Swamp,
    Arctic,
    Hills,
    Mountains
}

public class Hex : MonoBehaviour
{
    [SerializeField]
    private int x;
    public int X { get { return x; } set { x = value; } }

    [SerializeField]
    private int y;
    public int Y { get { return y; } set { y = value; } }

    [SerializeField]
    private Vector2 pos;
    public Vector2 Pos { get { return pos; } set { pos = value; } }

    [SerializeField]
    private HexType hexType = HexType.Plain;
    public HexType HexType { get { return hexType; } }

    [Header("Basic")]
    [SerializeField] 
    private SpriteRenderer terrainSprite;
    public SpriteRenderer TerrainSprite { get {  return terrainSprite; } }

    [SerializeField]
    private SpriteRenderer forestSprite;
    public SpriteRenderer ForestSprite { get { return forestSprite; } }

    [SerializeField]
    private SpriteRenderer coastSprite;
    public SpriteRenderer CoastSprite { get { return coastSprite; } }

    [Header("Fog of War")]
    [SerializeField]
    private SpriteRenderer fogSprite;

    [SerializeField]
    private SpriteRenderer darkSprite;

    [Header("Terrain")]
    [SerializeField]
    private Sprite[] terrainSprites;

    [Header("Forest")]
    [SerializeField]
    private Sprite[] forestSprites;

    [SerializeField]
    private string hexName;
    public string HexName { get { return hexName; } set { hexName = value; } }

    [SerializeField]
    private int[] resourceYield;
    public int[] ResourceYield { get { return resourceYield; } set { resourceYield = value; } }

    [Header("Special")]
    [SerializeField]
    private bool specialHex;
    public bool SpecialHex { get { return specialHex; } set { specialHex = value; } }

    [Header("Town")]
    [SerializeField]
    private bool hasTown;
    public bool HasTown { get { return hasTown; } set { hasTown = value; } }

    [SerializeField]
    private Town town;
    public Town Town { get { return town; } set { town = value; } }

    [Header("River")]
    [SerializeField]
    private bool hasRiver;

    [Header("Forest")]
    [SerializeField]
    private bool hasForest;
    public bool HasForest { get { return hasForest; } set { hasForest = value; } }

    [SerializeField]
    private int moveCost = 1;
    public int MoveCost { get { return moveCost; } }

    [SerializeField]
    protected bool visible = false;
    public bool Visible { get { return visible; } set { visible = value; } }

    [SerializeField]
    private List<Unit> unitsInHex = new List<Unit>();
    public List<Unit> UnitsInHex { get { return unitsInHex; } set { unitsInHex = value; } }

    [SerializeField]
    private string coastDigit = "000000";

    [SerializeField]
    private TMP_Text hexText;

    [SerializeField]
    private Unit labor;
    public Unit Labor { get { return labor; } set { labor = value; } }

    [SerializeField]
    private int yieldId = -1; //current ID of resource yield that labor is working
    public int YieldID { get { return yieldId; } set { yieldId = value; } }

    private GameManager gameMgr;
    private UIManager uiMgr;

    void Awake()
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseOver()
    {
        if (gameMgr.CurUnit == null)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            if (gameMgr.CurUnit.CurHex == null)
                return;

            //Debug.Log($"Hex:{x}, {y}");
            if (gameMgr.CheckIfHexIsAdjacent(gameMgr.CurUnit.CurHex, this))
            {
                //Debug.Log("True");
                gameMgr.CurUnit.PrepareMoveToHex(this);
            }
        }
    }

    public void HexInit(int x, int y, Vector2 pos, GameManager gameMgr, int i)
    {
        this.x = x;
        this.y = y;
        this.pos = pos;
        this.gameMgr = gameMgr;

        hexText.text = $"{x},{y}";

        hexName = gameMgr.HexData[i].hexName;
        hexType = gameMgr.HexData[i].type;
        terrainSprites = gameMgr.HexData[i].terrainSprites;
        forestSprites = gameMgr.HexData[i].forestSprites;
        resourceYield = gameMgr.HexData[i].resourceYield;
        moveCost = gameMgr.HexData[i].moveCost;

        //Debug.Log($"{x},{y}: {hexType}");
        RandomTerrainSprite(terrainSprites);

        //85% forest
        int n = Random.Range(1, 101);

        if (n <= 85)
        {
            if (forestSprites.Length > 0)
            {
                RandomForestSprite(forestSprites);
                hasForest = true;
                moveCost += 1;
            }
        }
    }

    private void RandomTerrainSprite(Sprite[] sprites)
    {
        int i = Random.Range(0, sprites.Length);
        terrainSprite.sprite = sprites[i];
    }

    private void RandomForestSprite(Sprite[] sprites)
    {
        int i = Random.Range(0, sprites.Length);
        forestSprite.gameObject.SetActive(true);
        forestSprite.sprite = sprites[i];
    }

    public void ToggleAllBasicText(bool flag)
    {
        hexText.gameObject.SetActive(flag);
    }

    private void ToggleFog(bool flag)
    {
        fogSprite.gameObject.SetActive(flag);
    }

    private void ToggleDark(bool flag)
    {
        darkSprite.gameObject.SetActive(flag);
    }

    public void DiscoverHex()
    {
        //Debug.Log($"Discover:{x}:{y}");

        ToggleFog(false);
        ToggleDark(false);
        visible = true;
    }

    public void SeenHex()
    {
        ToggleFog(true);
        ToggleDark(false);
        visible = false;
    }

    public void ClearForest()
    {
        hasForest = false;
        forestSprite.gameObject.SetActive(false);
    }
}
