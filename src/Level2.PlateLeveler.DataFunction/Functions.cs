using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace Level2.PlateLeveler.DataFunction {
    public static class Functions {
        public static DirectoryInfo GetSettingsDirectory() {
            var sFldSetting = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dir = new DirectoryInfo(sFldSetting);

            if (!dir.Exists) {
                dir.Create();
            }

            return dir;
        }

        public static string App_Path() => Right(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase), 6);

        public static string Left(string str, int length) => str.Remove(length, str.Length - length);

        public static string Right(string str, int length) => str.Remove(0, length);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        public static List<float> GetTargetCurveFromIniFile(string iniFile, string category) {
            var buffer = new byte[2048];

            _ = GetPrivateProfileSection(category, buffer, 2048, iniFile);
            var tmp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');
            _ = new string[2];

            var result = new List<float>();
            foreach (var item in tmp) {
                var temp = item.Split('=');
                if (IsNumber<float>(temp[1])) {
                    result.Add(Convert.ToSingle(temp[1]));
                } else {
                    result.Add(0f);
                }
            }
            return result;
        }

        public static void StartProcess(string str_Path) => Process.Start(str_Path);

        public static bool CheckIfProcessIsRunning(string str_Path) {
            var n = str_Path.LastIndexOf(@"\");
            var str_Filename = str_Path.Substring(n + 1, str_Path.LastIndexOf(@".") - n - 1);
            return Process.GetProcessesByName(str_Filename).Length > 0;
        }

        public static string getUserName() {
            var str = @"\";
            var arrStr = WindowsIdentity.GetCurrent().Name.Split(str.ToCharArray());
            return arrStr[1].ToLower(CultureInfo.CurrentCulture);
        }

        public static string getDomainName() {
            var str = @"\";
            var arrStr = WindowsIdentity.GetCurrent().Name.Split(str.ToCharArray());
            return arrStr[0];
        }

        public static object GetValueByS7TypeString(string str_Type, byte[] arrValue) {
            object result;
            var btList = new List<byte>();
            var idx = 0;
            switch (str_Type) {
                case "int":
                    Array.Reverse(arrValue);
                    result = BitConverter.ToInt16(arrValue, 0);
                    break;
                case "real":
                    Array.Reverse(arrValue);
                    result = BitConverter.ToSingle(arrValue, 0);
                    break;
                default:
                    for (var n = arrValue.Length - 1; n >= 0; n--) {
                        if (arrValue[n] != 0) {
                            idx = n;
                            break;
                        }
                    }
                    for (var n = 0; n <= idx; n++) {
                        btList.Add(arrValue[n]);
                    }

                    arrValue = [.. btList];
                    btList = [];
                    for (var n = 0; n < arrValue.Length; n++) {
                        if (arrValue[n] != 0) {
                            btList.Add(arrValue[n]);
                        }
                    }

                    result = btList.Count > 0 ? Encoding.ASCII.GetString([.. btList]).Trim() : (object)null;

                    break;
            }

            return result;
        }

        public static float GetApproximateValue(float val, bool bThickness) {
            var result = bThickness ? val : val / 100;
            var val1 = (float)Math.Floor((double)result);
            var val2 = val1 + (float)0.5;
            if (val2 < result) {
                val1 = val2;
                val2 += (float)0.5;
            }
            result = (result - val1 > val2 - result) ? val2 : val1;

            return bThickness ? result : result * 100;
        }

        public static object GetValueByL3TypeString(string format, byte[] arrValue) {
            var str = Encoding.ASCII.GetString(arrValue).Trim();

            if (string.IsNullOrEmpty(str)) {
                return null;
            }

            object result;
            switch (format) {
                case "real":
                    if (str.Substring(0, 1).Equals("-", StringComparison.Ordinal)) {
                        str = str.Remove(0);
                    }

                    result = Convert.ToSingle(str.Replace(".", ","));
                    break;
                case "int":
                    if (str.Substring(0, 1).Equals("-", StringComparison.Ordinal)) {
                        str = str.Remove(0);
                    }

                    result = Convert.ToInt16(str);
                    break;
                default:
                    result = str.Trim();
                    break;
            }
            return result;
        }

        // Convert the Value of the database in a string which could be sendet back to Level3
        public static byte[] ConvertValueToString(object Value, int Length, float factor) {
            var str = "";
            if (Value == null) {
                for (var n = 0; n < Length; n++) {
                    str += "0";
                }
            } else if (Value is string) {
                str = Value.ToString();
                for (var n = str.Length; n < Length; n++) {
                    str += " ";
                }
            } else {
                int val;
                if (Value is float v) {
                    var fl = factor > 0 ? v / factor : v;
                    val = Convert.ToInt32(fl);
                } else {
                    val = Convert.ToInt32(Value);
                }

                str = val < 0 ? "-" : "";
                for (var n = val.ToString().Length; n < Length; n++) {
                    str += "0";
                }

                str += val.ToString();
            }
            Encoding enc = new ASCIIEncoding();
            return enc.GetBytes(str);
        }

        public static T[] GetValue<T>(object obj) => (T[])Convert.ChangeType(obj, typeof(T[]));

        public static byte[] ConvertS7StringToByteArray(string Value, int Length) {
            var btArr = new byte[Value.Length];
            _ = new List<byte>();

            Encoding enc = new ASCIIEncoding();

            if (!string.IsNullOrEmpty(Value)) {
                btArr = enc.GetBytes(Value);
            }

            List<byte> result = [.. btArr];

            for (var n = Value.Length; n < Length; n++) {
                result.Add(0);
            }
            return [.. result];
        }

        public static string ChangeSampleID(string samplid) {
            try {
                var SamplidPart = samplid.Length > 1 ? samplid.Substring(1) : "";
                var n = Convert.ToInt32(samplid.Substring(0, 1)) + 64;
                var chr = (char)n;
                return chr.ToString() + SamplidPart;
            } catch {
                return samplid;
            }
        }

        public static string GetTimeString(DateTime date) => date.ToString("hhmmss");

        public static string GetDateString(DateTime date) => date.ToString("yyyyMMdd");

        public static string GetDateTimeString(DateTime date) => date.ToString("yyyyMMddHHmmss");

        public static DateTime GetDateFromString(string strDate) {
            var dt = strDate.Substring(0, 4) + "." + strDate.Substring(4, 2) + "." + strDate.Substring(6, 2) + " " + strDate.Substring(8, 2) + ":" + strDate.Substring(10, 2) + ":" + strDate.Substring(12, 2);
            return DateTime.Parse(dt, new CultureInfo("de-DE"));
        }

        /// <summary>
        /// Get a string from DateTime usable in database, schema: YYYY.MM.DD HH:MM:SS
        /// <br/>NOTE: Renember to put this string in '' when using as SQL command!
        /// </summary>
        /// <param name="date">Date in DateTime</param>
        /// <returns>String for DB</returns>
        public static string GetDBStringDateTimeFromDateTime(DateTime date) => date.ToString("yyyy.MM.dd HH:mm:ss");//Other attemps://return date.ToString("yyyy-MM-dd HH:mm:ss.fff");//return date.ToString("s");//return date.ToString(System.Globalization.CultureInfo.InvariantCulture);//return date.ToString("s", System.Globalization.CultureInfo.InvariantCulture);

        public static string GetAppPath() => Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);

        public static void AddList<T>(ref List<T> baseList, T[] addList) {
            foreach (var item in addList) {
                baseList.Add(item);
            }
        }

        public static string GetStringByLength(object val, int length) {
            var str = "";
            if (val is string) {
                for (var n = val.ToString().Length; n < length; n++) {
                    str = val.ToString() + " ";
                }
            } else {
                for (var n = val.ToString().Replace(",", ".").Length; n < length; n++) {
                    str += "0";
                }

                str += val.ToString().Replace(",", ".");
            }
            return str;
        }

        public static string GetStringFirstLetterBig(string str) {
            var str1 = str.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) + str.Substring(1).ToLower(CultureInfo.CurrentCulture);
            var str2 = "id";
            return str1.Replace(str2, str2.ToUpper(CultureInfo.CurrentCulture));
        }

        public static bool IsNumber(string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }

            foreach (var chr in text.ToCharArray()) {
                if (!char.IsNumber(chr)) {
                    return false;
                }
            }
            return true;
        }

        public static bool IsNumber(string text, char delimiter) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }

            foreach (var chr in text.ToCharArray()) {
                if (!char.IsNumber(chr) && chr != delimiter) {
                    return false;
                }
            }
            return true;
        }

        public static string AddStringByCount(char chr, int count) {
            var list = new List<char>();
            for (var n = 0; n < count; n++) {
                list.Add(chr);
            }

            return new string([.. list]);
        }

        public static string ConvertFloatToStringFormat(int count) {
            //string result = "{0:0";
            //if (count > 0)
            //   result += ",";
            //result += AddStringByCount('0', count);
            //return result + "}";
            var result = "0";
            if (count > 0) {
                result += ".";
            }

            result += AddStringByCount('0', count);
            return result;
        }
        public static bool IsNumber<T>(string text) {
            try {
                var t = (T)Convert.ChangeType(text, typeof(T));
                return true;
            } catch {
                return false;
            }
        }
        public static DbType GetDbType(object obj) {
            if (obj is short) {
                return DbType.Int16;
            } else {
                return obj is int
                    ? DbType.Int32
                    : obj is long
                                    ? DbType.Int64
                                    : obj is double
                                                    ? DbType.Double
                                                    : obj is float ? DbType.Single : obj is DateTime ? DbType.DateTime : obj is Array ? DbType.Binary : DbType.String;
            }
        }

        public static string GetInitPath(string key) => ConfigurationManager.AppSettings[key].ToString();

        public static void CreatePath(string path) {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists) {
                dir.Create();
            }
        }

        public static object ConvertByteArrayToObjectWithType(byte[] btArr, TypeCode code, bool bEndian, string trimStr = "") {
            _ = new object();

            object result;
            switch (code) {
                case TypeCode.Byte:
                    result = btArr[0];
                    break;
                case TypeCode.Double:
                    result = bEndian ? BitConverter.ToDouble(btArr, 0) : Convert.ToDouble(Encoding.GetEncoding(1251).GetString(btArr).ToString().Trim());
                    break;
                case TypeCode.Int16:
                    result = bEndian ? BitConverter.ToInt16(btArr, 0) : Convert.ToInt16(Encoding.GetEncoding(1251).GetString(btArr).ToString().Trim());
                    break;
                case TypeCode.Int32:
                    result = bEndian ? BitConverter.ToInt32(btArr, 0) : Convert.ToInt32(Encoding.GetEncoding(1251).GetString(btArr).ToString().Trim());
                    break;
                case TypeCode.Single:
                    result = bEndian ? BitConverter.ToSingle(btArr, 0) : Convert.ToSingle(Encoding.GetEncoding(1251).GetString(btArr).ToString().Trim());
                    break;
                default:
                    var btStringArr = new byte[btArr.Length];
                    for (var n = 0; n < btArr.Length; n++) {
                        btStringArr[n] = btArr[n].Equals(0) ? (byte)32 : btArr[n];
                    }

                    result = !string.IsNullOrEmpty(trimStr) ? Encoding.GetEncoding(1251).GetString(btStringArr).ToString().Trim(trimStr.ToCharArray()) : Encoding.GetEncoding(1251).GetString(btStringArr).ToString().Trim();
                    if (string.IsNullOrEmpty(result.ToString())) {
                        result = null;
                    }

                    break;
            }

            return result;
        }
        public static T GetObjectFromByteArray<T>(byte[] btArr, TypeCode code, bool bNumeric = true) {
            object obj;
            switch (code) {
                case TypeCode.Boolean:
                    obj = bNumeric ? BitConverter.ToBoolean(btArr, 0) : Convert.ToBoolean(Encoding.GetEncoding(1251).GetString(btArr).ToString().Trim());
                    break;
                case TypeCode.Byte:
                    obj = (T)Convert.ChangeType(btArr[0], code);
                    break;
                case TypeCode.Double:
                    obj = bNumeric ? BitConverter.ToDouble(btArr, 0) : Convert.ToDouble(Encoding.GetEncoding(1251).GetString(btArr).ToString().Trim());
                    break;
                case TypeCode.Int16:
                    obj = bNumeric ? BitConverter.ToInt16(btArr, 0) : Convert.ToInt16(Encoding.GetEncoding(1251).GetString(btArr).ToString().Trim());
                    break;
                case TypeCode.Int32:
                    obj = bNumeric ? BitConverter.ToInt32(btArr, 0) : Convert.ToInt32(Encoding.GetEncoding(1251).GetString(btArr).ToString().Trim());
                    break;
                case TypeCode.Int64:
                    obj = bNumeric ? BitConverter.ToInt64(btArr, 0) : Convert.ToInt64(Encoding.GetEncoding(1251).GetString(btArr).ToString().Trim());
                    break;
                case TypeCode.Single:
                    obj = bNumeric ? BitConverter.ToSingle(btArr, 0) : Convert.ToSingle(Encoding.GetEncoding(1251).GetString(btArr).ToString().Trim());
                    break;
                default:
                    var btList = btArr.ToList();
                    btArr = [.. btList.GetRange(2, btList.Count - 2)];
                    obj = Encoding.GetEncoding(1251).GetString(btArr).ToString().Trim();
                    break;
            }
            return (T)Convert.ChangeType(obj, code);
        }

        public static byte[] GetBitString(bool[] bArray) {
            var res = bArray.Select(Convert.ToInt32).ToArray();
            return GetBitString(res);
        }

        public static byte[] GetByteArrayFromType<T>(T obj, TypeCode type, int length = 0) {
            var btList = new List<byte>();

            switch (type) {
                case TypeCode.Boolean:
                    btList.AddRange(BitConverter.GetBytes(Convert.ToBoolean(obj)));
                    break;
                case TypeCode.Byte:
                    btList.AddRange(BitConverter.GetBytes(Convert.ToByte(obj)));
                    break;
                case TypeCode.Double:
                    btList.AddRange(BitConverter.GetBytes(Convert.ToDouble(obj)));
                    break;
                case TypeCode.Int16:
                    btList.AddRange(BitConverter.GetBytes(Convert.ToInt16(obj)));
                    break;
                case TypeCode.Int32:
                    btList.AddRange(BitConverter.GetBytes(Convert.ToInt32(obj)));
                    break;
                case TypeCode.Int64:
                    btList.AddRange(BitConverter.GetBytes(Convert.ToInt64(obj)));
                    break;
                case TypeCode.Single:
                    btList.AddRange(BitConverter.GetBytes(Convert.ToSingle(obj)));
                    break;
                default:
                    var str = obj.ToString();
                    for (var n = str.Length; n < length; n++) {
                        str += " ";
                    }

                    return Encoding.GetEncoding(1251).GetBytes(str);
            }
            return [.. btList];
        }

        public static byte[] GetBitString(int[] intArray) {
            var strArray = intArray.Select(x => Convert.ToString(x)).ToArray();
            var str = string.Join("", strArray);
            var n = (int)Math.Ceiling(str.Length / 8m);

            var bytesAsStr = Enumerable.Range(0, n).Select(i => str.Substring(8 * i, Math.Min(8, str.Length - (8 * i)))).ToArray();
            var result = bytesAsStr.Select(z => Convert.ToByte(z, 2)).ToArray();

            return result;
        }

        public static bool[] GetBoolArrayFromBytes(byte[] btArray) {
            var strArr = btArray.Select(x => Convert.ToString(x, 2).PadLeft(8, '0')).ToArray();
            var str = string.Join("", strArr);
            var chrList = strArr[0].Reverse().ToList();
            chrList.AddRange([.. strArr[1].Reverse()]);
            var chrArr = chrList.ToArray();

            strArr = [.. chrArr.Select(y => Convert.ToString(y))];
            var intArray = Array.ConvertAll(strArr, s => int.Parse(s));

            return [.. intArray.Select(Convert.ToBoolean)];
        }
        //public static bool[] GetBoolArrayFromBytes(byte[] btArray)
        //{
        //   string[] strArr = btArray.Select(x => Convert.ToString(x, 2).Reverse<char>().PadLeft(8, '0')).ToArray();
        //   string str = string.Join("", strArr);
        //   char[] chrArr = str.ToCharArray();
        //   int[] intArray = Array.ConvertAll(chrArr, s => int.Parse(s.ToString()));
        //   return intArray.Select(z => Convert.ToBoolean(z)).ToArray();
        //}

        public static int[] GetIntArrayFromBytes(byte[] btArray) {
            var strArr = btArray.Select(x => Convert.ToString(x, 2).PadLeft(8, '0')).ToArray();
            var str = "";

            for (var n = strArr.Length - 1; n >= 0; n--) {
                str += strArr[n];
            }

            strArr = [.. str.ToCharArray().Select(y => Convert.ToString(y))];
            var strList = strArr.Reverse();
            return Array.ConvertAll(strList.ToArray(), s => int.Parse(s));
        }

        public static int? GetIntFromHexString(string hexString) {
            try {
                return int.Parse(hexString, NumberStyles.AllowHexSpecifier);
            } catch {
                return null;
            }
        }

        public static DateTime GetMonday(int week, int year) {
            // die 1. KW ist die mit mindestens 4 Tagen im Januar des nächsten Jahres
            var dt = new DateTime(year, 1, 4);

            // Beginn auf Montag setzten
            dt = dt.AddDays(-(int)((dt.DayOfWeek != DayOfWeek.Sunday) ? dt.DayOfWeek - 1 : DayOfWeek.Saturday));

            // Wochen dazu addieren
            return dt.AddDays((--week) * 7);
        }

        public static int GetCalendarWeek(DateTime date) {
            var myCal = CultureInfo.CurrentUICulture.Calendar;
            return myCal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static string GetMessageBoxQuestion(bool bDelete) {
            var result = "Do you want to ";
            result += bDelete ? "delete" : "save";
            result += " this item?";
            return result;
        }

        #region Data
        /// <summary>
        /// Check if given object is of Type Byte, SByte, Char, Int16, UInt16, Int32, UInt32, Int64, UInt64, Decimal, Single or Double
        /// </summary>
        /// <param name="o">Object to check</param>
        /// <returns>True if one of the above, False if not</returns>
        public static bool IsNumericType(object o) => IsNumericType(o.GetType());

        /// <summary>
        /// Check if given object is of Type Byte, SByte, Char, Int16, UInt16, Int32, UInt32, Int64, UInt64, Decimal, Single or Double
        /// </summary>
        /// <param name="o">Type to check</param>
        /// <returns>True if one of the above, False if not</returns>
        public static bool IsNumericType(Type t) {
            return Type.GetTypeCode(t) switch {
                TypeCode.Byte or TypeCode.SByte or TypeCode.Char or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal => true,
                TypeCode.Empty => throw new NotImplementedException(),
                TypeCode.Object => throw new NotImplementedException(),
                TypeCode.DBNull => throw new NotImplementedException(),
                TypeCode.Boolean => throw new NotImplementedException(),
                TypeCode.DateTime => throw new NotImplementedException(),
                TypeCode.String => throw new NotImplementedException(),
                _ => false,
            };
        }
        #endregion Data
    }
}
