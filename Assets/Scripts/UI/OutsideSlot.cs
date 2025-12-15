using UnityEngine;
using UnityEngine.EventSystems;

public class OutsideSlot : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private UIManager uiMgr;

    [SerializeField]
    private GameManager gameMgr;

    void Awake()
    {
        uiMgr = UIManager.instance;
        gameMgr = GameManager.instance;
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
        //Debug.Log("On Drop - Out Town");

        GameObject unitObj = eventData.pointerDrag;
        UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
        if (unitDrag == null)
            return;

        unitDrag.IconParent = transform;

        unitDrag.QuitOldTerrainSlot(); //old slot remove this labor
        unitDrag.LandUnit.UnitStatus = UnitStatus.None;
    }
}
