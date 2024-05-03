using System.Text;

namespace SensitiveWords
{
    /// <summary>
    /// 拼音信息集合
    /// </summary>
    public class PinYinInfoCollection : InternalReadOnlyCollection<PinYinInfo>
    {
        private long _version;
        private long _lastVersion = -1;
        private string _chars;
        private string _pinYin;
        private string _jianPin;
        private string _homophone;

        internal PinYinInfoCollection()
        {
        }

        /// <summary>
        /// 字符串
        /// </summary>
        public string Chars
        {
            get
            {
                Refresh();

                return _chars;
            }
        }

        /// <summary>
        /// 拼音
        /// </summary>
        public string PinYin
        {
            get
            {
                Refresh();

                return _pinYin;
            }
        }

        /// <summary>
        /// 简拼
        /// </summary>
        public string JianPin
        {
            get
            {
                Refresh();

                return _jianPin;
            }
        }

        /// <summary>
        /// 同音字
        /// </summary>
        public string Homophone
        {
            get
            {
                Refresh();

                return _homophone;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="item"></param>
        protected internal override void Add(PinYinInfo item)
        {
            base.Add(item);

            _version++;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="item"></param>
        protected internal override void Remove(PinYinInfo item)
        {
            base.Remove(item);

            _version++;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="index"></param>
        protected internal override void RemoveAt(int index)
        {
            base.RemoveAt(index);

            _version++;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected internal override void Clear()
        {
            base.Clear();

            _version++;
        }

        /// <summary>
        /// Refresh
        /// </summary>
        private void Refresh()
        {
            if (_version == _lastVersion)
            {
                return;
            }

            var stringBuilder = new StringBuilder();
            var pyStringBuilder = new StringBuilder();
            var jpStringBuilder = new StringBuilder();
            var hStringBuilder = new StringBuilder();

            foreach (var pinYinInfo in this)
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

            _chars = stringBuilder.ToString();
            _pinYin = pyStringBuilder.ToString();
            _jianPin = jpStringBuilder.ToString();
            _homophone = hStringBuilder.ToString();

            _lastVersion = _version;
        }
    }
}