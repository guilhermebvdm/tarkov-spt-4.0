using System;
using System.Collections.Generic;
using EFT;

namespace TRLDynamicSpawn.Helpers
{
    /// <summary>
    /// Mapeia cada chefe único não-nativo ao(s) seu(s) tipo(s) de guarda dedicado do EFT.
    /// Fonte: enum WildSpawnType.
    /// // ref: Assembly-CSharp/EFT/WildSpawnType.cs:1-69
    /// Chefes ausentes deste mapa (bossKilla, bossPartisan, gifter) não têm guarda dedicado por
    /// WildSpawnType no EFT e devem sempre nascer sozinhos. arenaFighterEvent (Bloodhounds) nunca
    /// consulta este mapa — vai pelo caminho de esquadrão genérico (ver isGruntSquad no chamador).
    /// ref: CR-016-01 — item 016 (elite-nao-nativo-clona-boss)
    /// </summary>
    public static class EliteFollowerMap
    {
        private static readonly WildSpawnType[] Empty = Array.Empty<WildSpawnType>();

        private static readonly Dictionary<WildSpawnType, WildSpawnType[]> Map = new()
        {
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:31 (bossKnight=26) / :32,33 (followerBigPipe=27, followerBirdEye=28)
            { WildSpawnType.bossKnight,   new[] { WildSpawnType.followerBigPipe, WildSpawnType.followerBirdEye } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:27,28 (bossTagilla=22 / followerTagilla=23)
            { WildSpawnType.bossTagilla,  new[] { WildSpawnType.followerTagilla } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:34,35 (bossZryachiy=29 / followerZryachiy=30)
            { WildSpawnType.bossZryachiy, new[] { WildSpawnType.followerZryachiy } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:16-20 (bossGluhar=11 / followerGluhar{Assault,Security,Scout,Snipe}=12..15)
            { WildSpawnType.bossGluhar,   new[] { WildSpawnType.followerGluharAssault, WildSpawnType.followerGluharSecurity, WildSpawnType.followerGluharScout, WildSpawnType.followerGluharSnipe } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:21,22 (followerSanitar=16 / bossSanitar=17)
            { WildSpawnType.bossSanitar,  new[] { WildSpawnType.followerSanitar } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:47-49 (bossKolontay=43 / followerKolontay{Assault,Security}=44,45)
            { WildSpawnType.bossKolontay, new[] { WildSpawnType.followerKolontayAssault, WildSpawnType.followerKolontaySecurity } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:8,10 (bossBully=3 / followerBully=5) — painel chama esse chefe de "Reshala"
            { WildSpawnType.bossBully,    new[] { WildSpawnType.followerBully } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:36,37,45,46 (bossBoar=32 / followerBoar=33 / followerBoarClose1=41 / followerBoarClose2=42) — painel chama esse chefe de "Kaban"
            { WildSpawnType.bossBoar,     new[] { WildSpawnType.followerBoar, WildSpawnType.followerBoarClose1, WildSpawnType.followerBoarClose2 } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:12,13 (bossKojaniy=7 / followerKojaniy=8) — painel chama esse chefe de "Shturman"
            { WildSpawnType.bossKojaniy,  new[] { WildSpawnType.followerKojaniy } },
            // ref: Assembly-CSharp/EFT/WildSpawnType.cs:25,26 (sectantWarrior=20 / sectantPriest=21)
            { WildSpawnType.sectantPriest, new[] { WildSpawnType.sectantWarrior } },
        };

        public static WildSpawnType[] GetFollowers(WildSpawnType bossRole)
        {
            return Map.TryGetValue(bossRole, out var followers) ? followers : Empty;
        }
    }
}
