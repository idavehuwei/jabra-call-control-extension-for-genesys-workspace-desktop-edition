using System;
using System.Collections.Generic;
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
    }

    #region API Notifications Placeholder

    private void SendNotificationToHeadsetABC(IInteractionVoice ixnVoice, string status)
    {
      log.Debug("Notify HeadsetABC of call in " + status);
      // TODO
      MessageBox.Show("Notify HeadsetABC of call in " + status);
    }

    private void SendNotificationToHeadsetABC(SIPEndpoint sipEndpoint, bool isMicrophoneMuted)
    {
      log.Debug("Notify HeadsetABC of Microphone " + (isMicrophoneMuted ? "Mute" : "Unmute"));
      // TODO
      MessageBox.Show("Notify HeadsetABC of Microphone " + (isMicrophoneMuted ? "Mute" : "Unmute"));
    }

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
      interactionManager.InteractionEvent += new System.EventHandler<EventArgs<IInteraction>>(HeadsetABCModule_InteractionEvent);
    }

    public void UnregisterInteractionEventHandler()
    {
      interactionManager.InteractionEvent -= new System.EventHandler<EventArgs<IInteraction>>(HeadsetABCModule_InteractionEvent);
    }

    private void HeadsetABCModule_InteractionEvent(object sender, EventArgs<IInteraction> e)
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
            SendNotificationToHeadsetABC(iv, "Ringing");
          }
        }
        else if (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Connected)
        {
          if (receivedEvent.Id == EventEstablished.MessageId)
          {
            SendNotificationToHeadsetABC(iv, "Established");
          }
          else if (receivedEvent.Id == EventRetrieved.MessageId)
          {
            SendNotificationToHeadsetABC(iv, "Retrieved");
          }
        }
        else if (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Held)
        {
          if (receivedEvent.Id == EventHeld.MessageId)
          {
            SendNotificationToHeadsetABC(iv, "Held");
          }
        }
        else if ((iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Ended) ||
                 (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Abandonned) ||
                 (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Dropped) ||
                 (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.Redirected))
        {
          // NB: Sent two times....
          SendNotificationToHeadsetABC(iv, "Ended");
        }
        else if (iv.State == Genesyslab.Enterprise.Model.Interaction.InteractionStateType.PresentedOut)
        {
          if (receivedEvent.Id == EventDialing.MessageId)
          {
            RegisterSIPEPEventHandlers(iv);
            SendNotificationToHeadsetABC(iv, "Dialing");
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
        SendNotificationToHeadsetABC(sipEndpoint, microphoneMuted);
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