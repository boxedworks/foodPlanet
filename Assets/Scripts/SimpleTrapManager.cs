
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
  }

  //
  public Dictionary<int, SimpleTrap> _traps;
  public SimpleTrapManager()
  {
    s_Singleton = this;

    _traps = new();
  }

  //
  public static SimpleTrap SpawnTrap(TrapType trapType, Vector3 atPosition)
  {
    var newTrap = new SimpleTrap(trapType, atPosition)
    {
      _Id = s_Id++
    };
    s_Singleton._traps.Add(newTrap._Id, newTrap);

    //
    return newTrap;
  }

}