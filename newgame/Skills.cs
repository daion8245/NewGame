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



    /// <summary>
    /// 해당 클래스는 스킬을 관리하는 클래스이다.
    /// </summary>
    internal class Skills
    {

        #region 사용 가능한 스킬 보이기&사용 가능 스킬
        /// <summary>
        /// 플레이어의 현제 사용 가능 스킬
        /// </summary>
        List<SkillType> canUseSkill = new List<SkillType>();

        //public void AddCanUseSkill(SkillType skills) => canUseSkill.Add(skills);

        /// <summary>
        /// 매개변수로 받은 스킬의 이름이 전체 스킬 리스트에 있는지 확인하고, 있다면 플레이어의
        /// 사용 가능 스킬을 추가하는 함수
        /// </summary>
        /// <param name="name"></param>
        public void AddCanUseSkill(string name)
        {
            var skill = GameManager.Instance.FindSkillByName(name);
            if (skill != null)
            {
                canUseSkill.Add(skill.Value);
            }
        }

        /// <summary>
        /// 플레이어의 사용 가능 스킬을 초기화시키는 함수
        /// </summary>
        public void ClearAllCanUseSkills() => canUseSkill.Clear();

        /// <summary>
        /// 매개변수로 받은 스킬의 이름이 전체 스킬 리스트에 있는지 확인하고, 있다면 플레이어의
        /// 사용 가능 스킬을 제거하는 함수
        /// </summary>
        /// <param name="idx"></param>
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

        /// <summary>
        /// 플레이어가 현제 사용 가능한 스킬 목록을 보여주는 함수
        /// </summary>
        /// <returns></returns>
        public SkillType ShowCanUseSkill()
        {
            if (canUseSkill.Count == 0)
            {
                return default(SkillType);
            }

            List<string> canUseSkillList = new List<string>();

            //모든 사용 가능 스킬 리스트를 canUseSkillList(string형 List)에 추가하는 로직
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

            int selected = UiHelper.SelectMenu(canUseSkillList.ToArray());
            if (selected >= 0 && selected < canUseSkill.Count)
            {
                return canUseSkill[selected];
            }

            return default(SkillType);
        }
        #endregion

        #region 스킬 사용
        /// <summary>
        /// 스킬과 적 타입을 받아 데미지와 특수효과를 계산하는 함수.
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="target"></param>
        public void UseSkill(SkillType skill, Character target)
        {
            Console.WriteLine($"{skill.name}! 이 {target.MyStatus.Name} 에게 적중!");
            target.MyStatus.hp -= skill.skillDamage;
        }
        #endregion
    }
}
