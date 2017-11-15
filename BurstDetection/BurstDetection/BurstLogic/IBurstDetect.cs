using System;
using System.Collections.Generic;
using System.Text;

namespace BurstDetection.BurstLogic
{
    /**
     * A burst detection method is provided here.
     * 
     * Tweets are processed and the named entity's sliding window is updated.
     * 
     * */

    /// <summary>
    /// Provides a burst detection method.
    /// </summary>
    public interface IBurstDetect
    {
        /// <summary>
        /// Processes a tweet and checks for a potential event.
        /// </summary>
        /// <param name="tweet">The tweet to process.</param>
        void Process(Tweet tweet);

    }
}
