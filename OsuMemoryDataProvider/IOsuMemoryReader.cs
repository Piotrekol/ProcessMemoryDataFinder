using System;
using System.Collections.Generic;

namespace OsuMemoryDataProvider
{
    [Obsolete("This version of reader is not updated anymore with new values. Use StructuredOsuMemoryReader in new implementations.", false)]
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
        int GetPlayingMods();
        int GetMapId();
        float GetMapAr();
        float GetMapCs();
        float GetMapHp();
        float GetMapOd();
        int GetMapSetId();
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
        bool IsReplay();

        /// <summary>
        ///this works for both normal score and V2 but requires 5 pointer jumps compared to 2 in <see cref="ReadScore"/>
        /// </summary>
        /// <returns></returns>
        int ReadScoreV2();
        TourneyIpcState GetTourneyIpcState(out int ipcNumber);
        int ReadTourneyLeftStars();
        int ReadTourneyRightStars();
        int ReadTourneyBO();
        bool ReadTourneyStarsVisible();
        bool ReadTourneyScoreVisible();

        OsuMemoryStatus GetCurrentStatus(out int statusNumber);
    }
}