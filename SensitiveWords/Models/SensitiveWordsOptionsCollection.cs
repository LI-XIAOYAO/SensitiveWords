using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SensitiveWords
{
    /// <summary>
    /// 敏感词处理选项
    /// </summary>
    public class SensitiveWordsOptionsCollection : Collection<SensitiveWordsOptions>
    {
        /// <summary>
        /// 通过标签获取敏感词选项
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <returns></returns>
        public IEnumerable<SensitiveWordsOptions> this[string tag]
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item.Tag == tag)
                    {
                        yield return item;
                    }
                }
            }
        }

        /// <summary>
        /// 通过处理选项获取敏感词选项
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEnumerable<SensitiveWordsOptions> this[HandleOptions options]
        {
            get
            {
                foreach (var item in Items)
                {
                    if ((item.HandleOptions & options) > 0)
                    {
                        yield return item;
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定索引值
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new SensitiveWordsOptions this[int index]
        {
            get
            {
                return Items[index];
            }
        }
    }
}