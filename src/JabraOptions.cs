using Genesyslab.Desktop.Infrastructure;
using Genesyslab.Desktop.Infrastructure.Configuration;

namespace JabraCallControlExtension
{
  public class JabraOptions : Options
  {
    // Options stored in Genesys Configuration Layer (centralized configuration)
    // Define the option names stored in [interaction-workspace] section
    // It can be Workspace Desktop Edition own options or custom options
    // Options can be stored in Options/Annex of the following objects (rule of precedence apply):
    // Tenant, Application (Workspace Desktop Edition), Agent Group, Agent/Person
    private const string HeadsetABC_Option1_Str = "headsetabc.option1";
    private const string HeadsetABC_Option2_Str = "headsetabc.option2";
    private const string HeadsetABC_Option3_Str = "headsetabc.option3";
    private const string GenesysSIPEndpoint_UseHeadsetOption3Str = "sipendpoint.policy.device.use_headset";

    // Privileges (Genesys Roles & Privileges)
    // Define the names of the privileges which can be checked from the module
    // It can be Workspace Desktop Edition existing privileges or custom ones
    // Privileges can be enabled for an Agent/Person or for an entire Access Group
    private const string Privilege_CanUse_Str = "JabraCallControlExtension.canUse";
    private const string Privilege_CanUseGenesysSIPEndpoint_Str = "InteractionWorkspace.SIP.UseSIPEndpoint";

    #region Singleton

    public static JabraOptions Default { get; private set; }

    private JabraOptions(IConfigManager configManager) : base(configManager) { }

    /// <summary>
    /// Creates the singleton instance.
    /// </summary>
    /// <param name="theConfigManager">The config manager.</param>
    public static void CreateInstance(IConfigManager theConfigManager)
    {
      Default = new JabraOptions(theConfigManager);
    }

    #endregion

    #region Access Privileges

    // Check if the current Workspace Desktop Edition user has been assigned the JabraCallControlExtension CanUse privilege
    public bool CanUse()
    {
      // TODO - TO CHANGE - Verification of JabraCallControlExtension privilege disabled (for testing) - Always allow
      return true;

      if (this.Task[Privilege_CanUse_Str])
        return true;
      else
        return false;
    }

    // Check if the current Workspace Desktop Edition user has been assigned the Genesys SIPEndpoint privilege
    // The Agent could also be using a 3rd party SIP Phone (softphone or hardphone running aside, managed via 3rd party call control through Genesys SIP Server)
    public bool IsUsingGenesysSIPEndpoint()
    {
      if (this.Task[Privilege_CanUseGenesysSIPEndpoint_Str])
        return true;
      else
        return false;
    }

    #endregion

    #region Access Options

    // Retrieve Option Value as string array
    // ex: "headsetabc.option1" = "value1, value2, value3"
    public string[] Option1 { get { return configManager.GetValueAsStringArray(HeadsetABC_Option1_Str, null); } }

    // Retrieve Option Value as String
    // ex: "headsetabc.option2" = "my string option"
    public string GetOption2()
    {
      return this.configManager.GetValueAsString(HeadsetABC_Option2_Str, "default value");
    }

    // Retrieve Option Value as boolean
    // ex: "headsetabc.option3" = "true"
    public bool GetOption3()
    {
      return this.configManager.GetValueAsBoolean(HeadsetABC_Option3_Str, false);
    }

    #endregion

  }
}

