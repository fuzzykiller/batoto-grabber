using System.Collections.Generic;

namespace BatotoGrabber
{
    public static class DictionaryExtensions
    {
        public static IEnumerable<TValue> GetMany<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
            {
                yield return source[key];
            }
        }
    }
}