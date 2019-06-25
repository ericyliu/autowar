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
  int step = 0;
  int nextId = 0;
  // Start is called before the first frame update
  public void Start()
  {
    Thread thread = new Thread(new ThreadStart(this.GameLoop));
    thread.Start();
  }

  public void Attack()
  {
    foreach (Unit unit in this.units)
    {
      unit.target = this.enemyTarget;
    }
  }

  void GameLoop()
  {
    while (true)
    {
      if (step % 100 == 0) Spawn();
      foreach (Unit unit in this.units)
      {
        unit.Act();
      }
      Thread.Sleep(60);
      step++;
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
