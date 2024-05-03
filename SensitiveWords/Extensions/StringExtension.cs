using Microsoft.International.Converters.PinYinConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SensitiveWords
{
    /// <summary>
    /// 字符串扩展
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 正则字符转义 (\, *, +, ?, |, {, [, (,), ^, $,., #, and white space)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Escape(this string str) => Regex.Escape(str);

        /// <summary>
        /// 根据索引替换字符串
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Replace(this string str, int startIndex, int count, string value) => str.Remove(startIndex, count).Insert(startIndex, value);

        /// <summary>
        /// 获取字符串拼音信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static PinYinInfoCollection ToPinYinInfo(this string str)
        {
            var pinYinInfos = new PinYinInfoCollection();

            if (string.IsNullOrEmpty(str))
            {
                return pinYinInfos;
            }

            var chars = str.AsSpan();
            for (int i = 0; i < chars.Length; i++)
            {
                var pinYinInfo = new PinYinInfo
                {
                    Char = chars[i]
                };

                if (ChineseChar.IsValidChar(chars[i]))
                {
                    var chineseChar = new ChineseChar(chars[i]);
                    var hastSet = new HashSet<string>();
                    for (int j = 0; j < chineseChar.PinyinCount; j++)
                    {
                        hastSet.Add(Regex.Replace(chineseChar.Pinyins[j], @"\d", string.Empty));
                    }

                    void SetHomophone(string pinYin)
                    {
                        pinYinInfo.PinYin = pinYin.ToLower();

                        for (int h = 1; h <= chineseChar.Pinyins.Count && null == pinYinInfo.Homophone; h++)
                        {
                            pinYinInfo.Homophone = ChineseChar.GetChars($"{pinYinInfo.PinYin}{h}")?.Where(c => c != pinYinInfo.Char).ToList();
                        }
                    }

                    if (chineseChar.IsPolyphone && hastSet.Count > 1 && HomophoneRegexGroupOptions.HomophoneRegexMaps.Count > 0)
                    {
                        foreach (var item in hastSet)
                        {
                            if (HomophoneRegexGroupOptions.HomophoneRegexMaps.TryGetValue(item, out var homophoneRegex)
                                && ((i > 0 && ChineseChar.IsValidChar(chars[i - 1]) && Regex.IsMatch(chars.Slice(i - 1, 2).ToString(), homophoneRegex))
                                    || i + 1 < chars.Length && ChineseChar.IsValidChar(chars[i + 1]) && Regex.IsMatch(chars.Slice(i, 2).ToString(), homophoneRegex))
                            )
                            {
                                SetHomophone(item);

                                break;
                            }
                        }
                    }

                    if (null == pinYinInfo.PinYin)
                    {
                        SetHomophone(hastSet.First());
                    }
                }

                pinYinInfos.Add(pinYinInfo);
            }

            return pinYinInfos;
        }
    }
}