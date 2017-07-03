using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Text;

namespace Akamud.Numericedittext
{
    /// <summary>
    /// Numeric Edit Text that supports decimal separator and group separator according to your culture.
    /// </summary>
    [Register("br.com.akamud.NumericEditText")]
    public class NumericEditText : EditText
    {
        /// <summary>
        /// Gets or sets the maximum number of digits before the decimal point.
        /// </summary>
        /// <value>The maximum number of digits before the decimal point</value>
        public int MaxDigitsBeforeDecimal { get; set; }


        /// <summary>
        /// Gets or sets the maximum number of digits after the decimal point.
        /// </summary>
        /// <value>The maximum number of digits after the decimal point</value>
        public int MaxDigitsAfterDecimal { get; set; }

        /// <summary>
        /// If flag is seted, string value will be with Currency symbol.
        /// </summary>
        public bool ShowCurrencySymbol { get; set; }

        /// <summary>
        /// If ShowCurrencySymbol sets to <b>True</b>, you can override the currency symbol to any other char
        /// <remarks>
        /// If you put more that 1 char, will using onlu first simbol
        /// </remarks>
        /// </summary>
        public string OverrideCurrencySymbol { get; set; } = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;

        private new static readonly string Tag = "X:" + typeof(NumericEditText).Name;

        private const int DefaultDigitsBeforeDecimal = 0;
        private const int DefaultDigitsAfterDecimal = 2;
        private const bool DefaultShowCurrencySymbol = false;
        private const string LeadingZeroFilterRegex = "^0+(?!$)";
        private const string AcceptedKeys = "0123456789,.";

        private readonly string _groupingSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator;
        private readonly string _decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private readonly int _currencyPositivePattern = CultureInfo.CurrentCulture.NumberFormat.CurrencyPositivePattern;

        private string _previousText = string.Empty;
        private string _numberFilterRegex = string.Empty;

        /// <summary>
        /// <para>Occurs when numeric value changed.</para>
        /// <para>DOES NOT occur when the input is cleared.</para>
        /// </summary>
        public event EventHandler<NumericValueChangedEventArgs> NumericValueChanged;

        /// <summary>
        /// Occurs when numeric value cleared.
        /// </summary>
        public event EventHandler<NumericValueClearedEventArgs> NumericValueCleared;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericEditText"/> class.
        /// </summary>
        /// <param name="context">Context</param>
        public NumericEditText(Context context)
            : base(context)
        {
            InitAttrs(context, null, 0);
            InitComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericEditText"/> class.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="attrs">Attributes for component initialization</param>
        public NumericEditText(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            InitAttrs(context, attrs, 0);
            InitComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericEditText"/> class.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="attrs">Attributes for component initialization</param>
        /// <param name="defStyleAttr">Style attributes for component initialization</param>
        public NumericEditText(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            InitAttrs(context, attrs, defStyleAttr);
            InitComponent();
        }

        /// <summary>
        /// Clears the text from the NumericEditText.
        /// </summary>
        public void Clear()
        {
            SetTextInternal(string.Empty);
            HandleNumericValueChanged();
        }

        /// <summary>
        /// Gets the double value represented by the text. Returns double.NaN if number is invalid
        /// </summary>
        /// <returns>The double value represented by the text</returns>
        public double GetNumericValue()
        {
            var original = Regex.Replace(Text, _numberFilterRegex, string.Empty);
            try
            {
                return NumberFormat.Instance.Parse(original).DoubleValue();
            }
            catch (ParseException ex)
            {
                Log.Debug(Tag, ex.Message);
                return double.NaN;
            }
        }

        /// <summary>
        /// Gets the double value represented by the text. Returns 0 if number is invalid
        /// </summary>
        /// <returns>The double value represented by the text</returns>
        public double GetNumericValueOrDefault()
        {
            var original = Regex.Replace(Text, _numberFilterRegex, string.Empty);
            try
            {
                return NumberFormat.Instance.Parse(original).DoubleValue();
            }
            catch (ParseException ex)
            {
                Log.Debug(Tag, ex.Message);
                return default(double);
            }
        }

        private void InitAttrs(Context context, IAttributeSet attrs, int defStyleAttr)
        {
            var attributes = context.ObtainStyledAttributes(attrs, Resource.Styleable.NumericEditText, defStyleAttr, 0);

            try
            {
                MaxDigitsBeforeDecimal = attributes.GetInt(Resource.Styleable.NumericEditText_maxDigitsBeforeDecimal, DefaultDigitsBeforeDecimal);
                MaxDigitsAfterDecimal = attributes.GetInt(Resource.Styleable.NumericEditText_maxDigitsAfterDecimal, DefaultDigitsAfterDecimal);
                ShowCurrencySymbol = attributes.GetBoolean(Resource.Styleable.NumericEditText_showCurrencySymbol, DefaultShowCurrencySymbol);
                OverrideCurrencySymbol = attributes.GetString(Resource.Styleable.NumericEditText_overrideCurrencySymbol).FirstOrDefault().ToString();
            }
            catch (Exception ex)
            {
                Log.Debug(Tag, ex.Message);
            }
            finally
            {
                attributes.Recycle();
            }
        }

        private void InitComponent()
        {
            _numberFilterRegex = "[^\\d\\" + _decimalSeparator + "]";
            AfterTextChanged += TextChangedHandler;
            Click += (sender, e) =>
            {
                SetSelection(Text.Length);
            };
            KeyListener = DigitsKeyListener.GetInstance(AcceptedKeys);
        }

        private void TextChangedHandler(object sender, AfterTextChangedEventArgs e)
        {
            var newText = e.Editable.ToString();

            if (newText == OverrideCurrencySymbol)
            {
                Clear();
                return;
            }

            var decimalPointPosition = e.Editable.ToString().IndexOf(_decimalSeparator, StringComparison.CurrentCulture);
            if (decimalPointPosition > 0)
            {
                if (newText.Substring(decimalPointPosition).IndexOf(_groupingSeparator, StringComparison.CurrentCulture) > 0)
                {
                    DiscardInput(_previousText);
                    return;
                }
            }

            if (newText.Length == 1 && newText == _decimalSeparator)
            {
                DiscardInput(_previousText);
                return;
            }

            var splitText = newText.Split(_decimalSeparator.ToCharArray());
            var leftPart = splitText[0];
            string rightPart = null;
            if (splitText.Length > 1)
            {
                rightPart = splitText[1];
                rightPart = rightPart.Replace(OverrideCurrencySymbol, string.Empty);
            }

            if (MaxDigitsBeforeDecimal > 0 && leftPart != null && 
                    leftPart.Replace(_groupingSeparator, string.Empty)
                    .Replace(OverrideCurrencySymbol, string.Empty)
                    .Length > MaxDigitsBeforeDecimal)
            {
                DiscardInput(_previousText);
                return;
            }

            if (rightPart != null && rightPart.Length > MaxDigitsAfterDecimal)
            {
                DiscardInput(_previousText);
                return;
            }

            if (newText.Length > 2)
            {
                var lastChar = newText[newText.Length - 1].ToString();
                var secToLastChar = newText[newText.Length - 2].ToString();
                if (lastChar == _decimalSeparator || lastChar == _groupingSeparator)
                {
                    if (lastChar == secToLastChar)
                    {
                        DiscardInput(_previousText);
                        return;
                    }
                }
            }

            if (CountMatches(e.Editable.ToString(), _decimalSeparator) > 1)
            {
                DiscardInput(_previousText);
                return;
            }

            if (e.Editable.Length() == 0)
            {
                HandleNumericValueCleared();
                return;
            }

            SetTextInternal(Format(e.Editable.ToString()));
            SetSelection(Text.Length);
            HandleNumericValueChanged();
        }

        private void DiscardInput(string previousText)
        {
            Text = previousText;
            SetSelection(previousText.Length);
        }

        private void HandleNumericValueCleared()
        {
            _previousText = string.Empty;
            var handler = NumericValueCleared;
            handler?.Invoke(this, new NumericValueClearedEventArgs());
        }

        private void HandleNumericValueChanged()
        {
            _previousText = Text;
            var handler = NumericValueChanged;
            handler?.Invoke(this, new NumericValueChangedEventArgs(GetNumericValue()));
        }
        
        private void SetTextInternal(string text)
        {
            AfterTextChanged -= TextChangedHandler;
            Text = text;
            AfterTextChanged += TextChangedHandler;
        }
        
        private string Format(string original)
        {
            var parts = original.Split(_decimalSeparator.ToCharArray());
            var number = Regex.Replace(parts[0], _numberFilterRegex, string.Empty);
            number = ReplaceFirst(number, LeadingZeroFilterRegex, string.Empty);

            number = Reverse(Regex.Replace(Reverse(number), "(.{3})", "$1" + _groupingSeparator));

            number = RemoveStart(number, _groupingSeparator);

            if (parts.Length > 1)
            {
                number += _decimalSeparator + parts[1];
            }

            if (!ShowCurrencySymbol)
            {
                return number;
            }

            switch (_currencyPositivePattern)
            {
                case 0:
                    return $"{OverrideCurrencySymbol}{number}";
                case 1:
                    return $"{number}{OverrideCurrencySymbol}";
                case 2:
                    return $"{OverrideCurrencySymbol} {number}";
                case 3:
                    return $"{number} {OverrideCurrencySymbol}";
                default:
                    return number;
            }
        }

        private static string ReplaceFirst(string text, string search, string replace)
        {
            var pos = text.IndexOf(search, StringComparison.CurrentCulture);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private static string Reverse(string original)
        {
            if (original == null || original.Length <= 1)
            {
                return original;
            }

            var c = original.ToCharArray();
            Array.Reverse(c);
            return new string(c);
        }

        private static string RemoveStart(string str, string remove)
        {
            if (TextUtils.IsEmpty(str))
            {
                return str;
            }
            if (str.StartsWith(remove))
            {
                return str.Substring(remove.Length);
            }
            return str;
        }

        private static int CountMatches(string str, string sub)
        {
            if (TextUtils.IsEmpty(str))
            {
                return 0;
            }
            var lastIndex = str.LastIndexOf(sub, StringComparison.CurrentCulture);
            if (lastIndex < 0)
            {
                return 0;
            }
            return 1 + CountMatches(str.Substring(0, lastIndex), sub);
        }
    }
}