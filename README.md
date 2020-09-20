# ProcessMemoryDataFinder <a href="https://www.nuget.org/packages/ProcessMemoryDataFinder"><img src="https://img.shields.io/nuget/v/ProcessMemoryDataFinder?label=ProcessMemoryDataFinder" alt="nuget"></a> | <a href="https://www.nuget.org/packages/OsuMemoryDataProvider"><img src="https://img.shields.io/nuget/v/OsuMemoryDataProvider?label=OsuMemoryDataProvider" alt="nuget"></a>

Provides means to find data based on pre-made memory signatures and read that data in windows processes.

It was mainly created to be able to read various values from the game called [osu!](https://osu.ppy.sh) but it was written in a way that should allow use for any windows program.

# Structure:
 * ProcessMemoryDataFinder - main implementation, does not have any application-specifc code.
 * OsuMemoryDataProvider / OsuMemoryDataProviderTester - example usage for the previously mentioned osu! game
 
It should be noted that ProcessMemoryDataFinder targets x86 platform. Make sure that immediate works also target that platform in order for everything to work correctly.


## License
This software is licensed under GNU GPLv3. You can find the full text of the license [here](https://github.com/Piotrekol/ProcessMemoryDataFinder/blob/master/LICENSE).
