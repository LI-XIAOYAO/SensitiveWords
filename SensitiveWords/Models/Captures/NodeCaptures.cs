using System;
using System.Collections.Generic;

namespace SensitiveWords
{
    /// <summary>
    /// 节点捕获集
    /// </summary>
    public class NodeCaptures : InternalReadOnlyCollection<NodeCapture>
    {
        internal NodeCaptures(string value, WordsNodes wordsNodes)
        {
            Value = value;
            _wordsNodes = wordsNodes;
        }

        /// <summary>
        /// 节点捕获集
        /// </summary>
        public IReadOnlyCollection<NodeCapture> Captures => (IReadOnlyCollection<NodeCapture>)Items;

        private readonly WordsNodes _wordsNodes;

        /// <summary>
        /// 单词节点集
        /// </summary>
        public IReadOnlyCollection<WordsNode> WordsNodes => _wordsNodes;

        /// <summary>
        /// 捕获值
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="nodeCapture"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected internal override void Add(NodeCapture nodeCapture)
        {
            if (null == nodeCapture)
            {
                throw new ArgumentNullException(nameof(nodeCapture));
            }

            nodeCapture.Captures = this;

            base.Add(nodeCapture);
        }

        /// <summary>
        /// 替换
        /// </summary>
        /// <param name="matchEvaluator"></param>
        /// <returns></returns>
        public string Replace(Func<NodeCapture, string> matchEvaluator)
        {
            if (null == matchEvaluator || 0 == Items.Count)
            {
                return Value;
            }

            var offset = 0;
            var text = Value;
            foreach (var nodeCapture in Items)
            {
                var replacement = matchEvaluator(nodeCapture) ?? string.Empty;
                if (replacement != nodeCapture.Value)
                {
                    text = text.Replace(nodeCapture.Index + offset, nodeCapture.Length, replacement);

                    offset += replacement.Length - nodeCapture.Length;
                }
            }

            return text;
        }

        /// <summary>
        /// 替换为指定字符串
        /// </summary>
        /// <param name="replacement"></param>
        /// <param name="isMaxMatch"></param>
        /// <returns></returns>
        public string Replace(string replacement) => Replace(c => replacement);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{nameof(Count)} = {Count}, {nameof(WordsNodes)} = {WordsNodes.Count}";
    }
}