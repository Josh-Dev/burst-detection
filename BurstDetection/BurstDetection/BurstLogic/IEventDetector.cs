using System;
using System.Collections.Generic;
using System.Text;

namespace BurstDetection.BurstLogic
{
 
   /// <summary>
    /// Provides an event detection method.
    /// </summary>
    public interface IEventDetector
    {
        /// <summary>
        /// Processes a tweet and checks for a potential event.
        /// </summary>
        /// <param name="tweet">The tweet to process.</param>
        void Process(Tweet tweet);

    }
}
