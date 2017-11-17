using System;
using System.Collections.Generic;
using System.Linq;

namespace BurstDetection.BurstLogic
{
    /// <summary>
    /// Represents a set of sliding windows for an entity and performs calculations to maintain it.
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
        private List<EntityEvent> EntityEvents { get; set; }

        public string EntityName { get; set; }
        private double[] StandardDeviations { get; set; }
        private double[] Means { get; set; }
        private int[][] Frequencies { get; set; }
        private long[] NextUpdateTime { get; set; }
        private bool[] IsBursting { get; set; }
        private long[] BurstEndTime { get; set; }

        public bool IsCurrentlyBursting { get; set; }

        public override string ToString()
        {
            return $"{EntityName}, Bursting: {IsCurrentlyBursting}";
        }


        public SlidingWindowCollection(string entityName, long currentUnixTime)
        {
            EntityName = entityName;
            EntityTweets = new List<Tweet>();
            EntityEvents = new List<EntityEvent>();
            StandardDeviations = new double[7];
            Means = new double[7];
            IsBursting = new bool[7];
            BurstEndTime = new long[7];
            Frequencies = new int[7][] { new int[7], new int[7], new int[7], new int[7], new int[7], new int[7], new int[7] };
            NextUpdateTime = new long[7] { currentUnixTime + FIVE_MINUTE_INDEX, currentUnixTime + TEN_MINUTE_INDEX, currentUnixTime + TWENTY_MINUTE_INDEX,
            currentUnixTime + FOURTY_MINUTE_INDEX, currentUnixTime + EIGHTY_MINUTE_INDEX, currentUnixTime + ONE_SIXTY_MINUTE_INDEX, currentUnixTime + THREE_SIXTY_MINUTE_INDEX};
        }

        public EntityEvent AddTweet(long currentUnixTime, Tweet tweet)
        {
            if (tweet.ClusterNamedEntity == "netflix")
            {
                /*Console.WriteLine("Netflix Tweet detected");
                Console.WriteLine($"5 Minute window.\t\tFreq:{Frequencies[FIVE_MINUTE_INDEX][0]}\tAvg:{Means[FIVE_MINUTE_INDEX]}\tStd:{StandardDeviations[FIVE_MINUTE_INDEX]}\tBurst:{IsBursting[FIVE_MINUTE_INDEX]}");
                Console.WriteLine($"10 Minute window.\t\tFreq:{Frequencies[TEN_MINUTE_INDEX][0]}\tAvg:{Means[TEN_MINUTE_INDEX]}\tStd:{StandardDeviations[TEN_MINUTE_INDEX]}\tBurst:{IsBursting[TEN_MINUTE_INDEX]}");
                Console.WriteLine($"20 Minute window.\t\tFreq:{Frequencies[TWENTY_MINUTE_INDEX][0]}\tAvg:{Means[TWENTY_MINUTE_INDEX]}\tStd:{StandardDeviations[TWENTY_MINUTE_INDEX]}\tBurst:{IsBursting[TWENTY_MINUTE_INDEX]}");
                Console.WriteLine($"40 Minute window.\t\tFreq:{Frequencies[FOURTY_MINUTE_INDEX][0]}\tAvg:{Means[FOURTY_MINUTE_INDEX]}\tStd:{StandardDeviations[FOURTY_MINUTE_INDEX]}\tBurst:{IsBursting[FOURTY_MINUTE_INDEX]}");
                Console.WriteLine($"80 Minute window.\t\tFreq:{Frequencies[EIGHTY_MINUTE_INDEX][0]}\tAvg:{Means[EIGHTY_MINUTE_INDEX]}\tStd:{StandardDeviations[EIGHTY_MINUTE_INDEX]}\tBurst:{IsBursting[EIGHTY_MINUTE_INDEX]}");
                Console.WriteLine($"160 Minute window.\t\tFreq:{Frequencies[ONE_SIXTY_MINUTE_INDEX][0]}\tAvg:{Means[ONE_SIXTY_MINUTE_INDEX]}\tStd:{StandardDeviations[ONE_SIXTY_MINUTE_INDEX]}\tBurst:{IsBursting[ONE_SIXTY_MINUTE_INDEX]}");
                Console.WriteLine($"360 Minute window.\t\tFreq:{Frequencies[THREE_SIXTY_MINUTE_INDEX][0]}\tAvg:{Means[THREE_SIXTY_MINUTE_INDEX]}\tStd:{StandardDeviations[THREE_SIXTY_MINUTE_INDEX]}\tBurst:{IsBursting[THREE_SIXTY_MINUTE_INDEX]}");*/

            }
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
                CalculateWindow(currentUnixTime, THREE_SIXTY_MINUTE_INDEX, UNIX_360_MINUTES);

            if (!IsCurrentlyBursting)
            {
                if (IsBursting.Where(x => x == true).Any())
                    IsCurrentlyBursting = true;

                if (IsCurrentlyBursting)
                {
                    //Create new event
                    //EntityEvents.Add(new EntityEvent() { Start = currentUnixTime, EventNamedEntity = tweet.ClusterNamedEntity });

                    return new EntityEvent(tweet.ClusterNamedEntity, currentUnixTime);
                    //Console.WriteLine($"DET: Event detected: {tweet.ClusterNamedEntity}. Event: {EntityEvents.Count()}, Start: {currentUnixTime}");

                }

            }
            else if (!IsBursting.Where(x => x == true).Any())
            {
                IsCurrentlyBursting = false;
            }

            return null;

        }

        private void CalculateWindow(long currentUnixTime, int windowIndex, long windowLength)
        {
            //5 minute window
            var lastSeven = EntityTweets.Where(m => currentUnixTime - m.UnixTimeStamp < (windowLength * NUMBER_OF_PAST_WINDOWS));
            var frequencies = lastSeven.GroupBy(i => (int)((currentUnixTime - i.UnixTimeStamp) / windowLength)).Select(i => new { Window = i.Key, Freq = i.Count() });

            Frequencies[windowIndex] = new int[] { 0, 0, 0, 0, 0, 0, 0 };

            foreach (var freq in frequencies)
            {
                Frequencies[windowIndex][freq.Window] = freq.Freq;
            }

            //Calculate mean
            var mean = Frequencies[windowIndex].Average();

            var curr = Frequencies[windowIndex][0];
            var threeSigma = Means[windowIndex] + (3 * StandardDeviations[windowIndex]);

            //Calculate standard deviation
            if (!IsBursting[windowIndex])
            {
                var squaredSum = Frequencies[windowIndex].Sum(i => (i - mean) * (i - mean));

                var standardDev = Math.Sqrt(squaredSum / 7);

                StandardDeviations[windowIndex] = standardDev;
                Means[windowIndex] = mean;
            }

            //Burst Detection
            if (curr > threeSigma)
            {
                IsBursting[windowIndex] = true;
                BurstEndTime[windowIndex] = (long)(currentUnixTime + (windowLength * 1.5));
            }
            else if (currentUnixTime < BurstEndTime[windowIndex])
                IsBursting[windowIndex] = true;
            else
            {
                IsBursting[windowIndex] = false;
            }

            NextUpdateTime[windowIndex] = currentUnixTime + windowLength;

        }


    }


}
