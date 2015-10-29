using System;

namespace Akamud.Numericedittext
{
    public class NumericValueChangedEventArgs : EventArgs
    {
        public double NewValue {
            get;
            set;
        }

        public NumericValueChangedEventArgs(double newValue)
        {
            this.NewValue = newValue;
        }
    }
}

