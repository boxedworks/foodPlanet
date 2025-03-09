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

  public static PickupableType GetPickupableType(GameObject pickupable)
  {
    return System.Enum.Parse<PickupableType>(pickupable.transform.GetChild(0).gameObject.name, true);
  }

  static void SetPickupableModel(GameObject networkObject, PickupableType ofType)
  {
    var model = GameObject.Instantiate(Resources.Load($"Pickupables/{ofType}"), GameController.s_Game) as GameObject;
    model.transform.parent = networkObject.transform;
    model.transform.localPosition = Vector3.zero;
    model.transform.rotation = Quaternion.identity;

    model.name = ofType.ToString();
  }

  public static void RequestSpawnPickupableRpc(ulong networkId, PickupableType asType)
  {

    var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkId];
    obj.gameObject.name = "Pickupable";

    SetPickupableModel(obj.gameObject, asType);

    s_Singleton._pickups.Add(obj.gameObject);
  }

  public static void RequestChangePickupableToRpc(ulong networkId, PickupableType toType)
  {

    var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkId];

    GameObject.Destroy(obj.transform.GetChild(0).gameObject);

    SetPickupableModel(obj.gameObject, toType);
  }
}