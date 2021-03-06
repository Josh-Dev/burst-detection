﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BurstDetection.BurstLogic
{
    /**
     * We have cheated a bit here as the tweets are already clustered. We emulate the dynamic calculation of centroid time by only considering tweets before the
     * 'current time'.
     * 
     * */
    public class EventDetector : IEventDetector
    {
        private readonly IDictionary<string, SlidingWindowCollection> _EntityWindows;
        private readonly IDictionary<int, EntityCluster> _EntityClusters;
        private readonly List<EntityEvent> _EntityEvents;
        private readonly int MINIMUM_CLUSTER_SIZE = 10;

        private long CurrentUnixTime;

        public EventDetector()
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

                //Link checking
                foreach (var e in evs.ToList())
                {
                    var entities = e.GetTweetEntitiesWithFrequency(0.5);

                    foreach (var ent in entities)
                    {
                        if (e.ContainsEntity(ent))
                            continue;

                        var linkEvent = _EntityEvents.Where(i => i.ContainsEntity(ent) && !i.HasEnded).FirstOrDefault();
                        if (linkEvent != null)
                        {
                            //Merge?
                            e.MergeEvent(linkEvent);
                            _EntityEvents.Remove(linkEvent);
                        }
                    }

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

        public IEnumerator<EntityEvent> GetEnumerator()
        {
            foreach (var eve in _EntityEvents.Where(x => x.ClusterCount > 0).ToList())
            {
                yield return eve;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Access the potential events.
        /// </summary>
        /// <param name="index">The index of the events found.</param>
        /// <returns></returns>
        public EntityEvent this[int index]
        {
            get { return _EntityEvents.Where(x => x.ClusterCount > 0).ToList()[index]; }
            private set { }
        }


        public void Test()
        {
            var events = _EntityEvents.Where(x => x.ClusterCount > 0);

            foreach (var ev in events)
            {
                Console.WriteLine(ev);
            }
        }
    }

}
