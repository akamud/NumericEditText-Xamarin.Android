# NumericEditText-Xamarin.Android
A NumericEditText for Xamarin.Android that accepts decimal numbers that work with any culture

## Motivation
The original Android EditText has this annoying bug when used with `inputType=number|numberDecimal`, it won't work for different cultures that use different decimal separators (like pt-BR's , (comma)), so you can't have it accept 105,60 as a valid number.

This project is based on two other projects:  
[Android-NumberEditText](https://github.com/hyperax/Android-NumberEditText) by [@hyperax](https://github.com/hyperax)  
[numeric-edittext](https://github.com/hidroh/numeric-edittext) by [@hidroh](https://github.com/hidroh)

They were adapted to fit my goals, it is a bit hacky but  I think someone else might find it useful.

## License
[MIT License](https://github.com/akamud/NumericEditText-Xamarin.Android/blob/master/LICENSE)
