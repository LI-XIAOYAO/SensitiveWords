using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace SensitiveWords
{
    /// <summary>
    /// RegexExtension
    /// </summary>
    internal static class RegexExtension
    {
        private static readonly ConcurrentDictionary<RegexKey, Regex> _cache = new ConcurrentDictionary<RegexKey, Regex>();

        /// <summary>
        /// GetRegex
        /// </summary>
        /// <param name="regexKey"></param>
        /// <returns></returns>
        public static Regex GetRegex(this RegexKey regexKey)
        {
            return _cache.GetOrAdd(regexKey, _ => new Regex(regexKey.Pattern, regexKey.Options, regexKey.MatchTimeout));
        }

        /// <summary>
        /// AddRegex
        /// </summary>
        /// <param name="regexKey"></param>
        /// <returns></returns>
        public static bool AddRegex(this RegexKey regexKey)
        {
            return _cache.TryAdd(regexKey, new Regex(regexKey.Pattern, regexKey.Options, regexKey.MatchTimeout));
        }

        /// <summary>
        /// RemoveRegex
        /// </summary>
        /// <param name="regexKey"></param>
        /// <returns></returns>
        public static bool RemoveRegex(this RegexKey regexKey)
        {
            return _cache.TryRemove(regexKey, out var _);
        }

        /// <summary>
        /// IsMatch
        /// </summary>
        /// <param name="regexKey"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsMatch(this RegexKey regexKey, string input)
        {
            return regexKey.GetRegex().IsMatch(input);
        }

        /// <summary>
        /// Replace
        /// </summary>
        /// <param name="regexKey"></param>
        /// <param name="input"></param>
        /// <param name="matchEvaluator"></param>
        /// <returns></returns>
        public static string Replace(this RegexKey regexKey, string input, MatchEvaluator matchEvaluator)
        {
            return regexKey.GetRegex().Replace(input, matchEvaluator);
        }

        /// <summary>
        /// Replace
        /// </summary>
        /// <param name="regexKey"></param>
        /// <param name="input"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string Replace(this RegexKey regexKey, string input, string replacement)
        {
            return regexKey.GetRegex().Replace(input, replacement);
        }

        /// <summary>
        /// Matches
        /// </summary>
        /// <param name="regexKey"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static MatchCollection Matches(this RegexKey regexKey, string input)
        {
            return regexKey.GetRegex().Matches(input);
        }
    }
}