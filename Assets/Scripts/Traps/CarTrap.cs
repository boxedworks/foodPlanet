
using Unity.Netcode;
using UnityEngine;
using TrapType = SimpleTrapManager.TrapType;

public class CarTrap : SimpleTrap
{

  GameObject _model;
  Rigidbody _rb;

  AudioSource _audioEngine;

  Vector3 _moveDirection;

  bool _active { get { return _activateTime < 1f; } }
  float _activateTime, _activeTimeMax, _speed;

  public CarTrap(Vector3 atPos, Vector3 moveDirection) : base(TrapType.CAR, atPos)
  {
    var model = GameObject.Find("Car");
    _model = GameObject.Instantiate(model);
    _model.name = model.name;
    SetCollider(_model.transform.GetChild(0).GetComponent<Collider>());

    _rb = _model.GetComponent<Rigidbody>();

    _audioEngine = AudioManager.PlayAudio("engine", _model.transform.position);
    _audioEngine.loop = true;

    _model.transform.position = atPos;

    _moveDirection = moveDirection;
    _model.transform.LookAt(atPos + _moveDirection * 10f);

    _activateTime = _activeTimeMax = 0f;
  }
  public override void Destroy()
  {
    base.Destroy();

    _audioEngine.Stop();
    _audioEngine.loop = false;
    _audioEngine = null;

    GameObject.Destroy(_model);
    _model = null;
    _rb = null;
  }

  public override void Activate()
  {
    _model.transform.position = _position;

    var distance = 50f;
    var time = 1.5f;

    _speed = distance;
    _activateTime = _activeTimeMax = time;
  }

  public override void Update()
  {
    _audioEngine.transform.position = _model.transform.position;

    if (_activateTime > 0f)
    {
    }
    else
    {
      if (NetworkManager.Singleton?.IsServer ?? false)
        ActivateRpc();
    }
  }

  public override void FixedUpdate()
  {
    if (_activateTime > 0f)
    {
      _activateTime = Mathf.Clamp(_activateTime - Time.fixedDeltaTime * 1f, 0f, _activeTimeMax);
      _rb.MovePosition(_position + _moveDirection * (1f - (_activateTime / _activeTimeMax)) * _speed);
    }
  }

  public void HandlePlayerCollision(PlayerController p)
  {

    if (!_active) return;

    var playerDirection = (p._Rb.position - _rb.position).normalized;
    Debug.Log($"{_moveDirection} .. {playerDirection} .. {(_moveDirection - playerDirection).magnitude}");
    if ((_moveDirection - playerDirection).magnitude <= 0.6f)
    {
      p.TakeDamageRpc(100);
      p.ApplyBodyForceRpc(playerDirection * 10f, HumanBodyBones.Hips);
    }

  }

}