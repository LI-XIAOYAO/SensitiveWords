using System.Collections.Generic;

namespace SensitiveWords
{
    /// <summary>
    /// 拼音信息
    /// </summary>
    public class PinYinInfo
    {
        /// <summary>
        /// 字符
        /// </summary>
        public char Char { get; set; }

        /// <summary>
        /// 拼音
        /// </summary>
        public string PinYin { get; set; }

        /// <summary>
        /// 简拼
        /// </summary>
        public char? JianPin => null != PinYin ? PinYin[0] : new char?();

        /// <summary>
        /// 同音字
        /// </summary>
        public IReadOnlyList<char> Homophone { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Char} {PinYin}";
    }
}