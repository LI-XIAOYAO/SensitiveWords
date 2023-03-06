namespace SensitiveWords
{
    /// <summary>
    /// 节点捕获
    /// </summary>
    public class NodeCapture
    {
        private NodeCaptures _captures;

        internal NodeCapture(int index, int length)
        {
            Index = index;
            Length = length;
        }

        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 捕获长度
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 捕获值
        /// </summary>
        public string Value { get; internal set; }

        /// <summary>
        /// 节点捕获集
        /// </summary>
        public NodeCaptures Captures
        {
            get => _captures;
            internal set
            {
                Value = (_captures = value).Value?.Substring(Index, Length);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{nameof(Index)} = {Index}, {nameof(Length)} = {Length}, {nameof(Value)} = {Value}";
    }
}