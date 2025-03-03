using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Services.Vivox;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

  //
  static List<PlayerController> s_players;
  public static PlayerController s_LocalPlayer;

  //
  PlayerModelController _model;
  Collider _collider;
  Rigidbody _rb;

  NetworkObject _networkObject;
  VivoxController _vivoxController;

  public bool _isAlive { get { return _health > 0; } }
  int _health;

  Vector3 _moveDirection;

  NetworkVariable<float> _lookRotX = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

  HandController _leftHand, _rightHand;
  class HandController
  {
    public Rigidbody _Rb;
    public NetworkObject _NetworkObject;

    public HandController(NetworkObject networkObject)
    {
      _Rb = networkObject.GetComponent<Rigidbody>();
      _NetworkObject = networkObject;
    }
  }

  //
  PlayerNetworkData _networkData;
  struct PlayerNetworkData : INetworkSerializable
  {
    public Vector3 Position;
    public Quaternion Rotation;

    // INetworkSerializable
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
      serializer.SerializeValue(ref Position);
      serializer.SerializeValue(ref Rotation);
    }
    // ~INetworkSerializable
  }

  //


  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    s_players ??= new();
    s_players.Add(this);
    if (IsLocalPlayer)
    {
      s_LocalPlayer = this;

      GameController.OnClientConnected();
    }

    //
    _collider = transform.GetChild(0).GetComponent<Collider>();
    _rb = GetComponent<Rigidbody>();

    // Network setup
    _networkObject = GetComponent<NetworkObject>();
    if (_networkObject.IsOwner)
    {
      Cursor.lockState = CursorLockMode.Locked;

      _vivoxController = new();
    }

    Spawn();
  }

  void JoinChannelLobby()
  {
    StartCoroutine(JoinChannelLobbyCo());
  }
  IEnumerator JoinChannelLobbyCo()
  {
    var t = _vivoxController.InitializeAsync();
    yield return new WaitUntil(() => t.IsCompleted);

    t = _vivoxController.LoginToVivoxAsync();
    yield return new WaitUntil(() => t.IsCompleted);

    t = _vivoxController.JoinChannelAsync();
    yield return new WaitUntil(() => t.IsCompleted);

    //if (!_networkObject.IsOwnedByServer)
      VivoxService.Instance.MuteInputDevice();
  }
  void JoinChannelDeath()
  {
    StartCoroutine(JoinChannelDeathCo());
  }
  IEnumerator JoinChannelDeathCo()
  {
    var t = _vivoxController.InitializeAsync();
    yield return new WaitUntil(() => t.IsCompleted);

    t = _vivoxController.LoginToVivoxAsync();
    yield return new WaitUntil(() => t.IsCompleted);

    t = _vivoxController.JoinChannelDeathAsync();
    yield return new WaitUntil(() => t.IsCompleted);

    //if (!_networkObject.IsOwnedByServer)
      VivoxService.Instance.MuteInputDevice();
  }

  //
  void HandleInput()
  {

    if (_isAlive)
    {

      // Look up / down + left /right
      var mouseDelta = Input.mousePositionDelta;
      _lookRotX.Value = Mathf.Clamp(_lookRotX.Value + mouseDelta.y / 100f, -2f, 2f);
      transform.Rotate(new Vector3(0f, mouseDelta.x, 0f) / 3f);

      // Move
      var moveDir = Vector3.zero;
      if (Input.GetKey(KeyCode.A))
        moveDir += -transform.right;
      if (Input.GetKey(KeyCode.D))
        moveDir += transform.right;
      if (Input.GetKey(KeyCode.W))
        moveDir += transform.forward;
      if (Input.GetKey(KeyCode.S))
        moveDir += -transform.forward;
      moveDir = moveDir.normalized;
      if (_isTakingLongAction)
      {
        if (_isLongActionCancelable && moveDir.magnitude > 0.25f)
        {
          EmoteRpc(2, "", false);
        }

        _moveDirection = Vector3.zero;
      }
      else
        _moveDirection = moveDir;

      var leftClick = Input.GetMouseButtonDown(0);
      var rightClick = Input.GetMouseButtonDown(1);

      var emptyInteract = false;

      RaycastHit hit;
      if (Physics.SphereCast(Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f)), 0.1f, out hit))
      {

        if (leftClick || rightClick)
        {

          void HandleHand(Side side)
          {
            var hand = side == Side.LEFT ? _leftHand : _rightHand;
            if (hand != null)
            {

              // Place item on counter
              if (hit.collider.name.StartsWith("Counter"))
              {

                // Get counter block
                var counterBlock = BlockManager.GetBlock(BlockManager.BlockType.COUNTER, hit.collider.gameObject) as BlockManager.CounterBlock;
                if (counterBlock != null)
                {

                  // Check if counter empty
                  if (!counterBlock._HasObject)
                  {

                    var networkId = hand._NetworkObject.NetworkObjectId;

                    SetUnGrabRpc(networkId, side);
                    hand._Rb.isKinematic = true;

                    NetworkObjectWithBlockRpc(networkId, counterBlock._Id);
                  }

                }
              }
            }
            else
            {
              var pickupable = hit.transform.gameObject;
              if (pickupable.name.Equals("Pickupable"))
                GrabObject(pickupable.GetComponent<NetworkObject>(), side);
              else
                emptyInteract = true;
            }
          }

          if (leftClick) HandleHand(Side.LEFT);
          if (rightClick) HandleHand(Side.RIGHT);
        }

      }

      // Clicked air
      else
      {

        if (leftClick && _leftHand == null)
          emptyInteract = true;
        else if (rightClick && _rightHand == null)
          emptyInteract = true;

      }

      //
      if (!_isTakingLongAction)
      {

        // Either clicked on empty space, or clicked and there was nothing to interact with
        if (emptyInteract)
        {

          // Try punch...
          if (leftClick)
            Punch(Side.LEFT);
          else if (rightClick)
            Punch(Side.RIGHT);
        }

        // Emote
        if (Input.GetKeyDown(KeyCode.Alpha1))
          Emote(0, true);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
          Emote(1, true);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
          Emote(2, true);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
          Emote(3, true);
      }

      // Server stuff
      if (Input.GetKeyDown(KeyCode.O))
      {
        PickupableManager.SpawnPickupable(new Vector3(2f, 10f, 0f), PickupableManager.PickupableType.APPLE);
        PickupableManager.SpawnPickupable(new Vector3(2.2f, 11f, 0f), PickupableManager.PickupableType.BANANA);
      }
    }
    else
    {
      if (Input.GetKeyDown(KeyCode.P))
      {
        SpawnServerRpc();
      }
    }
  }

  //
  bool _isTakingLongAction, _isLongActionCancelable;

  [Rpc(SendTo.Everyone)]
  void EmoteRpc(int mode, string emoteName, bool cancelable)
  {
    switch (mode)
    {

      case 0:

        EmoteStart(emoteName, cancelable);
        break;
      case 1:

        EmoteEnd();
        break;
      case 2:

        EmoteCancel();
        break;

    }
  }
  void EmoteStart(string emoteName, bool cancelable)
  {
    _isTakingLongAction = true;
    _isLongActionCancelable = cancelable;

    _model._FullBodyWeight = 1f;

    _model._Animator.applyRootMotion = true;

    _model._Animator.ResetTrigger("ExitEmote");
    _model._Animator.Play(emoteName, 3);
  }
  void EmoteEnd()
  {
    _isTakingLongAction = false;
    _model._FullBodyWeight = 0f;

    _model._Animator.applyRootMotion = false;
  }
  void EmoteCancel()
  {
    _model._Animator.SetTrigger("ExitEmote");
  }

  //
  void Punch(Side side)
  {
    EmoteRpc(0, side == Side.LEFT ? "Punch_Left" : "Punch_Right", false);
  }

  void Emote(int index, bool cancelable)
  {
    EmoteRpc(0, $"Emote{index}", cancelable);
  }

  // Update is called once per frame
  void Update()
  {


    if (_isAlive)
    {

      if (IsLocalPlayer)
      {

        // Vivox
        if (VivoxService.Instance != null && VivoxService.Instance.ActiveChannels.Count > 0 && _vivoxController._InChannelMain)
          VivoxService.Instance.Set3DPosition(gameObject, _vivoxController._ChannelName);
      }

      //
      _model.Update(transform, _isTakingLongAction);
    }

    // Handle input
    if (IsLocalPlayer)
      HandleInput();
  }

  //
  public enum Side
  {
    LEFT,
    RIGHT
  }
  void GrabObject(NetworkObject obj, Side side)
  {

    var objId = obj.NetworkObjectId;

    // Set as object owner on server
    if (!obj.IsOwner)
      SetAsNetworkOwnerRpc(objId);

    // Broadcast object grab to all clients
    GrabObjectRpc(objId, side);
  }

  [Rpc(SendTo.Server)]
  void SetAsNetworkOwnerRpc(ulong objId)
  {
    NetworkManager.Singleton.SpawnManager.SpawnedObjects[objId].ChangeOwnership(_networkObject.OwnerClientId);
  }
  [Rpc(SendTo.Everyone)]
  void GrabObjectRpc(ulong objId, Side side)
  {

    var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objId];

    // Check others grabbed
    foreach (var player in s_players)
    {

      foreach (var side_ in new Side[] { Side.LEFT, Side.RIGHT })
      {
        var grabbedObj = side_ == Side.LEFT ? player._leftHand : player._rightHand;
        if (grabbedObj == null) continue;

        if (grabbedObj._NetworkObject.NetworkObjectId == objId)
        {
          player.UnGrab(side_);
          break;
        }
      }
    }

    // Check counters
    foreach (BlockManager.CounterBlock counter in BlockManager.GetBlocksByType(BlockManager.BlockType.COUNTER))
    {

      if (!counter._HasObject) continue;
      if (!counter.HasThisObject(obj.gameObject)) continue;

      counter.UnsetObject();
    }

    // Grab
    var rb = obj.GetComponent<Rigidbody>();
    if (side == Side.LEFT)
      _leftHand = new(obj);
    else
      _rightHand = new(obj);
    rb.isKinematic = true;

    var transformObj = obj.GetComponent<NetworkTransform>();
    transformObj.SyncPositionX = false;
    transformObj.SyncPositionY = false;
    transformObj.SyncPositionZ = false;

    Debug.Log("Grab");

    _model.SetArmWeight(1f, side);
    AudioManager.PlayAudio("pickup_object", _model._Transform.position);
  }

  [Rpc(SendTo.Everyone)]
  void SetUnGrabRpc(ulong objId, Side side)
  {

    var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objId];

    // Check others grabbed
    foreach (var player in s_players)
    {

      var grabbedObj = side == Side.LEFT ? player._leftHand : player._rightHand;
      if (grabbedObj == null) continue;

      if (grabbedObj._NetworkObject.NetworkObjectId == objId)
        player.UnGrab(side);
    }

    var transformObj = obj.GetComponent<NetworkTransform>();
    transformObj.SyncPositionX = true;
    transformObj.SyncPositionY = true;
    transformObj.SyncPositionZ = true;

    //
    //obj.GetComponent<Collider>().enabled = true;
  }

  void UnGrab(Side side)
  {
    var grabbedObj = side == Side.LEFT ? _leftHand : _rightHand;
    if (grabbedObj._NetworkObject.OwnerClientId == s_LocalPlayer.OwnerClientId)
      grabbedObj._Rb.isKinematic = false;
    if (side == Side.LEFT)
      _leftHand = null;
    else
      _rightHand = null;

    _model.SetArmWeight(0f, side);
    AudioManager.PlayAudio("pickup_object", _model._Transform.position);
  }

  // Interact with block
  [Rpc(SendTo.Everyone)]
  void NetworkObjectWithBlockRpc(ulong objId, int blockId)
  {

    // Get network object and block
    var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objId];
    var block = BlockManager.GetBlockById(blockId);

    // Check different interactions per block type
    switch (block._Type)
    {

      case BlockManager.BlockType.COUNTER:

        var counter = block as BlockManager.CounterBlock;
        if (!counter._HasObject)
        {
          counter.SetObject(obj.gameObject);
        }

        break;

    }

  }
  [Rpc(SendTo.Everyone)]
  void BlockRpc(int blockId)
  {

    // Get block
    var block = BlockManager.GetBlockById(blockId);

    // Check different interactions per block type
    switch (block._Type)
    {

      case BlockManager.BlockType.COUNTER:

        var counter = block as BlockManager.CounterBlock;
        if (!counter._HasObject)
        {
          counter.UnsetObject();
        }

        break;

    }

  }

  //
  public void FixedUpdate()
  {

    if (_isAlive)
    {
      if (IsLocalPlayer)
      {

        //
        if (_isTakingLongAction)
        {
          var pos = _model._Animator.GetBoneTransform(HumanBodyBones.Hips).position;
          pos.y = _rb.position.y;
          _rb.MovePosition(pos);
        }

        // Move player rb
        else if (_moveDirection.magnitude > 0f)
        {
          var moveSpeed = 8f;
          var movePosition = _rb.position + _moveDirection * Time.fixedDeltaTime * moveSpeed;
          _rb.MovePosition(movePosition);
        }

      }

    }
  }

  public void LateUpdate()
  {

    if (_isAlive)
    {

      // Set spine tilt
      var canSetSpineTilt = !_isTakingLongAction || !_isLongActionCancelable || (_model._Animator.GetCurrentAnimatorClipInfo(3).Length > 0 && _model._Animator.GetCurrentAnimatorClipInfo(3)[0].clip.isLooping);
      if (canSetSpineTilt)
        _model.SetSpineTilt(Mathf.Lerp(70f, -70f, (_lookRotX.Value + 2f) / 4f));

      if (IsLocalPlayer)
      {
        // Move camera with player
        var camera = Camera.main;
        camera.transform.position = _model._Transform.position + _model._Transform.up * 0.15f + _model._Transform.forward * -0.1f; //transform.position + transform.forward * 0.2f + transform.up * 1f;
        camera.transform.rotation = Quaternion.LookRotation(new Vector3(transform.forward.x, _lookRotX.Value, transform.forward.z));
      }

      // Move grabbed object
      if (_leftHand != null)
      {

        var movePosition = _model._Animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
        if (IsLocalPlayer)
          _leftHand._Rb.rotation = Camera.main.transform.rotation;
        _leftHand._Rb.position = movePosition;
      }
      if (_rightHand != null)
      {

        var movePosition = _model._Animator.GetBoneTransform(HumanBodyBones.RightHand).position;
        if (IsLocalPlayer)
          _rightHand._Rb.rotation = Camera.main.transform.rotation;
        _rightHand._Rb.position = movePosition;
      }
    }
  }

  //
  void OnCollisionEnter(Collision collision)
  {
    //Debug.Log($"{collision.collider.name} {collision.collider.isTrigger}");
  }
  void OnTriggerEnter(Collider other)
  {

    var obj = other.gameObject;
    var name = other.name;
    if (name == "Mesh")
    {
      obj = other.transform.parent.gameObject;
      name = obj.name;
    }

    // Check network collision
    var networkObj = obj.GetComponent<NetworkObject>();
    if (IsLocalPlayer && networkObj != null)
    {
      HandleNetworkCollision(networkObj.NetworkObjectId);
      HandleNetworkCollisionRpc(networkObj.NetworkObjectId);
    }
  }

  //
  [Rpc(SendTo.NotMe)]
  void HandleNetworkCollisionRpc(ulong collideWithId)
  {
    HandleNetworkCollision(collideWithId);
  }
  void HandleNetworkCollision(ulong collideWithId)
  {

    var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[collideWithId];
    switch (obj.name)
    {

      // Mine explode
      case "Mine":

        var mine = obj.GetComponent<MineController>();
        mine.Trigger();

        TakeDamage(_health);
        break;

    }
  }

  //
  void TakeDamage(int damage)
  {

    if (!_isAlive) return;
    _health = Mathf.Clamp(_health - damage, 0, 100);

    // Check dead
    if (!_isAlive)
    {
      Die();
    }
  }

  //
  public void OnAnimationEvent(AnimationEvent animationEvent)
  {

    switch (animationEvent.stringParameter)
    {
      case "step":
        if (animationEvent.animatorClipInfo.weight < 0.5)
          break;

        AudioManager.PlayAudio("footstep", _model._Transform.position);
        break;

      case "punch":
        Debug.Log(animationEvent.stringParameter);
        break;

      //
      case "endAnimation":
        Debug.Log(animationEvent.stringParameter);

        EmoteEnd();

        break;

      default:

        Debug.LogWarning(animationEvent.stringParameter);

        break;
    }
  }

  //
  [Rpc(SendTo.Server)]
  void SpawnServerRpc()
  {
    SpawnClientRpc();
  }
  [Rpc(SendTo.Everyone)]
  void SpawnClientRpc()
  {
    Spawn();
  }
  void Spawn()
  {
    _health = 100;

    _rb.isKinematic = false;
    _collider.enabled = true;

    _rb.position = new Vector3(0f, 5f, 0f);

    _model = new PlayerModelController();
    _model.SetArmWeight(0f, Side.LEFT);
    _model.SetArmWeight(0f, Side.RIGHT);

    _model._Animator.GetComponent<AnimationListener>().RegisterController(this);

    if (IsLocalPlayer)
      JoinChannelLobby();
  }

  //
  void Die()
  {
    _rb.isKinematic = true;
    _collider.enabled = false;

    // Ragdoll body
    _model.Ragdoll();

    // Drop object if holding
    if (_leftHand != null)
      SetUnGrabRpc(_leftHand._NetworkObject.NetworkObjectId, Side.LEFT);
    if (_rightHand != null)
      SetUnGrabRpc(_rightHand._NetworkObject.NetworkObjectId, Side.RIGHT);

    // Move to death lobby
    if (IsLocalPlayer)
      JoinChannelDeath();
  }

  // Network helper functions
  [Rpc(SendTo.Everyone)]
  public void RequestSpawnPickupableRpc(ulong networkId, PickupableManager.PickupableType asType)
  {
    PickupableManager.RequestSpawnPickupableRpc(networkId, asType);
  }
}