using System;
using System.Collections.Generic;
using System.Reflection;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.DataConverter {
    public class PriorityConverter {
        public FaultCompensation LoadFaultCompensation(IEnumerable<PrioritiesAttribute> priorities) {
            var result = new FaultCompensation();
            try {
                foreach (var item in priorities) {
                    var info = typeof(FaultCompensation).GetProperty(item.Priority);
                    info.SetValue(result, item.PriorityID);
                }

                return result;
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return result;
            }
        }

        public PriorityList LoadPriorities(FaultCompensation fc) {
            var result = new PriorityList();
            _ = new PrioritiesAttribute();
            var type = fc.GetType();
            var propertyInfos = type.GetProperties();
            foreach (var propertyInfo in propertyInfos) {
                var attr = (PrioritiesAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(PrioritiesAttribute));
                if (attr != null) {
                    var obj = propertyInfo.GetValue(fc, null);
                    if (obj != null) {
                        attr.PriorityID = int.Parse(obj.ToString());
                    }

                    result.Add(attr);
                }
            }
            return result;
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

        public List<Limitation> GetLimitationDB(LimitationData limit) {
            var result = new List<Limitation>();
            _ = new Limitation();
            var infos = limit.GetType().GetProperties();
            foreach (var propertyInfo in infos) {
                var l = (Limitation)Attribute.GetCustomAttribute(propertyInfo, typeof(Limitation));
                if (l != null) {
                    var obj = propertyInfo.GetValue(limit, null);
                    if (obj != null) {
                        l.Value = Convert.ToSingle(obj);
                    }

                    result.Add(l);
                }
            }
            return result;
        }
    }
}
