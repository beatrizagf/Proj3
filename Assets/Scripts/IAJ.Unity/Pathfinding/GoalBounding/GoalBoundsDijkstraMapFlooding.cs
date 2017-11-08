using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding
{
    //The Dijkstra algorithm is similar to the A* but with a couple of differences
    //1) no heuristic function
    //2) it will not stop until the open list is empty
    //3) we dont need to execute the algorithm in multiple steps (because it will be executed offline)
    //4) we don't need to return any path (partial or complete)
    //5) we don't need to do anything when a node is already in closed, porque a heuristica e inadmissivel e inconsistente
    public class GoalBoundsDijkstraMapFlooding
    {
        public NavMeshPathGraph NavMeshGraph { get; protected set; }
        public NavigationGraphNode StartNode { get; protected set; }
        public NodeGoalBounds NodeGoalBounds { get; protected set; }
        protected NodeRecordArray NodeRecordArray { get; set; }

        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }

        public GoalBoundsDijkstraMapFlooding(NavMeshPathGraph graph)
        {
            this.NavMeshGraph = graph;
            //do not change this
            var nodes = this.GetNodesHack(graph);
            this.NodeRecordArray = new NodeRecordArray(nodes);
            this.Open = this.NodeRecordArray;
            this.Closed = this.NodeRecordArray;

        }

        //cria rectangulos
        public void Search(NavigationGraphNode startNode, NodeGoalBounds nodeGoalBounds)
        {
            //TODO: Implement the algorithm that calculates the goal bounds using a dijkstra
            //Given that the nodes in the graph correspond to the edges of a polygon, we won't be able to use the vertices of the polygon to update the bounding boxes


            // mete os vizinhos do no inicial. Inicializacao da lista
            var outConnections = startNode.OutEdgeCount;
            for (int i = 0; i < outConnections; i++) {
                
                NavigationGraphEdge edge = startNode.EdgeOut(i);

                var childNode = edge.ToNode;
                var childNodeRecord = this.NodeRecordArray.GetNodeRecord(childNode);
                //adicionar ao open
                NodeRecordArray.AddToOpen(childNodeRecord);

                //transformar em vector3 para inicializar cada rectangulo
                childNodeRecord.StartNodeOutConnectionIndex = i;
          
            }

            //giro:  var startTime = Time.realtimeSinceStartup;

            //enquanto houver nos no conj open
            while (this.Open.CountOpen() > 0){
                NodeRecord bestNode = this.Open.GetBestAndRemove();

                //aumentar o rectangulo
                nodeGoalBounds.connectionBounds[bestNode.StartNodeOutConnectionIndex].UpdateBounds(bestNode.node.Position); //isto e a cor do rectangulo. falta updateBounds

                this.Closed.AddToClosed(bestNode);

                //para ver as ligacoes do no que acabamos de ver
                var outConnections2 = bestNode.node.OutEdgeCount;
                for (int j = 0; j < outConnections2; j++)
                {
                   this.ProcessChildNode(bestNode, bestNode.node.EdgeOut(j), bestNode.StartNodeOutConnectionIndex);
                }
                // giro: this.MaxOpenNodes = Mathf.Max(this.Open.CountOpen(), this.MaxOpenNodes);
               
            }
        }


        protected void ProcessChildNode(NodeRecord parent, NavigationGraphEdge connectionEdge, int connectionIndex)
        {
            float g;

            var childNode = connectionEdge.ToNode;
            var childNodeRecord = this.NodeRecordArray.GetNodeRecord(childNode);

            //custo da sol ate agora (valor do no anterior mais a aresta do bestNode ate ao childnode)
            g = parent.gValue + connectionEdge.Cost;

            if (childNodeRecord.status == NodeStatus.Unvisited)
            {
                childNodeRecord.gValue = g;
                childNodeRecord.parent = parent;
                NodeRecordArray.AddToOpen(childNodeRecord);
                childNodeRecord.StartNodeOutConnectionIndex = connectionIndex;   //das cor


            }
            else if (childNodeRecord.status == NodeStatus.Open && (childNodeRecord.gValue > g ))
            {
  
                childNodeRecord.gValue = g;
                childNodeRecord.parent = parent;
                NodeRecordArray.Replace(childNodeRecord, childNodeRecord);
                childNodeRecord.StartNodeOutConnectionIndex = connectionIndex;    //trocas cor


            }

        }

        private List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
        {
            //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
            //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
            //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
            //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
            //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
            //that's why we're using the type of the base class in the reflection call
            return (List<NavigationGraphNode>)Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
        }

    }
}

