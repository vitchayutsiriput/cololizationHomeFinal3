using System.Collections.Generic;
using UnityEngine;

public class UnitAI : MonoBehaviour
{
    [SerializeField]
    private List<Hex> adjacentHex = new List<Hex>();

    [SerializeField]
    private List<Unit> adjacentEnemy = new List<Unit>();
    public List<Unit> AdjacentEnemy { get { return adjacentEnemy; } }

    [SerializeField]
    private Unit unit;
    public Unit Unit { get { return unit; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unit = GetComponent<Unit>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
