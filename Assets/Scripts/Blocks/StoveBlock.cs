using Unity.Netcode;
using UnityEngine;
using BlockType = BlockManager.BlockType;

//
public class StoveBlock : Block
{

  PickupableSlot _slot;
  public bool _HasObject { get { return _slot._HasObject; } }
  public ulong _HeldObjectId { get { return _slot._Pickupable._NetworkObject.NetworkObjectId; } }

  EffectManager.EffectPair _effect;

  float _setTimer;

  public StoveBlock(GameObject gameObject) : base(BlockType.STOVE, gameObject)
  {
    _slot = new();
  }

  //
  public void Set(Pickupable pickupable)
  {
    _slot.Set(pickupable);
    _slot._Pickupable._GameObject.transform.position = _gameObject.transform.position + new Vector3(0f, 2f, 0f);

    _setTimer = Time.time;
  }
  public void Unset()
  {
    _slot.Unset();
  }

  //
  public override void Update()
  {

    //
    if (NetworkManager.Singleton?.IsServer ?? false)
    {

      if (_slot._HasObject)
        if (_slot._Pickupable._Type == PickupableManager.PickupableType.APPLE)
        {
          if (Time.time - _setTimer > 3f)
            PlayerController.s_LocalPlayer.RequestChangePickupableRpc(_slot._Pickupable._NetworkObject.NetworkObjectId, PickupableManager.PickupableType.BANANA);
        }
    }

    // Fx
    if (_slot._HasObject)
    {
      if (_effect == null)
      {
        _effect = EffectManager.PlayEffectAt(EffectManager.EffectType.GRILL_SIZZLE, _gameObject.transform.position);
        _effect._AudioSource.loop = true;
      }
    }
    else
    {
      if (_effect != null)
      {
        _effect._AudioSource.Stop();
        _effect._AudioSource.loop = false;

        _effect = null;
      }
    }

  }

  //
  public bool HasThisObject(Pickupable pickupable)
  {
    if (!_HasObject) return false;
    return _slot._Pickupable.Equals(pickupable);
  }
}