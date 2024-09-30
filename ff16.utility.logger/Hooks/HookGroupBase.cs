using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using ff16.utility.logger.Configuration;
using Reloaded.Hooks.Definitions;
using Reloaded.Mod.Interfaces;

namespace ff16.utility.logger.Hooks;

public abstract class HookGroupBase
{
    protected Config _configuration;
    protected IReloadedHooks _hooks;
    protected ILogger _logger;

    public HookGroupBase(Config config, IReloadedHooks hooks, ILogger logger)
    {
        _configuration = config;
        _hooks = hooks;
        _logger = logger;
    }

    public abstract void Setup(IStartupScanner scanner);

    public virtual void UpdateConfig(Config configuration)
    {
        _configuration = configuration;
    }
}
