using ff16.utility.logger.Configuration;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ff16.utility.logger.Hooks;

public unsafe class FileAccessHooks : HookGroupBase
{
    public delegate int* HashPathDelegate(void* a1, string path);
    private IHook<HashPathDelegate> _hashPathHook;

    public delegate FileResult* OpenFileDelegate(void* a1, void* a2, string a3, uint* hashPtr, uint a5, ulong a6, ulong a7);
    private IHook<OpenFileDelegate> _openFileHook;

    public delegate void OpenFileAndCacheDelegate(void* a1, FileResult* a2);
    private IHook<OpenFileAndCacheDelegate> _openFileAndCacheHook;


    public FileAccessHooks(Config config, IReloadedHooks hooks, ILogger logger)
        : base(config, hooks, logger)
    {

    }

    public override void Setup(IStartupScanner startupScanner)
    {
        /* This was just the path hasher. Was very early and crude
        startupScanner.SigScan("40 53 48 83 EC 30 44 8A 0A", "", address =>
        {
            _hashPathHook = _hooks.CreateHook<HashPathDelegate>(HashPathHookImpl, address).Activate();
        });
        */

        /* We don't use that one. It grabs cached files (so spam the log with file "reads")
        startupScanner.SigScan("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 70 33 DB", "", address =>
        {
            _openFileHook = _hooks.CreateHook<OpenFileDelegate>(OpenFileHookImpl, address).Activate();
        });
        */

        // We use this one. This will actually get files uncached for the first time.
        startupScanner.SigScan("48 8B C4 48 89 58 ?? 48 89 68 ?? 48 89 70 ?? 48 89 78 ?? 41 56 48 83 EC 20 33 ED 48 8B FA", "", address =>
        {
            _openFileAndCacheHook = _hooks.CreateHook<OpenFileAndCacheDelegate>(OpenFileAndCacheHookImpl, address).Activate();
        });
    }

    /*
    private unsafe int* HashPathHookImpl(void* a1, string path)
    {
        if (_configuration.LogPathHashes)
            _logger.WriteLine($"[FFXVI PathHasher] {path}");

        return _hashPathHook.OriginalFunction(a1, path);
    }
    */

    /*
    private unsafe FileResult* OpenFileHookImpl(void* a1, void* a2, string a3, uint* hashPtr, uint a5, ulong a6, ulong a7)
    {
        var res = _openFileHook.OriginalFunction(a1, a2, a3, hashPtr, a5, a6, a7);

        if (_configuration.LogPathHashes)
        {
            if (res->FileSize != 0)
                _logger.WriteLine($"[FFXVI FileLogger] ok: {Marshal.PtrToStringAnsi((nint)res->PathPtr)} ({res->FileSize} bytes)");
            else
                _logger.WriteLine($"[FFXVI FileLogger] not found/empty: {Marshal.PtrToStringAnsi((nint)res->PathPtr)}");
        }

        return res;
    }
    */

    private unsafe void OpenFileAndCacheHookImpl(void* a1, FileResult* a2)
    {
        _openFileAndCacheHook.OriginalFunction(a1, a2);

        if (a2 != null)
        {
            if (_configuration.LogPathHashes)
            {
                if (a2->FileSize != 0)
                    _logger.WriteLine($"[FFXVI FileLogger] ok: {Marshal.PtrToStringAnsi((nint)a2->PathPtr)} ({a2->FileSize} bytes)");
                else
                    _logger.WriteLine($"[FFXVI FileLogger] not found/empty: {Marshal.PtrToStringAnsi((nint)a2->PathPtr)}");
            }
        }
    }


    public struct FileResult
    {
        public void* VTable;
        public uint field_0x08;
        public uint handleId; // maybe?
        public ulong field_0x10;
        public char* PathPtr;
        public ushort field_0x20;
        public ushort field_0x22;
        public ushort field_0x24;
        public ushort field_0x26;
        public void* field_0x28;
        public void* field_0x30;
        public ulong Empty;
        public ulong FileSize;
        public ulong Field_0x48;
        public uint field_0x50;
        public uint field_0x54;
    }

    /* we have to dive two or three calls within the highest level open file code
     * because files are cached and i.e a step sound theoretically tries to reopen/read the same file
     * but it's cached so that doesn't actually happen
     * 
     * code is more or less like this:
     * [ffxvi.exe sub_1409A4474 - 1.0.1]
     * -----------------------------------------------
    
    [...]

    v20 = GetCachedFileMaybe(a1, (__int64 *)&v25, a4, a4 & 0x1FF, a2, a3, a5, 0, (__int64)&v24, a7);
    if ( v20 == 1 ) // Is cached already?
    {
      v7 = v25; // return state
    }
    else if ( v20 >= 0 ) // not cached?
    {
      ReleaseSRWLockExclusive(v13);
      if ( v18 ) // constant file/pack?
      {
        // this path is only taken by actual loading of .pac files among other fixed files like:
        sound/driverconfig/sead.config
        shader/vfx.tec
        system/graphics/texture/omni_cube_index.tex
        gracommon/texture/lightmask/t_light_mask.tex

        v21 = v25;
        v22 = ExtractFile((__int64)v25);
        if ( v22 < 0 )
          LogError((int)byte_14157E580, v23, v22, 2);
        if ( (unsigned int)(_InterlockedExchangeAdd((volatile signed __int32 *)&v21->gap44[12], 0) - 2) <= 1 )
        {
          if ( v25 )
            (*(void (__fastcall **)(TextureClass *))(*(_QWORD *)v25->gap0 + 32LL))(v25);
          return 0LL;
        }
        return v25;
      }
      else
      {
        v7 = v25;
        sub_1409A7348((__int64)a1, v25); // Extracts trivial file
      }
      return v7;
    }

    [...]
    */
}
