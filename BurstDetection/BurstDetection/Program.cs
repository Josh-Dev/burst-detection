using BurstDetection.BurstLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BurstDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a burst detector
            EventDetector _detector = new EventDetector();

            //Import named entities 
            IDictionary<long, string[]> namedEntities = new Dictionary<long, string[]>();

            var entReader = new StreamReader(@"C:\Users\Josh\Documents\Uni\WebScience\coursework\1day\namedentities.sortedby.time.csv");

            var isFirstLine = true;
            while (!entReader.EndOfStream)
            {

                var line = entReader.ReadLine();
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue;
                }

                var values = line.Split(',');

                var tweetID = long.Parse(values[0]);

                namedEntities.Add(tweetID, values.Skip(1).Take(values.Length - 1).ToArray());
            }

            //Import the tweet stream
            var reader = new StreamReader(@"C:\Users\Josh\Documents\Uni\WebScience\coursework\1day\clusters.sortedby.time.csv");

            //Cut up the CSV and shovel the tweets into the event detector
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
                    TweetText = values[6],
                };

                tweet.Entities = namedEntities[tweet.TweetID];

                _detector.Process(tweet);

            }

            _detector.Test();

            var t = 3;

        }
    }
}
