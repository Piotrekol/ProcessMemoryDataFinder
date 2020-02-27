using System.Collections.Generic;

namespace OsuMemoryDataProvider
{
    public interface IOsuMemoryReader
    {
        IOsuMemoryReader GetInstanceForWindowTitleHint(string windowTitleHint);

        /// <summary>
        /// Fills all fields of PlayContainer with data read from osu! memory.
        /// </summary>
        /// <param name="playContainer">Initalized object to fill with data</param>
        void GetPlayData(PlayContainer playContainer);

        List<int> HitErrors();

        string PlayerName();
        int GetMods();
        int GetPlayingMods();
        int GetMapId();
        float GetMapAr();
        float GetMapCs();
        float GetMapHp();
        float GetMapOd();
        float GetMapSetId();
        string GetSkinFolderName();
        string GetOsuFileName();
        string GetMapFolderName();
        string GetSongString();
        string GetMapMd5();
        int ReadPlayTime();
        double ReadPlayerHp();
        double ReadDisplayedPlayerHp();
        int ReadPlayedGameMode();
        int ReadSongSelectGameMode();

        ushort ReadHit300();
        ushort ReadHit100();
        ushort ReadHit50();
        ushort ReadHitGeki();
        ushort ReadHitKatsu();
        ushort ReadHitMiss();
        double ReadAcc();
        ushort ReadCombo();
        ushort ReadComboMax();

        int GetRetrys();

        int ReadScore();
        
        OsuMemoryStatus GetCurrentStatus(out int statusNumber);
    }
}