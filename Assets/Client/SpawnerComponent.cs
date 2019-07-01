using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerComponent : MonoBehaviour
{
  public RectTransform healthBarsRect;
  public GameObject healthBarPrefab;

  // Units
  public GameObject basePrefab;
  public GameObject soldierPrefab;
  public GameObject archerPrefab;
  public GameObject priestPrefab;
  public GameObject firemagePrefab;

  // Projectiles
  public GameObject arrowPrefab;

  public UnitComponent SpawnUnit(Unit unit)
  {
    var prefab = this.GetUnitPrefab(unit.type);
    var unitObject = Instantiate(
      prefab,
      VectorUtil.Vector2To3(unit.position, unit.height),
      Quaternion.identity
    );
    unitObject.name = unit.id + ": " + unit.type;
    var unitComponent = unitObject.GetComponent<UnitComponent>();
    unitComponent.unit = unit;
    unitComponent.Initialize();
    unitComponent.healthBar = this.AttachHealthBar(unitObject, unit);
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

  public GameObject GetUnitPrefab(UnitType type)
  {
    if (type == UnitType.Base) return basePrefab;
    else if (type == UnitType.Soldier) return soldierPrefab;
    else if (type == UnitType.Archer) return archerPrefab;
    else if (type == UnitType.Priest) return priestPrefab;
    else if (type == UnitType.FireMage) return firemagePrefab;
    throw new Exception("No prefab exists for " + type);
  }

  public GameObject GetProjectilePrefab(ProjectileType type)
  {
    if (type == ProjectileType.Arrow) return arrowPrefab;
    throw new Exception("No prefab exists for " + type);
  }
}