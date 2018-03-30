using System.Text;
using Nashet.UnityUIUtils;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    //[MenuItem("Tools/MyTool/Do It in C#")]
    public class BattleResult
    {
        private readonly Staff attacker, defender;

        //Army attackerArmy, attackerLoss, defenderArmy, defenderLoss;
        private int attackerArmy, attackerLoss, defenderArmy, defenderLoss;

        private bool result;
        private Province place;
        private StringBuilder sb = new StringBuilder();
        private string attackerBonus; private string defenderBonus;

        //public BattleResult(Country attacker, Country defender, Army attackerArmy, Army attackerLoss, Army defenderArmy, Army defenderLoss, bool result)
        public BattleResult(Staff attacker, Staff defender, int attackerArmy, int attackerLoss, int defenderArmy, int defenderLoss,
            Province place, bool result, string attackerBonus, string defenderBonus)
        {
            this.attacker = attacker;
            this.defender = defender;
            //this.attackerArmy = new Army(attackerArmy); this.attackerLoss = new Army(attackerLoss); this.defenderArmy = new Army(defenderArmy); this.defenderLoss = new Army(defenderLoss);
            this.attackerArmy = attackerArmy; this.attackerLoss = attackerLoss; this.defenderArmy = defenderArmy; this.defenderLoss = defenderLoss;
            this.result = result;
            this.place = place;
            this.defenderBonus = defenderBonus;
            this.attackerBonus = attackerBonus;
            //Game.allBattles.Add(this);
        }

        internal bool isAttackerWon()
        {
            return result;
        }

        internal bool isDefenderWon()
        {
            return !result;
        }

        internal void createMessage()
        {
            sb.Clear();

            if (attacker.IsHuman && isAttackerWon())
            {
                //.Append(" owned by ").Append(place.Country)
                sb.Append("Our glorious army attacked ").Append(place)
                    .Append(" with army of ").Append(attackerArmy).Append(" men.");
                sb.Append(" Modifiers: ").Append(attackerBonus);
                sb.Append("\n\nWhile enemy had ").Append(defenderArmy).Append(" men. Modifiers:  ").Append(defenderBonus);
                sb.Append("\n\nWe won, enemy lost all men and we lost ").Append(attackerLoss).Append(" men");
                sb.Append("\nProvince ").Append(place).Append(" is our now!");
                // sb.Append("\nDate is ").Append(Game.date);
                Message.NewMessage("We won a battle!", sb.ToString(), "Fine", false, place.getPosition());
            }
            else if (defender.IsHuman && isDefenderWon())
            {
                sb.Append("Our glorious army attacked in province ").Append(place).Append(" by evil ").Append(attacker)
                    .Append(" with army of ").Append(attackerArmy).Append(" men.");
                sb.Append(" Modifiers: ").Append(attackerBonus);
                sb.Append("\n\nWhile we had ").Append(defenderArmy).Append(" men. Modifiers: ").Append(defenderBonus);
                sb.Append("\n\nWe won, enemy lost all men and we lost ").Append(defenderLoss).Append(" men");
                // sb.Append("\nDate is ").Append(Game.date);
                Message.NewMessage("We won a battle!", sb.ToString(), "Fine", true, place.getPosition());
            }
            else if (attacker.IsHuman && isDefenderWon())
            {
                //.Append(" owned by ").Append(place.Country)
                sb.Append("Our glorious army attacked ").Append(place)
                    .Append(" with army of ").Append(attackerArmy).Append(" men");
                sb.Append(" Modifiers: ").Append(attackerBonus);
                sb.Append("\n\nWhile enemy had ").Append(defenderArmy).Append(" men. Modifiers:  ").Append(defenderBonus);
                sb.Append("\n\nWe lost, our invasion army is destroyed, while enemy lost ").Append(defenderLoss).Append(" men");
                // sb.Append("\nDate is ").Append(Game.date);
                Message.NewMessage("We lost a battle!", sb.ToString(), "Fine", false, place.getPosition());
            }
            else if (defender.IsHuman && isAttackerWon())

            {
                sb.Append("Our glorious army attacked in province ").Append(place).Append(" by evil ").Append(attacker)
                    .Append(" with army of ").Append(attackerArmy).Append(" men");
                sb.Append(" Modifiers: ").Append(attackerBonus);
                sb.Append("\n\nWhile we had ").Append(defenderArmy).Append(" men. Modifiers:  ").Append(defenderBonus);
                sb.Append("\n\nWe lost, our home army is destroyed, while enemy lost  ").Append(attackerLoss).Append(" men");
                var movement = attacker as Movement;
                if (movement == null)
                    sb.Append("\nProvince ").Append(place).Append(" is not our anymore!");
                else
                    sb.Append("\nWe had to enact ").Append(movement.getGoal());
                // sb.Append("\nDate is ").Append(Game.date);
                Message.NewMessage("We lost a battle!", sb.ToString(), "Not fine really", false, place.getPosition());
            }
        }

        internal Staff getDefender()
        {
            return defender;
        }

        internal Staff getAttacker()
        {
            return attacker;
        }
    }
}