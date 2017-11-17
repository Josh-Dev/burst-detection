using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BurstDetection.BurstLogic
{
    /// <summary>
    /// Represents an event.
    /// </summary>
    public class EntityEvent
    {

        public long Start { get; private set; }

        public long End { get; private set; }

        public DateTimeOffset StartAsDate { get; private set; }
        public DateTimeOffset EndAsDate { get; private set; }

        public bool HasEnded { get; private set; }

        public int ClusterCount { get { return _Clusters.Count(); } private set { } }

        private List<EntityCluster> _Clusters { get; set; }

        private IDictionary<string,bool> _Entities { get; set; }

        public EntityEvent(string entity, long startTime)
        {
            Start = startTime;
            StartAsDate = DateTimeOffset.FromUnixTimeMilliseconds(Start);
            _Clusters = new List<EntityCluster>();
            _Entities = new Dictionary<string, bool>();

            AddEntity(entity);
        }

        public bool ContainsEntity(string entity)
        {
            if (!_Entities.ContainsKey(entity))
                return false;
            return _Entities[entity];
        }

        public void AddEntity(string entity)
        {
            _Entities.Add(entity, true);
        }

        public void StopEntityBursting(string entity, long time)
        {
            _Entities[entity] = false;

            if (!_Entities.Where(m => m.Value == true).Any())
            {
                End = time;
                HasEnded = true;
                EndAsDate = DateTimeOffset.FromUnixTimeMilliseconds(End);
            }

        }

        public bool ContainsCluster(EntityCluster cluster)
        {
            return _Clusters.Where(m => m.ClusterID == cluster.ClusterID).Any();
        }

        public void AddCluster(EntityCluster cluster)
        {
            _Clusters.Add(cluster);
        }


        public override string ToString()
        {
            return $"Entities: {_Entities.Select(x => x.Key).FirstOrDefault() + "..."?? "--"}," 
                + $" Clusters:{_Clusters.Count()}, Ended: {HasEnded}" + ((HasEnded)? $" ,Start: {StartAsDate.ToString("MM/dd/yy H:mm:ss")} End:{EndAsDate.ToString("MM/dd/yy H:mm:ss")}" : "");
        }


    }
}
