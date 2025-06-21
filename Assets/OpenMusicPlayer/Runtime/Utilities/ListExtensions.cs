using System.Collections.Generic;

namespace OpenMusicPlayer.Utilities
{
    public static class ListExtensions
    {
        private static readonly System.Random Rng = new(); // Initialize a Random instance

        public static void Shuffle<T>(this IList<T> list) 
        {
            // Fisher-Yates (or Knuth) shuffle algorithm
            var n = list.Count;
            while (n > 1) 
            {
                n--;
                var k = Rng.Next(n + 1); 
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}