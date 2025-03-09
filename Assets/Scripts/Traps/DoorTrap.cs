
using System;
using Unity.Netcode;
using UnityEngine;
using TrapType = SimpleTrapManager.TrapType;

public class DoorTrap : SimpleTrap
{

  GameObject _model;
  Rigidbody _rb;

  AudioSource _audioDoor;

  public bool _IsOpen { get { return _isOpen; } }
  bool _isOpen;

  float _timer, _timerMax;
  Vector3 _openPosition;

  public DoorTrap(Vector3 atPos, Vector3 lookDir) : base(TrapType.DOOR, atPos)
  {
    var model = GameObject.Find("Door");
    _model = GameObject.Instantiate(model);
    _model.name = model.name;

    _rb = _model.transform.GetChild(0).transform.GetComponent<Rigidbody>();
    SetCollider(_rb.GetComponent<Collider>());

    _model.transform.position = atPos;
    _model.transform.LookAt(atPos + lookDir * 10f);

    _openPosition = _rb.transform.position + _rb.transform.TransformDirection(new Vector3(1f, 0f, 0f)) * 3.5f;
    _timerMax = 1f;

    _audioDoor = AudioManager.PlayAudio("door", atPos);
    _audioDoor.loop = true;
  }
  public override void Destroy()
  {
    base.Destroy();

    _audioDoor.Stop();
    _audioDoor.loop = false;
    _audioDoor = null;

    GameObject.Destroy(_model);
    _model = null;
    _rb = null;
  }

  public override void Activate()
  {
    _isOpen = !_isOpen;
  }

  public override void Update()
  {
    if (NetworkManager.Singleton?.IsServer ?? false)
      if (_timer == 0f || _timer == _timerMax)
      {
        if (UnityEngine.Random.value < 0.02f)
          ActivateRpc();

        _audioDoor.pitch = 0f;
      }
      else
        _audioDoor.pitch = _isOpen ? 1.1f : 0.8f;
  }

  public override void FixedUpdate()
  {
    _timer = Mathf.Clamp(_timer + Time.fixedDeltaTime * 1f * (_isOpen ? 1 : -1), 0f, _timerMax);

    var pos = Vector3.Lerp(_position, _openPosition, _timer / _timerMax);
    _rb.MovePosition(pos);
  }

  public void HandlePlayerCollision(PlayerController p)
  {

  }

}