using System;
using System.Collections.Generic;

namespace newgame
{
    internal sealed class BattleLogService
    {
        private readonly string[] battleLog = new string[2];

        public string[] SnapshotBattleLog()
        {
            return new[]
            {
                battleLog[0] ?? string.Empty,
                battleLog[1] ?? string.Empty
            };
        }

        public void ResetBattleLog()
        {
            battleLog[0] = string.Empty;
            battleLog[1] = string.Empty;
        }

        public bool IsPlayer(Character? actor)
        {
            if (actor == null)
            {
                return false;
            }

            Player? activePlayer = GameManager.Instance.player;
            return activePlayer != null && ReferenceEquals(actor, activePlayer);
        }

        private static string FormatLogLine(string message, bool isFirstLine)
        {
            string content = (message ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            string prefix = isFirstLine ? "->" : " ->";
            return prefix + content;
        }

        public void UpdateBattleMessage(Character attacker, string message, bool clearOpponentMessage)
        {
            bool isPlayer = IsPlayer(attacker);
            int index = isPlayer ? 0 : 1;

            string formatted = FormatLogLine(message, true);
            battleLog[index] = formatted;

            if (clearOpponentMessage)
            {
                battleLog[isPlayer ? 1 : 0] = string.Empty;
            }
        }

        public void AppendBattleMessage(Character actor, string message)
        {
            bool isPlayer = IsPlayer(actor);
            int index = isPlayer ? 0 : 1;

            string existing = battleLog[index] ?? string.Empty;
            bool isFirstLine = string.IsNullOrEmpty(existing);
            string formatted = FormatLogLine(message, isFirstLine);

            if (string.IsNullOrEmpty(formatted))
            {
                return;
            }

            battleLog[index] = isFirstLine ? formatted : existing + "\n" + formatted;
        }

        public void ClearBattleMessageForActor(Character? actor)
        {
            if (actor == null)
            {
                return;
            }

            Player? activePlayer = GameManager.Instance.player;
            Monster? activeMonster = GameManager.Instance.monster;

            if (activePlayer != null && ReferenceEquals(actor, activePlayer))
            {
                battleLog[0] = string.Empty;
            }
            else if (activeMonster != null && ReferenceEquals(actor, activeMonster))
            {
                battleLog[1] = string.Empty;
            }
        }

        public void ClearBattleMessageForOpponent(Character? actor)
        {
            if (actor == null)
            {
                return;
            }

            Player? activePlayer = GameManager.Instance.player;
            Monster? activeMonster = GameManager.Instance.monster;

            if (activePlayer != null && ReferenceEquals(actor, activePlayer))
            {
                battleLog[1] = string.Empty;
            }
            else if (activeMonster != null && ReferenceEquals(actor, activeMonster))
            {
                battleLog[0] = string.Empty;
            }
        }

        public void ApplyTickLogs(IEnumerable<SkillTickLog> tickLogs)
        {
            foreach (var log in tickLogs)
            {
                AppendBattleMessage(log.Actor, log.Message);

                if (log.ClearOpponent)
                {
                    ClearBattleMessageForOpponent(log.Actor);
                }
            }
        }

        public string BuildActionMessage(
            Character attacker,
            Character defender,
            int damage,
            string? actionName,
            bool targetDefeated,
            bool isCritical = false)
        {
            string label = string.IsNullOrWhiteSpace(actionName) ? "공격" : actionName!;
            string prefix = $"{attacker.MyStatus.Name}의 {label}!";

            if (isCritical)
            {
                prefix = $"[치명타!] {prefix}";
            }

            string suffix = $"{defender.MyStatus.Name}은 {damage}의 피해를 입었다. 남은 체력: {defender.MyStatus.Hp}/{defender.MyStatus.maxHp}";

            if (targetDefeated)
            {
                suffix += " (쓰러짐)";
            }

            return $"{prefix} {suffix}";
        }

        public void ShowBattleInfo(Character context, Character? target)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Console.Clear();

            string[] log = SnapshotBattleLog();
            string msg0 = log.Length > 0 ? log[0] ?? string.Empty : string.Empty;
            string msg1 = log.Length > 1 ? log[1] ?? string.Empty : string.Empty;

            Character? playerChar = GameManager.Instance.player;
            Character? monsterChar = GameManager.Instance.monster;

            if (playerChar == null)
            {
                if (context is Player)
                {
                    playerChar = context;
                }
                else if (target is Player)
                {
                    playerChar = target;
                }
            }

            if (monsterChar == null)
            {
                if (context is Monster)
                {
                    monsterChar = context;
                }
                else if (target is Monster)
                {
                    monsterChar = target;
                }
            }

            Status playerStatus = playerChar?.MyStatus ?? context.MyStatus;
            Status monsterStatus = monsterChar?.MyStatus ?? (target?.MyStatus ?? context.MyStatus);

            const int width = 68;
            string border = new string('=', width);
            string divider = new string('-', width);

            static string Fit(string text, int width)
            {
                text ??= string.Empty;
                return text.Length > width ? text.PadRight(width) : text.PadRight(width);
            }

            static string FormatStatus(string label, Character? character, Status status)
            {
                string name = string.IsNullOrWhiteSpace(status.Name) ? "??" : status.Name;
                string effectLabel = character?.StatusEffects.GetActiveSkillEffectDisplay() ?? string.Empty;
                return $"{label} : {name}{effectLabel}  Lv.{status.level}  HP {status.Hp}/{status.maxHp}";
            }

            string playerLine = FormatStatus("플레이어", playerChar, playerStatus);
            string monsterLine = FormatStatus("몬스터  ", monsterChar, monsterStatus);

            static void PrintLog(string label, string message, int width, Func<string, int, string> formatter)
            {
                string prefix = $"{label} ▶ ";
                string indent = new string(' ', prefix.Length);

                string[] lines = string.IsNullOrEmpty(message)
                    ? Array.Empty<string>()
                    : message.Split('\n');

                if (lines.Length == 0)
                {
                    Console.WriteLine(formatter(prefix, width));
                    return;
                }

                for (int i = 0; i < lines.Length; i++)
                {
                    string linePrefix = i == 0 ? prefix : indent;
                    string content = lines[i];
                    Console.WriteLine(formatter(linePrefix + content, width));
                }
            }

            Console.WriteLine(border);
            Console.WriteLine(Fit(playerLine, width));
            Console.WriteLine(Fit(monsterLine, width));
            Console.WriteLine(divider);
            PrintLog("플레이어", msg0, width, Fit);
            PrintLog("몬스터  ", msg1, width, Fit);
            Console.WriteLine(border);
            Console.WriteLine();
        }
    }
}
