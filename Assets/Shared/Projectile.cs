using UnityEngine;

public enum ProjectileType
{
  Arrow
}

public class Projectile
{
  public int id;
  public ProjectileType type;
  public int damage = 30;
  public bool alive = true;
  public Vector2 position = Vector2.zero;
  public Unit target;
  Game game;
  float speed = 10f;

  public Projectile(int id, ProjectileType type, Game game, Vector2 position, Unit target)
  {
    this.id = id;
    this.type = type;
    this.game = game;
    this.position = position;
    this.target = target;
  }

  public void Act()
  {
    if (Vector2.Distance(this.target.position, this.position) < this.target.size)
    {
      this.target.TakeDamage(this.damage);
      this.alive = false;
      return;
    }
    var direction = this.target.position - this.position;
    this.position += direction.normalized * speed / 10;
  }
}