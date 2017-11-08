using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    //hashtable implementation of the closed sets
    public class HashtableSet : IClosedSet
    {
        private HashSet<NodeRecord> NodeRecords { get; set; }

        public HashtableSet()
        {
            this.NodeRecords = new HashSet<NodeRecord>();
        }

        public void Initialize()
        {
            this.NodeRecords.Clear();
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            this.NodeRecords.Add(nodeRecord);
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            this.NodeRecords.Remove(nodeRecord);
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            //here I cannot use the == comparer because the nodeRecord will likely be a different computational object
            //and therefore pointer comparison will not work, we need to use Equals
            //LINQ with a lambda expression
            return this.NodeRecords.FirstOrDefault(n => n.Equals(nodeRecord));
        }

        public ICollection<NodeRecord> All()
        {
            return this.NodeRecords;
        }

    }
}