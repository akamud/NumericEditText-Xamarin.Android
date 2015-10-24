# NumericEditText-Xamarin.Android
A NumericEditText for Xamarin.Android that accepts decimal numbers that work with any culture

## Usage

It will automatically get the phone's language using `CultureInfo.CurrentCulture` to figure out which characters to use as `CurrencyGroupSeparator` and `NumberDecimalSeparator` and automatically add them as you type.

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

To get the number typed without mask you can use the method `GetNumericValue()`:
```C#
double value = txtNumeric.GetNumericValue();
```

### Examples  
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

## Motivation
The original Android EditText has this annoying bug when used with `inputType=number|numberDecimal`, it won't work for different cultures that use different decimal separators (like pt-BR's `,` (comma)), so you can't have it accept `105,60` as a valid number.

This project is based on two other projects:  
[Android-NumberEditText](https://github.com/hyperax/Android-NumberEditText) by [@hyperax](https://github.com/hyperax)  
[numeric-edittext](https://github.com/hidroh/numeric-edittext) by [@hidroh](https://github.com/hidroh)

They were adapted to fit my goals, it is a bit hacky but  I think someone else might find it useful.

## License
[MIT License](https://github.com/akamud/NumericEditText-Xamarin.Android/blob/master/LICENSE)
