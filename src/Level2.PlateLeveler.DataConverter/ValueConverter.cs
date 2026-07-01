using System;
using System.Reflection;
using Level2.PlateLeveler.DataFunction;

namespace Level2.PlateLeveler.DataConverter {
    public class ValueConverter {
        public object ChangeIntToFloat100<T>(T obj) {
            object result = obj;
            try {
                var infos = obj.GetType().GetProperties();
                foreach (var propertyInfo in infos) {
                    if (propertyInfo.PropertyType != typeof(DateTime?)) {
                        if (propertyInfo.PropertyType != typeof(DateTime)) {
                            if (propertyInfo.PropertyType.IsArray) {
                                var arr = (Array)propertyInfo.GetValue(result, null);
                                for (var n = 0; n < arr.Length; n++) {
                                    if (arr.GetValue(n).GetType().Equals(typeof(float))) {
                                        arr.SetValue(Convert.ToSingle(arr.GetValue(n)) / 100, n);
                                    }
                                }

                                propertyInfo.SetValue(typeof(Array), arr);
                            } else
                               if (propertyInfo.GetValue(obj, null).GetType().Equals(typeof(float))) {
                                propertyInfo.SetValue(obj, Convert.ToSingle(propertyInfo.GetValue(result, null)) / 100);
                            }
                        }
                    }
                }
                return result;
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return result;
            }
        }

        public object ChangeFloatToInt100<T>(T obj) {
            object result = obj;
            try {
                var infos = obj.GetType().GetProperties();
                foreach (var propertyInfo in infos) {
                    if (propertyInfo.PropertyType.IsArray) {
                        var arr = (Array)propertyInfo.GetValue(result, null);
                        if (arr != null) {
                            for (var n = 0; n < arr.Length; n++) {
                                if (arr.GetValue(n).GetType().Equals(typeof(float))) {
                                    arr.SetValue(Convert.ToSingle(arr.GetValue(n)) * 100, n);
                                }
                            }
                            //propertyInfo.SetValue(typeof(Array), arr);
                        }
                    } else {
                        if (propertyInfo.GetValue(obj, null) != null) {
                            if (propertyInfo.GetValue(obj, null).GetType().Equals(typeof(float))) {
                                propertyInfo.SetValue(obj, Convert.ToSingle(Math.Round(Convert.ToSingle(propertyInfo.GetValue(result, null)) * 100f)));
                            }
                        } else {
                            if (propertyInfo.PropertyType.Equals(typeof(float))) {
                                propertyInfo.SetValue(obj, 0f);
                            } else if (propertyInfo.PropertyType.Equals(typeof(short))) {
                                propertyInfo.SetValue(obj, 0);
                            } else if (propertyInfo.PropertyType.Equals(typeof(int))) {
                                propertyInfo.SetValue(obj, 0);
                            } else if (propertyInfo.PropertyType.Equals(typeof(string))) {
                                propertyInfo.SetValue(obj, "");
                            }
                        }
                    }
                }
                return result;
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }
    }
}
