using System;
using System.Collections.Generic;

namespace newgame
{
    public struct SkillType
    {
        public string name;
        public int skillId;
        public int skillMana;
        public int skillDamage;
        public int skillTurn;
    }



    // 해당 클래스는 RPG 게임에서 캐릭터의 기술을 정의하는 클래스입니다.
    internal class Skills
    {

        #region 사용 가능한 스킬 보이기&사용 가능 스킬
        List<SkillType> canUseSkill = new List<SkillType>();

        public void AddCanUseSkill(SkillType skills) => canUseSkill.Add(skills);
        public void AddCanUseSkill(string name)
        {
            var skill = GameManager.Instance.FindSkillByName(name);
            if (skill != null)
            {
                canUseSkill.Add(skill.Value);
            }
        }
        public void ClearAllCanUseSkills() => canUseSkill.Clear();

        public void RemoveCanUseSkill(int idx)
        {
            int temp = idx - 1;

            if (temp >= 0 && temp < canUseSkill.Count)
            {
                canUseSkill.RemoveAt(idx - 1);
                return;
            }

            Console.WriteLine("해당 번호의 스킬이 존재하지 않습니다.");
        }

        public int ShowCanUseSkill()
        {
            if (canUseSkill.Count == 0)
            {
                return -1;
            }

            List<string> canUseSkillList = new List<string>();

            for (int i = 0; i < canUseSkill.Count; i++)
            {
                var skill = canUseSkill[i];
                string extra = string.Empty;
                if (skill.skillDamage != 0)
                {
                    extra = $" , 데미지: {skill.skillDamage}";
                }
                else if (skill.skillTurn != 0)
                {
                    extra = $" , 효과 지속 시간: {skill.skillTurn}";
                }

                canUseSkillList.Add($"{skill.name} --- 마나 사용량 : {skill.skillMana}{extra}");
            }

            int SelectSkill = UiHelper.SelectMenu(canUseSkillList.ToArray());
            return SelectSkill;
        }
        #endregion


    }
}