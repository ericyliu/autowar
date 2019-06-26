using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameComponent : MonoBehaviour
{
  public int id = 0;
  public Player player;
  public GameObject soldierPrefab;
  public UI ui;
  public Game game;
  public List<GameStep> steps = new List<GameStep>();
  public Dictionary<int, UnitComponent> unitComponents = new Dictionary<int, UnitComponent>();

  void Start()
  {
    this.ui.LookAtBase(this.id);
  }

  void Update()
  {
    this.StepGame();
    this.SpawnUnits();
  }

  void StepGame()
  {
    List<GameStep> currentSteps = this.steps;
    this.steps = new List<GameStep>();
    foreach (GameStep step in currentSteps) game.Step(step);
  }

  void SpawnUnits()
  {
    foreach (Unit unit in this.game.units)
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
