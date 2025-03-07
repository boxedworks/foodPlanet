

using Unity.Netcode;
using UnityEngine;

public class CustomNetworkController : NetworkBehaviour
{
  public bool _isAlive { get { return _health > 0; } }
  protected int _health;

  protected Collider _collider;
  protected Rigidbody _rb;
  protected Vector3 _moveDirection;
  protected bool _isRunning;

  protected HumanModelController _model;

  //
  public void Start()
  {
    _rb = GetComponent<Rigidbody>();
  }

  public void Update()
  {

    // Move model if alive
    if (_isAlive)
      _model.Update(transform);
  }

  public void FixedUpdate()
  {

    // If alive, sync model transform with controller gameobject
    if (_isAlive)
      if (IsLocalPlayer)
      {

        // If animating, move controller with model
        if (_model._IsAnimating)
        {
          var pos = _model._Animator.GetBoneTransform(HumanBodyBones.Hips).position;
          pos.y = _rb.position.y;
          _rb.position = pos;
        }

        // Move player rb
        else if (_moveDirection.magnitude > 0f)
        {
          var moveSpeed = _isRunning ? 8f : 3f;
          var movePosition = _rb.position + _moveDirection * Time.fixedDeltaTime * moveSpeed;
          _rb.MovePosition(movePosition);
        }
      }
  }

}