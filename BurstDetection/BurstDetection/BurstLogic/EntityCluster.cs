using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BurstDetection.BurstLogic
{

    /// <summary>
    /// Represents a cluster of tweets.
    /// </summary>
    public class EntityCluster
    {
        private readonly long UNIX_360_MINUTES = 360 * 60 * 1000;

        public EntityCluster(int clusterID, string namedEntity)
        {
            ClusterID = ClusterID;
            ClusterNamedEntity = namedEntity;
            ClusterTweets = new List<Tweet>();
        }

        public int ClusterID { get; set; }

        public int Count { get { return ClusterTweets.Count(); } private set { } }

        public String ClusterNamedEntity { get; set; }

        private List<Tweet> ClusterTweets { get; set; }

        public long GetCentroidTime(long currentUnixTime)
        {
            return (long)ClusterTweets.Where(x=>currentUnixTime - UNIX_360_MINUTES<= x.UnixTimeStamp && x.UnixTimeStamp<=currentUnixTime).Select(x => x.UnixTimeStamp).Average();
        }

        public void AddTweet(Tweet tweet)
        {
            ClusterTweets.Add(tweet);
        }

        public List<Tweet> GetTweets()
        {
            return ClusterTweets;
        }

        public override string ToString()
        {
            return $"Cluster: {ClusterID}. Entity: {ClusterNamedEntity}. Tweets: {Count}";
        }
    }
}
