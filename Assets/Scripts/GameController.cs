
using UnityEngine;

class GameController : MonoBehaviour
{

  //
  public static GameController s_Singleton;
  public static Transform s_Game { get { return s_Singleton.transform; } }

  //
  public AudioScriptableObjectCollection _AudioDatas;

  //
  void Start()
  {
    s_Singleton = this;

    new BlockManager();
    new AudioManager();
    new SimpleTrapManager();
  }

  void Update()
  {
    PickupableManager.s_Singleton?.Update();
    AudioManager.Update();
  }

  //
  public static void OnClientConnected()
  {
    new PickupableManager();
  }

}