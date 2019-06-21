using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
  public List<Unit> units = new List<Unit>();
  public List<GameObject> spawns = new List<GameObject>();
  public GameObject soldierPrefab;
  public GameObject spawnTarget;
  public GameObject enemyTarget;
  int step = 0;
  // Start is called before the first frame update
  void Start()
  {
    foreach (Unit unit in this.units)
    {
      unit.position = VectorUtil.Vector3To2(unit.transform.position);
      unit.target = VectorUtil.Vector3To2(this.spawnTarget.transform.position);
      unit.game = this;
    }
    StartCoroutine("GameLoop");
  }

  public void Attack()
  {
    foreach (Unit unit in this.units)
    {
      unit.target = VectorUtil.Vector3To2(this.enemyTarget.transform.position);
    }
  }

  IEnumerator GameLoop()
  {
    while (true)
    {
      if (step % 100 == 0) Spawn();
      foreach (Unit unit in this.units)
      {
        unit.Act();
      }
      yield return new WaitForSecondsRealtime(0.06f);
      step++;
    }
  }

  void Spawn()
  {
    foreach (GameObject go in this.spawns)
    {
      GameObject unitObject = Instantiate(soldierPrefab, go.transform.position, Quaternion.identity);
      Unit unit = unitObject.GetComponent<Unit>();
      this.units.Add(unit);
      unit.game = this;
      unit.position = VectorUtil.Vector3To2(go.transform.position);
      unit.target = VectorUtil.Vector3To2(this.spawnTarget.transform.position);
    }
  }
}
