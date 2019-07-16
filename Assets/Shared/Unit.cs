using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
  public int id;
  public UnitType type;
  public Vector2 position;
  public Vector2 target;
  public Unit attackTarget;
  public float attackRange = 1f;
  public float attackSpeed = 25;
  public float attackDamageDelay = 18;
  public float aggroRadius = 10f;
  public float speed = 3f;
  public float size = 1.5f;
  public float height = 1f;
  public int maxHealth = 100;
  public int health = 100;
  public int damage = 20;
  public bool invisible = false;
  public Game game;
  public Player player;
  public bool attacking = false;
  public int lastAttackedStep = 0;
  public List<Effect> effects = new List<Effect>();
  public Action OnStartAct;
  public List<Action> OnAttack = new List<Action>();
  public Action<Unit, Unit> OnLinkEffect;
  public Action DoDuringAttackFrame;
  public Action DoDamageOverride;
  public Func<bool> AcquireTargetOverride;

  public Unit(int id, UnitType type, Player player, Vector2 position)
  {
    if (type == UnitType.Null) throw new Exception("Unit type cannot be null");
    this.id = id;
    this.type = type;
    this.player = player;
    this.position = position;
  }

  public void Initialize()
  {
    this.maxHealth = this.maxHealth + (this.player.upgrade * 10);
    this.health = this.maxHealth;
  }

  public void Act()
  {
    this.HandleEffects();
    if (this.OnStartAct != null) this.OnStartAct();
    if (this.Attack()) return;
    this.attacking = false;
    this.lastAttackedStep = 0;
    if (this.ShouldStop()) return;
    this.Move();
  }

  public void TakeDamage(int damage, bool trueDamage = false)
  {
    if (!trueDamage)
    {
      var linkEffect = this.effects.Find(e => e.type == EffectType.Link);
      if (linkEffect != null && linkEffect.source.health > 0)
      {
        damage = damage / 2;
        linkEffect.source.TakeDamage(damage, true);
        if (this.OnLinkEffect != null) this.OnLinkEffect(this, linkEffect.source);
      }
    }
    this.health = Math.Max(0, this.health - damage);
  }

  public List<Unit> GetUnitsWithin(float f, Func<Unit, bool> filter = null)
  {
    return this.game.units.FindAll(u =>
      {
        if (this == u) return false;
        var passFilter = filter == null || filter(u);
        return passFilter && this.GetDistanceAway(u) <= f;
      }
    );
  }

  public void Heal(int damage)
  {
    this.health = Math.Min(this.maxHealth, this.health + damage);
  }

  void HandleEffects()
  {
    this.effects.ForEach(e => e.timeLeft--);
    this.effects.RemoveAll(e => e.timeLeft == 0);
  }

  bool Attack()
  {
    if (!this.AcquireTarget()) return false;
    if (this.GetDistanceAway(this.attackTarget) > this.attackRange) return false;
    this.attacking = true;
    if (this.game.step > this.lastAttackedStep + this.attackSpeed)
    {
      this.OnAttack.ForEach(fn => fn());
      this.lastAttackedStep = this.game.step;
    }
    if (this.DoDuringAttackFrame != null) this.DoDuringAttackFrame();
    if (this.game.step == this.lastAttackedStep + this.attackDamageDelay)
    {
      this.DoDamage();
    }
    return true;
  }

  bool AcquireTarget()
  {
    if (this.AcquireTargetOverride != null) return this.AcquireTargetOverride();
    if (this.attackTarget == null || this.attackTarget.health <= 0 || this.attackTarget.invisible)
    {
      var units = this.GetUnitsWithin(this.aggroRadius, unit =>
        unit.player.id == this.player.enemy.id &&
        unit.health > 0 &&
        !unit.invisible
      );
      if (units.Count == 0)
      {
        this.attackTarget = null;
        return false;
      }
      units
        .Sort((unit1, unit2) =>
        {
          var unit1Closer = Vector2.Distance(unit1.position, this.position) <= Vector2.Distance(unit2.position, this.position);
          if (unit1Closer) return -1;
          else return 1;
        });
      this.attackTarget = units[0];
    }
    return this.attackTarget != null;
  }

  void DoDamage()
  {
    if (this.DoDamageOverride != null) this.DoDamageOverride();
    else this.attackTarget.TakeDamage(this.damage + (this.player.upgrade * 20));
  }

  bool ShouldStop()
  {
    if (Vector2.Distance(this.position, this.target) <= this.size) return true;
    Vector2 futurePosition = this.position;
    for (int i = 0; i < 20; i++)
    {
      futurePosition = GetNextPosition(futurePosition, this.target);
    }
    if (Vector2.Distance(futurePosition, this.position) < this.size * 2f) return true;
    return false;
  }

  void Move()
  {
    var target = this.attackTarget != null ? this.attackTarget.position : this.target;
    this.position = this.GetNextPosition(this.position, target);
  }

  Vector2 GetNextPosition(Vector2 position, Vector2 target)
  {
    Vector2 move = (target - position).normalized;
    move = GetMoveWithCollision(move, position);
    return position + (move * this.speed / 10);
  }

  Vector2 GetMoveWithCollision(Vector2 move, Vector2 position)
  {
    // Dont Walk into Units
    List<Unit> collidedUnits = this.GetUnitsWithin(0f);
    foreach (Unit unit in collidedUnits)
    {
      move = move + (position - unit.position).normalized;
    }
    // Go Left or Right
    if (collidedUnits.Count > 0)
    {
      if (Vector2.SignedAngle(move, this.target - position) < 0)
      {
        move = move + Vector2.Perpendicular(this.target - position).normalized;
      }
      else
      {
        move = move - Vector2.Perpendicular(this.target - position).normalized;
      }
    }
    return move.normalized;
  }

  float GetDistanceAway(Unit unit)
  {
    return Vector2.Distance(this.position, unit.position) -
      (this.size / 2f) -
      (unit.size / 2f);
  }
}
