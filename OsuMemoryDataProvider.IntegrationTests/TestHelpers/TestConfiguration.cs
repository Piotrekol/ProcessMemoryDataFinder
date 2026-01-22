using ProcessMemoryDataFinder;
using System;

namespace OsuMemoryDataProvider.IntegrationTests.TestHelpers;

public class TestConfiguration
{
    public string ProcessName { get; }
    public string WindowTitleHint { get; }
    public bool? Target64Bit { get; }

    public TestConfiguration()
    {
        ProcessName = Environment.GetEnvironmentVariable("OSU_PROCESS_NAME") ?? "osu!";
        WindowTitleHint = Environment.GetEnvironmentVariable("OSU_PROCESS_WINDOW_HINT");
        string bitnessEnv = Environment.GetEnvironmentVariable("OSU_TARGET_BITNESS");

        if (!string.IsNullOrEmpty(bitnessEnv))
        {
            if (bitnessEnv.Equals("x64", StringComparison.OrdinalIgnoreCase))
            {
                Target64Bit = true;
            }
            else if (bitnessEnv.Equals("x86", StringComparison.OrdinalIgnoreCase))
            {
                Target64Bit = false;
            }
        }
        else
        {
            Target64Bit = null;
        }
    }

    public ProcessTargetOptions ToProcessTargetOptions() => new(ProcessName, WindowTitleHint, Target64Bit);
}
