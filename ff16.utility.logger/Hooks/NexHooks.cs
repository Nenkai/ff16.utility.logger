using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Hooks.Definitions;

using ff16.utility.logger.Configuration;

namespace ff16.utility.logger.Hooks;

public unsafe class NexHooks : HookGroupBase
{
    private Dictionary<nint, TableType> _globalToTableId = new();

    private HashSet<TableType> _tablesToIgnore = new();
    private HashSet<TableType> _tablesToInclude = new();

    public delegate void* NexGetTableDelegate(void* a1, TableType tableId);
    private IHook<NexGetTableDelegate> _nexGetTableHook;

    public delegate void* NexSearchRow1KDelegate(void* a1, int rowId);
    private IHook<NexSearchRow1KDelegate> _nexSearchRow1KHook;

    public delegate void* NexSearchRow2KDelegate(void* a1, int key1, int key2);
    private IHook<NexSearchRow2KDelegate> _nexSearchRow2KHook;

    public delegate void* NexSearchRow3KDelegate(void* a1, int key1, int key2, int key3);
    private IHook<NexSearchRow3KDelegate> _nexSearchRow3KHook;

    public NexHooks(Config config, IReloadedHooks hooks, ILogger logger)
        : base(config, hooks, logger)
    {

    }

    public override void Setup(IStartupScanner startupScanner)
    {
        startupScanner.SigScan("45 33 C0 89 54 24 ?? 45 8B D0 4C 8B D9 49 B9 25 23 22 84 E4 9C F2 CB 42 0F B6 44 14 ?? 48 B9 B3 01 00 00 00 01 00 00 4C 33 C8 49 FF C2 4C 0F AF C9 49 83 FA 04 72 ?? 49 8B 4B ?? 49 23 C9 4D" +
            " 8B 4B ?? 48 03 C9 49 8B 44 C9 ?? 49 3B 43 ?? 74 ?? 4D 8B 0C C9 EB ?? 49 3B C1 74 ?? 48 8B 40 ?? 3B 50 ?? 75 ?? EB ?? 49 8B C0 48 85 C0 49 0F 44 43 ?? 49 3B 43 ?? 74 ?? 4C 8B 40", "", address =>
            {
                _nexGetTableHook = _hooks.CreateHook<NexGetTableDelegate>(NexGetTableHookImpl, address).Activate();
            });

        startupScanner.SigScan("48 8B 41 ?? 48 85 C0 74 ?? 48 83 E8 01 74 ?? 48 83 F8 01 74 ?? 45 33 C9 45 33 C0", "", address =>
        {
            _nexSearchRow1KHook = _hooks.CreateHook<NexSearchRow1KDelegate>(NexSearchRow1KHookImpl, address).Activate();
        });

        startupScanner.SigScan("48 8B 41 ?? 48 85 C0 74 ?? 48 83 E8 01 74 ?? 48 83 F8 01 74 ?? 45 33 C9 E9", "", address =>
        {
            _nexSearchRow2KHook = _hooks.CreateHook<NexSearchRow2KDelegate>(NexSearchRow2KHookImpl, address).Activate();
        });

        startupScanner.SigScan("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 48 83 79 ?? ?? 49 8B F1", "", address =>
        {
            _nexSearchRow3KHook = _hooks.CreateHook<NexSearchRow3KDelegate>(NexSearchRow3KHookImpl, address).Activate();
        });
    }

    public override void UpdateConfig(Config configuration)
    {
        base.UpdateConfig(configuration);

        _tablesToIgnore = _configuration.NexTablesToExclude.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => Enum.Parse<TableType>(e))
            .ToHashSet();

        _tablesToInclude = _configuration.NexTablesToInclude.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => Enum.Parse<TableType>(e))
            .ToHashSet();
    }


    // This lets us know the location of the globals for each table
    // So we can reverse lookup the table id
    private unsafe void* NexGetTableHookImpl(void* a1, TableType tableId)
    {
        void* res = _nexGetTableHook.OriginalFunction(a1, tableId);
        if (_globalToTableId.TryAdd((nint)res, tableId))
        {
            if (_configuration.EnableNexLogging)
            {
                _logger.WriteLine($"[FFXVI NEX Logger] table id {tableId} global: 0x{(nint)res:X}");
            }
        }

        return res;
    }

    // 1 keyed row lookup
    private unsafe void* NexSearchRow1KHookImpl(void* tablePtr, int key1)
    {
        if (_configuration.EnableNexLogging)
        {
            var tableId = _globalToTableId[(nint)tablePtr];

            if (!_tablesToIgnore.Contains(tableId) && _tablesToInclude.Count == 0 || _tablesToInclude.Contains(tableId))
                _logger.WriteLine($"[FFXVI NEX Logger] Search(tableId: {tableId}, key1: {key1})");
        }

        return _nexSearchRow1KHook.OriginalFunction(tablePtr, key1);
    }

    // 2 keyed row lookup
    private unsafe void* NexSearchRow2KHookImpl(void* tablePtr, int key1, int key2)
    {
        var tableId = _globalToTableId[(nint)tablePtr];
        if (_configuration.EnableNexLogging)
        {
            if (!_tablesToIgnore.Contains(tableId) && _tablesToInclude.Count == 0 || _tablesToInclude.Contains(tableId))
                _logger.WriteLine($"[FFXVI NEX Logger] Search(tableId: {tableId}, key1: {key1}, key2: {key2})");
        }

        return _nexSearchRow2KHook.OriginalFunction(tablePtr, key1, key2);
    }

    // 3 keyed row lookup
    private unsafe void* NexSearchRow3KHookImpl(void* tablePtr, int key1, int key2, int key3)
    {
        var tableId = _globalToTableId[(nint)tablePtr];
        if (_configuration.EnableNexLogging)
        {
            if (!_tablesToIgnore.Contains(tableId) && _tablesToInclude.Count == 0 || _tablesToInclude.Contains(tableId))
                _logger.WriteLine($"[FFXVI NEX Logger] Search(tableId: {tableId}, key1: {key1}, key2: {key2}, key3: {key3})");
        }

        return _nexSearchRow3KHook.OriginalFunction(tablePtr, key1, key2, key3);
    }
}
