using System;
using System.Collections.Generic;

namespace newgame
{
    internal interface IActiveItemEffectNotifier
    {
        void WriteLine(string message);
    }

    internal sealed class ConsoleActiveItemEffectNotifier : IActiveItemEffectNotifier
    {
        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }

    internal class ActiveItemEffectManager
    {
        private readonly List<ActiveItemEffect> activeEffects = new();
        private readonly IActiveItemEffectNotifier notifier;

        public ActiveItemEffectManager(IActiveItemEffectNotifier? notifier = null)
        {
            this.notifier = notifier ?? new ConsoleActiveItemEffectNotifier();
        }

        public void AddEffect(Item item)
        {
            bool isFound = false;

            foreach (ActiveItemEffect effect in activeEffects)
            {
                if (effect.ItemType == item.ItemType)
                {
                    effect.RemainingTurn += item.ItemUsedCount;
                    effect.TotalBonus += item.ItemStatus;

                    notifier.WriteLine($"{item.ItemType} 효과가 누적되었습니다! → +{item.ItemStatus} / 남은 턴: {effect.RemainingTurn} (소모 시점: {effect.ConsumeType})");
                    isFound = true;
                    break;
                }
            }

            if (!isFound)
            {
                ActiveItemEffect newEffect = new ActiveItemEffect(item);
                activeEffects.Add(newEffect);

                notifier.WriteLine($"{item.ItemType} 효과가 새롭게 적용되었습니다! → +{item.ItemStatus} / {item.ItemUsedCount}턴 간 지속 (소모 시점: {newEffect.ConsumeType})");
            }
        }

        public void OnBattleStart()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveItemEffect effect = activeEffects[i];
                if (effect.ConsumeType == ConsumeType.BattleStart)
                {
                    effect.RemainingTurn--;
                    notifier.WriteLine($"{effect.ItemType} 효과가 1턴 차감되었습니다. → 남은 턴: {effect.RemainingTurn}");

                    if (effect.RemainingTurn <= 0)
                    {
                        notifier.WriteLine($"{effect.ItemType} 효과가 종료되었습니다. (전투 시작 시 차감)");
                        activeEffects.RemoveAt(i);
                    }
                }
            }
        }

        public void OnTurnPassed()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveItemEffect effect = activeEffects[i];
                if (effect.ConsumeType == ConsumeType.PerTurn)
                {
                    effect.RemainingTurn--;
                    notifier.WriteLine($"{effect.ItemType} 효과가 1턴 차감되었습니다. → 남은 턴: {effect.RemainingTurn}");

                    if (effect.RemainingTurn <= 0)
                    {
                        notifier.WriteLine($"{effect.ItemType} 효과가 종료되었습니다. (턴 경과로 차감)");
                        activeEffects.RemoveAt(i);
                    }
                }
            }
        }

        public int GetTotalBonus(ItemType effectType)
        {
            int total = 0;

            foreach (ActiveItemEffect effect in activeEffects)
            {
                if (effect.ItemType == effectType)
                {
                    total += effect.TotalBonus;
                }
            }

            return total;
        }
    }
}
