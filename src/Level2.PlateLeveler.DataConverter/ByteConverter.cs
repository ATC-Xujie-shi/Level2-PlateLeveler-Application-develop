using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.DataConverter {
    public class ByteConverter {
        private readonly InitData _Initialization;

        public ByteConverter() {
        }

        public ByteConverter(InitData init) {
            this._Initialization = init;
        }

        public LimitationData GetLimitations(List<Limitation> limit) {
            var result = new LimitationData();
            var lt = new Limitation();
            var infos = result.GetType().GetProperties();
            try {
                foreach (var propertyInfo in infos) {
                    lt = (Limitation)Attribute.GetCustomAttribute(propertyInfo, typeof(Limitation));
                    if (lt != null) {
                        var lt1 = limit.Find(l => string.Equals(l.Limit, lt.Limit, StringComparison.Ordinal));
                        if (lt1 != null) {
                            if (propertyInfo.GetValue(result).GetType().Equals(typeof(float))) {
                                propertyInfo.SetValue(result, lt1.Value);
                            } else if (propertyInfo.GetValue(result).GetType().Equals(typeof(bool))) {
                                propertyInfo.SetValue(result, Convert.ToBoolean(lt1.Value));
                            } else {
                                propertyInfo.SetValue(result, Convert.ToInt32(lt1.Value));
                            }
                        }
                    }
                }

                return result;
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return result;
            }
        }

        public void LoadTelegramFromByteArray(ref TelegramData telegram, byte[] btArr, CommunicationData com) {
            _ = new byte[2];
            var idx = 0;
            try {
                if (telegram.Length.Equals(btArr.Length)) {
                    foreach (var val in telegram.TelegramValues) {
                        byte[] btTempArr;
                        if (val.Length > 0) {
                            btTempArr = btTempArr = [.. btArr.ToList().GetRange(idx, val.Length)];
                            val.BytePosition = idx;
                            if (com.Endian.Equals(0) && !val.Typ.Equals(TypeCode.String)) {
                                Array.Reverse(btTempArr);
                            }

                            if (val.Typ.Equals(TypeCode.String) && com.Index == 1) {
                                var btList = btTempArr.ToList();
                                btTempArr = [.. btList.GetRange(2, btList.Count - 2)];
                            }
                            val.Value = Functions.ConvertByteArrayToObjectWithType(btTempArr, val.Typ, com.Endian < 2);
                            this.SetFactor(val);
                            idx += val.Length;
                        } else if (val.TelegramArray != null) {
                            if (val.TelegramArray.Length > 0) {
                                foreach (var item in val.TelegramArray) {
                                    val.BytePosition = idx;
                                    btTempArr = [.. btArr.ToList().GetRange(idx, item.Length)];
                                    if (com.Endian.Equals(0) && !item.Typ.Equals(TypeCode.String)) {
                                        Array.Reverse(btTempArr);
                                    }

                                    item.Value = Functions.ConvertByteArrayToObjectWithType(btTempArr, item.Typ, com.Endian < 2);
                                    this.SetFactor(item);
                                    idx += item.Length;
                                }
                            }
                        } else if (val.TelegramValues != null) {
                            if (val.TelegramValues.Count > 0) {
                                val.TelegramValues.TelegramValueArray = new TelegramValueList[val.Count];
                                for (var n = 0; n < val.Count; n++) {
                                    val.TelegramValues.TelegramValueArray[n] = [];
                                    foreach (var item in val.TelegramValues) {
                                        var block = new TelegramValueData {
                                            Length = item.Length,
                                            InObject = item.InObject,
                                            Name = item.Name,
                                            Format = item.Format,
                                            Factor = item.Factor,
                                            BytePosition = idx
                                        };

                                        btTempArr = [.. btArr.ToList().GetRange(idx, item.Length)];
                                        block.Value = Functions.ConvertByteArrayToObjectWithType(btTempArr, item.Typ, com.Endian < 2);
                                        this.SetFactor(block);
                                        idx += item.Length;
                                        val.TelegramValues.TelegramValueArray[n].Add(block);
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        public TelegramData LoadTelegramFromByteArray(TelegramList telegrams, byte[] btArr, CommunicationData com) => this.LoadTelegramFromByteArray(telegrams, btArr, com, out _);

        public TelegramData LoadTelegramFromByteArray(TelegramList telegrams, byte[] btArr, CommunicationData com, out string error) {
            var result = new TelegramData();
            var btList = new List<byte>();
            var length = 0;
            try {
                btList.AddRange(btArr.ToList().GetRange(com.TypeLengthOffset, com.TypeLengthSize));

                var btTempArr = btList.ToArray();
                var idx = 0;
                if (com.Endian > 1) {
                    var typeString = Encoding.GetEncoding(1251).GetString(btTempArr).ToString().Trim();
                    result = telegrams.FirstOrDefault(t => t.MessageID == typeString);
                    if (result == null) {
                        idx = Convert.ToInt32(typeString);
                        btList = [];
                        btList = [.. btArr];
                        btTempArr = [.. btList];
                        typeString = Encoding.GetEncoding(1251).GetString(btTempArr).ToString().Trim();
                        if (string.IsNullOrEmpty(typeString)) {
                            typeString = null;
                        }

                        result = telegrams.FirstOrDefault(t => t.TelegramID.Equals(idx) && t.MessageID == typeString);
                    }
                    // wenn ein Telegramm eine variable Länge hat
                } else {
                    if (com.Endian.Equals(0)) {
                        Array.Reverse(btTempArr);
                    }

                    idx = BitConverter.ToInt16(btTempArr, 0);
                    result = telegrams.FirstOrDefault(t => t.TelegramID == idx);
                }

                if (result == null) {
                    error = "Error: Did not find telegram for " + btArr.Length + " bytes and (maybe false) telegram ID " + idx;
                    Logging.SendMessage(error, MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, this.GetType());
                    return null;
                }

                var cnt = 0;
                var block = new TelegramValueData();
                // bei variabler Länge wird aus der Telegram.xml der Wert "StringLength" ausgelesen. Dieser Wert beschreibt den TelegramValue, in dem der Multiplikator für die Anzahl des TelegramListArrays steht.
                // Dieser Wert wird mit der Bytelänge einzelnen Telegramliste multipliziert. mit diesem Wert und der summierten Bytelängen der TelegramValues bis zur TelegramList ergeben die zu erwartende Gesamtlänge
                if (result.StringLength != null) {
                    result.TelegramValues.ForEach(delegate (TelegramValueData item) {
                        item.BytePosition = cnt;
                        if (item.TelegramArray != null) {
                            if (item.Count != 0) {
                                cnt += item.Count * item.TelegramArray[0].Length;
                            } else {
                                cnt += item.Length;
                            }
                        } else {
                            cnt += item.Length;
                        }
                    });
                    block = result.TelegramValues.FirstOrDefault(t => t.Name == result.StringLength);
                    btTempArr = [.. btArr.ToList().GetRange(block.BytePosition, block.Length)];
                    block.Value = Functions.ConvertByteArrayToObjectWithType(btTempArr, block.Typ, com.Endian < 2);

                    var listValue = result.TelegramValues.FirstOrDefault(t => t.Name == "TelegramList");
                    length = cnt + (listValue.Count * Convert.ToInt32(block.Value));
                    cnt = 0;
                }

                length = length == 0 ? result.Length : length;

                idx = 0;

                if (result != null) {
                    //if (length.Equals(btArr.Length))
                    //{
                    foreach (var val in result.TelegramValues) {
                        if (val.TelegramArray != null) {
                            if (val.TelegramArray.Length > 0) {
                                foreach (var item in val.TelegramArray) {
                                    item.BytePosition = idx;
                                    btTempArr = [.. btArr.ToList().GetRange(idx, item.Length)];
                                    if (com.Endian.Equals(0) && !item.Typ.Equals(TypeCode.String)) {
                                        Array.Reverse(btTempArr);
                                    }

                                    item.Value = Functions.ConvertByteArrayToObjectWithType(btTempArr, item.Typ, com.Endian < 2);
                                    this.SetFactor(item);
                                    idx += item.Length;
                                }
                            }
                        } else if (val.TelegramValues != null) {
                            if (val.TelegramValues.Count > 0) {
                                val.TelegramValues.TelegramValueArray = new TelegramValueList[val.Count];
                                for (var n = 0; n < val.Count; n++) {
                                    val.TelegramValues.TelegramValueArray[n] = [];
                                    foreach (var item in val.TelegramValues) {
                                        block = new TelegramValueData {
                                            Length = item.Length,
                                            InObject = item.InObject,
                                            Name = item.Name,
                                            Format = item.Format,
                                            Factor = item.Factor
                                        };

                                        btTempArr = [.. btArr.ToList().GetRange(idx, item.Length)];
                                        block.Value = Functions.ConvertByteArrayToObjectWithType(btTempArr, item.Typ, com.Endian < 2);
                                        this.SetFactor(block);
                                        idx += item.Length;
                                        val.TelegramValues.TelegramValueArray[n].Add(block);
                                    }
                                }
                            }
                        } else {
                            if (val.Length > 0) {
                                btTempArr = btTempArr = [.. btArr.ToList().GetRange(idx, val.Length)];
                                val.BytePosition = idx;
                                if (com.Endian.Equals(0) && !val.Typ.Equals(TypeCode.String)) {
                                    Array.Reverse(btTempArr);
                                }
                                //if (val.Value == null | val.Typ.Equals(TypeCode.String))
                                //{
                                val.Value = Functions.ConvertByteArrayToObjectWithType(btTempArr, val.Typ, com.Endian < 2);
                                //}
                                this.SetFactor(val);
                                idx += val.Length;
                            }
                        }
                    }
                    //}
                }
                error = null;
                return result;
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                error = e.Message;
                return result;
            }
        }
        public TelegramValueData LoadTelegramValueFromByteArray(TelegramValueData telegramValue, byte[] btArr, bool bigEndian) {
            var result = new TelegramValueData();

            _ = new List<byte>();
            byte[] btTempArr;
            try {
                result = telegramValue;

                var idx = 0;

                //fill the object with recieved data
                if (result != null) {
                    if (result.Length.Equals(btArr.Length)) {
                        //telegram has no BlockList
                        if (result.TelegramValues.TelegramValueArray.Length == 0) {
                            if (result.Length > 0) {
                                btTempArr = new byte[result.Length];

                                for (var n = 0; n < result.Length; n++) {
                                    btTempArr[n] = btArr[n + idx];
                                }

                                if (bigEndian && !result.Typ.Equals(TypeCode.String)) {
                                    Array.Reverse(btTempArr);
                                }

                                result.Value = Functions.ConvertByteArrayToObjectWithType(btTempArr, result.Typ, true);
                                result.BytePosition = result.BytePosition == 0 ? idx : result.BytePosition;
                                idx += result.Length;
                            }
                        }
                        //telegram has BlockList
                        else if (result.TelegramValues.TelegramValueArray.Length > 0) {
                            //if (result.CountRef != null)
                            //   result.Count = Convert.ToInt32(result.Value);
                            if (result.Count > 0) {
                                for (var i = 0; i < result.Count; i++) {
                                    foreach (var subValue in result.TelegramValues.TelegramValueArray[i]) {
                                        subValue.Value = null;
                                        if (subValue.Length > 0) {
                                            btTempArr = new byte[subValue.Length];

                                            for (var n = 0; n < subValue.Length; n++) {
                                                btTempArr[n] = btArr[n + idx];
                                            }

                                            if (bigEndian && !subValue.Typ.Equals(TypeCode.String)) {
                                                Array.Reverse(btTempArr);
                                            }

                                            subValue.Value = Functions.ConvertByteArrayToObjectWithType(btTempArr, subValue.Typ, true);
                                            subValue.BytePosition = subValue.BytePosition == 0 ? idx : subValue.BytePosition;
                                            idx += subValue.Length;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return result;
            } catch (Exception) {
                //Logging.SendErrorMessage(System.Reflection.MethodInfo.GetCurrentMethod().Name, e, this.GetType());
                return result;
            }
        }

        public void SetFactor(TelegramValueData data) {
            if (data.Value != null) {
                if (data.Value.GetType().Equals(typeof(float))) {
                    if (data.Factor != 0) {
                        data.Value = Convert.ToSingle(data.Value) / data.Factor;
                    }
                }
            }
        }

        public byte[] ConvertTelegramToByteArray(TelegramData telegram, CommunicationData com) {
            var result = new List<byte>();
            try {
                var bCharacter = telegram.MessageID == null | string.IsNullOrEmpty(telegram.MessageID);
                this.HandleTelegramValueList(ref result, [.. telegram.TelegramValues], com.Endian, com, bCharacter);

                return [.. result];
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return [.. result];
            }
        }

        private void HandleTelegramValueList(ref List<byte> result, TelegramValueData[] list, int Endian, CommunicationData com, bool bCharacter = true) {
            try {
                foreach (var item in list) {
                    if (item.TelegramArray != null) {
                        this.HandleTelegramValueList(ref result, item.TelegramArray, Endian, com);
                    } else if (item.TelegramValues != null) {
                        if (item.TelegramValues.Count > 0) {
                            foreach (var itemList in item.TelegramValues.TelegramValueArray) {
                                this.HandleTelegramValueList(ref result, [.. itemList], Endian, com);
                            }
                        }
                    } else {

                        this.AddBytesToList(ref result, item, com, bCharacter);
                    }
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void AddBytesToList(ref List<byte> result, TelegramValueData telBlock, CommunicationData com, bool bCharacter = true) {
            var btArr = this.GetArrayValues(telBlock, com, bCharacter);
            Functions.AddList(ref result, btArr);
        }

        public byte[] GetArrayValues(TelegramValueData data, CommunicationData com, bool bCharacter = true) {
            _ = new byte[2];
            var result = new List<byte>();
            var builder = new StringBuilder();
            try {
                byte[] btArr;
                if (com.Endian > 1) {
                    if (data.Typ.Equals(TypeCode.String)) {
                        if (!data.InObject) {
                            if (data.Name is not "TelegramType" and not "TelegramLength") {
                                data.Value = "";
                            }
                        }
                        if (data.Value == null | data.Value == DBNull.Value) {
                            data.Value = " ";
                        }

                        btArr = Encoding.GetEncoding(1251).GetBytes(data.Value.ToString());
                        Functions.AddList(ref result, btArr);

                        // fill bytelist with empty strings
                        for (var n = btArr.Length; n < data.Length; n++) {
                            result.Add(Encoding.GetEncoding(1251).GetBytes(" ")[0]);
                        }
                    } else {
                        data.Value ??= "";

                        for (var n = 0; n < data.Length - data.Value.ToString().Length; n++) {
                            _ = data.Value.ToString().Length > n
                                ? data.Value.ToString().Substring(n).Equals("-", StringComparison.Ordinal)
                                    ? builder.Append(data.Value.ToString().Substring(n))
                                    : builder.Append('0')
                                : builder.Append('0');
                        }
                        _ = builder.Append(data.Value.ToString().Replace("-", ""));
                        data.Value = builder.ToString();
                        btArr = Encoding.GetEncoding(1251).GetBytes(data.Value.ToString());
                        Functions.AddList(ref result, btArr);
                    }
                } else {
                    if (data.Typ.Equals(TypeCode.String)) {
                        if (data.Value == null | data.Value == DBNull.Value) {
                            data.Value = "";
                        }

                        if (com.Endian < 1 && bCharacter) {
                            builder = new StringBuilder(data.Value.ToString());
                            for (var n = data.Value.ToString().Length; n < data.Length - 2; n++) {
                                _ = builder.Append(' ');
                            }

                            btArr = Encoding.GetEncoding(1251).GetBytes(builder.ToString());
                            var btList = new List<byte> {
                                (byte)(data.Length - 2),
                                (byte)(data.Length - 2)
                            };
                            btList.AddRange(btArr);

                            Functions.AddList(ref result, [.. btList]);
                        } else {
                            var str = data.Value.ToString();
                            for (var n = data.Value.ToString().Length; n < data.Length; n++) {
                                str = str.Insert(n, " ");
                            }

                            btArr = Encoding.GetEncoding(1251).GetBytes(str);
                            Functions.AddList(ref result, btArr);
                        }
                    } else {
                        //if (!data.InObject)
                        //{
                        //   if (data.Name != "TelegramType" && data.Name != "TelegramLength")
                        //      data.Value = 0;
                        //}

                        //if (data.Value == null)
                        //   data.Value = 0;
                        if (data.Factor == 0) {
                            data.Factor = 1;
                        }

                        btArr = data.Typ switch {
                            TypeCode.Single => BitConverter.GetBytes((float)Math.Round(Convert.ToSingle(data.Value) * data.Factor, 4)),
                            TypeCode.Int16 => BitConverter.GetBytes(Convert.ToInt16(data.Value)),
                            TypeCode.Int32 => BitConverter.GetBytes(Convert.ToInt32(data.Value)),
                            TypeCode.Int64 => BitConverter.GetBytes(Convert.ToInt64(data.Value)),
                            TypeCode.Double => BitConverter.GetBytes(Math.Round(Convert.ToDouble(data.Value) * (double)data.Factor, 4)),
                            TypeCode.Boolean => BitConverter.GetBytes(Convert.ToInt16(data.Value)),
                            TypeCode.Empty => throw new NotImplementedException(),
                            TypeCode.Object => throw new NotImplementedException(),
                            TypeCode.DBNull => throw new NotImplementedException(),
                            TypeCode.Char => throw new NotImplementedException(),
                            TypeCode.SByte => throw new NotImplementedException(),
                            TypeCode.Byte => throw new NotImplementedException(),
                            TypeCode.UInt16 => throw new NotImplementedException(),
                            TypeCode.UInt32 => throw new NotImplementedException(),
                            TypeCode.UInt64 => throw new NotImplementedException(),
                            TypeCode.Decimal => throw new NotImplementedException(),
                            TypeCode.DateTime => throw new NotImplementedException(),
                            TypeCode.String => throw new NotImplementedException(),
                            _ => [0],
                        };
                        if (com.Endian == 0) {
                            Array.Reverse(btArr);
                        }

                        Functions.AddList(ref result, btArr);
                    }
                }
                return [.. result];
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return [.. result];
            }
        }

        public byte[] GetArrayValue(TelegramValueData val, object value, CommunicationData com) {
            var btArr = new byte[2];

            try {
                value ??= 0;

                if (val.Factor == 0) {
                    val.Factor = 1;
                }

                switch (val.Typ) {
                    case TypeCode.Single:
                        btArr = BitConverter.GetBytes((float)Math.Round(Convert.ToSingle(value) * val.Factor, 4));
                        break;
                    case TypeCode.Int16:
                        btArr = BitConverter.GetBytes(Convert.ToInt16(value));
                        break;
                    case TypeCode.Int32:
                        btArr = BitConverter.GetBytes(Convert.ToInt32(value));
                        break;
                    case TypeCode.Int64:
                        btArr = BitConverter.GetBytes(Convert.ToInt64(value));
                        break;
                    case TypeCode.Double:
                        btArr = BitConverter.GetBytes(Math.Round(Convert.ToDouble(value) * (double)val.Factor, 4));
                        break;
                    case TypeCode.Boolean:
                        btArr = BitConverter.GetBytes(Convert.ToInt16(value));
                        break;
                    default:
                        if (value == null | value == DBNull.Value) {
                            value = " ";
                        }

                        btArr = Encoding.GetEncoding(1251).GetBytes(value.ToString());
                        var result = new List<byte>();
                        Functions.AddList(ref result, btArr);

                        // fill bytelist with empty strings
                        for (var n = btArr.Length; n < val.Length; n++) {
                            result.Add(Encoding.GetEncoding(1251).GetBytes(" ")[0]);
                        }

                        btArr = [.. result];
                        break;
                }
                if (com.Endian == 0) {
                    Array.Reverse(btArr);
                }

                return btArr;
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return btArr;
            }
        }
        //public byte[] GetArrayValues(TelegramValueData data, CommunicationData com)
        //{
        //   byte[] btArr = new byte[2];
        //   List<byte> result = new List<byte>();
        //   StringBuilder builder = new StringBuilder();
        //   try
        //   {
        //      if (com.Endian > 1)
        //      {
        //         if (data.Typ.Equals(TypeCode.String))
        //         {
        //            if (data.Value == null | data.Value == DBNull.Value)
        //               data.Value = " ";
        //            btArr = Encoding.GetEncoding(1251).GetBytes(data.Value.ToString());
        //            Functions.AddList<byte>(ref result, btArr);
        //            // fill bytelist with empty strings
        //            string pad = " ";

        //            for (int n = btArr.Length; n < data.Length; n++)
        //               result.Add(Encoding.GetEncoding(1251).GetBytes(pad)[0]);
        //         }
        //         else
        //         {
        //            if (data.Value == null)
        //               data.Value = "";
        //            for (int n = 0; n < data.Length - data.Value.ToString().Length; n++)
        //            {
        //               if (data.Value.ToString().Length > n)
        //               {
        //                  if (data.Value.ToString().Substring(n).Equals("-"))
        //                     builder.Append(data.Value.ToString().Substring(n));
        //                  else
        //                     builder.Append("0");
        //               }
        //               else
        //                  builder.Append("0");
        //            }
        //            builder.Append(data.Value.ToString().Replace("-", ""));
        //            data.Value = builder.ToString();
        //            btArr = Encoding.GetEncoding(1251).GetBytes(data.Value.ToString());
        //            Functions.AddList<byte>(ref result, btArr);
        //         }
        //      }
        //      else
        //      {
        //         if (!data.InObject)
        //         {
        //            btArr = new byte[data.Length];
        //            Functions.AddList<byte>(ref result, btArr);
        //         }
        //         else
        //         {
        //            if (data.Typ.Equals(TypeCode.String))
        //            {
        //               if (data.Value == null | data.Value == DBNull.Value)
        //                  data.Value = "";
        //               if (com.Endian < 1)
        //               {
        //                  builder = new StringBuilder(data.Value.ToString());
        //                  for (int n = data.Value.ToString().Length; n < data.Length - 2; n++)
        //                     builder.Append(" ");
        //                  btArr = Encoding.GetEncoding(1251).GetBytes(builder.ToString());
        //                  List<byte> btList = new List<byte>();
        //                  btList.Add((byte)(data.Length - 2));
        //                  btList.Add((byte)(data.Length - 2));
        //                  btList.AddRange(btArr);

        //                  Functions.AddList<byte>(ref result, btList.ToArray());
        //               }
        //               else
        //               {
        //                  string str = data.Value.ToString();
        //                  for (int n = data.Value.ToString().Length; n < data.Length; n++)
        //                     str = str.Insert(n, " ");
        //                  btArr = Encoding.GetEncoding(1251).GetBytes(str);
        //                  Functions.AddList<byte>(ref result, btArr);
        //               }
        //            }
        //            else
        //            {
        //               if (data.Value == null)
        //                  data.Value = 0;
        //               if (data.Factor == 0)
        //                  data.Factor = 1;
        //               switch (data.Typ)
        //               {
        //                  case TypeCode.Single:
        //                     btArr = BitConverter.GetBytes((float)Math.Round(Convert.ToSingle(data.Value) * data.Factor, 4));
        //                     break;
        //                  case TypeCode.Int16:
        //                     btArr = BitConverter.GetBytes(Convert.ToInt16(data.Value));
        //                     break;
        //                  case TypeCode.Int32:
        //                     btArr = BitConverter.GetBytes(Convert.ToInt32(data.Value));
        //                     break;
        //                  case TypeCode.Int64:
        //                     btArr = BitConverter.GetBytes(Convert.ToInt64(data.Value));
        //                     break;
        //                  case TypeCode.Double:
        //                     btArr = BitConverter.GetBytes(Math.Round(Convert.ToDouble(data.Value) * (double)data.Factor, 4));
        //                     break;
        //                  case TypeCode.Boolean:
        //                     btArr = BitConverter.GetBytes(Convert.ToInt16(data.Value));
        //                     break;
        //                  default:
        //                     btArr = new byte[] { (byte)0 };
        //                     break;
        //               }
        //               if (com.Endian == 0)
        //                  Array.Reverse(btArr);
        //               Functions.AddList<byte>(ref result, btArr);
        //            }
        //         }
        //      }
        //         // fill bytelist with zero values

        //         return result.ToArray<byte>();
        //   }
        //   catch (Exception e)
        //   {
        //      Logging.SendErrorMessage(System.Reflection.MethodInfo.GetCurrentMethod().Name, e, this.GetType());
        //      return result.ToArray<byte>();
        //   }
        //}
    }
}
