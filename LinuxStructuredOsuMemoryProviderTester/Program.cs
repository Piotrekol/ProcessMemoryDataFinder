using CommandLine;
using OsuMemoryDataProvider;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

var parsedArgs = Parser.Default.ParseArguments<CommandLineOptions>(args).MapResult((o) => o, o => null);
if (parsedArgs == null)
    return;

Console.WriteLine(JsonSerializer.Serialize(parsedArgs));

var reader = StructuredOsuMemoryReader.Instance;
reader.InvalidRead += readerOnInvalidRead;
var baseAddresses = StructuredOsuMemoryReader.Instance.OsuMemoryAddresses;

JsonSerializerOptions jsonSerializerOptions = new()
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = parsedArgs.Indented,
};
var stopwatch = Stopwatch.StartNew();
double readTimeMs, readTimeMsMin, readTimeMsMax;
double _memoryReadTimeMin = double.PositiveInfinity;
double _memoryReadTimeMax = double.NegativeInfinity;
while (true)
{
    while (!reader.CanRead)
    {
        Console.WriteLine("Waiting for osu! process...");
        await Task.Delay(200);
    };

    stopwatch = Stopwatch.StartNew();

    reader.TryRead(baseAddresses.Beatmap);
    reader.TryRead(baseAddresses.Skin);
    reader.TryRead(baseAddresses.GeneralData);
    reader.TryRead(baseAddresses.BanchoUser);

    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.SongSelect)
        reader.TryRead(baseAddresses.SongSelectionScores);
    else
        baseAddresses.SongSelectionScores.Scores.Clear();

    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.ResultsScreen)
        reader.TryRead(baseAddresses.ResultsScreen);

    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.Playing)
    {
        reader.TryRead(baseAddresses.Player);
        reader.TryRead(baseAddresses.LeaderBoard);
        reader.TryRead(baseAddresses.KeyOverlay);
    }
    else
    {
        baseAddresses.LeaderBoard.Players.Clear();
    }

    var hitErrors = baseAddresses.Player?.HitErrors;
    if (hitErrors != null)
    {
        var hitErrorsCount = hitErrors.Count;
        hitErrors.Clear();
        hitErrors.Add(hitErrorsCount);
    }

    stopwatch.Stop();
    readTimeMs = stopwatch.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond;
    if (readTimeMs < _memoryReadTimeMin) _memoryReadTimeMin = readTimeMs;
    if (readTimeMs > _memoryReadTimeMax) _memoryReadTimeMax = readTimeMs;

    readTimeMsMin = _memoryReadTimeMin;
    readTimeMsMax = _memoryReadTimeMax;

    if (parsedArgs.Output)
        Console.WriteLine(JsonSerializer.Serialize(baseAddresses, jsonSerializerOptions));

    Console.WriteLine($"ReadTimeMS: {readTimeMs}{Environment.NewLine}" +
                      $"Min ReadTimeMS: {readTimeMsMin}{Environment.NewLine}" +
                      $"Max ReadTimeMS: {readTimeMsMax}{Environment.NewLine}" +
                      $"Press any key to reset min/max values{Environment.NewLine}");
    if (parsedArgs.ExitAfter)
        break;

    if (Console.KeyAvailable)
    {
        Console.ReadKey(true);
        while (Console.KeyAvailable)
            Console.ReadKey(true);

        _memoryReadTimeMin = double.PositiveInfinity;
        _memoryReadTimeMax = double.NegativeInfinity;

    }

    await Task.Delay(parsedArgs.Delay);
}

void readerOnInvalidRead(object sender, (object readObject, string propPath) e)
{
    Console.WriteLine($"{DateTime.Now:T} Error reading {e.propPath}{Environment.NewLine}");
}

class CommandLineOptions
{
    [Option('o', "output", Required = false, Default = false, HelpText = "Send read memory values to stdout")]
    public bool Output { get; set; }
    [Option('i', "indentedOutput", Required = false, Default = false, HelpText = "Format memory values sent to stdout")]
    public bool Indented { get; set; }
    [Option('e', "exitAfter", Required = false, Default = false, HelpText = "exit after reading memory values once")]
    public bool ExitAfter { get; set; }
    [Option('d', "delay", Required = false, Default = 200, HelpText = "delay between reads when continuous is enabled")]
    public int Delay { get; set; }
}
