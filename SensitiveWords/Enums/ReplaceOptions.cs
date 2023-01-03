namespace SensitiveWords
{
    /// <summary>
    /// 替换选项
    /// </summary>
    public enum ReplaceOptions
    {
        /// <summary>
        /// 字符
        /// </summary>
        Character = 1,

        /// <summary>
        /// 拼音
        /// </summary>
        PinYin = 2,

        /// <summary>
        /// 简拼
        /// </summary>
        JianPin = 4,

        /// <summary>
        /// 同音字
        /// </summary>
        Homophone = 8
    }
}