
using Unity.Netcode;

public class NetworkTools : NetworkBehaviour
{

  public static NetworkTools s_Singleton;

  void Start()
  {
    s_Singleton = this;
  }

  // Network helper functions
  [Rpc(SendTo.Everyone)]
  public void RequestSpawnPickupableRpc(ulong networkId, PickupableManager.PickupableType asType)
  {
    PickupableManager.RequestSpawnPickupableRpc(networkId, asType);
  }

}