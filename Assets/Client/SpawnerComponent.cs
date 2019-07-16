using System;
using UnityEngine;

public class SpawnerComponent : MonoBehaviour
{
  public static string GetUnitObjectName(Unit unit)
  {
    return unit.id + ": " + unit.type;
  }

  public GameComponent gameComponent;
  public UnitMetaComponent unitMeta;
  public RectTransform healthBarsRect;
  public GameObject healthBarPrefab;

  // Projectiles
  public GameObject arrowPrefab;
  public GameObject fireballPrefab;
  public GameObject sniperShotPrefab;

  // Effects
  public GameObject explosionPrefab;
  public GameObject smitePrefab;
  public GameObject linkEffectPrefab;

  public UnitComponent SpawnUnit(Unit unit)
  {
    var prefab = unitMeta.GetUnitPrefab(unit.type);
    var unitObject = Instantiate(
      prefab,
      VectorUtil.Vector2To3(unit.position, unit.height),
      Quaternion.identity
    );
    unitObject.name = SpawnerComponent.GetUnitObjectName(unit);
    var unitComponent = unitObject.GetComponent<UnitComponent>();
    unitComponent.unit = unit;
    unitComponent.gameComponent = this.gameComponent;
    unitComponent.Initialize();
    unitComponent.healthBar = this.AttachHealthBar(unitObject, unit);
    unit.OnLinkEffect = (Unit source, Unit target) =>
    {
      Instantiate(
        this.gameComponent.spawnerComponent.linkEffectPrefab,
        VectorUtil.Vector2To3(source.position, source.height),
        Quaternion.identity
      ).GetComponent<LinkEffectComponent>().Initialize(target);
    };
    return unitComponent;
  }

  public ProjectileComponent SpawnProjectile(Projectile projectile)
  {
    var prefab = this.GetProjectilePrefab(projectile.type);
    var projectileObject = Instantiate(
      prefab,
      VectorUtil.Vector2To3(projectile.position, 1f),
      Quaternion.identity
    );
    projectileObject.name = projectile.id + ": " + projectile.type;
    var projectileComponent = projectileObject.GetComponent<ProjectileComponent>();
    projectileComponent.projectile = projectile;
    projectileComponent.Initialize();
    return this.DecorateProjectile(projectileComponent);
  }

  ProjectileComponent DecorateProjectile(ProjectileComponent projectileComponent)
  {
    switch (projectileComponent.projectile.type)
    {
      case ProjectileType.Fireball:
        {
          projectileComponent.projectile.OnDamageDealt.Add(() =>
          {
            Instantiate(
              this.explosionPrefab,
              projectileComponent.transform.position,
              Quaternion.identity
            );
          });
          break;
        }
      case ProjectileType.Smite:
        {
          projectileComponent.projectile.OnDamageDealt.Add(() =>
          {
            Instantiate(
              this.smitePrefab,
              projectileComponent.transform.position,
              Quaternion.identity
            );
          });
          break;
        }
    }
    return projectileComponent;
  }

  HealthBarComponent AttachHealthBar(GameObject unitObject, Unit unit)
  {
    var healthBar = Instantiate(this.healthBarPrefab);
    var hbc = healthBar.GetComponent<HealthBarComponent>();
    hbc.SetHealthBarData(
      unitObject.transform,
      healthBarsRect,
      unit.size * 15f
    );
    healthBar.transform.SetParent(healthBarsRect, false);
    healthBar.transform.localScale = healthBar.transform.localScale * unit.size;
    return hbc;
  }

  public GameObject GetProjectilePrefab(ProjectileType type)
  {
    if (type == ProjectileType.Arrow) return arrowPrefab;
    if (type == ProjectileType.Fireball) return fireballPrefab;
    if (type == ProjectileType.Smite) return smitePrefab;
    if (type == ProjectileType.SniperShot) return sniperShotPrefab;
    throw new Exception("No prefab exists for " + type);
  }
}