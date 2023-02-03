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
    }
}