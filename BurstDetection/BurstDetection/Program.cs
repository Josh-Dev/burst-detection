using BurstDetection.BurstLogic;
using System;
using System.IO;

namespace BurstDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a burst detector
            SimpleBurstDetect _detector = new SimpleBurstDetect();

            //Import the data
            var reader = new StreamReader(@"C:\Users\Josh\Documents\Uni\WebScience\coursework\1day\clusters.sortedby.time.csv");

            //Cut up the CSV and shovel the tweets into the burst detector
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                var tweet = new Tweet()
                {
                    ClusterID = Int32.Parse(values[0]),
                    ClusterNamedEntity = values[1],
                    TweetID = long.Parse(values[2]),
                    UnixTimeStamp = long.Parse(values[3]),
                    UserID = Int32.Parse(values[4]),
                    TweetTokens = values[5].Split(" "),
                    TweetText = values[6]
                };

                _detector.Process(tweet);

            }

            _detector.Test();

            var t = 3;

        }
    }
}
