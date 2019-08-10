using System.Collections.Generic;

namespace OsuMemoryDataProvider
{
    public interface IOsuMemoryReader
    {
        /// <summary>
        /// Fills all fields of PlayContainer with data read from osu! memory.
        /// </summary>
        /// <param name="playContainer">Initalized object to fill with data</param>
        void GetPlayData(PlayContainer playContainer);

        List<int> HitErrors();

        string PlayerName();
        int GetMods();
        int GetMapId();
        float GetMapAr();
        float GetMapCs();
        float GetMapHp();
        float GetMapOd();
        float GetMapSetId();

        string GetOsuFileName();
        string GetMapFolderName();
        string GetSongString();
        string GetMapMd5();
        int ReadPlayTime();
        double ReadPlayerHp();
        double ReadDisplayedPlayerHp();
        int ReadPlayedGameMode();
        int ReadSongSelectGameMode();
        int GetRetrys();

        int ReadScore();
        
        OsuMemoryStatus GetCurrentStatus(out int statusNumber);
    }
}