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
      case ProjectileType.SniperShot:
        projectile.speed = 15f;
        break;
    }
    return projectile;
  }
}