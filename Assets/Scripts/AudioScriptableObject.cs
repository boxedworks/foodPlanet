using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AudioScriptableObject", order = 1)]
public class AudioScriptableObject : ScriptableObject
{
  public AudioClip _AudioClip;
  public int _Priority;

  public float _Volume = 1f, _PitchMin = 1f, _PitchMax = 1f, _DistanceMin = 5f, _DistanceMax = 100f;
}