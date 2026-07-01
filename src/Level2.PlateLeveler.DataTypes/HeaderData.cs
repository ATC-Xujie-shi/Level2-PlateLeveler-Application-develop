using System;
using System.Collections.Generic;

namespace Level2.PlateLeveler.DataTypes {
    public class HeaderData {

        protected int _TelegramLength;
        public int TelegramLength {
            get => this._TelegramLength; set => this._TelegramLength = value;
        }

        protected string _Recipient;
        public string Recipient {
            get => this._Recipient; set => this._Recipient = value;
        }

        protected string _Sender;
        public string Sender {
            get => this._Sender; set => this._Sender = value;
        }

        protected int _TelegramType;
        public int TelegramType {
            get => this._TelegramType; set => this._TelegramType = value;
        }

        public void ChangeSenderRecipient() {
            var sender = this._Sender;
            var recipient = this._Recipient;
            this._Sender = recipient;
            this._Recipient = sender;
        }
    }

    public class LanguageData {
        protected string _Language;
        public string Language {
            get => this._Language; set => this._Language = value;
        }
        protected string _LanguageKey;
        public string LanguageKey {
            get => this._LanguageKey; set => this._LanguageKey = value;
        }
    }

    public class LanguageList : List<LanguageData> {
        public LanguageList()
           : base() {
        }

        public LanguageData GetItem(string language) {
            var data = new LanguageData();
            foreach (var item in this) {
                if (item.Language.Equals(language, StringComparison.Ordinal)) {
                    return item;
                }
            }

            return data;
        }
    }
}