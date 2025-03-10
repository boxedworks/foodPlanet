
using System.Collections.Generic;
using UnityEngine;

public class EffectManager
{

  public static EffectManager s_Singleton;

  //
  public class EffectPair
  {

    //
    public ParticleSystem _Particles;
    public string _AudioPath;

    public AudioSource _AudioSource;

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
        _AudioSource = AudioManager.PlayAudio(_AudioPath, atPosition);
      else
        _AudioSource = null;
    }
  }

  //
  public enum EffectType
  {
    NONE,

    BLOOD_SPURT,

    GRILL_SIZZLE,

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

    _effects.Add(EffectType.GRILL_SIZZLE, new()
    {
      //_Particles = GameObject.Find("particles_blood").GetComponent<ParticleSystem>(),
      _AudioPath = "sizzle"
    });
  }

  public static EffectPair PlayEffectAt(EffectType effectType, Vector3 atPosition)
  {
    var effectPair = s_Singleton._effects[effectType];
    effectPair.Play(atPosition);
    return effectPair;
  }


}