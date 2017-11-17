using System;
using System.Collections.Generic;
using System.Linq;

namespace BurstDetection.BurstLogic
{
    /**
     * We have cheated a bit here as the tweets are already clustered. We emulate the dynamic calculation of centroid time by only considering tweets before the
     * 'current time'.
     * 
     * */
    public class SimpleBurstDetect : IBurstDetect
    {
        private readonly IDictionary<string, SlidingWindowCollection> _EntityWindows;
        private readonly IDictionary<int, EntityCluster> _EntityClusters;
        private readonly List<EntityEvent> _EntityEvents;
        private readonly int MINIMUM_CLUSTER_SIZE = 10;

        private long CurrentUnixTime;

        public SimpleBurstDetect()
        {
            _EntityWindows = new Dictionary<string, SlidingWindowCollection>();
            _EntityEvents = new List<EntityEvent>();
            _EntityClusters = new Dictionary<int, EntityCluster>();
        }

        public void Process(Tweet tweet)
        {
            CurrentUnixTime = tweet.UnixTimeStamp;

            if (!_EntityWindows.ContainsKey(tweet.ClusterNamedEntity))
                _EntityWindows.Add(tweet.ClusterNamedEntity, new SlidingWindowCollection(tweet.ClusterNamedEntity, CurrentUnixTime));

            if (!_EntityClusters.ContainsKey(tweet.ClusterID))
                _EntityClusters.Add(tweet.ClusterID, new EntityCluster(tweet.ClusterID, tweet.ClusterNamedEntity));

            //Get window associated with given entity and update. Check for potential burst event.
            var windows = _EntityWindows[tweet.ClusterNamedEntity];
            var ev = windows.AddTweet(CurrentUnixTime, tweet);

            if (ev != null)
                _EntityEvents.Add(ev);

            //Get cluster associated with given tweet
            var cluster = _EntityClusters[tweet.ClusterID];
            cluster.AddTweet(tweet);

            //Check if cluster can be added to any potential event(s)
            if (cluster.Count > MINIMUM_CLUSTER_SIZE)
            {
                var evs = _EntityEvents.Where(e => e.ContainsEntity(cluster.ClusterNamedEntity) && !e.HasEnded);

                foreach (var e in evs)
                {
                    if (e != null)
                        if (cluster.GetCentroidTime(CurrentUnixTime) > e.Start && !e.ContainsCluster(cluster))
                            e.AddCluster(cluster);
                }

            }

            //Check if window has stopped bursting and update events
            if (!windows.IsCurrentlyBursting)
            {
                var evs = _EntityEvents.Where(e => e.ContainsEntity(windows.EntityName));
                foreach (var e in evs)
                {
                    e.StopEntityBursting(windows.EntityName, CurrentUnixTime);
                }
            }

        }

        public void Test()
        {
            var events = _EntityEvents.Where(x => x.ClusterCount > 0);

            foreach(var ev in events)
            {
                Console.WriteLine(ev);
            }
        }

    }

}
