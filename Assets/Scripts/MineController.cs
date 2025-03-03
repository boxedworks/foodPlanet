using UnityEngine;

public class MineController : MonoBehaviour
{

  bool _activated;

  //
  public void Trigger()
  {
    if (_activated) return;
    _activated = true;

    GameObject.Destroy(gameObject);
  }
}
