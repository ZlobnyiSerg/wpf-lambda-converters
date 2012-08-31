using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using DynamicExpression = System.Linq.Dynamic.DynamicExpression;
using Expression = System.Linq.Expressions.Expression;

namespace WpfLambdaConverters
{
    public class ExpressionKey
    {
        public string Expression { get; private set; }

        public object Parameter { get; private set; }

        public object Value { get; private set; }

        public Type TargetType { get; private set; }

        public Type ParameterType { get; private set; }

        public Type ValueType { get; private set; }

        private static Type GetObjectType(object o)
        {
            return o == null ? typeof(object) : o.GetType();
        }

        public ExpressionKey(string expression, Type targetType, object parameter, object value)
        {
            if (string.IsNullOrEmpty(expression))
                throw new ArgumentException("'expression' must be a non-empty string");

            Parameter = parameter;
            Value = value;

            Expression = expression;
            TargetType = targetType;
            ParameterType = GetObjectType(parameter);
            ValueType = GetObjectType(value);
        }

        // All the methods below have to access certain member properties in the same order.
        // Therefore it makes sence to decouple traversal from computation.
        IEnumerable<object> GetList()
        {
            yield return Expression;
            yield return TargetType;
            yield return ParameterType;
            yield return ValueType;
        }

        public override string ToString()
        {
            return GetList().Aggregate(new StringBuilder(), (sb, item) => sb.Append(item).Append(';')).ToString();
        }

        public override bool Equals(object obj)
        {
            return (obj is ExpressionKey && GetList().SequenceEqual((obj as ExpressionKey).GetList()));
        }

        public override int GetHashCode()
        {
            return GetList().Aggregate(0, (hash, o) => hash ^ o.GetHashCode());
        }
    }


    /// <summary>
    /// Usage examples see at http://archive.msdn.microsoft.com/LambdaConverter
    /// </summary>
    [ContentProperty("Lambda")]
    public class LambdaConverter : IValueConverter
    {
        private readonly static Dictionary<ExpressionKey, Delegate> CompiledCache = new Dictionary<ExpressionKey, Delegate>();


        public string Lambda { get; set; }
        public string BackLambda { get; set; }

        protected object ConvertValue(ExpressionKey key)
        {
            Delegate compiled;
            if (!CompiledCache.TryGetValue(key, out compiled))
            {
                var inparams = new[]
                                   {
                                       Expression.Parameter(key.ParameterType, "parameter"),
                                       Expression.Parameter(key.ValueType, "value"),
                                   };

                var lambda = DynamicExpression.ParseLambda(inparams, key.TargetType, key.Expression, key.Parameter, key.Value);
                compiled = lambda.Compile();

                CompiledCache.Add(key, compiled);
            }
            
            var result = compiled.DynamicInvoke(key.Parameter, key.Value);
            return result;
        }

        #region IValueConverter members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertValue(new ExpressionKey(Lambda, targetType, parameter, value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertValue(new ExpressionKey(BackLambda, targetType, parameter, value));
        }

        #endregion


    }

    [ContentProperty("Lambda")]
    public class MultiLambdaConverter : IMultiValueConverter
    {
        private readonly static Dictionary<ExpressionKey, Delegate> CompiledCache = new Dictionary<ExpressionKey, Delegate>();


        public string Lambda { get; set; }
        public string BackLambda { get; set; }

        private static object ConvertValue(ExpressionKey key)
        {
            Delegate compiled;
            if (!CompiledCache.TryGetValue(key, out compiled))
            {
                var inparams = new[]
                                   {
                                       Expression.Parameter(key.ParameterType, "parameter"),
                                       Expression.Parameter(key.ValueType, "values"),
                                   };

                var lambda = DynamicExpression.ParseLambda(inparams, key.TargetType, key.Expression, key.Parameter, key.Value);
                compiled = lambda.Compile();

                CompiledCache.Add(key, compiled);
            }

            var result = compiled.DynamicInvoke(key.Parameter, key.Value);
            return result;
        }

        #region IValueConverter members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertValue(new ExpressionKey(Lambda, targetType, parameter, values));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
