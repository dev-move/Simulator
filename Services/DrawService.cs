using Simulator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Services
{
    public class DrawService
    {
        private readonly Random _random = new();

        private readonly (Rarity rarity, double percent)[] _rarityTable =
        {
            (Rarity.Legendary, 3.0),
            (Rarity.Rare, 7.0),
            (Rarity.High, 20.0),
            (Rarity.Common, 70.0)
        };

        private Rarity DrawRarity()
        {
            double total = _rarityTable.Sum(r => r.percent);
            double roll = _random.NextDouble() * total;

            double acc = 0;
            foreach (var (rarity, percent) in _rarityTable)
            {
                acc += percent;
                if (roll <= acc)
                {
                    return rarity;
                }
            }
            return Rarity.Common;
        }

        private Prize DrawPrizeRarity(IList<Prize> pool, Rarity rarity)
        {
            var candidates = pool.Where(p => p.Rarity == rarity).ToList();

            if (candidates.Count == 0)
            {
                candidates = pool.ToList();
            }

            double totalProbability = candidates.Sum(p => p.Probability <= 0? 1 :p.Probability);
            double roll = _random.NextDouble() * totalProbability;

            double acc = 0;
            foreach (var prize in candidates)
            {
                double prob = prize.Probability <= 0 ? 1 : prize.Probability;
                acc += prob;
                if (roll <= acc)
                {
                    return prize;
                }
            }
            return candidates[0];
        }

        public DrawResult DrawOne(IList<Prize> pool, UserState userState)
        {
            if(userState.RemainingDraws <= 0)
            {
                throw new InvalidOperationException("잔여 뽑기 수가 없습니다.");
            }

            var rarity = DrawRarity();
            var prize = DrawPrizeRarity(pool, rarity);

            userState.RemainingDraws--;
            userState.TotalDraws++;
            userState.Tickets += 1;

            return new DrawResult
            {
                Index = userState.TotalDraws,
                Prize = prize,
                DrawTime = DateTime.UtcNow,
            };
        }
    }
}
