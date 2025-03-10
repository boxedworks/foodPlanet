using UnityEngine;
using Unity.Netcode;
using PickupableType = PickupableManager.PickupableType;
using System.Collections.Generic;

//
public class Pickupable
{

  //
  public NetworkObject _NetworkObject;
  public GameObject _GameObject { get { return _NetworkObject.gameObject; } }

  public PickupableType _Type;

  //
  Dictionary<BlockManager.BlockType, float> _blockData;

  //
  public Pickupable(PickupableType type, NetworkObject networkObject)
  {
    _Type = type;

    _NetworkObject = networkObject;

    _blockData = new();
  }

  //
  public bool Equals(NetworkObject obj)
  {
    return _NetworkObject.NetworkObjectId == obj.NetworkObjectId;
  }
  public bool HasBlockData(BlockManager.BlockType blockType)
  {
    return GetBlockData(blockType) != -1f;
  }
  public float GetBlockData(BlockManager.BlockType blockType)
  {
    if (!_blockData.ContainsKey(blockType))
      return -1f;
    return _blockData[blockType];
  }
  public void SetBlockData(BlockManager.BlockType blockType, float val)
  {
    if (!_blockData.ContainsKey(blockType))
      _blockData.Add(blockType, -1f);
    _blockData[blockType] = val;
  }
}