namespace ProcessMemoryDataFinder;

/// <summary>
/// 
/// </summary>
/// <param name="ProcessName">Target process name, without extension.</param>
/// <param name="MainWindowTitleHint">Target main process window title to target.</param>
/// <param name="Target64Bit">Whenever memory reader should target x64 processes or x86. Setting this to null will skip this check.</param>
public record ProcessTargetOptions(
    string ProcessName,
    string MainWindowTitleHint = null,
    bool? Target64Bit = false);