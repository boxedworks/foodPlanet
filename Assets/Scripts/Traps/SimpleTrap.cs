
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
  public virtual void Destroy()
  {
    _collider = null;
  }

  public virtual void Update()
  {

  }

  public virtual void FixedUpdate()
  {

  }

  public virtual void Activate()
  {

  }
  public void ActivateRpc()
  {
    PlayerController.ActivateSimpleTrap(_Id);
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