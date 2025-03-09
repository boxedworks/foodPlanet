using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Services.Vivox;
using UnityEngine;

using System.Linq;

public class PlayerController : CustomNetworkController
{

  //
  static List<PlayerController> s_players;
  public static PlayerController s_LocalPlayer;

  //
  NetworkObject _networkObject;
  VivoxController _vivoxController;

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

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  new void Start()
  {
    base.Start();

    s_players ??= new();
    s_players.Add(this);
    if (IsLocalPlayer)
    {
      s_LocalPlayer = this;

      GameController.OnClientConnected();
    }

    //
    _collider = transform.GetChild(0).GetComponent<Collider>();

    // Network setup
    _networkObject = GetComponent<NetworkObject>();
    if (_networkObject.IsOwner)
    {
      Cursor.lockState = CursorLockMode.Locked;

      _vivoxController = new();
    }

    Spawn();
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
      if (_model._IsAnimating)
      {
        if (_model._IsAnimationCancelable && moveDir.magnitude > 0.25f)
          EmoteRpc(2, "", false);

        _moveDirection = Vector3.zero;
      }
      else
        _moveDirection = moveDir;

      // Check running
      _isRunning = Input.GetKey(KeyCode.LeftShift);

      //
      var leftClick = Input.GetMouseButtonDown(0);
      var rightClick = Input.GetMouseButtonDown(1);

      var emptyInteract = false;

      var hit = SimpleSpherecast(0.1f, 10f);
      if (hit.collider != null)
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
                var counterBlock = BlockManager.GetBlock(BlockManager.BlockType.COUNTER, hit.collider.gameObject) as CounterBlock;
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
              var counterBlock = BlockManager.GetBlock(BlockManager.BlockType.COUNTER, hit.collider.gameObject) as CounterBlock;

              // Check pickupable
              if (pickupable.name.Equals("Pickupable"))
                GrabObject(pickupable.GetComponent<NetworkObject>(), side);

              // Check counter
              else if (counterBlock != null)
                BlockRpc(counterBlock._Id, side);

              //
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
      if (!_model._IsAnimating)
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
      if (Input.GetKeyDown(KeyCode.U))
      {
        SpawnSimpleTrap(SimpleTrapManager.TrapType.CAR, new Vector3(-25f, 1.2f, -25f));
        SpawnSimpleTrap(SimpleTrapManager.TrapType.DOOR, new Vector3(-28.6f, 1.5f, 0f));
      }
      if (Input.GetKeyDown(KeyCode.T))
      {
        SimpleTrapManager.Cleanup();
      }
    }
    else
    {

      // Respawn
      if (Input.GetKeyDown(KeyCode.P))
        SpawnServerRpc();
    }
  }

  // Update is called once per frame
  new void Update()
  {
    base.Update();

    //
    if (IsLocalPlayer)
    {

      // Move vivox 3d position with camera
      if (VivoxService.Instance != null && VivoxService.Instance.ActiveChannels.Count > 0 && _vivoxController._InChannelMain)
        VivoxService.Instance.Set3DPosition(Camera.main.gameObject, _vivoxController._ChannelName);

      // Handle input
      HandleInput();
    }
  }

  //
  new void FixedUpdate()
  {
    base.FixedUpdate();

    if (_isAlive)
      if (IsLocalPlayer)
      {

        // Check fall to death
        if (_rb.position.y < -30f)
          TakeDamageRpc(_health);
      }
  }

  public void LateUpdate()
  {

    if (_isAlive)
    {

      // Set spine tilt
      var canSetSpineTilt = !_model._IsAnimating || !_model._IsAnimationCancelable || (_model._Animator.GetCurrentAnimatorClipInfo(3).Length > 0 && _model._Animator.GetCurrentAnimatorClipInfo(3)[0].clip.isLooping);
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
    foreach (CounterBlock counter in BlockManager.GetBlocksByType(BlockManager.BlockType.COUNTER))
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

        var counter = block as CounterBlock;
        if (!counter._HasObject)
        {
          counter.SetObject(obj.gameObject);
        }

        break;

    }

  }
  [Rpc(SendTo.Everyone)]
  void BlockRpc(int blockId, Side side)
  {

    // Get block
    var block = BlockManager.GetBlockById(blockId);

    // Check different interactions per block type
    switch (block._Type)
    {

      case BlockManager.BlockType.COUNTER:

        var counter = block as CounterBlock;
        if (counter._HasObject)
        {
          GrabObjectRpc(counter._HeldObjectId, side);
        }

        break;

    }

  }

  // Network helper functions
  [Rpc(SendTo.Everyone)]
  public void RequestSpawnPickupableRpc(ulong networkId, PickupableManager.PickupableType asType)
  {
    PickupableManager.RequestSpawnPickupableRpc(networkId, asType);
  }
  [Rpc(SendTo.Everyone)]
  public void RequestChangePickupableRpc(ulong networkId, PickupableManager.PickupableType toType)
  {
    PickupableManager.RequestChangePickupableToRpc(networkId, toType);
  }

  #region Vivox

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


  #endregion
  #region Physics

  //
  void OnCollisionStay(Collision collision)
  {

    //
    if (!_isAlive) return;
    if (!IsLocalPlayer) return;

    //
    var obj = collision.gameObject;
    if (obj.name == "Mesh")
      obj = collision.gameObject.transform.parent.gameObject;

    //
    switch (obj.name)
    {

      case "Car":

        SimpleTrapManager.GetSimpleTrapFromCollider(collision.collider, (trap) =>
        {
          var carTrap = trap as CarTrap;
          carTrap.HandlePlayerCollision(this);
        });

        break;

    }
  }
  void OnTriggerEnter(Collider collider)
  {

    //
    if (!_isAlive) return;

    //
    var obj = collider.gameObject;
    if (obj.name == "Mesh")
      obj = collider.transform.parent.gameObject;

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
        ApplyBodyForce(new Vector3(0f, 1f, 0f) * 5f, HumanBodyBones.Hips);
        break;

    }
  }

  // Apply physics force to player if not alive; use this to affect body after death (ex; explosion force)
  [Rpc(SendTo.Everyone)]
  public void ApplyBodyForceRpc(Vector3 applyForce, HumanBodyBones bodyBone)
  {
    ApplyBodyForce(applyForce, bodyBone);
  }
  void ApplyBodyForce(Vector3 applyForce, HumanBodyBones bodyBone)
  {
    if (_isAlive) return;

    var bodyPart = _model._Animator.GetBoneTransform(bodyBone);
    bodyPart.GetComponent<Rigidbody>().AddForce(applyForce * 500f);
  }

  #endregion
  #region Animation

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
        if (IsLocalPlayer)
          SimpleSpherecastToPlayer(0.1f, 1.8f,
          (playerData) =>
          {
            var player = playerData._Player;

            player.TakeDamageRpc(50);
            player.ApplyBodyForceRpc((player._model._Transform.position - _model._Transform.position).normalized * 1f, playerData._BodyBone);
          },

          (rayCastHit) =>
          {
            GameObject.Find("p1").transform.position = rayCastHit.point;
            GameObject.Find("p1").GetComponent<ParticleSystem>().Play();
          });

        break;

      //
      case "endAnimation":
        EmoteEnd();

        break;

      default:

        Debug.LogWarning(animationEvent.stringParameter);

        break;
    }
  }

  //

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
  void EmoteStart(string emoteName, bool isCancelable)
  {
    _model.EmoteStart(emoteName, isCancelable);

    _rb.isKinematic = true;
  }
  void EmoteEnd()
  {
    _model.EmoteEnd();

    _rb.isKinematic = false;
  }
  void EmoteCancel()
  {
    _model.EmoteCancel();
  }
  void Emote(int index, bool cancelable)
  {
    EmoteRpc(0, $"Emote{index}", cancelable);
  }

  //
  void Punch(Side side)
  {
    EmoteRpc(0, side == Side.LEFT ? "Punch_Left" : "Punch_Right", false);
  }

  #endregion
  #region Spawning, Damage, and Death

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

    _model = new HumanModelController();
    _model.SetArmWeight(0f, Side.LEFT);
    _model.SetArmWeight(0f, Side.RIGHT);

    _model._Animator.GetComponent<AnimationListener>().RegisterController(this);

    if (IsLocalPlayer)
      JoinChannelLobby();
  }

  //
  [Rpc(SendTo.Everyone)]
  public void TakeDamageRpc(int damage)
  {
    TakeDamage(damage);
  }
  void TakeDamage(int damage)
  {

    if (!_isAlive) return;
    _health = Mathf.Clamp(_health - damage, 0, 100);

    // Fx
    EffectManager.PlayEffectAt(EffectManager.EffectType.BLOOD_SPURT, _model._Transform.position);

    // Check dead
    if (!_isAlive)
      Die();
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

  #endregion
  #region Spherecast Tools

  //
  RaycastHit SimpleSpherecast(float radius, float distance)
  {
    RaycastHit hit;
    Physics.SphereCast(Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f)), radius, out hit, distance);
    return hit;
  }
  void SimpleSpherecastAllOrdered(float radius, float distance, System.Action<RaycastHit[]> onSpherecast)
  {
    var hits = Physics.SphereCastAll(Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f)), radius, distance, LayerMask.GetMask(new string[] { "Default", "Visual", "Objects" }));
    System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
    onSpherecast?.Invoke(hits.Where((r) => r.distance != 0f).ToArray());
  }
  void SimpleSpherecastToPlayer(float radius, float distance, System.Action<PlayerWithBodyPart> onSpherecast, System.Action<RaycastHit> onNonPlayer = null)
  {

    SimpleSpherecastAllOrdered(radius, distance, (hits) =>
    {
      foreach (var hit in hits)
      {

        // Check if is player
        var hitPlayerData = GetPlayerFromBodyPart(hit.collider.gameObject);
        if (hitPlayerData != null)
          onSpherecast?.Invoke(hitPlayerData);
        else
          onNonPlayer?.Invoke(hit);

        // Don't go through walls
        break;
      }
    });
  }
  class PlayerWithBodyPart
  {
    public PlayerController _Player;
    public HumanBodyBones _BodyBone;
  }
  static PlayerWithBodyPart GetPlayerFromBodyPart(GameObject gameObject)
  {
    foreach (var player in s_players)
    {
      var foundPlayer = player.IsSelf(gameObject);
      if (foundPlayer != null)
        return foundPlayer;
    }

    return null;
  }
  PlayerWithBodyPart IsSelf(GameObject gameObject)
  {
    foreach (var bodyPart in new HumanBodyBones[]{
      HumanBodyBones.Head,
      HumanBodyBones.Spine,
      HumanBodyBones.Hips,
      HumanBodyBones.LeftLowerArm,
      HumanBodyBones.LeftUpperArm,
      HumanBodyBones.RightLowerArm,
      HumanBodyBones.RightUpperArm,
      HumanBodyBones.LeftLowerLeg,
      HumanBodyBones.LeftUpperLeg,
      HumanBodyBones.RightLowerLeg,
      HumanBodyBones.RightUpperLeg
    })
      if (_model._Animator.GetBoneTransform(bodyPart).gameObject.Equals(gameObject))
        return new PlayerWithBodyPart()
        {
          _Player = this,
          _BodyBone = bodyPart
        };
    return null;
  }

  #endregion
  #region RPC Wrappers

  [Rpc(SendTo.Everyone)]
  void SpawnSimpleTrapRPC(SimpleTrapManager.TrapType trapType, Vector3 atPosition)
  {
    SimpleTrapManager.SpawnTrap(trapType, atPosition);
  }
  public static void SpawnSimpleTrap(SimpleTrapManager.TrapType trapType, Vector3 atPosition)
  {
    s_LocalPlayer.SpawnSimpleTrapRPC(trapType, atPosition);
  }

  [Rpc(SendTo.Everyone)]
  void ActivateSimpleTrapRPC(int trapId)
  {
    SimpleTrapManager.ActivateTrapById(trapId);
  }
  public static void ActivateSimpleTrap(SimpleTrap simpleTrap)
  {
    s_LocalPlayer.ActivateSimpleTrapRPC(simpleTrap._Id);
  }
  public static void ActivateSimpleTrap(int trapId)
  {
    s_LocalPlayer.ActivateSimpleTrapRPC(trapId);
  }

  #endregion
}