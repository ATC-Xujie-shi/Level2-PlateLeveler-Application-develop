using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Level2.PlateLeveler.DataTypes {
    public static class ExpansionMethods {
        public static void PrintToList<T>(this IEnumerable<T> list) {
            try {
                _ = list.ToList();
            } catch {

            }
        }

        public static Temp SetNextTemp(this IEnumerable<Temp> list, float temperature) {
            var temp = new Temp {
                Temperature = 0,
                YieldPoint = 0,
                EModule = 0
            };
            List<Temp> tempList = [temp, .. list];
            if (tempList.Count > 1) {
                for (var n = 1; n < tempList.Count; n++) {
                    if (tempList[n].Temperature.Value > temperature && tempList[n - 1].Temperature.Value < temperature) {
                        return tempList[n];
                    }
                }
            }
            return temp;
        }

        public static bool ChangeAndNotify<T>(this PropertyChangedEventHandler handler, ref T field, T value, Expression<Func<T>> memberExpression) {
            if (memberExpression == null) {
                throw new ArgumentNullException(nameof(memberExpression));
            }
            if (memberExpression.Body is not MemberExpression body) {
                throw new ArgumentException("Lambda must return a property.");
            }
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return false;
            }

            if (body.Expression is ConstantExpression vmExpression) {
                var lambda = Expression.Lambda(vmExpression);
                var vmFunc = lambda.Compile();
                var sender = vmFunc.DynamicInvoke();

                handler?.Invoke(sender, new PropertyChangedEventArgs(body.Member.Name));
            }

            field = value;
            return true;
        }
    }
}
