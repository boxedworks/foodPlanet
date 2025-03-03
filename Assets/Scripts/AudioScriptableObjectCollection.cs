using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AudioScriptableObjectCollection", order = 1)]
public class AudioScriptableObjectCollection : ScriptableObject
{
  public AudioScriptableObject[] _AudioScriptableObjects;
}