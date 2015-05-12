WPF Lambda Converters
---------------------

Set of dynamic lambda-expression converters (for use with Binding). They're based on expression parser came with examples of VS 2008. All expressions are compiled and cached.

Examples
========

```xml
<LambdaConv:LambdaConverter x:Key="NullToVisibility">
    value==null?Visibility.Hidden:Visibility.Visible
</LambdaConv:LambdaConverter>
```

```xml
<LambdaConv:LambdaConverter x:Key="VisibleWhenPositive">
    value&gt;0?Visibility.Visible:Visibility.Hidden
</LambdaConv:LambdaConverter>
```

Multi-value converting is also supported:
```xml
<LambdaConv:MultiLambdaConverter x:Key="ProgressToMarginConverter">
    Helper.CreateThickness(0, 0, Convert.ToDouble(values[0]) * Convert.ToDouble(values[1]) / 100 - 1, 0)    
</LambdaConv:MultiLambdaConverter>
```
