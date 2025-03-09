
using System.Collections.Generic;
using UnityEngine;

public class EffectManager
{

  public static EffectManager s_Singleton;

  //
  class EffectPair
  {

    //
    public ParticleSystem _Particles;
    public string _AudioPath;

    //
    public void Play(Vector3 atPosition)
    {

      //
      if (_Particles != null)
      {
        _Particles.transform.position = atPosition;
        _Particles.Play();
      }

      //
      if (_AudioPath != null)
        AudioManager.PlayAudio(_AudioPath, atPosition);
    }
  }

  //
  public enum EffectType
  {
    NONE,

    BLOOD_SPURT,

  }
  Dictionary<EffectType, EffectPair> _effects;

  //
  public EffectManager()
  {
    s_Singleton = this;

    _effects = new();
    _effects.Add(EffectType.BLOOD_SPURT, new()
    {
      _Particles = GameObject.Find("particles_blood").GetComponent<ParticleSystem>(),
      _AudioPath = "blood"
    });
  }

  public static void PlayEffectAt(EffectType effectType, Vector3 atPosition)
  {
    s_Singleton._effects[effectType].Play(atPosition);
  }


}