using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.DecisionMakingActions;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        public int MaxIterations { get; set; }
        public int MaxIterationsProcessedPerFrame { get; set; }
        public int MaxPlayoutDepthReached { get; private set; }
        public int MaxSelectionDepthReached { get; private set; }
        public float TotalProcessingTime { get; private set; }
        public MCTSNode BestFirstChild { get; set; }
        public List<GOB.Action> BestActionSequence { get; private set; }

        //pesos para heuristica
        // public float WHp {get; set;}
        // public float WMana { get; set;}
        private float[] weights = new float[2];
        private const int WMoney = 0;
        private const int WTime = 2;
        private const int WXP = 1;
        private const int WLevel = 3;

        private int CurrentIterations { get; set; }
        private int CurrentIterationsInFrame { get; set; }
        private int CurrentDepth { get; set; }

        private CurrentStateWorldModel CurrentStateWorldModel { get; set; }
        private MCTSNode InitialNode { get; set; }
        private System.Random RandomGenerator { get; set; }
        

        public MCTS(CurrentStateWorldModel currentStateWorldModel)
        {
            this.InProgress = false;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.MaxIterations = 10000;
            this.MaxIterationsProcessedPerFrame = 2000;
            this.RandomGenerator = new System.Random();
        }


        public void InitializeMCTSearch()
        {
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.CurrentIterationsInFrame = 0;
            this.TotalProcessingTime = 0.0f;
            this.CurrentStateWorldModel.Initialize();
            this.InitialNode = new MCTSNode(this.CurrentStateWorldModel)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            this.BestActionSequence = new List<GOB.Action>();
            ///valor aos pesos
            //this.weights[WTime] = -0.9f;
            this.weights[WXP] = 0.1f;
            this.weights[WMoney] = 0.1f;
            //this.weights[WLevel] = 0.6f;
        }

        public GOB.Action Run()
        {
            MCTSNode selectedNode;
            Reward reward;

            var startTime = Time.realtimeSinceStartup;
            this.CurrentIterationsInFrame = 0;

            //TODO: implement
            while (this.CurrentIterations <= this.MaxIterations && this.CurrentIterationsInFrame <= this.MaxIterationsProcessedPerFrame)
            {
                selectedNode = Selection(this.InitialNode);
                reward = Playout(selectedNode.State);
                Backpropagate(selectedNode, reward);

                this.CurrentIterationsInFrame++;

            }

            this.CurrentIterations += this.CurrentIterationsInFrame;                //for debug
            this.InProgress = false;
            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            
            for (MCTSNode best = BestUCTChild(this.InitialNode); best.ChildNodes.Count != 0; best = BestUCTChild(best))
            {
                this.BestActionSequence.Add(best.Action);
            }

            BestFirstChild = BestUCTChild(this.InitialNode);
            return BestFirstChild.Action;
        }

        private MCTSNode Selection(MCTSNode initialNode)
        {
            GOB.Action nextAction;
            MCTSNode currentNode = initialNode;
            MCTSNode bestChild;///????


            //TODO: implement
            while (!currentNode.State.IsTerminal())
            {
                nextAction = currentNode.State.GetNextAction();     //para saber se ainda da para expandir
                if (nextAction != null)
                {
                    return Expand(currentNode, nextAction);
                }
                else
                {
                    currentNode = BestUCTChild(currentNode);
                }
                this.MaxSelectionDepthReached++;
            }
            return currentNode;
        }

        private Reward Playout(WorldModel initialPlayoutState)
        {
            //TODO: implement
            FutureStateWorldModel state = (FutureStateWorldModel)initialPlayoutState.GenerateChildWorldModel();
            while (!state.IsTerminal())
            {
                //escolher entre MCTS normal e MCTS bias
                //ChooseRandom(state).ApplyActionEffects(state);
                ChooseBias(state).ApplyActionEffects(state);
                state.CalculateNextPlayer();
                this.MaxPlayoutDepthReached++;
            }

            Reward reward = new Reward();
            reward.Value = state.GetScore();
            return reward;
        }

        private void Backpropagate(MCTSNode node, Reward reward)
        {
            //TODO: implement
            while (node != null)
            {
                node.N = node.N + 1;
                node.Q = node.Q + reward.Value;
                node = node.Parent;
            }
        }

        private MCTSNode Expand(MCTSNode parent, GOB.Action action)
        {
			//TODO: implement
			//WorldModel worldmodel = CurrentStateWorldModel.GenerateChildWorldModel();
			WorldModel worldmodel = parent.State.GenerateChildWorldModel();
			action.ApplyActionEffects(worldmodel);
            worldmodel.CalculateNextPlayer();
            MCTSNode n = new MCTSNode(worldmodel)
            {
                Action = action,
                Parent = parent,
                N = 0,
                Q = 0
            };
            parent.ChildNodes.Add(n);
            return n;
        }

        //gets the best child of a node, using the UCT formula
        private MCTSNode BestUCTChild(MCTSNode node)///??????
        {
            //TODO: implement
            MCTSNode bestChild = new MCTSNode(node.State);
            float bestChildValue = float.MinValue;

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                float childValue = node.ChildNodes[i].Q/node.ChildNodes[i].N + C * Mathf.Sqrt(Mathf.Log(node.N) / node.ChildNodes[i].N);
                if (childValue > bestChildValue)
                {
                    bestChildValue = childValue;
                    bestChild = node.ChildNodes[i];
                }

            }
            return bestChild;
        }

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        private MCTSNode BestChild(MCTSNode node)///?????
        {
            //TODO: implement
            MCTSNode bestChild = new MCTSNode(node.State);
            float bestChildValue = float.MinValue;
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                if (node.ChildNodes[i].Q > bestChildValue)
                {
                    bestChildValue = node.ChildNodes[i].Q;
                    bestChild = node.ChildNodes[i];
                }

            }
            return bestChild;

        }

        private GOB.Action ChooseRandom(FutureStateWorldModel state)
        {
            GOB.Action[] actions = state.GetExecutableActions();
            return actions[RandomGenerator.Next() % actions.Length];
        }

        private bool ChestDead(FutureStateWorldModel state, GOB.Action action, string enemyName, string chestName)
        {
            //bool cond1 = !(bool)state.GetProperty(enemyName);
            bool cond1 = GameObject.Find(enemyName) == null;
            bool cond2 = GameObject.Find(chestName) != null;
            //bool cond2 = (bool)state.GetProperty(chestName);
            bool cond3 = action is PickUpChest;
            bool cond4 = action is PickUpChest && ((PickUpChest)action).Target.name.Equals(chestName);
            /*
            if (cond3)
                cond4 = action is PickUpChest && ((PickUpChest)action).Target.tag.Equals(chestName);
            else
                cond4 = false;
                */
            return cond1 && cond2 && cond3 && cond4;

            //return !(bool)state.GetProperty(enemyName) && (bool)state.GetProperty(chestName) && action is PickUpChest && ((PickUpChest)action).Target.tag.Equals(chestName);

        }
        /*
        private bool ChestReallyDead(FutureStateWorldModel state, GOB.Action action, string enemyName, string chestName)
        {
            bool cond1 = !(bool)state.GetProperty(enemyName);
            bool cond1b = GameObject.Find(enemyName)
            bool cond2 = (bool)state.GetProperty(chestName);
            bool cond3 = action is PickUpChest;
            bool cond4 = action is PickUpChest && ((PickUpChest)action).Target.tag.Equals(chestName);
            /*
            if (cond3)
                cond4 = action is PickUpChest && ((PickUpChest)action).Target.tag.Equals(chestName);
            else
                cond4 = false;
                */
            //return cond1 && cond2 && cond3 && cond4;

            //return !(bool)state.GetProperty(enemyName) && (bool)state.GetProperty(chestName) && action is PickUpChest && ((PickUpChest)action).Target.tag.Equals(chestName);

        //}

        private GOB.Action ChooseBias(FutureStateWorldModel state)
        {
            GOB.Action[] actions = state.GetExecutableActions();
            int[] features = new int[2];

            int size = features.Length;
            double H = 0;
            double[] exp = new double[actions.Length];    //array com as exponenciais ja calculadas
            double[] P = new double[actions.Length];    //array com as probabilidades ja calculadas para escolher a melhor

            for (int j = 0; j < actions.Length; j++) {
                float h = 0;

                if (actions[j] is SwordAttack && (int)state.GetProperty(Properties.HP) + ((SwordAttack)actions[j]).hpChange <= 0) {
                    //actions = actions.Where(val => val != action).ToArray();  //para a nao optimizacao
                    exp[j] = 0;
                    continue;  //do for, para passa a proxima accao
                }
                if (ChestDead(state, actions[j], "Skeleton1", "Chest1") || ChestDead(state, actions[j], "Skeleton2", "Chest4")
                 || ChestDead(state, actions[j], "Orc1", "Chest3") || ChestDead(state, actions[j], "Orc2", "Chest2") || ChestDead(state, actions[j], "Dragon", "Chest5")) {
                    h = 99999;
                    exp[j] = Mathf.Exp(h); 
                    H += Mathf.Exp(h);
                }
              



                else
                {
                    FutureStateWorldModel possibleState = (FutureStateWorldModel)state.GenerateChildWorldModel();
                    actions[j].ApplyActionEffects(possibleState);
                    possibleState.CalculateNextPlayer();

                    features[WMoney] = (int)possibleState.GetProperty(Properties.MONEY);
                    //features[WTime] = (int) (float) possibleState.GetProperty(Properties.TIME);
                    features[WXP] = (int)possibleState.GetProperty(Properties.XP);
                    //features[WLevel] = (int)possibleState.GetProperty(Properties.LEVEL);

                    for (int i = 0; i < size; i++) {
                        h += features[i] * weights[i];   //cada peso para uma accao
                    }
                    exp[j] = Mathf.Exp(h);    //queremos guardar logo a exponencial para nao ter de calcular outra vez
                    H += Mathf.Exp(h);
                }
            }

            if (H == 0){return actions[0];}
            else{
                P[0] = exp[0] / H;      //o primeiro nao acumula
                for (int j = 1; j < actions.Length; j++){
                    P[j] = P[j-1]+exp[j] / H;   //para ser cumulativo
                }
                double rand = RandomGenerator.NextDouble();

                //prob maior mais pequena que o random
                return actions[Array.FindIndex(P, val => val >= rand)];
            }

        }

    }
}
