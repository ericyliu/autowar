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
    var unit = this.Spawn(UnitType.Base, player, position);
    unit.speed = 0f;
    unit.size = 4.5f;
    unit.height = 2.4f;
    unit.health = 1000;
    return unit;
  }

  public Unit SpawnSoldier(Player player, Vector2 position)
  {
    var unit = this.Spawn(UnitType.Soldier, player, position);
    unit.speed = 3f;
    unit.size = 1.5f;
    unit.height = 1f;
    return unit;
  }

  Unit Spawn(UnitType type, Player player, Vector2 position)
  {
    var unit = new Unit(this.nextId++, type, player, position);
    unit.game = this.game;
    this.game.units.Add(unit);
    return unit;
  }
}