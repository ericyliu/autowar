using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Game
{
  public List<Unit> units = new List<Unit>();
  public List<Vector2> spawns = new List<Vector2> {
    new Vector2(2f, 4f),
    new Vector2(2f, 20f)
  };
  public GameComponent gameComponent;
  public Vector2 spawnTarget = new Vector2(15f, 13f);
  public Vector2 enemyTarget = new Vector2(187f, 12f);
  public List<GameStep> steps = new List<GameStep>();
  public List<GameAction> actions = new List<GameAction>();
  public int step = 0;
  int nextId = 0;

  public GameStep Step(GameStep step = null)
  {
    if (step != null && step.id != this.step) throw new Exception();
    if (step == null)
    {
      step = new GameStep(this.step, this.actions);
      this.steps.Add(step);
      this.actions = new List<GameAction>();
    }
    foreach (GameAction action in step.actions)
    {
      if (action == GameAction.Attack) Attack();
    }
    if (step.id % 100 == 0) Spawn();
    foreach (Unit unit in this.units) unit.Act();
    this.step++;
    return step;
  }

  public void Attack()
  {
    foreach (Unit unit in this.units)
    {
      unit.target = this.enemyTarget;
    }
  }

  void Spawn()
  {
    foreach (Vector2 spawn in this.spawns)
    {
      Unit unit = new Unit();
      unit.id = this.nextId++;
      unit.game = this;
      unit.position = spawn;
      unit.target = this.spawnTarget;
      this.units.Add(unit);
    }
  }
}
