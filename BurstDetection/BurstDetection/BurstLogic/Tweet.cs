using System;
using System.Collections.Generic;
using System.Text;

namespace BurstDetection.BurstLogic
{
    /// <summary>
    /// Represents a tweet that has been shoved into a cluster.
    /// </summary>
    public class Tweet
    {
        public long TweetID { get; set; }

        public int ClusterID { get; set; }

        public string ClusterNamedEntity { get; set; }

        public long UnixTimeStamp { get; set; }

        //public DateTimeOffset TimeStampAsDate { get; set; }

        public int UserID { get; set; }

        public string[] TweetTokens { get; set; }

        public string TweetText { get; set; }

        public string [] Entities { get; set; }

    }
}
