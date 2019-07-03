using System;
using System.Collections.Generic;
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
      case UnitType.FireMage:
        unit = this.SpawnFireMage(player, position);
        break;
      case UnitType.Assassin:
        unit = this.SpawnAssassin(player, position);
        break;
      case UnitType.Linker:
        unit = this.SpawnLinker(player, position);
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
    unit.DoDamageOverride = () =>
    {
      var arrow = this.SpawnProjectile(unit.position, ProjectileType.Arrow, unit.attackTarget);
      arrow.damage = unit.damage + (unit.player.upgrade * 20);
    };
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
    unit.AcquireTargetOverride = this.GetUnitToHeal;
    unit.DoDamageOverride = () =>
    {
      unit.attackTarget.Heal(unit.damage + (unit.player.upgrade * 20));
      SpawnProjectile(unit.attackTarget.position, ProjectileType.Smite, unit.attackTarget);
    };
    return unit;
  }

  Unit SpawnFireMage(Player player, Vector2 position)
  {
    var unit = Spawn(UnitType.FireMage, player, position);
    unit.speed = 2f;
    unit.maxHealth = 50;
    unit.attackRange = 11f;
    unit.damage = 30;
    unit.attackSpeed = 70;
    unit.attackDamageDelay = 25f;
    unit.DoDamageOverride = () =>
    {
      var fireball = this.SpawnProjectile(unit.position, ProjectileType.Fireball, unit.attackTarget);
      fireball.damage = unit.damage + (unit.player.upgrade * 20);
      fireball.speed = 5f;
      fireball.DoDamage = damage =>
      {
        fireball.target.GetUnitsWithin(3f, u =>
          u.player.id == unit.player.enemy.id
        ).ForEach(u => u.TakeDamage(damage));
        fireball.target.TakeDamage(damage);
        fireball.alive = false;
      };
    };
    return unit;
  }

  Unit SpawnAssassin(Player player, Vector2 position)
  {
    var unit = Spawn(UnitType.Assassin, player, position);
    unit.speed = 2.5f;
    unit.maxHealth = 70;
    unit.damage = 50;
    unit.attackSpeed = 50;
    unit.aggroRadius = 20f;
    unit.invisible = true;
    var priorityList = new List<UnitType>(){
      UnitType.FireMage,
      UnitType.Priest
    };
    unit.AcquireTargetOverride = thisUnit =>
    {
      var units = thisUnit
        .GetUnitsWithin(thisUnit.aggroRadius, u =>
          u.player.id == thisUnit.player.enemy.id &&
          u.health > 0 &&
          !u.invisible
        );
      if (units.Count == 0)
      {
        thisUnit.attackTarget = null;
        return false;
      }
      units
        .Sort((unit1, unit2) =>
        {
          var unit1Priority = priorityList.IndexOf(unit1.type);
          var unit2Priority = priorityList.IndexOf(unit2.type);
          if (unit1Priority > unit2Priority) return -1;
          if (unit1Priority < unit2Priority) return 1;
          var unit1Closer = Vector2.Distance(unit1.position, unit.position) <= Vector2.Distance(unit2.position, unit.position);
          if (unit1Closer) return -1;
          else return 1;
        });
      thisUnit.attackTarget = units[0];
      return thisUnit.attackTarget != null;
    };
    unit.DoDuringAttackFrame = () =>
    {
      if (unit.lastAttackedStep + 5 == unit.game.step) unit.invisible = false;
    };
    unit.DoDamageOverride = () =>
    {
      unit.attackTarget.TakeDamage(unit.damage + (unit.player.upgrade * 20));
      unit.invisible = true;
    };
    return unit;
  }

  Unit SpawnLinker(Player player, Vector2 position)
  {
    var unit = this.Spawn(UnitType.Linker, player, position);
    unit.attackRange = 8f;
    unit.speed = 2.3f;
    unit.AcquireTargetOverride = this.GetUnitToHeal;
    unit.DoDamageOverride = () =>
    {
      var total = unit.attackTarget.health;
      var units = unit.attackTarget.GetUnitsWithin(unit.attackRange + 1f,
        u =>
          u.player.id == unit.player.id &&
          u.type != UnitType.Base
      );
      units.ForEach(u => total += u.health);
      var dividedHealth = total / (units.Count + 1) + 5;
      units.ForEach(u => u.health = dividedHealth);
      unit.attackTarget.health = dividedHealth;
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