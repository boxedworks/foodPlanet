
using UnityEngine;
using System.Collections.Generic;

public class SimpleTrapManager
{

  //
  public static SimpleTrapManager s_Singleton;
  public static int s_Id;

  //
  public enum TrapType
  {
    NONE,

    CAR,
    DOOR,
  }

  //
  public Dictionary<int, SimpleTrap> _traps;
  public SimpleTrapManager()
  {
    s_Singleton = this;

    _traps = new();
  }
  public static void Cleanup()
  {
    foreach (var trap in s_Singleton._traps)
      trap.Value.Destroy();
    s_Singleton._traps = new();
  }

  //
  public static void Update()
  {
    foreach (var trapPair in s_Singleton._traps)
    {
      var trap = trapPair.Value;
      trap.Update();
    }
  }
  public static void FixedUpdate()
  {
    foreach (var trapPair in s_Singleton._traps)
    {
      var trap = trapPair.Value;
      trap.FixedUpdate();
    }
  }

  //
  public static void GetSimpleTrapFromCollider(Collider c, System.Action<SimpleTrap> onTrapFound)
  {

    foreach (var trapPair in s_Singleton._traps)
    {
      var trap = trapPair.Value;
      if (trap.IsTrap(c))
      {
        onTrapFound.Invoke(trap);
        break;
      }
    }

  }

  //
  public static SimpleTrap SpawnTrap(TrapType trapType, Vector3 atPosition)
  {
    SimpleTrap newTrap;
    switch (trapType)
    {

      case TrapType.CAR:

        newTrap = new CarTrap(atPosition, new Vector3(0f, 0f, 1f));
        break;

      case TrapType.DOOR:

        newTrap = new DoorTrap(atPosition, new Vector3(1f, 0f, 0f));
        break;


      default:

        Debug.LogWarning($"Unhandle trap type: {trapType}");
        return null;

    }

    newTrap._Id = s_Id++;
    s_Singleton._traps.Add(newTrap._Id, newTrap);

    //
    return newTrap;
  }
  public static void ActivateTrapById(int trapId)
  {
    if (!s_Singleton._traps.ContainsKey(trapId))
    {
      Debug.LogError($"Attempting to activate null trap id: {trapId}");
      return;
    }

    var trap = s_Singleton._traps[trapId];
    trap.Activate();
  }

}