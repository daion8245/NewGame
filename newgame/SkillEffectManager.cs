using System;
using System.Collections.Generic;

namespace newgame
{
    internal static class SkillEffectManager
    {
        public static void ApplyEffects(Character target)
        {
            foreach (var entry in target.ActiveSkills)
            {
                switch (entry.Key)
                {
                    case "Burn":
                        target.MyStatus.hp -= 5;
                        break;
                    case "Regeneration":
                        target.MyStatus.hp = Math.Min(target.MyStatus.hp + 3, target.MyStatus.maxHp);
                        break;
                }
            }
        }
    }
}
