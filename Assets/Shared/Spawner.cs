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

  public Projectile SpawnProjectile(Vector2 position, ProjectileType type, Unit source, Unit target)
  {
    var projectile = new Projectile(this.nextId++, type, position, source, target);
    projectile = ProjectileMeta.Decorate(projectile);
    this.game.projectiles.Add(projectile);
    return projectile;
  }

  public Unit SpawnUnit(Player player, Vector2 position, UnitType type)
  {
    var unit = new Unit(this.nextId++, type, player, position);
    unit.game = this.game;
    unit = UnitMeta.Decorate(unit, this);
    unit.Initialize();
    this.game.units.Add(unit);
    return unit;
  }

  bool GetUnitToHeal(Unit unit)
  {
    if (
      unit.attackTarget == null ||
      unit.attackTarget.health >= unit.attackTarget.maxHealth ||
      unit.attackTarget.health <= 0
    )
    {
      var units = unit
        .GetUnitsWithin(unit.aggroRadius)
        .FindAll(u =>
          u.player.id == unit.player.id &&
          u.health < u.maxHealth &&
          u.health > 0 &&
          u.type != UnitType.Base
        );
      if (units.Count == 0) return false;
      units
        .Sort((unit1, unit2) =>
        {
          var unit1Closer =
            Vector2.Distance(unit1.position, unit.position) <=
            Vector2.Distance(unit2.position, unit.position);
          if (unit1Closer) return -1;
          else return 1;
        });
      unit.attackTarget = units[0];
    }
    return unit.attackTarget != null;
  }
}