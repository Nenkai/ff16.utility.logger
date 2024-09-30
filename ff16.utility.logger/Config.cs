using ff16.utility.logger.Template.Configuration;

using Reloaded.Mod.Interfaces.Structs;

using System.ComponentModel;

namespace ff16.utility.logger.Configuration
{
    public class Config : Configurable<Config>
    {
        [DisplayName("Log Path Hashes")]
        [Description("Whether to log path hashes")]
        [DefaultValue(true)]
        public bool LogPathHashes { get; set; } = true;

        [DisplayName("Enable Nex Logging")]
        [Description("Whether to enable nex access logging.")]
        [DefaultValue(true)]
        public bool EnableNexLogging { get; set; } = true;

        [DisplayName("Nex Tables to Exclude")]
        [Description("List of nex tables to exclude from all printing, separated by a comma (,) .\n" +
            "Example: window,cameratransition,cutsceneconnectquestseqarg")]
        public string NexTablesToExclude { get; set; } = "";

        [DisplayName("Nex Tables to Include")]
        [Description("List of nex tables that will ONLY be printed, separated by a comma (,) .")]
        public string NexTablesToInclude { get; set; } = "";

    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}
