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

        public int TweetCount { get { return _Clusters.Select(i => i.Count).Sum(); } private set { } }

        private List<EntityCluster> _Clusters { get; set; }

        private IDictionary<string, bool> _EntitiesBursting { get; set; }

        private IDictionary<string, int> _TweetEntityFrequencies { get; set; }

        public EntityEvent(string entity, long startTime)
        {
            Start = startTime;
            StartAsDate = DateTimeOffset.FromUnixTimeMilliseconds(Start);
            _Clusters = new List<EntityCluster>();
            _EntitiesBursting = new Dictionary<string, bool>();
            _TweetEntityFrequencies = new Dictionary<string, int>();

            AddEntity(entity);
        }

        /// <summary>
        /// Returns whether or not the given event contains the entity.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns></returns>
        public bool ContainsEntity(string entity)
        {
            if (!_EntitiesBursting.ContainsKey(entity))
                return false;
            return true;
        }

        public bool IsBurstingEntity(string entity)
        {
            if (!_EntitiesBursting.ContainsKey(entity))
                return false;
            return _EntitiesBursting[entity];
        }

        public void AddEntity(string entity)
        {
            _EntitiesBursting.Add(entity, true);
        }

        public void StopEntityBursting(string entity, long time)
        {
            _EntitiesBursting[entity] = false;

            if (!_EntitiesBursting.Where(m => m.Value == true).Any())
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
            var entities = cluster.GetTweets().Select(x => x.Entities).Distinct();

            //TODO: O(n^2)
            foreach (var entityArray in entities)
            {
                foreach (var entity in entityArray)
                {
                    if (!_TweetEntityFrequencies.ContainsKey(entity))
                    {
                        _TweetEntityFrequencies.Add(entity, 0);
                    }
                }

            }

        }

        public void MergeEvent(EntityEvent ev)
        {
            Start = Math.Max(Start, ev.Start);
            StartAsDate = DateTimeOffset.FromUnixTimeMilliseconds(Start);

            foreach (var clust in ev.GetClusters())
            {
                if (!ContainsCluster(clust))
                    AddCluster(clust);
            }

            foreach(var ent in ev.GetEntities())
            {
                if (ev.IsBurstingEntity(ent) && !ContainsEntity(ent))
                    AddEntity(ent);
            }

        }

        public IEnumerable<EntityCluster> GetClusters()
        {
            return _Clusters;
        }

        private void UpdateFrequencies()
        {
            foreach (var entity in _TweetEntityFrequencies.Keys.ToList())
            {
                var count = 0;

                foreach (var cluster in _Clusters)
                {
                    count += cluster.GetTweets().Where(x => x.Entities.Contains(entity)).Count();
                }

                _TweetEntityFrequencies[entity] = count;
            }
        }

        public string[] GetEntities()
        {
            return _EntitiesBursting.Keys.ToArray();
        }

        public string[] GetTweetEntitiesWithFrequency(double frequencyPercentage)
        {
            UpdateFrequencies();
            return _TweetEntityFrequencies.Where(i => i.Value >= (TweetCount * frequencyPercentage) && !_EntitiesBursting.ContainsKey(i.Key)).Select(m => m.Key).ToArray();
        }


        public override string ToString()
        {
            return $"Entities: {String.Join(",", _EntitiesBursting.Select(x => x.Key)) ?? "--"},"
                + $" Clusters:{_Clusters.Count()}, Ended: {HasEnded}" + ((HasEnded) ? $" ,Start: {StartAsDate.ToString("MM/dd/yy H:mm:ss")} End:{EndAsDate.ToString("MM/dd/yy H:mm:ss")}" : "");
        }


    }
}
