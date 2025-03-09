using Unity.Netcode;
using UnityEngine;
using BlockType = BlockManager.BlockType;

//
public class CounterBlock : Block
{

  GameObject _heldObject;
  public bool _HasObject { get { return _heldObject != null; } }

  public CounterBlock(GameObject gameObject) : base(BlockType.COUNTER, gameObject)
  {

  }

  //
  public void SetObject(GameObject gameObject)
  {
    _heldObject = gameObject;
    _heldObject.transform.position = _gameObject.transform.position + new Vector3(0f, 2f, 0f);
  }
  public void UnsetObject()
  {
    _heldObject = null;
  }

  public ulong _HeldObjectId { get { return _heldObject.GetComponent<NetworkObject>().NetworkObjectId; } }

  public bool HasThisObject(GameObject gameObject)
  {
    if (!_HasObject) return false;
    return _heldObject.Equals(gameObject);
  }
}