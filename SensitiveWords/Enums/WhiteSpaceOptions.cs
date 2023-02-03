namespace SensitiveWords
{
    /// <summary>
    /// 空白字符选项
    /// </summary>
    public enum WhiteSpaceOptions
    {
        /// <summary>
        /// 默认不处理
        /// </summary>
        Default = 1,

        /// <summary>
        /// 忽略空字符
        /// </summary>
        IgnoreWhiteSpace = 2,

        /// <summary>
        /// 忽略换行符
        /// </summary>
        IgnoreNewLine = 4
    }
}