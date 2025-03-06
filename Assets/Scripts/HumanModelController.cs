using UnityEngine;

public class HumanModelController
{

  Animator _animator;
  public Animator _Animator { get { return _animator; } }
  Transform _model, _spine, _head;
  public Transform _Transform { get { return _head; } }

  public float _FullBodyWeight { get { return _desiredFullBodyWeight; } set { _desiredFullBodyWeight = value; } }
  float _desiredFullBodyWeight, _fullBodyWeight;

  float _moveSpeed;

  public bool _IsTakingLongAction { get { return _isTakingLongAction; } }
  public bool _IsLongActionCancelable { get { return _isLongActionCancelable; } }
  bool _isTakingLongAction, _isLongActionCancelable;

  //
  public HumanModelController()
  {

    _model = GameObject.Instantiate(GameObject.Find("humanoid0")).transform;
    _spine = _model.Find("Armature/mixamorig:Hips/mixamorig:Spine");
    _head = _spine.Find("mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head");

    _animator = _model.GetComponent<Animator>();
  }

  //
  public void Update(Transform transform)
  {

    // Stop move speed
    if (_isTakingLongAction)
    {
      _moveSpeed += (0f - _moveSpeed) * Time.deltaTime * 5f;
    }

    // Move position
    else
    {
      var targetPosition = transform.position + new Vector3(0f, -2f, 0f);
      var moveDistance = targetPosition - _model.position;
      var moveSpeed = moveDistance.magnitude;
      if ((moveDistance.normalized - transform.forward).magnitude > 1)
        moveSpeed *= -1f;

      _model.position += (targetPosition - _model.position) * Time.deltaTime * 5f;
      _model.position = new Vector3(_model.position.x, targetPosition.y, _model.position.z);

      _moveSpeed += (moveSpeed - _moveSpeed) * Time.deltaTime * 5f;

    }

    // Set rotation
    _model.rotation = transform.rotation;

    // Update animator value
    _animator.SetFloat("MoveSpeed", _moveSpeed);

    // Lerp 4th layer weight
    _fullBodyWeight += (_desiredFullBodyWeight - _fullBodyWeight) * Time.deltaTime * 5f;
    _animator.SetLayerWeight(3, _fullBodyWeight);
  }

  //
  public void EmoteStart(string emoteName, bool isCancelable)
  {
    _isTakingLongAction = true;
    _isLongActionCancelable = isCancelable;

    _FullBodyWeight = 1f;

    _Animator.applyRootMotion = true;

    _Animator.ResetTrigger("ExitEmote");
    _Animator.Play(emoteName, 3);
  }
  public void EmoteEnd()
  {
    _isTakingLongAction = false;

    _FullBodyWeight = 0f;

    _Animator.applyRootMotion = false;
  }
  public void EmoteCancel()
  {
    _Animator.SetTrigger("ExitEmote");
  }

  //
  public void SetSpineTilt(float angle)
  {
    _spine.eulerAngles = new Vector3(angle, _spine.eulerAngles.y, _spine.eulerAngles.z);
  }

  public void SetLeftArmWeight(float weight)
  {
    _animator.SetLayerWeight(1, weight);
  }
  public void SetRightArmWeight(float weight)
  {
    _animator.SetLayerWeight(2, weight);
  }
  public void SetArmWeight(float weight, PlayerController.Side side)
  {
    if (side == PlayerController.Side.LEFT)
      SetLeftArmWeight(weight);
    else
      SetRightArmWeight(weight);
  }

  //
  public void Ragdoll()
  {
    void SetJointLimits(HingeJoint joint, int min, int max)
    {
      joint.useLimits = true;
      var limits = joint.limits;
      limits.min = min;
      limits.max = max;
      joint.limits = limits;
    }

    // Disable animator
    _Animator.enabled = false;

    // Add rigidbodies and joints to bones
    var hip = _Animator.GetBoneTransform(HumanBodyBones.Hips).gameObject.AddComponent<Rigidbody>();
    var spine = _Animator.GetBoneTransform(HumanBodyBones.Spine).gameObject.AddComponent<Rigidbody>();

    var joint = hip.gameObject.AddComponent<HingeJoint>();
    joint.connectedBody = spine;
    SetJointLimits(joint, -45, 90);

    var head = _Animator.GetBoneTransform(HumanBodyBones.Head).gameObject.AddComponent<Rigidbody>();

    joint = spine.gameObject.AddComponent<HingeJoint>();
    joint.connectedBody = head;
    SetJointLimits(joint, -45, 45);

    var legL = _Animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).gameObject.AddComponent<Rigidbody>();

    joint = hip.gameObject.AddComponent<HingeJoint>();
    joint.connectedBody = legL;
    SetJointLimits(joint, -45, 45);

    var legR = _Animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).gameObject.AddComponent<Rigidbody>();

    joint = hip.gameObject.AddComponent<HingeJoint>();
    joint.connectedBody = legR;
    SetJointLimits(joint, -45, 45);

    //
    //SetGameLayerRecursive(_model.gameObject, 9);
  }

  private void SetGameLayerRecursive(GameObject gameObject, int layer)
  {
    gameObject.layer = layer;
    foreach (Transform child in gameObject.transform)
    {
      SetGameLayerRecursive(child.gameObject, layer);
    }
  }

}
