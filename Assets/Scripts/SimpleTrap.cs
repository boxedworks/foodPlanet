
using UnityEngine;
using TrapType = SimpleTrapManager.TrapType;

//
public class SimpleTrap
{

  public int _Id;
  public TrapType _TrapType;
  Vector3 _position;

  public SimpleTrap(TrapType trapType, Vector3 atPos)
  {
    _TrapType = trapType;

    _position = atPos;
  }

}