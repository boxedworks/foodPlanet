using Unity.Netcode;
using UnityEngine;
using BlockType = BlockManager.BlockType;

//
public class StoveBlock : Block
{

  GameObject _heldObject;
  public bool _HasObject { get { return _heldObject != null; } }

  public StoveBlock(GameObject gameObject) : base(BlockType.COUNTER, gameObject)
  {

  }

  //
  public void SetObject(GameObject gameObject)
  {
    _heldObject = gameObject;
    _heldObject.transform.position = _gameObject.transform.position + new Vector3(0f, 2f, 0f);

    //
    if (NetworkManager.Singleton.IsServer)
    {
      var networkObj = _heldObject.GetComponent<NetworkObject>();
      var pickupType = PickupableManager.GetPickupableType(networkObj.gameObject);
      PlayerController.s_LocalPlayer.RequestChangePickupableRpc(networkObj.NetworkObjectId, pickupType == PickupableManager.PickupableType.APPLE ? PickupableManager.PickupableType.BANANA : PickupableManager.PickupableType.APPLE);
    }
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