using System.Collections.Generic;
using UnityEngine;

public class Spawner
{
  public Game game;
  public int nextId = 0;

  public Spawner(Game game)
  {
    this.game = game;
  }
  public Unit SpawnBase(Player player, Vector2 position)
  {
    var unit = this.Spawn(player, position);
    unit.speed = 0f;
    unit.size = 4.5f;
    unit.height = 2.4f;
    return unit;
  }

  public Unit SpawnSoldier(Player player, Vector2 position)
  {
    var unit = this.Spawn(player, position);
    unit.speed = 3f;
    unit.size = 1.5f;
    unit.height = 1f;
    return unit;
  }

  Unit Spawn(Player player, Vector2 position)
  {
    var unit = new Unit(this.nextId++, player, position);
    unit.game = this.game;
    this.game.units.Add(unit);
    return unit;
  }
}