using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameComponent : MonoBehaviour
{
  public SpawnerComponent spawnerComponent;
  public int id = 0;
  public Player player;

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
    this.DespawnUnits();
    this.SpawnUnits();
  }

  void StepGame()
  {
    List<GameStep> currentSteps = this.steps;
    this.steps = new List<GameStep>();
    foreach (GameStep step in currentSteps) game.Step(step);
  }

  void DespawnUnits()
  {
    var componentsToDelete = new List<int>();
    foreach (int id in this.unitComponents.Keys)
    {
      if (this.game.units.Find(unit => unit.id == id) == null)
      {
        componentsToDelete.Add(id);
      };
    }
    componentsToDelete.ForEach(id =>
    {
      Destroy(this.unitComponents[id].gameObject);
      this.unitComponents.Remove(id);
    });
  }

  void SpawnUnits()
  {
    foreach (Unit unit in this.game.units)
    {
      if (!unitComponents.ContainsKey(unit.id))
      {
        unitComponents.Add(unit.id, spawnerComponent.Spawn(unit));
      }
    }
  }
}
