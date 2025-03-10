using Unity.Netcode;
using UnityEngine;
using BlockType = BlockManager.BlockType;

//
public class CounterBlock : Block
{

  PickupableSlot _slot;
  public bool _HasObject { get { return _slot._HasObject; } }
  public ulong _HeldObjectId { get { return _slot._Pickupable._NetworkObject.NetworkObjectId; } }

  public CounterBlock(GameObject gameObject) : base(BlockType.COUNTER, gameObject)
  {
    _slot = new();
  }

  //
  public void Set(Pickupable pickupable)
  {
    _slot.Set(pickupable);
    _slot._Pickupable._GameObject.transform.position = _gameObject.transform.position + new Vector3(0f, 2f, 0f);
  }
  public void Unset()
  {
    _slot.Unset();
  }

  public bool HasThisObject(Pickupable pickupable)
  {
    if (!_HasObject) return false;
    return _slot._Pickupable.Equals(pickupable);
  }
}