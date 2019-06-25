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
  public int step = 0;
  int nextId = 0;

  public GameStep Step(GameStep step)
  {
    if (step.id != this.step) throw new Exception();
    if (this.step % 100 == 0) Spawn();
    foreach (Unit unit in this.units) unit.Act();
    GameStep nextStep = new GameStep(this.step++);
    this.steps.Add(nextStep);
    return nextStep;
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

public class GameStep
{
  public int id = 0;

  public GameStep(int id)
  {
    this.id = id;
  }

  public GameStep(byte[] bytes)
  {
    this.id = BitConverter.ToInt32(bytes, 0);
  }

  public byte[] ToByteArray()
  {
    return BitConverter.GetBytes(this.id);
  }
}
