using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Genesyslab.Desktop.Infrastructure;
using Genesyslab.Desktop.Infrastructure.Commands;
using Genesyslab.Desktop.Infrastructure.Events;
using Genesyslab.Desktop.Modules.Core.Model.Agents;
using Genesyslab.Desktop.Modules.Core.Model.Interactions;
using Genesyslab.Desktop.Modules.SIPEndpointCommunication;
using Genesyslab.Desktop.Modules.Voice.Model.Interactions;
using Genesyslab.Platform.Commons.Logging;
using Genesyslab.Platform.Commons.Protocols;
using Genesyslab.Platform.Voice.Protocols.TServer.Events;
using JabraHidTelephonyApi;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;

namespace JabraCallControlExtension
{
  public class JabraUtils
  {
    #region Private Members

    private readonly IUnityContainer container;
    private readonly ICommandManager commandManager;
    private readonly IInteractionManager interactionManager;
    private readonly IAgent agent;
    private readonly ILogger log;

    Dictionary<SIPEndpoint, SIPEndpoint> startedSipEndpoints = new Dictionary<SIPEndpoint, SIPEndpoint>();

    private IInteractionVoice incomingCall = null;
    private IInteractionVoice activeCall = null;
    private IInteractionVoice heldCall = null;

    #endregion

    #region Singleton

    public static JabraUtils Default { get; private set; }

    /// <summary>
    /// Creates the singleton instance.
    /// </summary>
    /// <param name="theConfigManager">The config manager.</param>
    public static void CreateInstance(IUnityContainer container,
      ICommandManager commandManager,
      IInteractionManager interactionManager,
      IAgent agent,
      ILogger log)
    {
      Default = new JabraUtils(container, commandManager, interactionManager, agent, log);
    }

    #endregion

    public JabraUtils(IUnityContainer container,
      ICommandManager commandManager,
      IInteractionManager interactionManager,
      IAgent agent,
      ILogger log)
    {
      this.container = container;
      this.commandManager = commandManager;
      // ICommandManager could also be retrieved/resolved via the container
      // this.commandManager = container.Resolve<ICommandManager>();
      this.interactionManager = interactionManager;
      // IInteractionManager could also be retrieved/resolved via the container
      // this.interactionManager = container.Resolve<IInteractionManager>();
      this.agent = agent;
      // IAgent could also be retrieved/resolved via the container
      // this.agent = container.Resolve<IAgent>();

      this.log = log;

      InitJabraLibrary();
    }

    #region Jabra device management

    private object jabraLock = new object();
    private IDeviceService jabraDeviceService;
    private List<IHidTelephonyDevice> jabraDevices = new List<IHidTelephonyDevice>();

    private void InitJabraLibrary()
    {
      // Get a device service interface
      jabraDeviceService = ServiceFactory.CreateDeviceService();

      // Get a list of currently available Jabra devices
      ReadOnlyCollection<IHidTelephonyDevice> devices = jabraDeviceService.AvailableDevices;
      foreach (var device in devices)
      {
        AddJabraDevice(device);
      }

      // Subscribe to device added/removed events
      jabraDeviceService.DeviceAdded += OnJabraDeviceAdded;
      jabraDeviceService.DeviceRemoved += OnJabraDeviceRemoved;
    }

    private void OnJabraDeviceAdded(object sender, DeviceAddedEventArgs deviceAddedEventArgs)
    {
      AddJabraDevice(deviceAddedEventArgs.Device);
    }

    private void OnJabraDeviceRemoved(object sender, DeviceRemovedEventArgs deviceRemovedEventArgs)
    {
      RemoveJabraDevice(deviceRemovedEventArgs.Device);
    }

    private void AddJabraDevice(IHidTelephonyDevice device)
    {
      lock (jabraLock)
      {
        device.ButtonInput += OnButtonInput;
        jabraDevices.Add(device);
      }
    }

    private void RemoveJabraDevice(IHidTelephonyDevice device)
    {
      lock (jabraLock)
      {
        device.ButtonInput -= OnButtonInput;
        jabraDevices.Remove(device);
      }
    }

    private void OnButtonInput(object sender, ButtonInputEventArgs e)
    {
      // TODO: Handle Jabra device button input here
    }

    private void SetHookState(bool offHook)
    {
      lock (jabraLock)
      {
        foreach (var jabraDevice in jabraDevices)
        {
          if (!jabraDevice.IsLocked)
          {
            jabraDevice.Lock();
          }
          if (offHook)
          {
            if (!jabraDevice.IsOffHook)
            {
              jabraDevice.SetHookState(true);
            }
          }
          else
          {
            if (jabraDevice.IsOffHook)
            {
              jabraDevice.SetHookState(false);
            }
            jabraDevice.Unlock();
          }
        }
      }
    }

    private void SetMicrophoneMuted(bool muted)
    {
      lock (jabraLock)
      {
        foreach (var jabraDevice in jabraDevices)
        {
          if (!jabraDevice.IsLocked)
          {
            jabraDevice.Lock();
          }
          if (muted)
          {
            if (!jabraDevice.IsMicrophoneMuted)
            {
              jabraDevice.SetMicrophoneMuted(true);
            }
          }
          else
          {
            if (jabraDevice.IsMicrophoneMuted)
            {
              jabraDevice.SetMicrophoneMuted(false);
            }
          }
        }
      }
    }

    private void SetRinger(bool ringing)
    {
      lock (jabraLock)
      {
        foreach (var jabraDevice in jabraDevices)
        {
          if (!jabraDevice.IsLocked)
          {
            jabraDevice.Lock();
          }
          if (ringing)
          {
            if (!jabraDevice.IsRinging)
            {
              jabraDevice.SetRinger(true);
            }
          }
          else
          {
            if (jabraDevice.IsRinging)
            {
              jabraDevice.SetRinger(false);
            }
          }
        }
      }
    }

    private void SetCallOnHold(bool onHold)
    {
      lock (jabraLock)
      {
        foreach (var jabraDevice in jabraDevices)
        {
          if (!jabraDevice.IsLocked)
          {
            jabraDevice.Lock();
          }
          if (onHold)
          {
            if (!jabraDevice.IsOnHold)
            {
              jabraDevice.SetCallOnHold(true);
            }
          }
          else
          {
            if (jabraDevice.IsOnHold)
            {
              jabraDevice.SetCallOnHold(false);
            }
          }
        }
      }
    }

    #endregion

    #region API Notifications Placeholder

    /*
    private void SendNotificationToDevice(IInteractionVoice ixnVoice, string status)
    {
      log.Debug("Notify device of call in " + status);
      // TODO
//      MessageBox.Show("Notify device of call in " + status);
    }

    private void SendNotificationToDevice(SIPEndpoint sipEndpoint, bool isMicrophoneMuted)
    {
      log.Debug("Notify device of microphone " + (isMicrophoneMuted ? "mute" : "unmute"));
      // TODO
//      MessageBox.Show("Notify device of microphone " + (isMicrophoneMuted ? "mute" : "unmute"));
    }
    */

    #endregion

    #region API Requests Placeholder

    public IInteractionVoice FindVoiceInteraction(string token)
    {
      IInteractionVoice interactionVoice = null;

      foreach (var interaction in interactionManager.Interactions)
      {
        IInteractionVoice iv = interaction as IInteractionVoice;
        if (iv != null && iv.Media.IsSIPMedia)
        {
          // TODO
          // Decide how to leverage token to recognize interaction (as there can be multiple active calls)
          // token can leverage Interaction.Id, ConnectionId, ANI, ..., or a key/value pair in UserData
          interactionVoice = iv;
          break;
        }
      }
      return interactionVoice;

    }

    delegate void RequestAnswerCallDelegate(IInteractionVoice ixnVoice);

    public void RequestAnswerCall(IInteractionVoice ixnVoice)
    {
      // To go to the main thread
      if (Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
      {
        object result = Application.Current.Dispatcher.Invoke(DispatcherPriority.Send,
          new RequestAnswerCallDelegate(RequestAnswerCall), ixnVoice);
      }
      else
      {
        log.Info("Execute RequestAnswerCall");
        TriggerVoiceChainOfCommands(ixnVoice, "InteractionVoiceAnswerCall");
      }
    }

    public void RequestAnswerCall(string token)
    {
      IInteractionVoice ixnVoice = this.FindVoiceInteraction(token);
      this.RequestAnswerCall(ixnVoice);
    }

    delegate void RequestReleaseCallDelegate(IInteractionVoice ixnVoice);

    public void RequestReleaseCall(IInteractionVoice ixnVoice)
    {
      // To go to the main thread
      if (Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
      {
        object result = Application.Current.Dispatcher.Invoke(DispatcherPriority.Send,
          new RequestReleaseCallDelegate(RequestReleaseCall), ixnVoice);
      }
      else
      {
        log.Info("Execute RequestReleaseCall");
        TriggerVoiceChainOfCommands(ixnVoice, "InteractionVoiceReleaseCall");
      }
    }

    public void RequestReleaseCall(string token)
    {
      IInteractionVoice ixnVoice = this.FindVoiceInteraction(token);
      this.RequestReleaseCall(ixnVoice);
    }

    delegate void RequestHoldCallDelegate(IInteractionVoice ixnVoice);

    public void RequestHoldCall(IInteractionVoice ixnVoice)
    {
      // To go to the main thread
      if (Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
      {
        object result = Application.Current.Dispatcher.Invoke(DispatcherPriority.Send,
          new RequestHoldCallDelegate(RequestHoldCall), ixnVoice);
      }
      else
      {
        log.Info("Execute RequestHoldCall");
        TriggerVoiceChainOfCommands(ixnVoice, "InteractionVoiceHoldCall");
      }
    }

    public void RequestHoldCall(string token)
    {
      IInteractionVoice ixnVoice = this.FindVoiceInteraction(token);
      this.RequestHoldCall(ixnVoice);
    }

    delegate void RequestRetrieveCallDelegate(IInteractionVoice ixnVoice);

    public void RequestRetrieveCall(IInteractionVoice ixnVoice)
    {
      // To go to the main thread
      if (Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
      {
        object result = Application.Current.Dispatcher.Invoke(DispatcherPriority.Send,
          new RequestRetrieveCallDelegate(RequestRetrieveCall), ixnVoice);
      }
      else
      {
        log.Info("Execute RequestRetrieveCall");
        TriggerVoiceChainOfCommands(ixnVoice, "InteractionVoiceRetrieveCall");
      }
    }

    public void RequestRetrieveCall(string token)
    {
      IInteractionVoice ixnVoice = this.FindVoiceInteraction(token);
      this.RequestRetrieveCall(ixnVoice);
    }

    #endregion

    #region Headset Voice Chains of Commands

    private void TriggerVoiceChainOfCommands(IInteractionVoice ixnVoice, String commandName)
    {
      try
      {
        IDictionary<string, object> parameters = new Dictionary<string, object>();

        parameters.Add("CommandParameter", ixnVoice);
        // parameters.Add("OtherKey", "OtherValue");

        IChainOfCommand voiceCallCommand = commandManager.GetChainOfCommandByName(commandName);

        if (voiceCallCommand != null)
        {
          voiceCallCommand.Execute(parameters);

          // Async way
          /*
          voiceCallCommand.BeginExecute(parameters,
                  delegate(IAsyncResult ar)
                  {
                      voiceCallCommand.EndExecute(ar);
                  }, null);
           */
        }
      }
      catch (Exception exp)
      {
        log.Error("Exception in TriggerVoiceChainOfCommands", exp);
      }

    }

    #endregion

    #region Headset MakeCall Chain of Commands

    delegate void RequestMakeCallDelegate(String destination, String location);

    public void RequestMakeCall(String destination, String location)
    {
      // To go to the main thread
      if (Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
      {
        object result = Application.Current.Dispatcher.Invoke(DispatcherPriority.Send,
          new RequestMakeCallDelegate(RequestMakeCall), destination, location);
      }
      else
      {
        log.Info("Execute RequestMakeCall");
        try
        {
          IDictionary<string, object> parameters = new Dictionary<string, object>();

          parameters.Add("CommandParameter", agent.FirstMediaVoice);
          parameters.Add("Destination", destination);
          if (location == null)
            location = "";
          parameters.Add("Location", location);
          parameters.Add("MakeCallType", Genesyslab.Enterprise.Model.Interaction.MakeCallType.Regular);

          IChainOfCommand voiceCallCommand = commandManager.GetChainOfCommandByName("MediaVoiceMakeCall");

          if (voiceCallCommand != null)
          {
            voiceCallCommand.Execute(parameters);

            // Async way
            /*
            voiceCallCommand.BeginExecute(parameters,
                    delegate(IAsyncResult ar)
                    {
                        voiceCallCommand.EndExecute(ar);
                    }, null);
             */
          }
        }
        catch (Exception exp)
        {
          log.Error("Exception in RequestMakeCall", exp);
        }
      }
    }

    #endregion

    #region Notifications

    public void RegisterInteractionEventHandler()
    {
      interactionManager.InteractionEvent += InteractionEvent;
    }

    public void UnregisterInteractionEventHandler()
    {
      interactionManager.InteractionEvent -= InteractionEvent;
    }

    private void InteractionEvent(object sender, EventArgs<IInteraction> e)
    {
      IInteraction interaction = e.Value;
      IInteractionVoice iv = interaction as IInteractionVoice;
      if (iv != null && iv.Media.IsSIPMedia)
      {
        IMessage receivedEvent = iv.EntrepriseLastInteractionEvent;

        if (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.PresentedIn)
        {
          if (receivedEvent.Id == EventRinging.MessageId)
          {
            RegisterSIPEPEventHandlers(iv);
//            SendNotificationToDevice(iv, "Ringing");
            SetRinger(true);
            activeCall = heldCall = null;
            incomingCall = iv;
          }
        }
        else if (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Connected)
        {
          if (receivedEvent.Id == EventEstablished.MessageId)
          {
//            SendNotificationToDevice(iv, "Established");
            SetRinger(false);
            SetHookState(true);
            SetCallOnHold(false);
            incomingCall = heldCall = null;
            activeCall = iv;
          }
          else if (receivedEvent.Id == EventRetrieved.MessageId)
          {
//            SendNotificationToDevice(iv, "Retrieved");
            SetRinger(false);
            SetHookState(true);
            SetCallOnHold(false);
            incomingCall = heldCall = null;
            activeCall = iv;
          }
        }
        else if (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Held)
        {
          if (receivedEvent.Id == EventHeld.MessageId)
          {
//            SendNotificationToDevice(iv, "Held");
            SetCallOnHold(true);
            SetHookState(false);
            SetRinger(false);
            incomingCall = activeCall = null;
            heldCall = iv;
          }
        }
        else if ((iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Ended) ||
                 (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Abandonned) ||
                 (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Dropped) ||
                 (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Redirected))
        {
          // NB: Sent two times....
//          SendNotificationToDevice(iv, "Ended");
          SetRinger(false);
          SetHookState(false);
          SetCallOnHold(false);
          activeCall = incomingCall = heldCall = null;
        }
        else if (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.PresentedOut)
        {
          if (receivedEvent.Id == EventDialing.MessageId)
          {
            RegisterSIPEPEventHandlers(iv);
//            SendNotificationToDevice(iv, "Dialing");
          }
          else if (receivedEvent.Id == EventNetworkReached.MessageId)
          {

          }
        }
        else if (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Busy)
        {

        }
        else if (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.InvalidDestination)
        {

        }
      }
    }

    #endregion

    #region Specific to mute/unmute

    delegate void RequestMuteCallViaSIPEndpointDelegate(IInteractionVoice ixnVoice);

    public void RequestMuteCallViaSIPEndpoint(IInteractionVoice ixnVoice)
    {
      // To go to the main thread
      if (Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
      {
        object result = Application.Current.Dispatcher.Invoke(DispatcherPriority.Send,
          new RequestMuteCallViaSIPEndpointDelegate(RequestMuteCallViaSIPEndpoint), ixnVoice);
      }
      else
      {
        log.Info("Execute RequestMuteCallViaSIPEndpoint");
        if (JabraOptions.Default.IsUsingGenesysSIPEndpoint())
        {
          try
          {
            if (ixnVoice != null)
            {
              SIPEndpoint sipEndpoint = container.Resolve<ISIPEndpointCommunication>().FindSIPEndpoint(ixnVoice);
              if (sipEndpoint != null)
              {
                sipEndpoint.IsMicrophoneMuted = true;
              }
            }
          }
          catch (Exception exp)
          {
            log.Error("Exception in RequestMuteCallViaSIPEndpoint", exp);
          }
        }
      }

    }

    public void RequestMuteCallViaSIPEndpoint(string token)
    {
      IInteractionVoice interactionVoice = this.FindVoiceInteraction(token);

      if (interactionVoice != null)
      {
        RequestMuteCallViaSIPEndpoint(interactionVoice);
      }
    }

    delegate void RequestUnmuteCallViaSIPEndpointDelegate(IInteractionVoice ixnVoice);

    public void RequestUnmuteCallViaSIPEndpoint(IInteractionVoice ixnVoice)
    {
      // To go to the main thread
      if (Application.Current.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
      {
        object result = Application.Current.Dispatcher.Invoke(DispatcherPriority.Send,
          new RequestUnmuteCallViaSIPEndpointDelegate(RequestUnmuteCallViaSIPEndpoint), ixnVoice);
      }
      else
      {
        log.Info("Execute RequestUnmuteCallViaSIPEndpoint");
        if (JabraOptions.Default.IsUsingGenesysSIPEndpoint())
        {
          try
          {
            if (ixnVoice != null)
            {
              SIPEndpoint sipEndpoint = container.Resolve<ISIPEndpointCommunication>().FindSIPEndpoint(ixnVoice);
              if (sipEndpoint != null)
              {
                sipEndpoint.IsMicrophoneMuted = false;
              }
            }
          }
          catch (Exception exp)
          {
            log.Error("Exception in RequestMuteCallViaSIPEndpoint", exp);
          }
        }
      }
    }

    public void RequestUnmuteCallViaSIPEndpoint(string token)
    {
      IInteractionVoice interactionVoice = this.FindVoiceInteraction(token);

      if (interactionVoice != null)
      {
        RequestUnmuteCallViaSIPEndpoint(interactionVoice);
      }
    }

    private void RegisterSIPEPEventHandlers(IInteractionVoice interactionVoice)
    {
      if (JabraOptions.Default.IsUsingGenesysSIPEndpoint())
      {
        try
        {
          SIPEndpoint sipEndpoint = container.Resolve<ISIPEndpointCommunication>().FindSIPEndpoint(interactionVoice);

          if (sipEndpoint != null)
          {
            // this.sipEP.EndpointStatusChanged += new System.EventHandler<EventArgs<bool>>(SIPEP_StatusChanged);
            if (!startedSipEndpoints.ContainsKey(sipEndpoint))
            {
              startedSipEndpoints[sipEndpoint] = sipEndpoint;

              sipEndpoint.PropertyChanged += new PropertyChangedEventHandler(SIPEP_PropertyChanged);
              sipEndpoint.EndpointStatusChanged += new System.EventHandler<EventArgs<bool>>(SIPEP_StatusChanged);
            }
          }
        }
        catch (Exception exp)
        {
          log.Error("Exception in RegisterSIPEPEventHandlers", exp);
        }
      }
    }

    private void UnregisterSIPEPEventHandlers(IInteractionVoice interactionVoice)
    {
      if (JabraOptions.Default.IsUsingGenesysSIPEndpoint())
      {
        try
        {
          SIPEndpoint sipEndpoint = container.Resolve<ISIPEndpointCommunication>().FindSIPEndpoint(interactionVoice);

          if (sipEndpoint != null)
          {
            // this.sipEP.EndpointStatusChanged += new System.EventHandler<EventArgs<bool>>(SIPEP_StatusChanged);
            if (startedSipEndpoints.ContainsKey(sipEndpoint))
            {
              sipEndpoint.PropertyChanged -= new PropertyChangedEventHandler(SIPEP_PropertyChanged);
              sipEndpoint.EndpointStatusChanged -= new System.EventHandler<EventArgs<bool>>(SIPEP_StatusChanged);

              startedSipEndpoints.Remove(sipEndpoint);
            }
          }
        }
        catch (Exception exp)
        {
          log.Error("Exception in UnregisterSIPEPEventHandlers", exp);
        }
      }
    }

    private void SIPEP_StatusChanged(object sender, EventArgs<bool> e)
    {
      log.Debug("SIPEP StatusChangedEvent received");
    }

    private void SIPEP_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "IsMicrophoneMuted")
      {
        SIPEndpoint sipEndpoint = sender as SIPEndpoint;
        bool microphoneMuted = sipEndpoint.IsMicrophoneMuted;
//        SendNotificationToDevice(sipEndpoint, microphoneMuted);

        SetMicrophoneMuted(microphoneMuted);
      }
      // Should also work with IsSpeakerMuted, MicrophoneVolume, SpeakerVolume
    }

    #endregion

    #region Generate Alert/System Message

    public void NotifyUserWithAlert(string alertId, object[] alertParams)
    {
      container.Resolve<IEventAggregator>().GetEvent<AlertEvent>().Publish(new Alert()
      {
        Section = "Login",
        Severity = SeverityType.Information,
        Id = alertId,
        Target = "Text",
        Parameters = alertParams
      });
    }

    #endregion

  }
}