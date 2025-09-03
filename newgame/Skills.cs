using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newgame
{
    public struct SkillType()
    {
        public string name;
        public int skillId;
        public int skillMana;
        public int skillDamage;
        public int skillTrun;

        #region Get 프로퍼티

        public string GetName
        {
            get => name;
            private set => name = value;
        }

        public int GetSkillID
        {
            get => skillId;
            private set => skillId = value;
        }

        public int GetSkillMana
        {
            get => skillMana;
            private set => skillMana = value;
        }

        public int GetSkillDamage
        {
            get => skillDamage;
            private set => skillDamage = value;
        }

        public int GetSkillTurn
        {
            get => skillTrun;
            private set => skillTrun = value;
        }

        #endregion
    }



    // 해당 클래스는 RPG 게임에서 캐릭터의 기술을 정의하는 클래스입니다.
    internal class Skills
    {

        #region 사용 가능한 스킬 보이기&사용 가능 스킬
        List<SkillType> canUseSkill = new List<SkillType>();

        public void AddCanUseSkill(SkillType skills) => canUseSkill.Add(skills);
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
            if (canUseSkill == null || canUseSkill.Count == 0)
            {
                return -1;
            }

            List<string> canUseSkillList = new List<string>();
            string? upType = null;

            for (int i = 0; i < canUseSkill.Count; i++)
            {
                if (canUseSkill[i].GetSkillDamage != 0)
                {
                    upType += $" , 데미지: {canUseSkill[i].GetSkillDamage}";
                }
                else if (canUseSkill[i].GetSkillTurn != 0)
                {
                    upType += $" , 효과 지속 시간: {canUseSkill[i].GetSkillTurn}";
                }

                canUseSkillList.Add($"{canUseSkill[i].GetName} --- 마나 사용량 : {canUseSkill[i].GetSkillMana}" + upType);
            }

            int SelectSkill = UiHelper.SelectMenu(canUseSkillList.ToArray());
            return SelectSkill;
        }
        #endregion


    }
}