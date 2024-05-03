using System.Collections.Generic;
using System.Linq;

namespace SensitiveWords
{
    /// <summary>
    /// 匹配结果集合
    /// </summary>
    public class MatchResultCollection : InternalReadOnlyCollection<MatchResult>
    {
        /// <summary>
        /// 匹配结果集合
        /// </summary>
        protected internal MatchResultCollection()
        {
        }

        /// <summary>
        /// 匹配结果集合
        /// </summary>
        /// <param name="list"></param>
        protected internal MatchResultCollection(IList<MatchResult> list)
            : base(list)
        {
        }

        /// <summary>
        /// 是否匹配上敏感词
        /// </summary>
        public bool IsMatch => this.Any(c => c.IsMatch);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{nameof(Count)} = {Count}, {nameof(IsMatch)} = {IsMatch}";
    }
}