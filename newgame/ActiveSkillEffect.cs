namespace newgame
{
    internal class ActiveSkillEffect
    {
        public int SkillId { get; private set; }
        public int RemainingTurn { get; set; }
        public int TotalPower { get; set; }

        public ActiveSkillEffect(SkillType skill)
        {
            SkillId = skill.skillId;
            RemainingTurn = skill.skillTurn;
            TotalPower = skill.skillDamage;
        }
    }
}
