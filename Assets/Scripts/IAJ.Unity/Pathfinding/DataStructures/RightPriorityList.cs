using System;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class RightPriorityList : IOpenSet, IComparer<NodeRecord>
    {   //ordena ordem decrescente
        private List<NodeRecord> Open { get; set; }

        public RightPriorityList()
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
            //TODO implement
            return this.Open[Open.Count - 1];
        }

        public void AddToOpen(NodeRecord nodeRecord)
        {
            //a little help here, notice the difference between this method and the one for the LeftPriority list
            //...this one uses a different comparer with an explicit compare function (which you will have to define below)
            int index = this.Open.BinarySearch(nodeRecord,this);
            if (index < 0)
            {
                this.Open.Insert(~index, nodeRecord);
            }
        }

        public void RemoveFromOpen(NodeRecord nodeRecord)
        {
            //TODO implement
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

        public int Compare(NodeRecord x, NodeRecord y)
        {
            //DONE implement
            //para ordem decrescente
            if (x.fValue == y.fValue)
            {
                return 0;
            }
            else if (x.fValue < y.fValue)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
}
