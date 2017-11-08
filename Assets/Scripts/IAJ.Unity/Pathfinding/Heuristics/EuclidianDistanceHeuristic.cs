using RAIN.Navigation.Graph;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{

    public class EuclidianDistanceHeuristic : IHeuristic
    {

        public float H(NavigationGraphNode node, NavigationGraphNode goalNode)
        {
            return (node.Position - goalNode.Position).magnitude;
        }
    }
}
