using System.Linq;

namespace SensitiveWords
{
    /// <summary>
    /// 字符节点
    /// </summary>
    public class WordsNode
    {
        /// <summary>
        /// 字符节点
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isEnd"></param>
        /// <param name="ignoreCase"></param>
        public WordsNode(char value, bool isEnd = false, bool ignoreCase = false)
        {
            Value = value;
            IsEnd = isEnd;
            IgnoreCase = ignoreCase;
            Nodes = new WordsNodes(IgnoreCase);
        }

        /// <summary>
        /// 是否忽略大小写
        /// </summary>
        public bool IgnoreCase { get; }

        /// <summary>
        /// 字符值
        /// </summary>
        public char Value { get; }

        /// <summary>
        /// 是否是结束节点
        /// </summary>
        public bool IsEnd { get; internal set; }

        /// <summary>
        /// 子节点
        /// </summary>
        public WordsNodes Nodes { get; }

        /// <summary>
        /// 有效词组数
        /// </summary>
        public int WordsCount => IsEnd.GetHashCode() + Nodes.Values.Sum(c => c.WordsCount);

        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="value"></param>
        /// <param name="isEnd"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        public static WordsNode Create(char value, bool isEnd = false, bool ignoreCase = false) => new WordsNode(value, isEnd, ignoreCase);

        /// <summary>
        /// 是否存在节点
        /// </summary>
        /// <returns></returns>
        public bool HasNodes() => Nodes.Count > 0;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{nameof(Value)} = {Value}, {nameof(IsEnd)} = {IsEnd}, {nameof(Nodes)} = {Nodes.Count}";
    }
}