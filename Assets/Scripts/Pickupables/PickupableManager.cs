using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class PickupableManager
{

  public static PickupableManager s_Singleton;

  //
  List<GameObject> _pickups;

  //
  public PickupableManager()
  {
    s_Singleton = this;

    _pickups = new List<GameObject>();
  }

  //
  public void Update()
  {

  }

  //
  public enum PickupableType
  {
    NONE,

    APPLE,
    BANANA,
  }
  public static GameObject SpawnPickupable(Vector3 atPosition, PickupableType ofType)
  {
    var prefab = HelloWorldManager.s_Singleton._NetworkPrefabs.PrefabList[1].Prefab;
    var instance = GameObject.Instantiate(prefab, atPosition, Quaternion.identity);

    var networkObject = instance.GetComponent<NetworkObject>();
    networkObject.Spawn();

    PlayerController.s_LocalPlayer.RequestSpawnPickupableRpc(networkObject.NetworkObjectId, ofType);

    return instance;
  }

  public static void RequestSpawnPickupableRpc(ulong networkId, PickupableType asType)
  {

    var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkId];

    var model = GameObject.Instantiate(Resources.Load($"Pickupables/{asType}"), GameController.s_Game) as GameObject;
    model.transform.parent = obj.transform;
    model.transform.localPosition = Vector3.zero;
    model.transform.rotation = Quaternion.identity;

    model.name = asType.ToString();
    model.transform.parent.name = "Pickupable";

    s_Singleton._pickups.Add(obj.gameObject);
  }
}