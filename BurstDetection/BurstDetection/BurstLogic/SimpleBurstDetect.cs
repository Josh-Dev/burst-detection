using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BurstDetection.BurstLogic
{
    /**
     * My implementation of sliding window. Every X minutes 
     * 
     * */
    public class SimpleBurstDetect : IBurstDetect
    {
        private readonly IDictionary<string, SlidingWindowCollection> _EntityWindows;
        private long CurrentUnixTime;

        public SimpleBurstDetect()
        {
            _EntityWindows = new Dictionary<string, SlidingWindowCollection>();
        }

        public void Process(Tweet tweet)
        {
            CurrentUnixTime = tweet.UnixTimeStamp;

            //Get windows associated with given entity
            if (!_EntityWindows.ContainsKey(tweet.ClusterNamedEntity))
                _EntityWindows.Add(tweet.ClusterNamedEntity, new SlidingWindowCollection(tweet.ClusterNamedEntity,CurrentUnixTime));

            var windows = _EntityWindows[tweet.ClusterNamedEntity];

            //Add tweet
            windows.AddTweet(CurrentUnixTime, tweet);

            //Check if it's time for an update on each window

        }

    }

    /// <summary>
    /// Represents a sliding window for an entity and performs calculations to maintain it.
    /// </summary>
    public class SlidingWindowCollection
    {
        private readonly int NUMBER_OF_PAST_WINDOWS = 7;
        private readonly long UNIX_5_MINUTES = 5 * 60 * 1000;
        private readonly long UNIX_10_MINUTES = 10 * 60 * 1000;
        private readonly long UNIX_20_MINUTES = 20 * 60 * 1000;
        private readonly long UNIX_40_MINUTES = 40 * 60 * 1000;
        private readonly long UNIX_80_MINUTES = 80 * 60 * 1000;
        private readonly long UNIX_160_MINUTES = 160 * 60 * 1000;
        private readonly long UNIX_360_MINUTES = 360 * 60 * 1000;

        private readonly int FIVE_MINUTE_INDEX = 0;
        private readonly int TEN_MINUTE_INDEX = 1;
        private readonly int TWENTY_MINUTE_INDEX = 2;
        private readonly int FOURTY_MINUTE_INDEX = 3;
        private readonly int EIGHTY_MINUTE_INDEX = 4;
        private readonly int ONE_SIXTY_MINUTE_INDEX = 5;
        private readonly int THREE_SIXTY_MINUTE_INDEX = 6;

        private List<Tweet> EntityTweets { get; set; }

        public string EntityName { get; set; }
        public double[] StandardDeviations { get; set; }
        public double[] Means { get; set; }
        public int[][] Frequencies { get; set; }
        public long[] NextUpdateTime { get; set; }

        public override string ToString()
        {
            return $"{EntityName}";
        }


        public SlidingWindowCollection(string entityName, long currentUnixTime)
        {
            EntityName = entityName;
            EntityTweets = new List<Tweet>();
            StandardDeviations = new double[7];
            Means = new double[7];
            Frequencies = new int[7][] { new int[7], new int[7], new int[7], new int[7], new int[7], new int[7], new int[7] };
            NextUpdateTime = new long[7] { currentUnixTime + FIVE_MINUTE_INDEX, currentUnixTime + TEN_MINUTE_INDEX, currentUnixTime + TWENTY_MINUTE_INDEX,
            currentUnixTime + FOURTY_MINUTE_INDEX, currentUnixTime + EIGHTY_MINUTE_INDEX, currentUnixTime + ONE_SIXTY_MINUTE_INDEX, currentUnixTime + THREE_SIXTY_MINUTE_INDEX};
        }

        public void AddTweet(long currentUnixTime, Tweet tweet)
        {
            EntityTweets.Add(tweet);
            if (currentUnixTime - NextUpdateTime[FIVE_MINUTE_INDEX] >= UNIX_5_MINUTES)
                CalculateWindow(currentUnixTime, FIVE_MINUTE_INDEX, UNIX_5_MINUTES);
            if (currentUnixTime - NextUpdateTime[TEN_MINUTE_INDEX] >= UNIX_10_MINUTES)
                CalculateWindow(currentUnixTime, TEN_MINUTE_INDEX, UNIX_10_MINUTES);
            if (currentUnixTime - NextUpdateTime[TWENTY_MINUTE_INDEX] >= UNIX_20_MINUTES)
                CalculateWindow(currentUnixTime, TWENTY_MINUTE_INDEX, UNIX_20_MINUTES);
            if (currentUnixTime - NextUpdateTime[FOURTY_MINUTE_INDEX] >= UNIX_40_MINUTES)
                CalculateWindow(currentUnixTime, FOURTY_MINUTE_INDEX, UNIX_40_MINUTES);
            if (currentUnixTime - NextUpdateTime[EIGHTY_MINUTE_INDEX] >= UNIX_80_MINUTES)
                CalculateWindow(currentUnixTime, EIGHTY_MINUTE_INDEX, UNIX_80_MINUTES);
            if (currentUnixTime - NextUpdateTime[ONE_SIXTY_MINUTE_INDEX] >= UNIX_160_MINUTES)
                CalculateWindow(currentUnixTime, ONE_SIXTY_MINUTE_INDEX, UNIX_160_MINUTES);
            if (currentUnixTime - NextUpdateTime[THREE_SIXTY_MINUTE_INDEX] >= UNIX_360_MINUTES)
                CalculateWindow(currentUnixTime, THREE_SIXTY_MINUTE_INDEX, UNIX_160_MINUTES);

        }

        private void CalculateWindow(long currentUnixTime, int windowIndex, long windowLength)
        {
            //5 minute window
            var lastSeven = EntityTweets.Where(m => currentUnixTime - m.UnixTimeStamp < (windowLength * NUMBER_OF_PAST_WINDOWS));
            var frequencies = lastSeven.GroupBy(i => (int)((currentUnixTime - i.UnixTimeStamp) / windowLength)).Select(i => new { Window = i.Key, Freq = i.Count() });

            foreach (var freq in frequencies)
            {
                Frequencies[windowIndex][freq.Window] = freq.Freq;
            }

            //Calculate mean
            var mean = Frequencies[windowIndex].Average();

            //Calculate standard deviation
            var squaredSum = Frequencies[windowIndex].Sum(i => (i - mean) * (i - mean));

            var standardDev = Math.Sqrt(squaredSum / 7);

            StandardDeviations[windowIndex] = standardDev;
            Means[windowIndex] = mean;


            NextUpdateTime[windowIndex] = currentUnixTime + windowLength;

        }


    }

    public static class BurstExtensions
    {

    }
}
