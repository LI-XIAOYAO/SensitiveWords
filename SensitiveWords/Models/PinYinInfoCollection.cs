using System.Collections.ObjectModel;
using System.Text;

namespace SensitiveWords
{
    /// <summary>
    /// 拼音信息集合
    /// </summary>
    public class PinYinInfoCollection : Collection<PinYinInfo>
    {
        /// <summary>
        /// 字符串
        /// </summary>
        public string Chars { get; private set; }

        /// <summary>
        /// 拼音
        /// </summary>
        public string PinYin { get; private set; }

        /// <summary>
        /// 简拼
        /// </summary>
        public string JianPin { get; private set; }

        /// <summary>
        /// 同音字
        /// </summary>
        public string Homophone { get; private set; }

        /// <summary>
        /// InsertItem
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int index, PinYinInfo item)
        {
            base.InsertItem(index, item);

            Refresh();
        }

        /// <summary>
        /// SetItem
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void SetItem(int index, PinYinInfo item)
        {
            base.SetItem(index, item);

            Refresh();
        }

        /// <summary>
        /// RemoveItem
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);

            Refresh();
        }

        /// <summary>
        /// ClearItems
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();

            Refresh();
        }

        /// <summary>
        /// Refresh
        /// </summary>
        private void Refresh()
        {
            var stringBuilder = new StringBuilder();
            var pyStringBuilder = new StringBuilder();
            var jpStringBuilder = new StringBuilder();
            var hStringBuilder = new StringBuilder();
            foreach (var pinYinInfo in Items)
            {
                stringBuilder.Append(pinYinInfo.Char);
                if (null != pinYinInfo.PinYin)
                {
                    pyStringBuilder.Append(pinYinInfo.PinYin);
                    jpStringBuilder.Append(pinYinInfo.JianPin);
                }
                else
                {
                    pyStringBuilder.Append(pinYinInfo.Char);
                    jpStringBuilder.Append(pinYinInfo.Char);
                }

                hStringBuilder.Append(null != pinYinInfo.Homophone && pinYinInfo.Homophone.Count > 0 ? pinYinInfo.Homophone[0] : pinYinInfo.Char);
            }

            Chars = stringBuilder.ToString();
            PinYin = pyStringBuilder.ToString();
            JianPin = jpStringBuilder.ToString();
            Homophone = hStringBuilder.ToString();
        }
    }
}