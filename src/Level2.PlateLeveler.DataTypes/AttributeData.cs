using System;
using System.Collections.Generic;

namespace Level2.PlateLeveler.DataTypes {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class TelegramDefinitionAttribute : Attribute {
        public TelegramDefinitionAttribute(string field) {
            this.Field = field;
        }

        public TelegramDefinitionAttribute(string field, string name) {
            this.Field = field;
            this.Name = name;
        }

        public string Field { get; internal set; }
        public string Name { get; internal set; }
        public int Index { get; set; }
        public object Value { get; set; }
    }

    public class TelegramDefinitionAttributeList : List<TelegramDefinitionAttribute> {
        public TelegramDefinitionAttributeList()
           : base() {
        }

        public void SortList() => this.Sort(new TelegramDefinitionAttributeComparer());
    }

    public class TelegramDefinitionAttributeComparer : IComparer<TelegramDefinitionAttribute> {
        public int Compare(TelegramDefinitionAttribute item1, TelegramDefinitionAttribute item2) {
            if (item1 == null && item2 == null) {
                return 0;
            }

            if (item1 == null) {
                return 1;
            }

            if (item2 == null) {
                return -1;
            }
            // Vergleich
            return item1.Index.CompareTo(item2.Index);
        }
    }

    public class ParameterArrayData {
        public string[] Fields { get; set; }
        public object[] Values { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class KGroupAttribute : Attribute {
        public KGroupAttribute(string stringName) {
            this.KGroup = stringName;
        }

        public KGroupAttribute(string stringName, int index) {
            this.KGroup = stringName;
            this.Index = index;
        }

        public string KGroup { get; }

        public int Index { get; internal set; }
        public int DecimalPoint { get; set; }
        public object Value { get; set; }

        public string StringValue {
            get {
                var str = this.KGroup;
                if (this.Index > -1) {
                    str += "/" + this.Index.ToString();
                }

                return str += this.Value != null ? " " + this.Value.ToString() : "";
            }
        }
        public string StringName { get; }
    }

    public class KGroupAttributeList : List<KGroupAttribute> {
        public KGroupAttributeList()
           : base() {
        }
        public KGroupAttribute GetItem(string kGroup, int index) {
            var data = new KGroupAttribute(kGroup);
            foreach (var item in this) {
                if (item.KGroup.Equals(kGroup, StringComparison.Ordinal) && item.Index.Equals(index)) {
                    return item;
                }
            }

            return data;
        }

        public void SortList() => this.Sort(new KGroupComparison());
    }

    public class KGroupComparison : IComparer<KGroupAttribute> {
        public int Compare(KGroupAttribute item1, KGroupAttribute item2) {
            if (item1 == null) {
                return -1;
            } else if (item2 == null) {
                return 1;
            }

            var n = item1.Index.CompareTo(item2.Index);
            return n == 0 ? item1.KGroup.CompareTo(item2.KGroup) : n;

        }
    }

    public class KGroupData {
        public int ID { get; set; }
        public string Category { get; set; }
        public string KField { get; set; }
        public string ParameterGroup { get; set; }
        public object Value { get; set; }
    }

    public class KGroupList : List<KGroupData> {
        public KGroupList()
           : base() {
        }

        public KGroupData GetItem(string parametergroup) {
            var data = new KGroupData();
            foreach (var item in this) {
                if (item.ParameterGroup.Equals(parametergroup, StringComparison.Ordinal)) {
                    return item;
                }
            }

            return data;
        }
    }
}
