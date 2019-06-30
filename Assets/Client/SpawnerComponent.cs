using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerComponent : MonoBehaviour
{
  public GameObject basePrefab;
  public GameObject soldierPrefab;
  public GameObject archerPrefab;
  public GameObject priestPrefab;

  public UnitComponent Spawn(Unit unit)
  {
    GameObject prefab = this.GetPrefab(unit.type);
    var unitObject = Instantiate(
      prefab,
      VectorUtil.Vector2To3(unit.position, unit.height),
      Quaternion.identity
    );
    unitObject.name = unit.id + ": " + unit.type;
    UnitComponent unitComponent = unitObject.GetComponent<UnitComponent>();
    unitComponent.unit = unit;
    unitComponent.Initialize();
    return unitComponent;
  }

  public GameObject GetPrefab(UnitType type)
  {
    if (type == UnitType.Base) return basePrefab;
    else if (type == UnitType.Soldier) return soldierPrefab;
    else if (type == UnitType.Archer) return archerPrefab;
    else if (type == UnitType.Priest) return priestPrefab;
    throw new Exception("No prefab exists for " + type);
  }
}