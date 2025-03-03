using UnityEngine;
using System.Collections.Generic;

public class AudioManager
{

  public static AudioManager s_Singleton;
  AudioScriptableObjectCollection _audiosDatas { get { return GameController.s_Singleton._AudioDatas; } }

  //
  class AudioData
  {
    public AudioSource _AudioSource;
    public AudioScriptableObject _AudioScriptableObject;
  }
  List<AudioData> _audio;
  Queue<AudioSource> _audioAvailable;

  Dictionary<string, AudioScriptableObject> _audioLibrary;

  //
  public AudioManager()
  {
    s_Singleton = this;

    _audio = new();

    // Create audio queue
    _audioAvailable = new();

    // Load audio library
    _audioLibrary = new();
    foreach (var audioData in _audiosDatas._AudioScriptableObjects)
      _audioLibrary.Add(audioData.name.ToLower(), audioData);
  }

  public static void Update()
  {

    // Check audio sources for done playing
    var audio = s_Singleton._audio;
    var audioAvailable = s_Singleton._audioAvailable;
    for (var i = audio.Count - 1; i >= 0; i--)
    {

      var audioData = audio[i];
      var audioSource = audioData._AudioSource;
      if (!audioSource.isPlaying)
      {

        audio.RemoveAt(i);
        audioAvailable.Enqueue(audioSource);

      }

    }

  }

  //
  public static AudioSource PlayAudio(string audioPath, Vector3 atPos)
  {

    //

    // Grab from audio queue
    AudioSource audioSource;
    if (s_Singleton._audioAvailable.Count > 0)
      audioSource = s_Singleton._audioAvailable.Dequeue();
    else
      audioSource = new GameObject("Audio").AddComponent<AudioSource>();
    audioSource.transform.position = atPos;

    // Grab audio data
    var audioData = s_Singleton._audioLibrary[audioPath];

    // Set audio source values
    audioSource.clip = audioData._AudioClip;
    audioSource.volume = audioData._Volume;
    audioSource.pitch = audioData._PitchMin == audioData._PitchMax ? audioData._PitchMin : Random.Range(audioData._PitchMin, audioData._PitchMax);
    audioSource.priority = audioData._Priority;

    audioSource.spatialBlend = 1f;
    audioSource.minDistance = audioData._DistanceMin;
    audioSource.maxDistance = audioData._DistanceMax;

    // Play
    audioSource.Play();
    s_Singleton._audio.Add(new AudioData()
    {
      _AudioSource = audioSource,
      _AudioScriptableObject = audioData
    });
    return audioSource;
  }

  // Change volume of all playing audio sources
  public static void ChangeVolume(float volume)
  {

  }

}