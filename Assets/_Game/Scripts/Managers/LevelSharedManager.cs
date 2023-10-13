using ClocknestGames.Library.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace ClocknestGames.Game.Core
{
    public class LevelSharedManager : PersistentSingleton<LevelSharedManager>
    {
        public SimpleObjectPooler GoldItemParticlePooler;
        public SimpleObjectPooler PointItemUIWorldPooler;
        public SimpleObjectPooler GlassBreakParticlePooler;
        public SimpleObjectPooler MoneyBreakParticlePooler;

        public List<Sprite> PositiveEmojis;
        public List<Sprite> NegativeEmojis;
        public List<Sprite> DestinationReachedEmojis;
        public List<Sprite> PanicEmojis;

        public Sprite GetRandomPositiveEmoji()
        {
            return PositiveEmojis[Random.Range(0, PositiveEmojis.Count)];
        }

        public Sprite GetRandomNegativeEmoji()
        {
            return NegativeEmojis[Random.Range(0, NegativeEmojis.Count)];
        }

        public Sprite GetRandomDestinationReachedEmoji()
        {
            return DestinationReachedEmojis[Random.Range(0, DestinationReachedEmojis.Count)];
        }

        public Sprite GetRandomPanicEmoji()
        {
            return PanicEmojis[Random.Range(0, PanicEmojis.Count)];
        }
    }
}