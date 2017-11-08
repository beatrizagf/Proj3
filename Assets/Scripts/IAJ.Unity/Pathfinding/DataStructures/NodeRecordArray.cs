using System;
using System.Collections.Generic;
using RAIN.Navigation.Graph;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class NodeRecordArray : IOpenSet, IClosedSet
    {
        private NodeRecord[] NodeRecords { get; set; }
        private List<NodeRecord> SpecialCaseNodes { get; set; } 
        private NodePriorityHeap Open { get; set; }

        public NodeRecordArray(List<NavigationGraphNode> nodes)
        {
            //this method creates and initializes the NodeRecordArray for all nodes in the Navigation Graph
            this.NodeRecords = new NodeRecord[nodes.Count];
            
            for(int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                node.NodeIndex = i; //we're setting the node Index because RAIN does not do this automatically
                this.NodeRecords[i] = new NodeRecord {node = node, status = NodeStatus.Unvisited};
            }

            this.SpecialCaseNodes = new List<NodeRecord>();

            this.Open = new NodePriorityHeap();
        }

        public NodeRecord GetNodeRecord(NavigationGraphNode node)
        {
            //do not change this method
            //here we have the "special case" node handling
            if (node.NodeIndex == -1)
            {
                for (int i = 0; i < this.SpecialCaseNodes.Count; i++)
                {
                    if (node == this.SpecialCaseNodes[i].node)
                    {
                        return this.SpecialCaseNodes[i];
                    }
                }
                return null;
            }
            else
            {
                return  this.NodeRecords[node.NodeIndex];
            }
        }

        public void AddSpecialCaseNode(NodeRecord node)
        {
            this.SpecialCaseNodes.Add(node);
        }

        void IOpenSet.Initialize()
        {
            this.Open.Initialize();
            //we want this to be very efficient (that's why we use for)
            for (int i = 0; i < this.NodeRecords.Length; i++)
            {
                this.NodeRecords[i].status = NodeStatus.Unvisited;
            }

            this.SpecialCaseNodes.Clear();
        }

        void IClosedSet.Initialize()
        {
            //Tá feito
        }

        public void AddToOpen(NodeRecord nodeRecord)
        {
            //Tá feito
            nodeRecord.status = NodeStatus.Open;
            this.Open.AddToOpen(nodeRecord);
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            //Tá feito
            nodeRecord.status = NodeStatus.Closed;
            //this.Open.RemoveFromOpen(nodeRecord);     //nos ja removemos com o getBestAndRemove, nao com este
        }

        public NodeRecord SearchInOpen(NodeRecord nodeRecord)
        {
            //Tá feito
            return this.Open.SearchInOpen(nodeRecord);
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            //Tá feito
            if (nodeRecord.status == NodeStatus.Closed)
                return nodeRecord;
            else
                return null;
        }

        public NodeRecord GetBestAndRemove()
        {
            //Tá feito
            var best = this.PeekBest();
            best.status = NodeStatus.Closed;    //como tiramos o no da lista open temos de alterar o status no caso de nao chegarmos a fazer addToClose que e quando encontramos a solucao
            return this.Open.GetBestAndRemove();
        }

        public NodeRecord PeekBest()
        {
            //Tá feito
            return this.Open.PeekBest();
        }

        public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace)
        {
            //Tá feito
            this.Open.Replace(nodeToBeReplaced, nodeToReplace);
        }

        public void RemoveFromOpen(NodeRecord nodeRecord)
        {
            //Tá feito
            //Cuidado! Não estamos a alterar o status       //nao e usado?
            this.Open.RemoveFromOpen(nodeRecord);
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            //Tá feito
            nodeRecord.status = NodeStatus.Open;
        }

        ICollection<NodeRecord> IOpenSet.All()
        {
            //Tá feito
            return this.Open.All();
        }

        ICollection<NodeRecord> IClosedSet.All()
        {
            //Tá feito
            //return null;  //temos de devolver os nos que tem status a closed
            List<NodeRecord> closed = new List<NodeRecord>();
            for (int i = 0; i < NodeRecords.Length; i++)
            {
                if (NodeRecords[i].status == NodeStatus.Closed)
                    closed.Add(NodeRecords[i]);
            }
            return closed;
        }

        public int CountOpen()
        {
            //Tá feito
            return this.Open.CountOpen();
        }
    }
}
