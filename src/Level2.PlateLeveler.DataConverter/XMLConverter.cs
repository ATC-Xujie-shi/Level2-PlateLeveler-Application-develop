using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.DataConverter {
    public class XMLConverter {
        public InitData Initialization { get; set; }

        public XMLConverter() {
        }
        public XMLConverter(InitData init) {
            this.Initialization = init;
        }

        public object ConvertTelegramToObjectWithAttributes<T>(TelegramData telegram, InitData data) {
            try {

                var infos = data.GetType().GetFields();
                var element = new XElement(typeof(T).Name);

                foreach (var fieldInfo in infos) {
                    if (fieldInfo.FieldType.Equals(typeof(T))) {
                        element = new XElement(fieldInfo.Name);
                    }
                }
                element.Add(new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"));
                element.Add(new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"));
                var attributes = new List<XAttribute>();
                foreach (var item in telegram.TelegramValues) {
                    if (item.InObject) {
                        if (data.Communications[telegram.ComIndex].Endian > 1) {
                            item.Value = item.Typ != TypeCode.String ? Convert.ChangeType(item.Value, item.Typ.GetType()) : item.Value.ToString().Trim();
                        } else {
                            if (item.Value is string) {
                                item.Value = item.Value.ToString().Trim();
                            }
                        }
                        var att = new XAttribute(item.Name, item.Value);

                        element.Add(att);
                    }
                }
                var serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(new StringReader(element.ToString().Replace(@"\", "")));
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }
        public object ConvertHeaderToObject<T>(TelegramData telegram) {
            var elements = new List<XElement>();
            try {
                var tp = typeof(T);
                foreach (var item in telegram.TelegramValues) {
                    if (item.EndOfHeader) {
                        break;
                    }

                    item.Value = item.Value switch {
                        string s => s.Trim(),
                        _ => item.Value ?? "0"
                    };

                    var child = new XElement(item.Name, item.Value);

                    elements.Add(child);
                }

                var element = new XElement(typeof(T).Name, [.. elements]);

                var serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(new StringReader(element.ToString()));
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }

        public object ConvertTelegramToObject<T>(TelegramData telegram) {
            var elements = new List<XElement>();

            try {
                var tp = typeof(T);
                foreach (var item in telegram.TelegramValues) {
                    if (item.InObject) {
                        if (item.Value is string) {
                            item.Value = item.Value.ToString().Trim();
                        } else {
                            item.Value ??= "0";
                        }
                        if (item.Typ == TypeCode.Boolean) {
                            item.Value = item.Value.ToString().ToLower(CultureInfo.CurrentCulture);
                        }

                        var child = new XElement(item.Name, item.Value.ToString());

                        elements.Add(child);
                    } else if (item.TelegramArray != null) {
                        var elements1 = new List<XElement>();
                        for (var n = 0; n < item.TelegramArray.Length; n++) {
                            if (item.TelegramArray[n].Value is string) {
                                item.TelegramArray[n].Value = item.TelegramArray[n].Value.ToString().Trim();
                            } else {
                                if (item.TelegramArray[n].Value == null) {
                                    item.TelegramArray[n].Value = "0";
                                }
                            }
                            var child1 = new XElement(item.TelegramArray[n].ShortTypeString, item.TelegramArray[n].Value);
                            elements1.Add(child1);
                            if (n.Equals(item.TelegramArray.Length - 1)) {
                                var child = new XElement(item.TelegramArray[n].Name, elements1);
                                elements.Add(child);
                            }
                        }
                    }
                }

                var element = new XElement(typeof(T).Name, [.. elements]);

                var serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(new StringReader(element.ToString()));
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }

        public object ConvertFileToObject<T>(string file) {

            try {
                var serializer = new XmlSerializer(typeof(T));
                var stream = new FileStream(file, FileMode.Open);
                var obj = serializer.Deserialize(stream);
                stream.Close();
                return obj;
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return null;
            }
        }

        public void ConvertObjectToFile<T>(ref string file, T obj) {

            try {
                var serializer = new XmlSerializer(typeof(T));
                var stream = new FileStream(file, FileMode.OpenOrCreate);
                serializer.Serialize(stream, obj);
                stream.Close();
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        #region Expand Arraytelegrams

        public TelegramList SetTelegramList(TelegramList list) {
            var result = new TelegramList();
            var data = new TelegramData();
            var block = new TelegramValueData();

            try {
                for (var n = 0; n < list.Count; n++) {
                    data = this.ConvertBlockToArray(list[n]);
                    block = data.TelegramValues.FirstOrDefault(b => b.Name == "TelegramLength");
                    _ = block?.Value = Convert.ChangeType(data.Length, block.Typ, new CultureInfo("en-us"));

                    block = data.TelegramValues.FirstOrDefault(b => b.Name == "TelegramType");
                    _ = block?.Value = Convert.ChangeType(data.TelegramID, block.Typ, new CultureInfo("en-us"));

                    block = data.TelegramValues.FirstOrDefault(b => b.Name == "MessageID");
                    _ = block?.Value = data.MessageID;

                    block = data.TelegramValues.FirstOrDefault(b => b.Name == "TelegramCounter");
                    var rnd = new Random();
                    _ = block?.Value = Convert.ToInt16(rnd.Next(10000));

                    var cnt = 0;
                    foreach (var item in list[n].TelegramValues) {
                        if (item.Name.Equals("TelegramList", StringComparison.Ordinal))
                        //if (item.Name.Equals("TelegramArray"))
                        {
                            for (var i = 0; i < item.Count; i++) {
                                foreach (var item1 in item.TelegramValues) {
                                    cnt += item1.Length;
                                }
                            }
                        } else {
                            cnt += item.Length;
                        }
                    }
                    result.Add(data);
                }
                return result;
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return result;
            }
        }

        public TelegramData ConvertBlockToArray(TelegramData telegram) {
            var data = new TelegramData {
                TelegramValues = []
            };
            try {
                for (var i = 0; i < telegram.TelegramValues.Count; i++) {
                    var item = telegram.TelegramValues[i];
                    telegram.TelegramValues[i] = this.GetNewBlock(item, telegram);
                    item = telegram.TelegramValues[i];
                    if (item.TelegramArray != null) {
                        var item2 = item.TelegramArray[0];
                        telegram.TelegramValues[i].TelegramArray = new TelegramValueData[item.Count];
                        for (var n = 0; n < item.Count; n++) {
                            telegram.TelegramValues[i].TelegramArray[n] = this.GetNewBlock(item2, telegram);
                        }

                        data.TelegramValues.Add(telegram.TelegramValues[i]);
                    } else if (item.TelegramValues != null) {
                        if (item.TelegramValues.Count > 0) {
                            item.TelegramValues.TelegramValueArray = new TelegramValueList[item.Count];
                            for (var n = 0; n < item.Count; n++) {
                                item.TelegramValues.TelegramValueArray[n] = this.GetNewBlockList(item.TelegramValues, telegram);
                                for (var k = 0; k < item.TelegramValues.Count; k++) {
                                    if (item.TelegramValues[k].TelegramArray != null) {
                                        item.TelegramValues.TelegramValueArray[n][k].TelegramArray = new TelegramValueData[item.TelegramValues[k].Count];
                                        for (var j = 0; j < item.TelegramValues[k].Count; j++) {
                                            item.TelegramValues.TelegramValueArray[n][k].TelegramArray[j] = this.GetNewBlock(item.TelegramValues[k].TelegramArray[0], telegram);
                                        }
                                    }
                                }
                            }
                        } else {
                            item.TelegramValues = null;
                        }

                        data.TelegramValues.Add(item);
                    } else {
                        data.TelegramValues.Add(item);
                    }
                }

                data.ComIndex = telegram.ComIndex;
                data.Length = telegram.Length;
                data.Name = telegram.Name;
                data.TelegramID = telegram.TelegramID;
                data.MessageID = telegram.MessageID;
                data.TelegramType = telegram.TelegramType;
                data.StringLength = telegram.StringLength;
                return data;
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return data;
            }
        }

        public TelegramValueList GetNewBlockList(TelegramValueList list, TelegramData tel) {
            var result = new TelegramValueList();
            foreach (var item in list) {
                result.Add(this.GetNewBlock(item, tel));
            }

            return result;
        }

        public TelegramValueData GetNewBlock(TelegramValueData block, TelegramData tel) {
            var data = new TelegramValueData {
                Count = block.Count,
                Format = block.Format,
                InDatabase = block.InDatabase,
                InObject = block.InObject,
                Length = block.Length,
                AdressNo = block.AdressNo,
                Name = block.Name,
                Identity = block.Identity,
                Factor = block.Factor
            };
            if (block.TelegramArray != null) {
                data.TelegramArray = block.TelegramArray;
            }

            if (block.TelegramValues != null) {
                data.TelegramValues = block.TelegramValues;
            }

            if (this.Initialization.Communications[tel.ComIndex].Endian > 1) {
                if (block.Value != null) {
                    block.Value = block.Typ != TypeCode.String ? Convert.ChangeType(block.Value, block.Typ.GetType()) : block.Value.ToString().Trim();
                }
            } else {
                block.Value = block.Value is string ? block.Value.ToString().Trim() : block.Value;
            }
            data.Value = block.Value;
            return data;
        }
        #endregion

        public object ConvertTelegramToListObjectWithArrays<TP, TC>(TelegramData telegram, string listName = "") {
            var elements = new List<XElement>();
            var elements1 = new List<XElement>();
            var elements2 = new List<XElement>();
            var elements3 = new List<List<XElement>>();

            try {
                var info = typeof(TP).GetProperties().ToList().FirstOrDefault(t => t.PropertyType.Equals(typeof(List<TC>)));
                if (info != null) {
                    listName = info.Name;
                }

                var child = new XElement(listName);
                var child1 = new XElement(listName);
                var child2 = new XElement(listName);
                var child3 = new XElement(listName);
                foreach (var item in telegram.TelegramValues) {
                    if (item.InObject) {
                        if (item.Value is string) {
                            item.Value = item.Value.ToString().Trim();
                        }

                        child = new XElement(item.Name, item.Value);
                        elements.Add(child);

                    } else if (item.Count > 0 | item.Identity != null) {
                        if (item.TelegramValues != null) {
                            var cnt = item.Count;
                            if (item.Identity != null) {
                                var dataIdentity = telegram.TelegramValues.FirstOrDefault(t => t.Name.Equals(item.Identity, StringComparison.Ordinal));
                                if (dataIdentity != null) {
                                    cnt = Convert.ToInt32(dataIdentity.Value);
                                }
                            }
                            var elements4 = new List<XElement>();
                            for (var i = 0; i < cnt; i++) {
                                elements2 = [];
                                foreach (var item1 in item.TelegramValues.TelegramValueArray[i]) {
                                    if (item1.TelegramArray != null) {
                                        elements1 = [];
                                        for (var n = 0; n < item1.TelegramArray.Length; n++) {
                                            child1 = new XElement(item1.TelegramArray[n].Format, item1.TelegramArray[n].Value);
                                            elements1.Add(child1);
                                        }
                                        child2 = new XElement(item1.Name, elements1);
                                    } else {
                                        item1.Value ??= item1.Typ == TypeCode.String ? "" : 0;
                                        child2 = new XElement(item1.Name, item1.Value);
                                    }
                                    elements2.Add(child2);
                                }
                                child3 = new XElement(typeof(TC).Name, elements2);
                                elements4.Add(child3);
                            }
                            elements3.Add(elements4);
                        } else if (item.TelegramArray != null) {
                            elements1 = [];
                            for (var n = 0; n < item.TelegramArray.Length; n++) {
                                child1 = new XElement(item.TelegramArray[n].ShortTypeString, item.TelegramArray[n].Value);
                                elements1.Add(child1);
                                if (n.Equals(item.TelegramArray.Length - 1)) {
                                    child = new XElement(item.TelegramArray[n].Name, elements1);
                                    elements.Add(child);
                                }
                            }
                        }
                    }
                }
                elements1 = [];
                foreach (var el in elements3) {
                    child1 = new XElement("ArrayOf" + typeof(TC).Name, el);
                    elements1.Add(child1);
                }
                child = new XElement(listName, elements1);
                elements.Add(child);

                var element = new XElement(typeof(TP).Name, [.. elements]);

                var serializer = new XmlSerializer(typeof(TP));
                return serializer.Deserialize(new StringReader(element.ToString()));
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }

        //TP: Type parent, TC: Type child
        public object ConvertTelegramToObjectWithArrays<TP, TC>(TelegramData telegram, string listName = "") {
            var elements = new List<XElement>();
            var elements1 = new List<XElement>();
            var elements2 = new List<XElement>();
            var elements3 = new List<XElement>();

            try {
                var info = typeof(TP).GetProperties().ToList().FirstOrDefault(t => t.PropertyType.Equals(typeof(List<TC>)));
                if (info != null) {
                    listName = info.Name;
                }

                var child = new XElement(listName);
                var child1 = new XElement(listName);
                var child2 = new XElement(listName);
                var child3 = new XElement(listName);
                foreach (var item in telegram.TelegramValues) {
                    if (item.InObject) {

                        if (item.Count > 0 | ((item.Identity != null) && (!string.IsNullOrEmpty(item.Identity)))) {
                            if (item.TelegramValues != null) {
                                if (item.TelegramValues.Count > 0) {
                                    var item2 = telegram.TelegramValues.FirstOrDefault(t => t.Name == item.Identity);

                                    var cnt = item.Count;
                                    if (item2 != null) {
                                        cnt = Convert.ToInt32(item2.Value);
                                    }

                                    elements3 = [];
                                    for (var i = 0; i < cnt; i++) {
                                        elements2 = [];
                                        foreach (var item1 in item.TelegramValues.TelegramValueArray[i]) {
                                            if (item1.TelegramArray != null) {
                                                elements1 = [];
                                                for (var n = 0; n < item1.TelegramArray.Length; n++) {
                                                    child1 = new XElement(item1.TelegramArray[n].ShortTypeString, item1.TelegramArray[n].Value);
                                                    elements1.Add(child1);
                                                }
                                                child2 = new XElement(item1.TelegramArray[0].Name, elements1);
                                            } else {
                                                item1.Value ??= item1.Typ == TypeCode.String ? "" : 0;
                                                child2 = new XElement(item1.Name, item1.Value);
                                            }
                                            elements2.Add(child2);
                                        }
                                        child3 = new XElement(typeof(TC).Name, elements2);
                                        elements3.Add(child3);
                                    }

                                    child = new XElement(listName, elements3);
                                    elements.Add(child);
                                }
                            }

                            if (item.TelegramArray != null) {
                                if (item.TelegramArray.Length > 0) {
                                    elements1 = [];
                                    for (var n = 0; n < item.TelegramArray.Length; n++) {
                                        child1 = new XElement(item.TelegramArray[n].ShortTypeString, item.TelegramArray[n].Value);
                                        elements1.Add(child1);
                                        if (n.Equals(item.TelegramArray.Length - 1)) {
                                            child = new XElement(item.TelegramArray[n].Name, elements1);
                                            elements.Add(child);
                                        }
                                    }
                                }
                            }
                        } else {
                            if (item.Value is string) {
                                item.Value = item.Value.ToString().Trim();
                            }

                            child = new XElement(item.Name, item.Value);
                            elements.Add(child);
                        }
                    }
                }

                var element = new XElement(typeof(TP).Name, [.. elements]);

                var serializer = new XmlSerializer(typeof(TP));
                return serializer.Deserialize(new StringReader(element.ToString()));
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }

        public TelegramData ConvertObjectToTelegram<T>(T obj, TelegramData telegram) {
            try {
                var ser = new XmlSerializer(typeof(T));
                var builder = new StringBuilder();
                var writer = new StringWriter(builder);
                ser.Serialize(writer, obj);

                var doc = new XmlDocument();
                doc.LoadXml(builder.ToString());
                XmlNode node = doc.DocumentElement;
                foreach (XmlNode node1 in node.ChildNodes) {
                    if (node1.ChildNodes.Count > 1) {
                        foreach (XmlNode node2 in node1.ChildNodes) {
                            foreach (var item1 in telegram.TelegramValues) {
                                if (item1.Name.Equals(node2.Name, StringComparison.Ordinal) && !string.IsNullOrEmpty(node2.InnerText)) {
                                    item1.Value = Convert.ChangeType(node2.InnerText, item1.Typ, new CultureInfo("en-us"));
                                }
                            }
                        }
                    } else {
                        var item = telegram.TelegramValues.FirstOrDefault(t => t.Name == node1.Name);
                        if (item != null) {
                            if (item.Typ == TypeCode.String) {
                                item.Value = node1.InnerText;
                            } else {
                                if (!string.IsNullOrEmpty(node1.InnerText)) {
                                    item.Value = Convert.ChangeType(node1.InnerText, item.Typ, new CultureInfo("en-us"));
                                }
                            }
                        }
                    }
                }
                return telegram;
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return telegram;
            }
        }

        #region SaveObjectValues into ArrayTelegrams

        public TelegramData ConvertObjectToTelegramWithArray<T>(T obj, TelegramData telegram) {
            var list = telegram.TelegramValues;
            this.ConvertObjectToTelegramWithArray(obj, ref list);
            return telegram;
        }

        public void ConvertObjectToTelegramWithArray(object obj, ref TelegramValueList list) {
            var block = new TelegramValueData();
            int cntArr = 0, cntBlock = 0, idxBlock = 0;
            var bTelArr = false;
            ;
            try {
                var infos = obj.GetType().GetProperties();
                foreach (var propertyInfo in infos) {
                    var attr = (TelegramDefinitionAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(TelegramDefinitionAttribute));
                    if (attr != null) {
                        if (attr.Field.Equals("TelegramValue", StringComparison.Ordinal)) {
                            try {
                                block = list.Single(tb => tb.Name == propertyInfo.Name);
                                block.Value = propertyInfo.GetValue(obj, null);
                            } catch { }
                        } else {
                            block = list.FindIndex(attr.Field, out idxBlock);
                            bTelArr = attr.Field.Equals("TelegramArray", StringComparison.Ordinal);
                            if (bTelArr) {
                                block = list.FirstOrDefault(tb => tb.Name == attr.Field && tb.Identity == attr.Name);
                                block ??= list.FirstOrDefault(tb => tb.Name == attr.Field);

                                if (block != null) {
                                    cntArr++;
                                }
                            } else {
                                block = list.Count > idxBlock + cntBlock ? list[idxBlock + cntBlock] : null;
                                cntBlock++;
                            }

                            if (propertyInfo.PropertyType.IsArray) {
                                var arr = (Array)propertyInfo.GetValue(obj, null);
                                for (var n = 0; n < arr.Length; n++) {
                                    if (bTelArr) {
                                        block.TelegramArray[n].Value = arr.GetValue(n);
                                    } else {
                                        this.ConvertObjectToTelegramWithArray(arr.GetValue(n), ref block.TelegramValues.TelegramValueArray[n]);
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }

        public void SerializeData<T>(T obj, string file) {
            var s = new XmlSerializer(typeof(T));
            s.Serialize(new FileStream(file, FileMode.OpenOrCreate), obj);
        }
        public T DeserializeData<T>(string file) {
            var s = new XmlSerializer(typeof(T));
            var stream = new FileStream(file, FileMode.OpenOrCreate);
            return (T)s.Deserialize(stream);
        }

        #endregion
    }
}
