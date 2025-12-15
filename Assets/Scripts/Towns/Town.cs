using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Town : MonoBehaviour
{
    [SerializeField]
    protected string townName;
    public string TownName { get { return townName; } set { townName = value; } }

    [SerializeField]
    protected Faction faction;
    public Faction Faction { get { return faction; } set { faction = value; } }

    [SerializeField]
    protected int visualRange;
    public int VisualRange { get { return visualRange; } }

    [SerializeField]
    private Vector2 curPos;
    public Vector2 CurPos { get { return curPos; } set { curPos = value; } }

    [SerializeField]
    protected Hex curHex;
    public Hex CurHex { get { return curHex; } set { curHex = value; } }

    [Header("Town")]
    [SerializeField]
    protected SpriteRenderer townSprite;
    public SpriteRenderer TownSprite { get { return townSprite; } }

    [Header("Border")]
    [SerializeField]
    protected SpriteRenderer borderSprite;

    [Header("Flag")]
    [SerializeField]
    protected SpriteRenderer flagSprite;

    [SerializeField]
    private int[] warehouse = new int[16];
    public int[] Warehouse { get { return warehouse; } set { warehouse = value; } }

    [SerializeField]
    private int[] totalYieldThisTurn = new int[16]; //no. of all resource production this turn
    public int[] TotalYieldThisTurn { get { return totalYieldThisTurn; } set { totalYieldThisTurn = value; } }

    [SerializeField]
    private int crossNum; //no. of crosses produced in this turn
    public int CrossNum { get { return crossNum; } set { crossNum = value; } }

    [SerializeField]
    private int bellNum; //no. of bells produced in this turn
    public int BellNum { get { return bellNum; } set { bellNum = value; } }

    [SerializeField]
    protected GameManager gameMgr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if (faction == gameMgr.PlayerFaction)
            gameMgr.SetupCurrentTown(this);
    }

    public void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //Debug.Log($"To Move Hex:{curHex.X}, {curHex.Y}");
            if (gameMgr.CheckIfHexIsAdjacent(gameMgr.CurUnit.CurHex, curHex))
            {
                if (faction == gameMgr.PlayerFaction)//same side unit
                {
                    //Debug.Log("True");
                    gameMgr.CurUnit.PrepareMoveToHex(curHex);
                }
                else//diff side unit
                {
                    //Debug.Log($"{gameMgr.CurUnit} Attacks {townName}");
                }
            }
        }
    }

    public void TownInit(GameManager gameMgr, Faction fact)
    {
        this.gameMgr = gameMgr;
        faction = fact;
        flagSprite.sprite = fact.FlagIcon;
        townSprite.sprite = fact.TownIcon;
    }

    public void AccumulateToWarehouse()
    {
        for (int i = 0; i < 16; i++)
        {
            //Debug.Log($"accumulate: {totalYieldThisTurn[i]} to warehouse: {warehouse[i]}");
            warehouse[i] += totalYieldThisTurn[i];
        }
    }
}