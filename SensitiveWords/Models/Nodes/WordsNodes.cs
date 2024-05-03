using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SensitiveWords
{
    /// <summary>
    /// 字符节点集
    /// </summary>
    public class WordsNodes : InternalReadOnlyDictionary<char, WordsNode>
    {
        /// <summary>
        /// 字符节点集
        /// </summary>
        /// <param name="ignoreCase"></param>
        protected internal WordsNodes(bool ignoreCase)
            : base(ignoreCase ? new CharIgnoreCaseComparer() : null)
        {
            IgnoreCase = ignoreCase;
        }

        /// <summary>
        /// 是否忽略大小写
        /// </summary>
        public bool IgnoreCase { get; }

        /// <summary>
        /// 有效词组数
        /// </summary>
        public int WordsCount => Values.Sum(c => c.WordsCount);

        /// <summary>
        /// 是否存在节点
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsNode(char value) => ContainsKey(value);

        /// <summary>
        /// 构建词组节点集
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static WordsNodes Build(IEnumerable<string> list, bool ignoreCase = true)
        {
            if (null == list)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var root = new WordsNodes(ignoreCase);
            foreach (var words in list)
            {
                var currentNodes = root;
                for (int i = 0; i < words.Length; i++)
                {
                    var current = words[i];
                    var wordsNode = (0 == i ? root : currentNodes)[current];

                    if (null == wordsNode)
                    {
                        currentNodes.Add(current, wordsNode = WordsNode.Create(current, i == words.Length - 1, ignoreCase));
                    }

                    wordsNode.IsEnd |= i == words.Length - 1;
                    currentNodes = wordsNode.Nodes;
                }
            }

            return root;
        }

        /// <summary>
        /// 匹配字符串
        /// </summary>
        /// <param name="text"></param>
        /// <param name="isMaxMatch"></param>
        /// <param name="whiteSpaceOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public NodeCaptures Matches(string text, bool isMaxMatch = true, WhiteSpaceOptions whiteSpaceOptions = WhiteSpaceOptions.Default, CancellationToken cancellationToken = default) => new NodeCaptures(text, this, isMaxMatch, whiteSpaceOptions, cancellationToken);

        /// <summary>
        /// 替换为指定字符串
        /// </summary>
        /// <param name="text"></param>
        /// <param name="replacement"></param>
        /// <param name="isMaxMatch"></param>
        /// <param name="whiteSpaceOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public string Replace(string text, string replacement, bool isMaxMatch = true, WhiteSpaceOptions whiteSpaceOptions = WhiteSpaceOptions.Default, CancellationToken cancellationToken = default) => Replace(text, c => replacement, isMaxMatch, whiteSpaceOptions, cancellationToken);

        /// <summary>
        /// 替换
        /// </summary>
        /// <param name="text"></param>
        /// <param name="matchEvaluator"></param>
        /// <param name="isMaxMatch"></param>
        /// <param name="whiteSpaceOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public string Replace(string text, Func<NodeCapture, string> matchEvaluator, bool isMaxMatch = true, WhiteSpaceOptions whiteSpaceOptions = WhiteSpaceOptions.Default, CancellationToken cancellationToken = default)
        {
            if (null == matchEvaluator)
            {
                return text;
            }

            var nodeCaptures = Matches(text, isMaxMatch, whiteSpaceOptions, cancellationToken);
            if (0 == nodeCaptures.Count)
            {
                return text;
            }

            return nodeCaptures.Replace(matchEvaluator, cancellationToken);
        }

        /// <summary>
        /// 获取有效词组
        /// </summary>
        /// <returns></returns>
        public List<string> GetWords(CancellationToken cancellationToken = default)
        {
            var list = new List<string>();
            void GetWords(WordsNodes wordsNodes, StringBuilder stringBuilder = null)
            {
                foreach (var item in wordsNodes.Values)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var nodeStringBuilder = wordsNodes.Count > 1 ? new StringBuilder(stringBuilder?.ToString()) : stringBuilder ?? new StringBuilder();
                    nodeStringBuilder.Append(item.Value);

                    if (item.IsEnd)
                    {
                        list.Add(nodeStringBuilder.ToString());
                    }

                    if (item.Nodes.Count > 0)
                    {
                        GetWords(item.Nodes, nodeStringBuilder);
                    }
                }
            }

            GetWords(this);

            return list;
        }

        private class CharIgnoreCaseComparer : IEqualityComparer<char>
        {
            public bool Equals(char x, char y)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(x.ToString(), y.ToString());
            }

            public int GetHashCode(char obj)
            {
                return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.ToString());
            }
        }
    }
}