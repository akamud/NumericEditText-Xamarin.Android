# NumericEditText-Xamarin.Android
![](https://raw.githubusercontent.com/akamud/NumericEditText-Xamarin.Android/master/art/Icon.png)

A NumericEditText for Xamarin.Android that accepts decimal numbers that work with any culture

It will automatically get the phone's language using `CultureInfo.CurrentCulture` to figure out which characters to use as `CurrencyGroupSeparator` and `NumberDecimalSeparator` and automatically add them as you type.

## Installing
![](https://img.shields.io/nuget/v/NumericEditText-Xamarin.Android.svg?style=flat)  
[NuGet package](https://www.nuget.org/packages/NumericEditText-Xamarin.Android/) available:
```
PM> Install-Package NumericEditText-Xamarin.Android
```

## Using
 Add the `res-auto` namespace:
```XML
xmlns:num="http://schemas.android.comas/apk/res-auto"
```
Then you can just use the component like so:
```XML
<br.com.akamud.NumericEditText 
	android:id="@+id/txtNumeric"
	android:layout_width="match_parent"
	android:layout_height="wrap_content"
	android:inputType="number|numberDecimal" />
```

To get the number typed without mask you can use the method `GetNumericValue()`:

```C#
double number = txtNumeric.GetNumericValue();
```
If the input is invalid it will return a `double.NaN`

### Changing precision
By default it uses 2 decimal digits and (virtually) infinite number digits, but you can change it to whatever you need using two attributes:  

Attribute | Description | Default Value  
:----: | :-------: | :---------:  
maxDigitsBeforeDecimal | Sets the maximum number of digits before the decimal point | 0 (infinite)   
maxDigitsAfterDecimal | Sets the maximum number of digits after the decimal point | 2

```XML
<br.com.akamud.NumericEditText 
	android:id="@+id/txtNumeric"
	android:layout_width="match_parent"
	android:layout_height="wrap_content"
	android:inputType="number|numberDecimal"
	num:maxDigitsBeforeDecimal="6"
	num:maxDigitsAfterDecimal="4" />
```

### Events
`NumericEditText` fires two events:  
`NumericValueChanged` when the value typed is changed  
`NumericValueCleared` when the input is cleared  
You can use them like a regular event:

```C#
txtNumeric.NumericValueCleared += (object sender, NumericValueClearedEventArgs e) => { 
	// Value cleared
};

txtNumeric.NumericValueChanged += (object sender, NumericValueChangedEventArgs e) => { 
	double newValue = e.NewValue;
	// New value
};
```

## Examples  
Using `en-US` culture:  
Input:
```
100,000.00
```
Output:
```
// double
100000.00
```

Using `pt-BR` culture:  
Input:
```
100.000,00
```
Output:
```
// double
100000.00
```

## Gif examples
### en-US culture:
![en-US gif](https://raw.githubusercontent.com/akamud/NumericEditText-Xamarin.Android/master/enus-sample.gif)

### pt-BR culture:
![pt-BR gif](https://raw.githubusercontent.com/akamud/NumericEditText-Xamarin.Android/master/ptbr-sample.gif)

## Motivation
The original Android EditText has [this annoying bug](https://code.google.com/p/android/issues/detail?id=2626) when used with `inputType="number|numberDecimal"`, it won't work for different cultures that use different decimal separators (like pt-BR's `,` (comma)), so you can't have it accept `105,60` as a valid number.

This project is based on two other projects:  
[Android-NumberEditText](https://github.com/hyperax/Android-NumberEditText) by [@hyperax](https://github.com/hyperax)  
[numeric-edittext](https://github.com/hidroh/numeric-edittext) by [@hidroh](https://github.com/hidroh)

They were adapted to fit my goals, it is a bit hacky but  I think someone else might find it useful.

## License
[MIT License](https://github.com/akamud/NumericEditText-Xamarin.Android/blob/master/LICENSE)
