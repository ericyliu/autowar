using System;
using UnityEngine;

public class Spawner
{
  public Game game;
  public ProjectileSpawner projectileSpawner;
  public int nextId = 0;

  public Spawner(Game game)
  {
    this.game = game;
    this.projectileSpawner = new ProjectileSpawner(game);
  }

  public Projectile SpawnProjectile(Vector2 position, ProjectileType type, Unit target)
  {
    return this.projectileSpawner.Spawn(position, type, target);
  }

  public Unit SpawnUnit(Player player, Vector2 position, UnitType type)
  {
    Unit unit = null;
    switch (type)
    {
      case UnitType.Base:
        unit = this.SpawnBase(player, position);
        break;
      case UnitType.Soldier:
        unit = this.SpawnSoldier(player, position);
        break;
      case UnitType.Archer:
        unit = this.SpawnArcher(player, position);
        break;
      case UnitType.Priest:
        unit = this.SpawnPriest(player, position);
        break;
    }
    if (unit == null) throw new Exception("unit type " + type + " not defined in spawner");
    unit.Initialize();
    return unit;
  }

  Unit SpawnBase(Player player, Vector2 position)
  {
    var unit = this.Spawn(UnitType.Base, player, position);
    unit.speed = 0f;
    unit.size = 4.5f;
    unit.height = 2.4f;
    unit.maxHealth = 1000;
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
    unit.maxHealth = 60;
    unit.attackRange = 8f;
    unit.damage = 30;
    unit.attackSpeed = 35;
    unit.attackDamageDelay = 15;
    unit.DoDamageOverride = (thisUnit =>
    {
      var arrow = this.SpawnProjectile(thisUnit.position, ProjectileType.Arrow, thisUnit.attackTarget);
      arrow.damage = unit.damage + (thisUnit.player.upgrade * 20);
    });
    return unit;
  }

  Unit SpawnPriest(Player player, Vector2 position)
  {
    var unit = this.Spawn(UnitType.Priest, player, position);
    unit.speed = 2.5f;
    unit.maxHealth = 50;
    unit.attackRange = 9f;
    unit.damage = 10;
    unit.attackSpeed = 35;
    unit.attackDamageDelay = 10;
    unit.AcquireTargetOverride = thisUnit =>
    {
      if (
        thisUnit.attackTarget == null ||
        thisUnit.attackTarget.health >= thisUnit.attackTarget.maxHealth ||
        thisUnit.attackTarget.health <= 0
      )
      {
        var units = thisUnit
          .GetUnitsWithin(thisUnit.aggroRadius)
          .FindAll(u =>
            u.player.id == thisUnit.player.id &&
            u.health < u.maxHealth &&
            u.health > 0
          );
        if (units.Count == 0) return false;
        units
          .Sort((unit1, unit2) =>
          {
            var unit1Closer =
              Vector2.Distance(unit1.position, thisUnit.position) <=
              Vector2.Distance(unit2.position, thisUnit.position);
            if (unit1Closer) return -1;
            else return 1;
          });
        thisUnit.attackTarget = units[0];
      }
      return thisUnit.attackTarget != null;
    };
    unit.DoDamageOverride = thisUnit =>
    {
      thisUnit.attackTarget.Heal(thisUnit.damage + (thisUnit.player.upgrade * 20));
    };
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