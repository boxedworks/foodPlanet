
using UnityEngine;
using TrapType = SimpleTrapManager.TrapType;

//
public class SimpleTrap
{

  public int _Id;
  public TrapType _TrapType;
  protected Vector3 _position;

  Collider _collider;

  public SimpleTrap(TrapType trapType, Vector3 atPos)
  {
    _TrapType = trapType;

    _position = atPos;
  }

  public virtual void Update()
  {

  }

  public virtual void Activate()
  {

  }

  protected void SetCollider(Collider c)
  {
    _collider = c;
  }
  public bool IsTrap(Collider c)
  {
    return _collider.Equals(c);
  }

}