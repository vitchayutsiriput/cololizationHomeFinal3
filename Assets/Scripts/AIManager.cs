using System.Collections;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager instance;

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

    private IEnumerator EnemyUnitMoves()
    {
        foreach (Faction faction in GameManager.instance.Factions)
        {
            //Debug.Log($"{faction.Nation}: 0");
            GameManager.instance.ResetAllUnits(faction);

            if (faction == GameManager.instance.PlayerFaction)
            {
                //Debug.Log($"Skip:{faction} vs {GameManager.instance.PlayerFaction}");
                continue;
            }

            foreach (Unit unit in faction.Units)
            {
                //Debug.Log($"{faction.Nation}: 1");

                GameManager.instance.SelectAiUnit(unit);
                UnitAI unitAI = unit.GetComponent<UnitAI>();

                if (unit.Visible)
                    CameraController.instance.MoveCamera(unit.CurPos);

                if (unit.UnitStatus == UnitStatus.OnBoard)
                    continue;

                //Unit move or attack
                //Debug.Log("Move");
                AutoMoveToHex(unit);

                yield return new WaitForSeconds(0.1f);
            }
        }
        GameManager.instance.GameTurn++;
        

        //New Turn Dialog and Focus on Player's unit

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //Debug.Log("Release Mouse");
        //Debug.Log("New Turn");
        GameManager.instance.StartNewTurn();
    }

    public void StartAITurn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(EnemyUnitMoves());
    }

    private void AutoMoveToHex(Unit unit)
    {
        unit.ShowHideSprite(true);

        if (unit.DestinationHex != null)
            unit.CheckMoveToDestination();
        else
            RandomMoveOneHex(unit);
    }

    public void RandomMoveOneHex(Unit unit)
    {
        Hex anyHex = null;

        int n = Random.Range(0, 6);

        //Move
        anyHex = HexCalculator.FindHexByDir(unit.CurHex, (HexDirection)n, GameManager.instance.AllHexes);

        if (anyHex == null)
            return;

        switch (unit.UnitType)
        {
            case UnitType.Land:
                LandUnit landUnit = (LandUnit)unit;
                landUnit.PrepareMoveToHex(anyHex);
                break;
            case UnitType.Naval:
                NavalUnit navalUnit = (NavalUnit)unit;
                navalUnit.PrepareMoveToHex(anyHex);
                break;
        }
    }
}
