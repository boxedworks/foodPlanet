using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using System.Threading.Tasks;

public class VivoxController
{

  public string _ChannelName = "Lobby";
  bool _inChannelMain, _inChannelDeath;

  public bool _InChannelMain { get { return _inChannelMain; } }
  public bool _InChannelDeath { get { return _inChannelDeath; } }

  public async Task InitializeAsync()
  {
    //if (UnityServices.State == ServicesInitializationState.Uninitialized)
    await UnityServices.InitializeAsync();
    //if (AuthenticationService.Instance == null || !AuthenticationService.Instance.IsAuthorized)
    await AuthenticationService.Instance.SignInAnonymouslyAsync();

    await VivoxService.Instance.InitializeAsync();
  }

  public async Task LoginToVivoxAsync()
  {
    if (!VivoxService.Instance.IsLoggedIn)
    {
      var options = new LoginOptions();
      options.DisplayName = "TEST_USER";
      //options.EnableTTS = true;
      await VivoxService.Instance.LoginAsync(options);
    }
  }

  public async Task JoinChannelAsync()
  {
    if (_inChannelDeath)
      await LeaveChannelDeathAsync();

    var channelToJoin = _ChannelName;
    var channelProperties = new Channel3DProperties(32, 2, 0.5f, AudioFadeModel.LinearByDistance);
    await VivoxService.Instance.JoinPositionalChannelAsync(channelToJoin, ChatCapability.TextAndAudio, channelProperties);
    Debug.Log($"Joined channel: {channelToJoin}");

    _inChannelMain = true;
  }

  public async Task LeaveChannelAsync()
  {
    _inChannelMain = false;

    var channelToLeave = _ChannelName;
    await VivoxService.Instance.LeaveChannelAsync(channelToLeave);
    Debug.Log($"Left channel: {channelToLeave}");
  }

  public async Task JoinChannelDeathAsync()
  {
    if (_inChannelMain)
      await LeaveChannelAsync();

    var channelToJoin = $"{_ChannelName}_death";
    await VivoxService.Instance.JoinGroupChannelAsync(channelToJoin, ChatCapability.TextAndAudio);
    Debug.Log($"Joined channel: {channelToJoin}");

    _inChannelDeath = true;
  }

  public async Task LeaveChannelDeathAsync()
  {
    _inChannelDeath = false;

    var channelToLeave = $"{_ChannelName}_death";
    await VivoxService.Instance.LeaveChannelAsync(channelToLeave);
    Debug.Log($"Left channel: {channelToLeave}");
  }

  public async Task LogoutOfVivoxAsync()
  {
    await VivoxService.Instance.LogoutAsync();
  }
}
