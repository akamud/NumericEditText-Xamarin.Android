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
    [Register("br.com.akamud.NumericEditText")]
    public class NumericEditText : EditText
    {
        private string GROUPING_SEPARATOR = CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator;
        private string DECIMAL_SEPARATOR = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        private const string LEADING_ZERO_FILTER_REGEX = "^0+(?!$)";
        private const int DefaultDigitsBeforeDecimal = 0;
        private const int DefaultDigitsAfterDecimal = 2;
        private int MaxDigitsBeforeDecimal;
        private int MaxDigitsAfterDecimal;

        private string defaultText = null;
        private string previousText = "";
        private string numberFilterRegex = "";

        public NumericEditText(Context context)
            :base(context) 
        {
            InitAttrs(context, null, 0);
            InitComponent();
        }

        public NumericEditText(Context context, IAttributeSet attrs)
            :base(context, attrs) 
        {
            InitAttrs(context, attrs, 0);
            InitComponent();
        }

        public NumericEditText(Context context, IAttributeSet attrs, int defStyleAttr)
			:base(context, attrs, defStyleAttr) 
        {
            InitAttrs(context, attrs, defStyleAttr);
            InitComponent();
        }

        private void InitAttrs(Context context, IAttributeSet attrs, int defStyleAttr)
        {
            TypedArray atributos = context.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.NumericEditText, defStyleAttr, 0);

            try
            {
				MaxDigitsBeforeDecimal = atributos.GetInt(Resource.Styleable.NumericEditText_maxDigitsBeforeDecimal, DefaultDigitsBeforeDecimal);
				MaxDigitsAfterDecimal = atributos.GetInt(Resource.Styleable.NumericEditText_maxDigitsAfterDecimal, DefaultDigitsAfterDecimal);
            }
            finally
            {
                atributos.Recycle();
            }
        }

        private void InitComponent()
        {
            numberFilterRegex = "[^\\d\\" + DECIMAL_SEPARATOR + "]";
            AfterTextChanged += TextChangedHandler;
            Click += (object sender, EventArgs e) => {
                SetSelection(Text.Length);
            };
            KeyListener = DigitsKeyListener.GetInstance("0123456789,.");
        }

        private void TextChangedHandler(object sender, AfterTextChangedEventArgs e)
        {
            string novoTexto = e.Editable.ToString();

            int posicaoCasaDecimal = e.Editable.ToString().IndexOf(DECIMAL_SEPARATOR);
            if (posicaoCasaDecimal > 0)
            {
                if (novoTexto.Substring(posicaoCasaDecimal).IndexOf(GROUPING_SEPARATOR) > 0)
                {
                    DiscardInput(previousText);
                    return;
                }
            }

            if (novoTexto.Length == 1 && novoTexto == DECIMAL_SEPARATOR)
            {
                DiscardInput(previousText);
                return;
            }

            string[] splitText = novoTexto.Split(DECIMAL_SEPARATOR.ToCharArray());
            string leftPart = splitText[0];
            string rightPart = null;
            if (splitText.Length > 1)
            {
                rightPart = splitText[1];
            }

            if (MaxDigitsBeforeDecimal > 0 && leftPart != null && leftPart.Replace(GROUPING_SEPARATOR, "").Length > MaxDigitsBeforeDecimal)
            {
                DiscardInput(previousText);
                return;
            }

            if (rightPart != null && rightPart.Length > MaxDigitsAfterDecimal)
            {
                DiscardInput(previousText);
                return;
            }

            if (novoTexto.Length > 2)
            {
                string ultimoCaractere = novoTexto[novoTexto.Length - 1].ToString();
                string penultimoCaractere = novoTexto[novoTexto.Length - 2].ToString();
                if (ultimoCaractere == DECIMAL_SEPARATOR || ultimoCaractere == GROUPING_SEPARATOR)
                {
                    if (ultimoCaractere == penultimoCaractere)
                    {
                        DiscardInput(previousText);
                        return;
                    }
                }
            }

            if (CountMatches(e.Editable.ToString(), DECIMAL_SEPARATOR.ToString()) > 1)
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
            // TODO: Add events for numberchanged
        }

        private void HandleNumericValueChanged()
        {
            previousText = Text.ToString();
			// TODO: Add events for numberchanged
        }

        public void SetDefaultNumericValue(double defaultNumericValue, string defaultNumericFormat)
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

        public void Clear()
        {
            SetTextInternal(defaultText != null ? defaultText : "");
            if (defaultText != null)
            {
                HandleNumericValueChanged();
            }
        }

        public double GetNumericValue()
        {
            string original = Regex.Replace(Text.ToString(), numberFilterRegex, "");
            try
            {
                return NumberFormat.Instance.Parse(original).DoubleValue();
            }
            catch (ParseException)
            {
                return Double.NaN;
            }
        }

        public string ReplaceFirst(string text, string search, string replace)
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
            string[] parts = original.Split(DECIMAL_SEPARATOR.ToCharArray());
            String number = Regex.Replace(parts[0], numberFilterRegex, "");
            number = ReplaceFirst(number, LEADING_ZERO_FILTER_REGEX, "");

            number = Reverse(Regex.Replace(Reverse(number), "(.{3})", "$1" + GROUPING_SEPARATOR));

            number = RemoveStart(number, GROUPING_SEPARATOR.ToString());

            if (parts.Length > 1)
            {
                number += DECIMAL_SEPARATOR + parts[1];
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
            }
            else
            {
                return 1 + CountMatches(str.Substring(0, lastIndex), sub);
            }
        }
    }
}