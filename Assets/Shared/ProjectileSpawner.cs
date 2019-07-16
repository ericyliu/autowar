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
    var projectile = new Projectile(this.nextId++, type, position, target);
    projectile = ProjectileMeta.Decorate(projectile);
    this.game.projectiles.Add(projectile);
    return projectile;
  }
}