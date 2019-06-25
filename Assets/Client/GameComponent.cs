using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameComponent : MonoBehaviour
{
  public GameObject soldierPrefab;
  public Game game;

  public Dictionary<int, UnitComponent> unitComponents = new Dictionary<int, UnitComponent>();

  void Update()
  {
    foreach (Unit unit in game.units)
    {
      if (!unitComponents.ContainsKey(unit.id))
      {
        unitComponents.Add(unit.id, Spawn(unit));
      }
    }
  }

  UnitComponent Spawn(Unit unit)
  {
    GameObject unitObject = Instantiate(
      soldierPrefab,
      VectorUtil.Vector2To3(unit.position, unit.height),
      Quaternion.identity
    );
    UnitComponent unitComponent = unitObject.GetComponent<UnitComponent>();
    unitComponent.unit = unit;
    return unitComponent;
  }
}
