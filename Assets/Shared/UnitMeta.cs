using System;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    Null,
    Base,
    Soldier,
    Archer,
    Priest,
    FireMage,
    Assassin,
    Linker,
}

public class UnitMeta : MonoBehaviour
{
    // Units
    public GameObject basePrefab;
    public GameObject soldierPrefab;
    public GameObject archerPrefab;
    public GameObject priestPrefab;
    public GameObject firemagePrefab;
    public GameObject assassinPrefab;
    public GameObject linkerPrefab;

    public static List<UnitType> GetBuyableUnits()
    {
        return new List<UnitType>(){
            UnitType.Soldier,
            UnitType.Archer,
            UnitType.Priest,
            UnitType.FireMage,
            UnitType.Assassin,
        };
    }
    
    public GameObject GetUnitPrefab(UnitType type)
    {
        if (type == UnitType.Base) return basePrefab;
        else if (type == UnitType.Soldier) return soldierPrefab;
        else if (type == UnitType.Archer) return archerPrefab;
        else if (type == UnitType.Priest) return priestPrefab;
        else if (type == UnitType.FireMage) return firemagePrefab;
        else if (type == UnitType.Assassin) return assassinPrefab;
        else if (type == UnitType.Linker) return linkerPrefab;
        throw new Exception("No prefab exists for " + type);
    }
}