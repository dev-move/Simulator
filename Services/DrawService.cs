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
        private Prize DrawPrize(IList<Prize> pool)
        {
            if (pool == null || pool.Count == 0)
                throw new InvalidOperationException("상품 풀이 비어 있습니다.");

            double total = pool.Sum(p => p.Probability > 0 ? p.Probability : 0);

            if (total <= 0)
            {
                int index = _random.Next(pool.Count);
                return pool[index];
            }

            double roll = _random.NextDouble() * total;
            double acc = 0;

            foreach (var prize in pool)
            {
                double weight = prize.Probability > 0 ? prize.Probability : 0;
                acc += weight;

                if (roll <= acc)
                    return prize;
            }

            return pool[pool.Count - 1];
        }

        public DrawResult DrawOne(IList<Prize> pool, UserState userState)
        {
            if (userState.RemainingDraws <= 0)
                throw new InvalidOperationException("잔여 뽑기 수가 없습니다.");

            var prize = DrawPrize(pool);

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