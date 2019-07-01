using System;
using UnityEngine;

public class ProjectileSpawner
{
  public Game game;
  public int nextId = 0;

  public ProjectileSpawner(Game game)
  {
    this.game = game;
  }

  public Projectile Spawn(Vector2 position, ProjectileType type, Unit target)
  {
    Projectile projectile = null;
    switch (type)
    {
      case ProjectileType.Arrow:
        projectile = this.SpawnArrow(position, target);
        break;
      case ProjectileType.Fireball:
        projectile = this.SpawnFireball(position, target);
        break;
    }
    if (projectile == null) throw new Exception("projectile type " + type + " not defined in spawner");
    return projectile;
  }

  Projectile SpawnArrow(Vector2 position, Unit target)
  {
    return this.SpawnBaseProjectile(ProjectileType.Arrow, position, target);
  }

  Projectile SpawnFireball(Vector2 position, Unit target)
  {
    return this.SpawnBaseProjectile(ProjectileType.Fireball, position, target);
  }

  Projectile SpawnBaseProjectile(ProjectileType type, Vector2 position, Unit target)
  {
    var projectile = new Projectile(this.nextId++, type, game, position, target);
    this.game.projectiles.Add(projectile);
    return projectile;
  }
}