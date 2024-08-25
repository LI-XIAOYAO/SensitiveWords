using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SensitiveWords
{
    /// <summary>
    /// 匹配结果
    /// </summary>
    public class MatchResult
    {
        internal MatchResult(SensitiveWordsOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// 敏感词选项
        /// </summary>
        public SensitiveWordsOptions Options { get; }

        /// <summary>
        /// 匹配上的普通敏感词
        /// </summary>
        public NodeCaptures NodeCaptures { get; internal set; }

        /// <summary>
        /// 匹配上的正则敏感词
        /// </summary>
        public IReadOnlyList<Match> RegexMatches { get; internal set; }

        /// <summary>
        /// 是否匹配上敏感词
        /// </summary>
        public bool IsMatch => NodeCaptures.Count > 0 || RegexMatches.Count > 0;
    }
}