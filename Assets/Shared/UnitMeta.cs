using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
  Null,
  Base,
  Soldier,
  Archer,
  Priest,
  FireMage,
  Assassin,
  Linker,
  Sniper,
}

public class UnitMeta
{
  public static Unit Decorate(Unit unit, Spawner spawner)
  {
    switch (unit.type)
    {
      case UnitType.Base:
        unit.speed = 0f;
        unit.size = 4.5f;
        unit.height = 2.4f;
        unit.maxHealth = 1000;
        break;
      case UnitType.Soldier:
        unit.speed = 3f;
        break;
      case UnitType.Archer:
        unit.speed = 2.5f;
        unit.maxHealth = 60;
        unit.attackRange = 8f;
        unit.damage = 30;
        unit.attackSpeed = 35;
        unit.attackDamageDelay = 15;
        unit.DoDamageOverride = () =>
        {
          spawner.SpawnProjectile(unit.position, ProjectileType.Arrow, unit, unit.attackTarget);
        };
        break;
      case UnitType.Priest:
        UnitMeta.DecoratePriest(unit, spawner);
        break;
      case UnitType.FireMage:
        unit.speed = 2f;
        unit.maxHealth = 50;
        unit.attackRange = 11f;
        unit.damage = 30;
        unit.attackSpeed = 70;
        unit.attackDamageDelay = 20f;
        unit.DoDamageOverride = () =>
        {
          spawner.SpawnProjectile(unit.position, ProjectileType.Fireball, unit, unit.attackTarget);
        };
        break;
      case UnitType.Assassin:
        UnitMeta.DecorateAssassin(unit);
        break;
      case UnitType.Linker:
        UnitMeta.DecorateLinker(unit);
        break;
      case UnitType.Sniper:
        break;
    }
    return unit;
  }

  static Unit DecoratePriest(Unit unit, Spawner spawner)
  {
    unit.speed = 2.5f;
    unit.maxHealth = 50;
    unit.attackRange = 9f;
    unit.damage = 10;
    unit.attackSpeed = 35;
    unit.attackDamageDelay = 10;
    unit.AcquireTargetOverride = () =>
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
    };
    unit.DoDamageOverride = () =>
    {
      unit.attackTarget.Heal(unit.damage + (unit.player.upgrade * 20));
      spawner.SpawnProjectile(unit.attackTarget.position, ProjectileType.Smite, unit, unit.attackTarget);
    };
    return unit;
  }

  static Unit DecorateAssassin(Unit unit)
  {
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
    unit.AcquireTargetOverride = () =>
    {
      // clear attackTarget and activate invisibility if target is dead
      if (unit.attackTarget != null && unit.attackTarget.health <= 0)
      {
        unit.invisible = true;
        unit.attackTarget = null;
      }
      if (unit.attackTarget == null)
      {
        var units = unit
          .GetUnitsWithin(unit.aggroRadius, u =>
            u.player.id == unit.player.enemy.id &&
            u.health > 0 &&
            !u.invisible
          );
        if (units.Count == 0) return false;
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
        unit.attackTarget = units[0];
        return unit.attackTarget != null;
      }
      return true;
    };
    unit.DoDuringAttackFrame = () =>
    {
      if (unit.lastAttackedStep + 5 == unit.game.step) unit.invisible = false;
    };
    unit.DoDamageOverride = () =>
    {
      unit.attackTarget.TakeDamage(unit.damage + (unit.player.upgrade * 20));
    };
    return unit;
  }

  static Unit DecorateLinker(Unit unit)
  {
    unit.attackRange = 8f;
    unit.speed = 2.3f;
    unit.damage = 0;
    unit.OnStartAct = () =>
    {
      if (unit.health <= 0) return;
      unit.GetUnitsWithin(unit.attackRange, u => u.player.id == unit.player.id)
        .ForEach(u =>
        {
          u.effects.RemoveAll(e => e.type == EffectType.Link);
          var effect = new Effect(EffectType.Link);
          effect.source = unit;
          u.effects.Add(effect);
        });
    };
    return unit;
  }

  static Unit DecorateSniper(Unit unit, Spawner spawner)
  {
    unit.attackRange = 8f;
    unit.speed = 2.3f;
    unit.damage = 30;
    unit.attackSpeed = 35;
    unit.attackDamageDelay = 15;
    unit.DoDamageOverride = () =>
    {
      spawner.SpawnProjectile(unit.position, ProjectileType.SniperShot, unit, unit.attackTarget);
    };
    return unit;
  }
}