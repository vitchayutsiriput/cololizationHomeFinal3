using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum UnitType
{
    Land,
    Naval
}

public enum UnitStatus
{
    None,
    OnBoard,
    Fortified,
    Clearing,
    Building,
    WorkInField,
    WorkInTown,
    Hidden,
    ToBoard
}

public abstract class Unit : MonoBehaviour
{
    [SerializeField]
    protected string unitName;
    public string UnitName { get { return unitName; } }

    [SerializeField]
    protected UnitType unitType;
    public UnitType UnitType { get { return unitType; } }

    [SerializeField]
    protected UnitStatus unitStatus;
    public UnitStatus UnitStatus { get { return unitStatus; } set { unitStatus = value; } }

    [SerializeField]
    protected int strength;
    public int Strength { get { return strength; } set { strength = value; } }

    [SerializeField]
    protected int movePoint;
    public int MovePoint { get { return movePoint; } set { movePoint = value; } }

    [SerializeField]
    protected int movePointMax;
    public int MovePointMax { get { return movePointMax; } }

    [SerializeField]
    protected Faction faction;
    public Faction Faction { get { return faction; } }

    [SerializeField]
    protected int visualRange;
    public int VisualRange { get { return visualRange; } }

    [SerializeField]
    private Vector2 curPos;
    public Vector2 CurPos { get { return curPos; } set { curPos = value; } }

    [SerializeField]
    protected Hex curHex;
    public Hex CurHex { get { return curHex; } set { curHex = value; } }

    [SerializeField]
    protected Hex targetHex;
    public Hex TargetHex { get { return targetHex; } set { targetHex = value; } }

    [SerializeField]
    protected Hex destinationHex;
    public Hex DestinationHex { get { return destinationHex; } set { destinationHex = value; } }

    [SerializeField]
    protected bool isMoving = false;
    public bool IsMoving { get { return isMoving; } set { isMoving = value; } }

    [SerializeField]
    protected bool visible = false;
    public bool Visible { get { return visible; } set { visible = value; } }

    [SerializeField]
    protected Unit targetUnit;
    public Unit TargetUnit { get { return targetUnit; } set { targetUnit = value; } }

    [SerializeField]
    protected int unitPrice;
    public int UnitPrice { get { return unitPrice; } set { unitPrice = value; } }

    [Header("Unit")]
    [SerializeField]
    protected SpriteRenderer unitSprite;
    public SpriteRenderer UnitSprite { get { return unitSprite; } }

    [Header("Flag")]
    [SerializeField]
    protected SpriteRenderer flagSprite;

    [Header("Border")]
    [SerializeField]
    protected SpriteRenderer borderSprite;

    [Header("Status")]
    [SerializeField]
    protected SpriteRenderer statusSprite;
    public SpriteRenderer StatusSprite { get { return statusSprite; } }

    [Header("Status Sprites")]
    [SerializeField]
    private Sprite[] statusSpritesList;
    public Sprite[] StatusSpritesList { get { return statusSpritesList; } }

    [SerializeField]
    protected GameManager gameMgr;

    [SerializeField]
    protected UIManager uiMgr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isMoving == true)
        {
            MoveToHex();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (this == gameMgr.CurUnit && curHex.HasTown)
            {
                if (curHex.Town != null)
                    gameMgr.SetupCurrentTown(curHex.Town);
            }
        }
    }

    private void OnMouseDown()
    {
        //Debug.Log("Mouse Down");
        if (faction == gameMgr.PlayerFaction)
        {
            gameMgr.SelectPlayerUnit(this);
        }
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
                    Debug.Log($"{gameMgr.CurUnit} tries to Attack {unitName}");
                    if (curHex.MoveCost > movePoint)
                        return;

                    gameMgr.CurUnit.AttackUnit(curHex, this);
                }
            }
        }
    }

    public void SetupPosition(Hex hex)
    {
        curHex = hex;
        curPos = hex.Pos;
        if (!hex.UnitsInHex.Contains(this))
            hex.UnitsInHex.Add(this);
    }

    public void ToggleBorder(bool flag, Color32 color)
    {
        //Debug.Log($"{faction}:{unitName}-border:{flag}:{color}");
        borderSprite.gameObject.SetActive(flag);
        borderSprite.color = color;
    }

    public virtual void PrepareMoveToHex(Hex target) //Begin to move by RC or AI auto movement
    {
        //Debug.Log($"MoveCost-{target.MoveCost}");
        //Debug.Log($"UnitMovementP-{movePoint}");

        if (target.MoveCost > movePoint)
            return;

        isMoving = true;
        targetHex = target;
        //Debug.Log($"Target Pos:{targetHex.transform.position.x},{targetHex.transform.position.y}");

        if (faction == gameMgr.PlayerFaction)
            gameMgr.LeaveSeenFogAroundUnit(this);

        gameMgr.ShowFirstOfOtherUnit(curHex);
    }

    protected virtual void StayOnHex(Hex targetHex)
    {
        //Old Hex
        curHex.UnitsInHex.Remove(this);

        isMoving = false;
        curHex = targetHex;
        targetHex = null;
        transform.position = curHex.transform.position; //confirm position to match this hex

        //New Hex
        curHex.UnitsInHex.Add(this);

        if (faction == gameMgr.PlayerFaction)
        {
            gameMgr.ClearDarkFogAroundUnit(this);
            ToggleBorder(true, Color.green);
        }
        gameMgr.HideOtherLandUnits(this);

        if (curHex == destinationHex)
            destinationHex = null;
    }

    private void MoveToHex()
    {
        ToggleBorder(false, Color.green);

        //Debug.Log($"CurPos-{curPos.x}:{curPos.y}");
        //Debug.Log(targetHex);

        transform.position = Vector2.MoveTowards(curPos, targetHex.transform.position, 4 * Time.deltaTime);
        curPos = transform.position;

        if (curPos == targetHex.Pos) //Reach Destination
        {
            movePoint -= targetHex.MoveCost;
            StayOnHex(targetHex);

            if (faction == gameMgr.PlayerFaction)
                gameMgr.ClearDarkFogAroundEveryUnit(faction);
        }
    }

    public void ShowHideSprite(bool flag)
    {
        unitSprite.gameObject.SetActive(flag);
        flagSprite.gameObject.SetActive(flag);
    }

    public void SetUnitToFrontLayerOrder()
    {
        unitSprite.sortingOrder = 5;
        flagSprite.sortingOrder = 6;
    }

    public void SetUnitToNormalLayerOrder()
    {
        unitSprite.sortingOrder = 2;
        flagSprite.sortingOrder = 3;
    }

    public void CheckMoveToDestination()
    {
        if (destinationHex == null)
            return;

        Hex nextHex = null;

        //Find destination hex
        
        Debug.Log($"dest:{destinationHex.X}, {destinationHex.Y}");
        nextHex = HexCalculator.FindNextHexToGo(this, CurHex, destinationHex, gameMgr.AllHexes);

        if (nextHex == null)
            return;

        PrepareMoveToHex(nextHex);
    }

    public void SetUnitDestination(Hex hex)
    {
        destinationHex = hex;
    }

    public void AttackUnit(Hex target, Unit defender) //Begin to Attack
    {
        //Debug.Log($"MoveCost-{target.MoveCost}");
        //Debug.Log($"UnitMovementP-{movePoint}");
        //Debug.Log("Attack");

        if (target.MoveCost > movePoint)
            return;

        //isMoving = true;
        targetHex = target;
        targetUnit = defender;
        //Debug.Log($"Target Pos:{targetHex.transform.position.x},{targetHex.transform.position.y}");

        //Attack Animation

        gameMgr.StartCombat(this, defender);
    }

    public void ChangeStatusIcon()
    {
        if (statusSprite == null)
            return;

        switch (unitStatus)
        {
            case UnitStatus.Clearing:
                statusSprite.gameObject.SetActive(true);
                statusSprite.sprite = statusSpritesList[0];
                break;
            case UnitStatus.Building:
                statusSprite.gameObject.SetActive(true);
                statusSprite.sprite = statusSpritesList[1];
                break;
            default:
                statusSprite.gameObject.SetActive(false);
                statusSprite.sprite = null;
                break;
        }
    }
}
