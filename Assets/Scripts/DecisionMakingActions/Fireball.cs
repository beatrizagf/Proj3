using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using UnityEngine;
using System;

namespace Assets.Scripts.DecisionMakingActions
{
    public class Fireball : WalkToTargetAndExecuteAction
    {
        private int xpChange;

		public Fireball(AutonomousCharacter character, GameObject target) : base("Fireball",character,target)
		{
            //TODO: implement
            if (target.tag.Equals("Skeleton"))
            {
                this.xpChange = 5;
            }
            else if (target.tag.Equals("Orc"))
            {
                this.xpChange = 10;
            }
            else if (target.tag.Equals("Dragon"))
            {
                this.xpChange = 15;
            }
        }

		public override float GetGoalChange(Goal goal)
		{
            //TODO: implement
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.GAIN_XP_GOAL)
            {
                change += -this.xpChange;
            }

            return change;
        }

		public override bool CanExecute()
		{
            //TODO: implement
            if (!base.CanExecute()) return false;
            return this.Character.GameManager.characterData.Mana > 5;
        }

		public override bool CanExecute(WorldModel worldModel)
		{
            //TODO: implement
            if (!base.CanExecute(worldModel)) return false;
            var mana = (int)worldModel.GetProperty(Properties.MANA);
            return mana > 5;
        }

		public override void Execute()
		{
            //TODO: implement
            base.Execute();
            this.Character.GameManager.Fireball(this.Target);
        }


		public override void ApplyActionEffects(WorldModel worldModel)
		{
            //TODO: implement
            base.ApplyActionEffects(worldModel);

            var xp = (int)worldModel.GetProperty(Properties.XP);
            worldModel.SetProperty(Properties.XP, xp + this.xpChange);

            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);
        }

    }
}
