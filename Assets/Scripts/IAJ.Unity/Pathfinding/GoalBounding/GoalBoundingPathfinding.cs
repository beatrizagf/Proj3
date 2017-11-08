using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using RAIN.Navigation.NavMesh;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using RAIN.Navigation.Graph;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding
{
    public class GoalBoundingPathfinding : NodeArrayAStarPathFinding
    {
        public GoalBoundingTable goalBoundingTable { get; protected set; }
        public int DiscardedEdges { get; protected set; }
        public int TotalEdges { get; protected set; }

        public GoalBoundingPathfinding(NavMeshPathGraph graph, IHeuristic heuristic, GoalBoundingTable goalBoundsTable) : base(graph, heuristic)
        {
            this.goalBoundingTable = goalBoundsTable;
        }

        public override void InitializePathfindingSearch(Vector3 startPosition, Vector3 goalPosition)
        {
            this.DiscardedEdges = 0;
            this.TotalEdges = 0;
            base.InitializePathfindingSearch(startPosition, goalPosition);
        }

        //protected override void ProcessChildNode(NodeRecord parentNode, NavigationGraphEdge connectionEdge, int edgeIndex)
        protected override void ProcessChildNode(NodeRecord parentNode, NavigationGraphEdge connectionEdge)
        {
            //TODO: Implement this method for the GoalBoundingPathfinding to Work. If you implemented the NodeArrayAStar properly, you wont need to change the search method.
            float f;
            float g;
            float h;

            var childNode = connectionEdge.ToNode;
            var childNodeRecord = this.NodeRecordArray.GetNodeRecord(childNode);

            if (childNodeRecord == null)
            {
                //this piece of code is used just because of the special start nodes and goal nodes added to the RAIN Navigation graph when a new search is performed.
                //Since these special goals were not in the original navigation graph, they will not be stored in the NodeRecordArray and we will have to add them
                //to a special structure
                //it's ok if you don't understand this, this is a hack and not part of the NodeArrayA* algorithm, just do NOT CHANGE THIS, or your algorithm will not work
                childNodeRecord = new NodeRecord
                {
                    node = childNode,
                    parent = parentNode,
                    status = NodeStatus.Unvisited
                };
                this.NodeRecordArray.AddSpecialCaseNode(childNodeRecord);
            }

            //TODO: implement the rest of your code here

            //custo da sol ate agora (valor do no anterior mais a aresta do bestNode ate ao childnode)
            g = parentNode.gValue + connectionEdge.Cost;
            //funcao heuristica: melhor custo estimado de n ate a solucao (como AStarPathFinding)
            h = this.Heuristic.H(childNode, this.GoalNode);
            f = F(g, h);

            //indice da cor do rectangulo
            var color = childNodeRecord.StartNodeOutConnectionIndex;

            //indice startNode
            var startNode = childNodeRecord.node.NodeIndex;

            //entrada da tabela dos rectangulos
            //var bbox = this.goalBoundingTable.table[color].connectionBounds[startNode];
            bool inBounds;

            if (this.goalBoundingTable.table[startNode] != null)
            {
                var bbox = this.goalBoundingTable.table[startNode].connectionBounds[color];
                inBounds = bbox.PositionInsideBounds(childNodeRecord.node.Position);
            }
            else
            {
                inBounds = true;
            }


            if (childNodeRecord.status == NodeStatus.Unvisited && inBounds)
            {
                childNodeRecord.fValue = f;
                childNodeRecord.gValue = g;
                childNodeRecord.hValue = h;
                childNodeRecord.parent = parentNode;
                NodeRecordArray.AddToOpen(childNodeRecord);
            }
            else if (childNodeRecord.status == NodeStatus.Open && (childNodeRecord.fValue > f || (f == childNodeRecord.fValue && childNodeRecord.hValue > h)))
            {
                childNodeRecord.fValue = f;
                childNodeRecord.gValue = g;
                childNodeRecord.hValue = h;
                childNodeRecord.parent = parentNode;
                NodeRecordArray.Replace(childNodeRecord, childNodeRecord);
            }
            else if (childNodeRecord.status == NodeStatus.Closed && f < childNodeRecord.fValue)
            {
                childNodeRecord.fValue = f;
                childNodeRecord.gValue = g;
                childNodeRecord.hValue = h;
                childNodeRecord.parent = parentNode;
                NodeRecordArray.RemoveFromClosed(childNodeRecord);
                NodeRecordArray.AddToOpen(childNodeRecord);
            }
        }
    }
}
