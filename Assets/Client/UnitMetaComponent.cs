using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitMetaComponent : MonoBehaviour
{
  // Units
  public GameObject basePrefab;
  public GameObject soldierPrefab;
  public GameObject archerPrefab;
  public GameObject priestPrefab;
  public GameObject firemagePrefab;
  public GameObject assassinPrefab;
  public GameObject linkerPrefab;
  public GameObject sniperPrefab;

  public GameObject GetUnitPrefab(UnitType type)
  {
    if (type == UnitType.Base) return basePrefab;
    if (type == UnitType.Soldier) return soldierPrefab;
    if (type == UnitType.Archer) return archerPrefab;
    if (type == UnitType.Priest) return priestPrefab;
    if (type == UnitType.FireMage) return firemagePrefab;
    if (type == UnitType.Assassin) return assassinPrefab;
    if (type == UnitType.Linker) return linkerPrefab;
    if (type == UnitType.Sniper) return sniperPrefab;
    throw new Exception("No prefab exists for " + type);
  }
}