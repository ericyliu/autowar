using System;
using System.Collections.Generic;
using UnityEngine;


public class Projectile
{
  public int id;
  public ProjectileType type;
  public int damage = 30;
  public bool alive = true;
  public Vector2 position = Vector2.zero;
  public Unit source;
  public Unit target;
  public Vector2 targetPosition;
  public float speed = 10f;
  public Action<int> DoDamage;
  public List<Action> OnDamageDealt = new List<Action>();
  public Action<Vector2> OnCheckHit;
  Vector2 targetLastPosition;

  public Projectile(int id, ProjectileType type, Vector2 position, Unit source, Unit target)
  {
    this.id = id;
    this.type = type;
    this.position = position;
    this.source = source;
    this.target = target;
    this.targetLastPosition = target.position;
  }

  public void Act()
  {
    var currentTargetPosition = this.GetTargetPosition();
    if (this.OnCheckHit != null) this.OnCheckHit(currentTargetPosition);
    else this.DefaultCheckHit(currentTargetPosition);
    var direction = currentTargetPosition - this.position;
    var travelVector = direction.normalized * speed / 10;
    this.position += travelVector;
    this.targetLastPosition = this.target.position;
  }

  public List<Unit> GetUnitsWithin(float f, Func<Unit, bool> filter = null)
  {
    return this.source.game.units.FindAll(u =>
      {
        var passFilter = filter == null || filter(u);
        var distanceAway = Vector2.Distance(this.position, u.position);
        return passFilter && distanceAway <= f;
      }
    );
  }

  Vector2 GetTargetPosition()
  {
    if (!this.targetPosition.Equals(Vector2.zero)) return this.targetPosition;
    if (this.target.health > 0) return this.target.position;
    return this.targetLastPosition;
  }

  void DefaultCheckHit(Vector2 targetPosition)
  {
    if (Vector2.Distance(targetPosition, this.position) < this.target.size)
    {
      if (this.DoDamage != null) this.DoDamage(damage);
      else
      {
        this.target.TakeDamage(this.damage);
        this.alive = false;
      }
      this.OnDamageDealt.ForEach(fn => fn());
      return;
    }
  }
}