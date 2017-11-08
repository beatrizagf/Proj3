using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class AStarPathfinding
    {
        public NavMeshPathGraph NavMeshGraph { get; protected set; }
        //how many nodes do we process on each call to the search method (this method will be called every frame when there is a pathfinding process active
        public uint NodesPerFrame { get; set; }

        public uint TotalExploredNodes { get; protected set; }
        public int MaxOpenNodes { get; protected set; }
        public float TotalProcessingTime { get; protected set; }
        public bool InProgress { get; protected set; }

        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }

        public NavigationGraphNode GoalNode { get; protected set; }
        public NavigationGraphNode StartNode { get; protected set; }
        public Vector3 StartPosition { get; protected set; }
        public Vector3 GoalPosition { get; protected set; }

        //heuristic function
        public IHeuristic Heuristic { get; protected set; }

        public AStarPathfinding(NavMeshPathGraph graph, IOpenSet open, IClosedSet closed, IHeuristic heuristic)
        {
            this.NavMeshGraph = graph;
            this.Open = open;
            this.Closed = closed;
            this.NodesPerFrame = uint.MaxValue; //by default we process all nodes in a single request
            this.InProgress = false;
            this.Heuristic = heuristic;
        }

        public virtual void InitializePathfindingSearch(Vector3 startPosition, Vector3 goalPosition)
        {
            this.StartPosition = startPosition;
            this.GoalPosition = goalPosition;
            this.StartNode = this.Quantize(this.StartPosition);
            this.GoalNode = this.Quantize(this.GoalPosition);

            //if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
            if (this.StartNode == null || this.GoalNode == null) return;

            //I need to do this because in Recast NavMesh graph, the edges of polygons are considered to be nodes and not the connections.
            //Theoretically the Quantize method should then return the appropriate edge, but instead it returns a polygon
            //Therefore, we need to create one explicit connection between the polygon and each edge of the corresponding polygon for the search algorithm to work
            ((NavMeshPoly)this.StartNode).AddConnectedPoly(this.StartPosition);
            ((NavMeshPoly)this.GoalNode).AddConnectedPoly(this.GoalPosition);

            this.InProgress = true;
            this.TotalExploredNodes = 0;
            this.TotalProcessingTime = 0.0f;
            this.MaxOpenNodes = 0;

            var initialNode = new NodeRecord
            {
                gValue = 0,
                hValue = this.Heuristic.H(this.StartNode, this.GoalNode),
                node = this.StartNode
            };

            initialNode.fValue = AStarPathfinding.F(initialNode);

            this.Open.Initialize(); 
            this.Open.AddToOpen(initialNode);
            this.Closed.Initialize();
        }

        protected virtual void ProcessChildNode(NodeRecord parentNode, NavigationGraphEdge connectionEdge)
        {
            //this is where you process a child node 
            var childNode = GenerateChildNodeRecord(parentNode, connectionEdge);
            //TODO: implement the rest of the code here
            var openNode = this.Open.SearchInOpen(childNode); //procura o child node em ambos os conjuntos
            var closedNode = this.Closed.SearchInClosed(childNode);

            //se nao existir este node, adiciona se ao conj Open
            if (openNode == null && closedNode == null)
            {
                this.Open.AddToOpen(childNode);
            }

            //se ja existir e ainda nao foi tratado
            else if (openNode != null)
            {
                //se openNode tiver um custo mais elevado substitui se pelo childNode (o caminho para chegar ao no e diferente, apesar do no ser o mesmo)
                if (openNode.fValue > childNode.fValue || (openNode.fValue == childNode.fValue && openNode.hValue > childNode.hValue))
                    this.Open.Replace(openNode, childNode);
            }

            //se ja existir e ja foi tratado(ja esta nos closed set) mas se o closed afinal for mais caro que o child entao 
            //retiramos este do closed e acrescentamos ao open para ser tratado
            else if (closedNode != null && closedNode.fValue > childNode.fValue)
            {

                this.Closed.RemoveFromClosed(closedNode);
                this.Open.AddToOpen(childNode);
            }
        }

        //devolve true se acabou (porque encontrou sol ou nao ha nenhuma) ou false se ainda nao acabou
        public bool Search(out GlobalPath solution, bool returnPartialSolution = false)
        {
            //TODO: implement this
            //to determine the connections of the selected nodeRecord you need to look at the NavigationGraphNode' EdgeOut  list
            //something like this
            //var outConnections = bestNode.node.OutEdgeCount;
            //for (int i = 0; i < outConnections; i++)
            //{
            //this.ProcessChildNode(bestNode, bestNode.node.EdgeOut(i));
            var startTime = Time.realtimeSinceStartup;
            var processedNodes = 0;

            while (processedNodes < this.NodesPerFrame)
            {
                //se houver nos no conj open
                if (this.Open.CountOpen() > 0)
                {
                    NodeRecord bestNode = this.Open.GetBestAndRemove();
                    if (bestNode.node == this.GoalNode)
                    {
                        //encontrou a sol
                        this.InProgress = false;
                        solution = this.CalculateSolution(bestNode, returnPartialSolution);
                        this.TotalProcessingTime = Time.realtimeSinceStartup - startTime;
                        return true;
                    }
                    //se nao passa ao proximo no
                    this.Closed.AddToClosed(bestNode);
                    this.TotalExploredNodes++;
                    processedNodes++;

                    //para ver as ligacoes do no que acabamos de ver
                    var outConnections = bestNode.node.OutEdgeCount;
                    for (int i = 0; i < outConnections; i++)
                    {
                        this.ProcessChildNode(bestNode, bestNode.node.EdgeOut(i));
                    }
                    //tamanho maximo da estrutura de dados
                    this.MaxOpenNodes = Mathf.Max(this.Open.CountOpen(), this.MaxOpenNodes);
                }
                else
                {
                    //se nao ha solucao retorna null e true
                    this.InProgress = false;
                    solution = null;
                    this.TotalProcessingTime = Time.realtimeSinceStartup - startTime;
                    return true;
                }
            }

            //se ja corremos o metodo ate ao numero de nos estabelecido e ainda nao encontrou o no
            if (returnPartialSolution)
            {
                //vai devolver o melhor ate agora
                solution = this.CalculateSolution(this.Open.PeekBest(), returnPartialSolution);
            }
            else
            {
                solution = null;
            }

            this.TotalProcessingTime = Time.realtimeSinceStartup - startTime;
            return false;
        }

        protected NavigationGraphNode Quantize(Vector3 position)
        {
            return this.NavMeshGraph.QuantizeToNode(position, 1.0f);
        }

        protected void CleanUp()
        {
            //I need to remove the connections created in the initialization process
            if (this.StartNode != null)
            {
                ((NavMeshPoly)this.StartNode).RemoveConnectedPoly();
            }

            if (this.GoalNode != null)
            {
                ((NavMeshPoly)this.GoalNode).RemoveConnectedPoly();    
            }
        }

        protected virtual NodeRecord GenerateChildNodeRecord(NodeRecord parent, NavigationGraphEdge connectionEdge)
        {
            var childNode = connectionEdge.ToNode;
            var childNodeRecord = new NodeRecord
            {
                node = childNode,
                parent = parent,
                gValue = parent.gValue + (childNode.LocalPosition-parent.node.LocalPosition).magnitude,
                hValue = this.Heuristic.H(childNode, this.GoalNode)
            };

            childNodeRecord.fValue = F(childNodeRecord);

            return childNodeRecord;
        }

        protected GlobalPath CalculateSolution(NodeRecord node, bool partial)
        {
            var path = new GlobalPath
            {
                IsPartial = partial,
                Length = node.gValue
            };
            var currentNode = node;

            path.PathPositions.Add(this.GoalPosition);

            //I need to remove the first Node and the last Node because they correspond to the dummy first and last Polygons that were created by the initialization.
            //And we don't want to be forced to go to the center of the initial polygon before starting to move towards my destination.

            //skip the last node, but only if the solution is not partial (if the solution is partial, the last node does not correspond to the dummy goal polygon)
            if (!partial && currentNode.parent != null)
            {
                currentNode = currentNode.parent;
            }
            
            while (currentNode.parent != null)
            {
                path.PathNodes.Add(currentNode.node); //we need to reverse the list because this operator add elements to the end of the list
                path.PathPositions.Add(currentNode.node.LocalPosition);

                if (currentNode.parent.parent == null) break; //this skips the first node
                currentNode = currentNode.parent;
            }

            path.PathNodes.Reverse();
            path.PathPositions.Reverse();
            return path;

        }

        public static float F(NodeRecord node)
        {
            return F(node.gValue,node.hValue);
        }

        public static float F(float g, float h)
        {
            return g + h;
        }

    }
}
