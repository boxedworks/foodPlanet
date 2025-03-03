using UnityEngine;

public class AnimationListener : MonoBehaviour
{

  PlayerController _controller;

  void Start()
  {

  }

  public void RegisterController(PlayerController controller)
  {
    _controller = controller;
  }

  void OnAnimationEvent(AnimationEvent animationEvent)
  {
    _controller?.OnAnimationEvent(animationEvent);
  }

}