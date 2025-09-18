using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newgame
{
    //해당 클래스는 RPG 게임에서 캐릭터의 직업을 정의하는 클래스입니다.
    internal class CharacterClass
    {
        public struct CharacterClassType
        {
            public string name;
            public string description;
            public int atk;
            public int def;
            public int hp;
            public int mp;
            public int CC;
            public int CD;
        }

        public CharacterClass()
        {

        }
    }
}
