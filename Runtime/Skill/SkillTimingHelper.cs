using System.Collections.Generic;

namespace MirrorRPG.Skill
{
    /// <summary>
    /// Helper utilities for skill hit timing queries.
    /// </summary>
    public static class SkillTimingHelper
    {
        /// <summary>
        /// Get indices of hit timings that are currently active
        /// </summary>
        public static List<int> GetActiveHitTimingIndices(IReadOnlyList<SkillHitTiming> timings, float currentTime)
        {
            var indices = new List<int>();
            for (int i = 0; i < timings.Count; i++)
            {
                if (timings[i].IsActive(currentTime))
                {
                    indices.Add(i);
                }
            }
            return indices;
        }

        /// <summary>
        /// Get indices of hit timings that just started between previousTime and currentTime
        /// </summary>
        public static List<int> GetStartingHitTimingIndices(IReadOnlyList<SkillHitTiming> timings, float previousTime, float currentTime)
        {
            var indices = new List<int>();
            for (int i = 0; i < timings.Count; i++)
            {
                if (timings[i].JustStarted(previousTime, currentTime))
                {
                    indices.Add(i);
                }
            }
            return indices;
        }

        /// <summary>
        /// Get indices of hit timings that just ended between previousTime and currentTime
        /// </summary>
        public static List<int> GetEndingHitTimingIndices(IReadOnlyList<SkillHitTiming> timings, float previousTime, float currentTime)
        {
            var indices = new List<int>();
            for (int i = 0; i < timings.Count; i++)
            {
                if (timings[i].JustEnded(previousTime, currentTime))
                {
                    indices.Add(i);
                }
            }
            return indices;
        }

        /// <summary>
        /// Get all hit timings that are currently active
        /// </summary>
        public static List<SkillHitTiming> GetActiveHitTimings(IReadOnlyList<SkillHitTiming> timings, float currentTime)
        {
            var result = new List<SkillHitTiming>();
            foreach (var timing in timings)
            {
                if (timing.IsActive(currentTime))
                {
                    result.Add(timing);
                }
            }
            return result;
        }

        /// <summary>
        /// Check if any hit timing is currently active
        /// </summary>
        public static bool HasActiveHitTiming(IReadOnlyList<SkillHitTiming> timings, float currentTime)
        {
            foreach (var timing in timings)
            {
                if (timing.IsActive(currentTime))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
