using System;
using System.ComponentModel;

namespace RSSReader.Model
{
    /// <summary>
    /// RSS の取得先情報
    /// </summary>
    public class RssSiteInfo : INotifyPropertyChanged 
    {
        /// <summary>
        /// DB登録番号
        /// </summary>
        public Int32 ID { get; set; }

        /// <summary>
        /// RSS配信サイト名
        /// </summary>
        public String SiteName { get; set; }

        /// <summary>
        /// RSSへのリンク
        /// </summary>
        private String _link;
        public String Link {
            get { return this._link; }
            set {
                if (value != this._link)
                {
					this._link = value;
                    OnPropertyChanged(nameof(this.Link));
                }
            }
        }

        /// <summary>
        /// RSS配信サイト情報
        /// </summary>
        public String Summary { get; set; }

        /// <summary>
        /// プロパティ変更イベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
          
        /// <summary>
        /// プロパティ変更イベント
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
