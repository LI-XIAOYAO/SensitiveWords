using Microsoft.International.Converters.PinYinConverter;
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

            var chars = str.ToCharArray();
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

                    if (chineseChar.IsPolyphone && hastSet.Count > 1 && HomophoneRegexGroupOptions.HomophoneRegexMaps.Count > 0)
                    {
                        int j = 0;
                        foreach (var item in hastSet)
                        {
                            if (HomophoneRegexGroupOptions.HomophoneRegexMaps.ContainsKey(item))
                            {
                                if (i > 0 && ChineseChar.IsValidChar(chars[i - 1]) && Regex.IsMatch(string.Concat(chars[i - 1], chars[i]), HomophoneRegexGroupOptions.HomophoneRegexMaps[item]))
                                {
                                    pinYinInfo.PinYin = item.ToLower();
                                    for (int h = 1; h <= chineseChar.PinyinCount && null == pinYinInfo.Homophone; h++)
                                    {
                                        pinYinInfo.Homophone = ChineseChar.GetChars($"{item}{h}")?.ToList();
                                    }

                                    break;
                                }

                                if (i + 1 < chars.Length && ChineseChar.IsValidChar(chars[i + 1]) && Regex.IsMatch(string.Concat(chars[i], chars[i + 1]), HomophoneRegexGroupOptions.HomophoneRegexMaps[item]))
                                {
                                    pinYinInfo.PinYin = item.ToLower();
                                    for (int h = 1; h <= chineseChar.PinyinCount && null == pinYinInfo.Homophone; h++)
                                    {
                                        pinYinInfo.Homophone = ChineseChar.GetChars($"{item}{h}")?.ToList();
                                    }

                                    break;
                                }
                            }

                            if (++j == hastSet.Count)
                            {
                                pinYinInfo.PinYin = hastSet.First().ToLower();
                                for (int h = 1; h <= chineseChar.PinyinCount && null == pinYinInfo.Homophone; h++)
                                {
                                    pinYinInfo.Homophone = ChineseChar.GetChars($"{hastSet.First()}{h}")?.ToList();
                                }
                            }
                        }
                    }
                    else
                    {
                        pinYinInfo.PinYin = hastSet.First().ToLower();
                        pinYinInfo.Homophone = ChineseChar.GetChars(chineseChar.Pinyins[0])?.ToList();
                        if (null == pinYinInfo.Homophone)
                        {
                            for (int h = 1; h <= chineseChar.PinyinCount && null == pinYinInfo.Homophone; h++)
                            {
                                pinYinInfo.Homophone = ChineseChar.GetChars($"{hastSet.First()}{h}")?.ToList();
                            }
                        }
                    }
                }

                if (null != pinYinInfo.Homophone)
                {
                    pinYinInfo.Homophone = pinYinInfo.Homophone.Where(c => c != pinYinInfo.Char).ToList();
                }

                pinYinInfos.Add(pinYinInfo);
            }

            return pinYinInfos;
        }
    }
}