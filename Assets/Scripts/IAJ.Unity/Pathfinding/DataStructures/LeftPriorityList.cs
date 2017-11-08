using System;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class LeftPriorityList : IOpenSet
    {   //ordena ordem crescente
        private List<NodeRecord> Open { get; set; }

        public LeftPriorityList()
        {
            this.Open = new List<NodeRecord>();    
        }
        public void Initialize()
        {
            //DONE implement
            this.Open.Clear();
        }

        public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace)
        {
            //DONE implement
            this.Open.Remove(nodeToBeReplaced);
            this.AddToOpen(nodeToReplace);
        }

        public NodeRecord GetBestAndRemove()
        {
            //DONE implement
            var best = this.PeekBest();
            this.Open.Remove(best);
            return best;
        }

        public NodeRecord PeekBest()
        {
            //DONE implement
            return this.Open[0];
        }

        public void AddToOpen(NodeRecord nodeRecord)
        {
            //a little help here
            //is very nice that the List<T> already implements a binary search method

            //se nao tiver la o no devolve negativo
            int index = this.Open.BinarySearch(nodeRecord);
            if (index < 0)
            {
                //~index para meter logo no sitio certo e a funcao ficar ordenada. da o indice onde devia estar e se tiver la alguma coisa manda para o lado
                this.Open.Insert(~index, nodeRecord);
            }
        }

        public void RemoveFromOpen(NodeRecord nodeRecord)
        {
            //DONE implement
            this.Open.Remove(nodeRecord);
        }

        public NodeRecord SearchInOpen(NodeRecord nodeRecord)
        {
            //DONE implement
            return this.Open.Find(n => n.Equals(nodeRecord));
        }

        public ICollection<NodeRecord> All()
        {
            //DONE implement
            return this.Open;
        }

        public int CountOpen()
        {
            //DONE implement
            return this.Open.Count;
        }
    }
}
