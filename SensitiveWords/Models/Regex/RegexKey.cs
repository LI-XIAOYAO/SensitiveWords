using System;
using System.Text.RegularExpressions;

namespace SensitiveWords
{
    /// <summary>
    /// RegexKey
    /// </summary>
    internal readonly struct RegexKey : IEquatable<RegexKey>
    {
        /// <summary>
        /// SensitiveWordsOptions
        /// </summary>
        public SensitiveWordsOptions SensitiveWordsOptions { get; }

        /// <summary>
        /// Pattern
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Options
        /// </summary>
        public RegexOptions Options { get; }

        /// <summary>
        /// MatchTimeout
        /// </summary>
        public TimeSpan MatchTimeout { get; }

        /// <summary>
        /// RegexKey
        /// </summary>
        /// <param name="sensitiveWordsOptions"></param>
        /// <param name="pattern"></param>
        /// <param name="options"></param>
        /// <param name="matchTimeout"></param>
        public RegexKey(SensitiveWordsOptions sensitiveWordsOptions, string pattern, RegexOptions options, TimeSpan matchTimeout)
        {
            SensitiveWordsOptions = sensitiveWordsOptions;
            Pattern = pattern;
            Options = options;
            MatchTimeout = matchTimeout;
        }

        public override bool Equals(object obj) => obj is RegexKey other && Equals(other);

        public bool Equals(RegexKey other) => SensitiveWordsOptions.Equals(other.SensitiveWordsOptions) && Pattern.Equals(other.Pattern) && Options == other.Options && MatchTimeout == other.MatchTimeout;

        public override int GetHashCode() => SensitiveWordsOptions.GetHashCode() ^ Pattern.GetHashCode();
    }
}