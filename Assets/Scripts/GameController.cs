
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
    new EffectManager();
  }

  void Update()
  {
    PickupableManager.s_Singleton?.Update();
    AudioManager.Update();
    SimpleTrapManager.Update();
    BlockManager.Update();
  }
  void FixedUpdate()
  {
    SimpleTrapManager.FixedUpdate();
  }

  //
  public static void OnClientConnected()
  {
    new PickupableManager();
  }

}