using System;
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

  public Unit SpawnUnit(Player player, Vector2 position, UnitType type)
  {
    switch (type)
    {
      case UnitType.Base:
        return this.SpawnBase(player, position);
      case UnitType.Soldier:
        return this.SpawnSoldier(player, position);
      case UnitType.Archer:
        return this.SpawnArcher(player, position);
    }
    throw new Exception("unit type " + type + " not defined in spawner");
  }

  Unit SpawnBase(Player player, Vector2 position)
  {
    var unit = this.Spawn(UnitType.Base, player, position);
    unit.speed = 0f;
    unit.size = 4.5f;
    unit.height = 2.4f;
    unit.health = 1000;
    return unit;
  }

  Unit SpawnSoldier(Player player, Vector2 position)
  {
    var unit = this.Spawn(UnitType.Soldier, player, position);
    unit.speed = 3f;
    return unit;
  }

  Unit SpawnArcher(Player player, Vector2 position)
  {
    var unit = this.Spawn(UnitType.Archer, player, position);
    unit.speed = 2.5f;
    unit.health = 60;
    unit.attackRange = 8f;
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