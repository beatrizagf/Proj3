using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using UnityEngine;

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
            this.MaxIterationsProcessedPerFrame = 100;
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
            
            for (MCTSNode best = BestChild(this.InitialNode); best.ChildNodes.Count != 0; best = BestChild(best))
            {
                this.BestActionSequence.Add(best.Action);
            }

            BestFirstChild = BestChild(this.InitialNode);
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
                ChooseRandom(state).ApplyActionEffects(state);
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
            WorldModel worldmodel = CurrentStateWorldModel.GenerateChildWorldModel();
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
                if ((node.ChildNodes[i].Q + 2 * (Mathf.Log(node.N) / node.ChildNodes[i].N)) > bestChildValue)
                {
                    bestChildValue = node.ChildNodes[i].Q + 2 * (Mathf.Log(node.N) / node.ChildNodes[i].N);
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

        private GOB.Action ChooseRandom(WorldModel state)
        {
            GOB.Action[] actions = state.GetExecutableActions();
            return actions[RandomGenerator.Next() % actions.Length];
        }

        private GOB.Action ChooseBias(WorldModel state)
        {
            GOB.Action[] actions = state.GetExecutableActions();
            return null;

        }

    }
}
