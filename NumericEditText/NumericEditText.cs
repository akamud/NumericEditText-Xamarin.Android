using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Views;
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

        private string groupingSeparator = CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator;
        private string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private const int DefaultDigitsBeforeDecimal = 0;
        private const int DefaultDigitsAfterDecimal = 2;
        private string defaultText = null;
        private string previousText = "";
        private string numberFilterRegex = "";
        private const string LeadingZeroFilterRegex = "^0+(?!$)";

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
        /// Initializes a new instance of the <see cref="Akamud.Numericedittext.NumericEditText"/> class.
        /// </summary>
        /// <param name="context">Context</param>
        public NumericEditText(Context context)
            : base(context)
        {
            InitAttrs(context, null, 0);
            InitComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Akamud.Numericedittext.NumericEditText"/> class.
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
        /// Initializes a new instance of the <see cref="Akamud.Numericedittext.NumericEditText"/> class.
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

        private void InitAttrs(Context context, IAttributeSet attrs, int defStyleAttr)
        {
            TypedArray attributes = context.ObtainStyledAttributes(attrs, Resource.Styleable.NumericEditText, defStyleAttr, 0);

            try
            {
                MaxDigitsBeforeDecimal = attributes.GetInt(Resource.Styleable.NumericEditText_maxDigitsBeforeDecimal, DefaultDigitsBeforeDecimal);
                MaxDigitsAfterDecimal = attributes.GetInt(Resource.Styleable.NumericEditText_maxDigitsAfterDecimal, DefaultDigitsAfterDecimal);
            } finally
            {
                attributes.Recycle();
            }
        }

        private void InitComponent()
        {
            numberFilterRegex = "[^\\d\\" + decimalSeparator + "]";
            AfterTextChanged += TextChangedHandler;
            Click += (object sender, EventArgs e) => {
                SetSelection(Text.Length);
            };
            KeyListener = DigitsKeyListener.GetInstance("0123456789,.");
        }

        private void TextChangedHandler(object sender, AfterTextChangedEventArgs e)
        {
            string newText = e.Editable.ToString();

            int decimalPointPosition = e.Editable.ToString().IndexOf(decimalSeparator);
            if (decimalPointPosition > 0)
            {
                if (newText.Substring(decimalPointPosition).IndexOf(groupingSeparator) > 0)
                {
                    DiscardInput(previousText);
                    return;
                }
            }

            if (newText.Length == 1 && newText == decimalSeparator)
            {
                DiscardInput(previousText);
                return;
            }

            string[] splitText = newText.Split(decimalSeparator.ToCharArray());
            string leftPart = splitText[0];
            string rightPart = null;
            if (splitText.Length > 1)
            {
                rightPart = splitText[1];
            }

            if (MaxDigitsBeforeDecimal > 0 && leftPart != null && leftPart.Replace(groupingSeparator, "").Length > MaxDigitsBeforeDecimal)
            {
                DiscardInput(previousText);
                return;
            }

            if (rightPart != null && rightPart.Length > MaxDigitsAfterDecimal)
            {
                DiscardInput(previousText);
                return;
            }

            if (newText.Length > 2)
            {
                string lastChar = newText[newText.Length - 1].ToString();
                string secToLastChar = newText[newText.Length - 2].ToString();
                if (lastChar == decimalSeparator || lastChar == groupingSeparator)
                {
                    if (lastChar == secToLastChar)
                    {
                        DiscardInput(previousText);
                        return;
                    }
                }
            }

            if (CountMatches(e.Editable.ToString(), decimalSeparator.ToString()) > 1)
            {
                DiscardInput(previousText);
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
            previousText = "";
            var handler = NumericValueCleared;
            if (handler != null)
                handler.Invoke(this, new NumericValueClearedEventArgs());
        }

        private void HandleNumericValueChanged()
        {
            previousText = Text.ToString();
            var handler = NumericValueChanged;
            if (handler != null)
                handler.Invoke(this, new NumericValueChangedEventArgs(GetNumericValue()));
        }

        private void SetDefaultNumericValue(double defaultNumericValue, string defaultNumericFormat)
        {
            defaultText = string.Format(defaultNumericFormat, defaultNumericValue);

            SetTextInternal(defaultText);
        }

        private void SetTextInternal(string text)
        {
            AfterTextChanged -= TextChangedHandler;
            Text = text;
            AfterTextChanged += TextChangedHandler;
        }

        /// <summary>
        /// Clears the text from the NumericEditText.
        /// </summary>
        public void Clear()
        {
            SetTextInternal(defaultText != null ? defaultText : "");
            if (defaultText != null)
            {
                HandleNumericValueChanged();
            }
        }

        /// <summary>
        /// Gets the double value represented by the text.
        /// </summary>
        /// <returns>The double value represented by the text</returns>
        public double GetNumericValue()
        {
            string original = Regex.Replace(Text.ToString(), numberFilterRegex, "");
            try
            {
                return NumberFormat.Instance.Parse(original).DoubleValue();
            } catch (ParseException)
            {
                return Double.NaN;
            }
        }

        private string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private string Format(string original)
        {
            string[] parts = original.Split(decimalSeparator.ToCharArray());
            String number = Regex.Replace(parts[0], numberFilterRegex, "");
            number = ReplaceFirst(number, LeadingZeroFilterRegex, "");

            number = Reverse(Regex.Replace(Reverse(number), "(.{3})", "$1" + groupingSeparator));

            number = RemoveStart(number, groupingSeparator.ToString());

            if (parts.Length > 1)
            {
                number += decimalSeparator + parts[1];
            }

            return number;
        }

        private string Reverse(string original)
        {
            if (original == null || original.Length <= 1)
            {
                return original;
            }
            return TextUtils.GetReverse(original, 0, original.Length).ToString();
        }

        private string RemoveStart(string str, string remove)
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

        private int CountMatches(string str, string sub)
        {
            if (TextUtils.IsEmpty(str))
            {
                return 0;
            }
            int lastIndex = str.LastIndexOf(sub);
            if (lastIndex < 0)
            {
                return 0;
            } else
            {
                return 1 + CountMatches(str.Substring(0, lastIndex), sub);
            }
        }
    }
}