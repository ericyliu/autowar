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
}