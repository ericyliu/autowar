using UnityEngine;

public enum ProjectileType
{
  Null,
  Arrow,
  Fireball,
  Smite,
  SniperShot
}

public class ProjectileMeta
{
  public static Projectile Decorate(Projectile projectile)
  {
    switch (projectile.type)
    {
      case ProjectileType.Arrow:
        projectile.damage = projectile.source.damage + (projectile.source.player.upgrade * 20);
        break;
      case ProjectileType.Fireball:
        projectile.damage = projectile.source.damage + (projectile.source.player.upgrade * 20);
        projectile.speed = 5f;
        projectile.DoDamage = damage =>
        {
          projectile.source.GetUnitsWithin(3f).ForEach(u => u.TakeDamage(damage));
          projectile.target.TakeDamage(damage);
          projectile.alive = false;
        };
        break;
      case ProjectileType.SniperShot:
        projectile.speed = 15f;
        projectile.damage = projectile.source.damage + (projectile.source.player.upgrade * 20);
        projectile.targetPosition = projectile.source.position + ((projectile.target.position - projectile.source.position).normalized * 20f);
        projectile.OnCheckHit = targetPosition =>
        {
          var units = projectile.GetUnitsWithin(1f, u => u.player.id == projectile.source.player.enemy.id);
          units.ForEach(u => u.TakeDamage(projectile.damage));
          if (Vector2.Distance(targetPosition, projectile.position) < 1f) projectile.alive = false;
        };
        break;
    }
    return projectile;
  }
}