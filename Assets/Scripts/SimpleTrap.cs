
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

public class CarTrap : SimpleTrap
{

  GameObject _model;

  Vector3 _moveDirection;

  bool _active { get { return _activateTime < 1f; } }
  float _activateTime;

  public CarTrap(Vector3 atPos, Vector3 moveDirection) : base(TrapType.CAR, atPos)
  {
    var carModel = GameObject.Find("Car");
    _model = GameObject.Instantiate(carModel);
    _model.name = carModel.name;
    SetCollider(_model.GetComponent<Collider>());

    _model.transform.position = atPos;
    _moveDirection = moveDirection;

    _activateTime = 1f;
  }

  public override void Activate()
  {
    _model.transform.position = _position;
    _activateTime = 0f;
  }

  public override void Update()
  {
    if (_activateTime < 1f)
    {
      _activateTime = Mathf.Clamp(_activateTime + Time.deltaTime * 0.5f, 0f, 1f);
      _model.GetComponent<Rigidbody>().MovePosition(_position + _moveDirection * _activateTime * 30f);
    }
  }

  public void HandlePlayerCollision(PlayerController p)
  {

    if (!_active) return;

    var playerDirection = (p.transform.position - _model.transform.position).normalized;
    //Debug.Log($"{_moveDirection} .. {playerDirection} .. {(_moveDirection - playerDirection).magnitude}");
    if ((_moveDirection - playerDirection).magnitude <= 0.5f)
    {
      p.TakeDamageRpc(100);
      p.ApplyBodyForceRpc(playerDirection * 10f, HumanBodyBones.Hips);
    }

  }

}