using System;
using UnityEngine;

public class SpawnerComponent : MonoBehaviour
{
  public GameObject basePrefab;
  public GameObject soldierPrefab;

  public UnitComponent Spawn(Unit unit)
  {
    GameObject prefab = null;
    if (unit.type == UnitType.Base) prefab = basePrefab;
    else if (unit.type == UnitType.Soldier) prefab = soldierPrefab;
    if (prefab == null) throw new Exception("No prefab exists for " + unit.type);
    var unitObject = Instantiate(
      prefab,
      VectorUtil.Vector2To3(unit.position, unit.height),
      Quaternion.identity
    );
    unitObject.name = unit.id + ": " + unit.type;
    UnitComponent unitComponent = unitObject.GetComponent<UnitComponent>();
    unitComponent.unit = unit;
    return unitComponent;
  }
}