using System;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileType
{
  Null,
  Arrow,
  Fireball,
  Smite,
  SniperShot
}

public class Projectile
{
  public int id;
  public ProjectileType type;
  public int damage = 30;
  public bool alive = true;
  public Vector2 position = Vector2.zero;
  public Unit target;
  public float speed = 10f;
  public Action<int> DoDamage;
  public List<Action> OnDamageDealt = new List<Action>();
  Vector2 targetLastPosition;

  public Projectile(int id, ProjectileType type, Vector2 position, Unit target)
  {
    this.id = id;
    this.type = type;
    this.position = position;
    this.target = target;
    this.targetLastPosition = target.position;
  }

  public void Act()
  {
    var targetPosition = this.target.health > 0 ? this.target.position : this.targetLastPosition;
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
    var direction = targetPosition - this.position;
    this.position += direction.normalized * speed / 10;
    this.targetLastPosition = this.target.position;
  }
}