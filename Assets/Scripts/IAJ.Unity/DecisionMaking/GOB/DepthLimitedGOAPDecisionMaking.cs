using Assets.Scripts.GameManager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class DepthLimitedGOAPDecisionMaking
    {
        public const int MAX_DEPTH = 1;
        public int ActionCombinationsProcessedPerFrame { get; set; }
        public float TotalProcessingTime { get; set; }
        public int TotalActionCombinationsProcessed { get; set; }
        public bool InProgress { get; set; }

        public CurrentStateWorldModel InitialWorldModel { get; set; }
        private List<Goal> Goals { get; set; }
        private List<Action> Actions { get; set; }
        private WorldModel[] Models { get; set; }
        private Action[] ActionPerLevel { get; set; }
        public Action[] BestActionSequence { get; private set; }
        public Action BestAction { get; private set; }
        public float BestDiscontentmentValue { get; private set; }
        private int CurrentDepth {  get; set; }

        public DepthLimitedGOAPDecisionMaking(CurrentStateWorldModel currentStateWorldModel, List<Action> actions, List<Goal> goals)
        {
            this.ActionCombinationsProcessedPerFrame = 200;
            this.Actions = actions;
            this.Goals = goals;
            this.InitialWorldModel = currentStateWorldModel;
        }

        public void InitializeDecisionMakingProcess()
        {
            this.InProgress = true;
            this.TotalProcessingTime = 0.0f;
            this.TotalActionCombinationsProcessed = 0;
            this.CurrentDepth = 0;
            this.Models = new WorldModel[MAX_DEPTH + 1];
            this.Models[0] = this.InitialWorldModel;
            this.ActionPerLevel = new Action[MAX_DEPTH];
            this.BestActionSequence = new Action[MAX_DEPTH];
            this.BestAction = null;
            this.BestDiscontentmentValue = float.MaxValue;
            this.InitialWorldModel.Initialize();
        }

        public void ShuffleActions()
        {
            int size = this.Actions.Count;

            for (int i = 0; i < size; i++)
            {
                Action swap = this.Actions[i];
                int slot = UnityEngine.Random.Range(0, size - 1);
                this.Actions[i] = this.Actions[slot];
                this.Actions[slot] = swap;
            }
        }

        public Action ChooseAction()
        {
			
			var processedActions = 0;

			var startTime = Time.realtimeSinceStartup;

			//TODO: Implement
			//throw new NotImplementedException();
            //(slide: 99)

            float value;
            Action nextAction = null;

            while (this.CurrentDepth >= 0)
            {
                ShuffleActions();

                if (this.InProgress == false || processedActions > this.ActionCombinationsProcessedPerFrame)
                    return null;

                //quando ja tivermos 3 accoes, vamos escolher a melhor (permite a personagem anticipar os efeitos e planear as suas accoes)
                if (this.CurrentDepth >= MAX_DEPTH)
                {
                    value = this.Models[this.CurrentDepth].CalculateDiscontentment(this.Goals);

                    //diminuimos o descontentamento
                    if (value < this.BestDiscontentmentValue)
                    {
                        this.BestDiscontentmentValue = value;
                        this.BestAction = this.ActionPerLevel[0];
                        this.BestActionSequence = this.ActionPerLevel.ToArray();   //substituimos a nova accao (que e melhor), na sequencia das melhores accoes
                    }
                    this.CurrentDepth -= 1;
                    continue;
                }
                //so processo 3 accoes: olho no maximo um futuro de 3 accoes
          
                //ve qual a proxima accao que pode ser executada ou da null se nao existirem mais accoes executaveis
                nextAction = this.Models[this.CurrentDepth].GetNextAction();

                if (nextAction != null)
                {
                    this.Models[this.CurrentDepth + 1] = this.Models[this.CurrentDepth].GenerateChildWorldModel();  //simula o novo estado do mundo consoante esta accao, 
                    nextAction.ApplyActionEffects(this.Models[this.CurrentDepth + 1]);  //para vermos que accoes vao estar disponiveis
                    processedActions++;
                    this.ActionPerLevel[this.CurrentDepth] = nextAction;   //adicionamos às accoes para depois vermos qual e melhor
                    this.CurrentDepth += 1;
                }
                //se nao houver mais accoes, diminuimos CurrentDepth para sairmos do ciclo
                else
                {
                    this.CurrentDepth -= 1;
                }
                
            }
            this.TotalActionCombinationsProcessed = processedActions;

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
			this.InProgress = false;
			return this.BestAction;
        }
    }
}
