using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Akamud.Numericedittext;

namespace NumericEditTextsample
{
	[Activity (Label = "NumericEditText-sample", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 1;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			NumericEditText txtNumeric = FindViewById<NumericEditText> (Resource.Id.txtNumeric);
			
			button.Click += delegate {
				double value = txtNumeric.GetNumericValue();
				Toast.MakeText(this, value.ToString(), ToastLength.Long).Show();
			};
		}
	}
}


