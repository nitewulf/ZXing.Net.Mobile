using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Android.OS;
using ZXing;
using ZXing.Mobile;
using System;
using Xamarin.Essentials;
using AndroidX.ConstraintLayout.Widget;

namespace Sample.Android
{
	[Activity(Label = "ZXing.Net.Mobile", MainLauncher = true, Theme = "@style/Theme.AppCompat.Light", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden)]
	public class Activity1 : AndroidX.AppCompat.App.AppCompatActivity
	{
		Button buttonScanCustomView;
		Button buttonScanCustomAreaView;
		Button buttonScanDefaultView;
		Button buttonContinuousScan;
		Button buttonFragmentScanner;
        Button buttonFragmentScannerCustomArea;
		Button buttonGenerate;

		MobileBarcodeScanner scanner;

        //custom scan area variables
        Button buttonRectangle;
        Button buttonSquare;
        Button buttonRandom;
        ImageButton buttonIncrease;
        ImageButton buttonDecrease;
        ToggleButton scanViewPosition;
        View scanArea;
        bool scanAreaIsRectangle = true;
        bool isScanAreaCentered = false;
        const int FRAGMENT_SCAN_REQUEST = 1;
        public const string RESULT_EXTRA_NAME = "scanResultText";

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			Xamarin.Essentials.Platform.Init(Application);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			//Create a new instance of our Scanner
			scanner = new MobileBarcodeScanner();

			buttonScanDefaultView = this.FindViewById<Button>(Resource.Id.buttonScanDefaultView);
			buttonScanDefaultView.Click += async delegate
			{

				//Tell our scanner to use the default overlay
				scanner.UseCustomOverlay = false;

				//We can customize the top and bottom text of the default overlay
				scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
				scanner.BottomText = "Wait for the barcode to automatically scan!";

				//Start scanning
				var result = await scanner.Scan();

				HandleScanResult(result);
			};

			buttonContinuousScan = FindViewById<Button>(Resource.Id.buttonScanContinuous);
			buttonContinuousScan.Click += delegate
			{

				scanner.UseCustomOverlay = false;

				//We can customize the top and bottom text of the default overlay
				scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
				scanner.BottomText = "Wait for the barcode to automatically scan!";

				var opt = new MobileBarcodeScanningOptions();
				opt.DelayBetweenContinuousScans = 3000;

				//Start scanning
				scanner.ScanContinuously(opt, HandleScanResult);
			};

			Button flashButton;
			View zxingOverlay;

			buttonScanCustomView = this.FindViewById<Button>(Resource.Id.buttonScanCustomView);
			buttonScanCustomView.Click += async delegate
			{

				//Tell our scanner we want to use a custom overlay instead of the default
				scanner.UseCustomOverlay = true;

				//Inflate our custom overlay from a resource layout
				zxingOverlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.ZxingOverlay, null);

				//Find the button from our resource layout and wire up the click event
				flashButton = zxingOverlay.FindViewById<Button>(Resource.Id.buttonZxingFlash);
				flashButton.Click += (sender, e) => scanner.ToggleTorch();

				//Set our custom overlay
				scanner.CustomOverlay = zxingOverlay;
                scanner.UseCustomOverlay = true;

				//Start scanning!
				var result = await scanner.Scan(new MobileBarcodeScanningOptions { AutoRotate = true });

				HandleScanResult(result);
			};

			buttonScanCustomAreaView = this.FindViewById<Button>(Resource.Id.buttonScanCustomAreaView);
			buttonScanCustomAreaView.Click += async delegate {

				//Tell our scanner we want to use a custom overlay instead of the default
				scanner.UseCustomOverlay = true;

				//Inflate our custom overlay from a resource layout
				zxingOverlay = LayoutInflater.FromContext(this).Inflate(Resource.Layout.ZxingOverlayCustomScanArea, null);                
                scanArea = zxingOverlay.FindViewById<View>(Resource.Id.scanView);

                //Find all the buttons and wire up their events
                buttonRectangle = zxingOverlay.FindViewById<Button>(Resource.Id.buttonSquareScanRectangle);
                buttonSquare = zxingOverlay.FindViewById<Button>(Resource.Id.buttonSquareScanView);
                scanViewPosition = zxingOverlay.FindViewById<ToggleButton>(Resource.Id.toggleButtonScanViewLocation);
                buttonIncrease = zxingOverlay.FindViewById<ImageButton>(Resource.Id.buttonIncrease);
                buttonDecrease = zxingOverlay.FindViewById<ImageButton>(Resource.Id.buttonDecrease);
                buttonRandom = zxingOverlay.FindViewById<Button>(Resource.Id.buttonRandom);

                buttonRectangle.Click += Rectangle_Click;
                buttonSquare.Click += Square_Click;
                scanViewPosition.Click += ScanViewPosition_Click;                
                buttonIncrease.Click += ButtonIncrease_Click;
                buttonDecrease.Click += ButtonDecrease_Click;
                buttonRandom.Click += ButtonRandom_Click;

                //Set our custom overlay and custom scan area
                scanner.CustomOverlay = zxingOverlay;
                scanner.UseCustomOverlay = true;
                scanner.CustomOverlayScanAreaView = scanArea;

                //Start scanning!
                var result = await scanner.Scan(new MobileBarcodeScanningOptions { AutoRotate = true });

				HandleScanResult(result);
			};

			buttonFragmentScanner = FindViewById<Button>(Resource.Id.buttonFragment);
			buttonFragmentScanner.Click += delegate
			{
				StartActivity(typeof(FragmentActivity));
			};

            buttonFragmentScannerCustomArea = FindViewById<Button>(Resource.Id.buttonFragmentCustomArea);
            buttonFragmentScannerCustomArea.Click += delegate {                                
                StartActivityForResult(typeof(FragmentActivityCustomScanArea), FRAGMENT_SCAN_REQUEST);                                    
            };

			buttonGenerate = FindViewById<Button>(Resource.Id.buttonGenerate);
			buttonGenerate.Click += delegate
			{
				StartActivity(typeof(ImageActivity));
			};
		}

		void HandleScanResult(ZXing.Result result)
		{
			var msg = "";

			if (result != null && !string.IsNullOrEmpty(result.Text))
				msg = "Found Barcode: " + result.Text;
			else
				msg = "Scanning Canceled!";

			RunOnUiThread(() => Toast.MakeText(this, msg, ToastLength.Short).Show());
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		[Java.Interop.Export("UITestBackdoorScan")]
		public Java.Lang.String UITestBackdoorScan(string param)
		{
			var expectedFormat = BarcodeFormat.QR_CODE;
			Enum.TryParse(param, out expectedFormat);
			var opts = new MobileBarcodeScanningOptions
			{
				PossibleFormats = new List<BarcodeFormat> { expectedFormat }
			};
			var barcodeScanner = new MobileBarcodeScanner();

			Console.WriteLine("Scanning " + expectedFormat);

			//Start scanning
			barcodeScanner.Scan(opts).ContinueWith(t =>
			{

				var result = t.Result;

				var format = result?.BarcodeFormat.ToString() ?? string.Empty;
				var value = result?.Text ?? string.Empty;

				RunOnUiThread(() =>
				{

					AlertDialog dialog = null;
					dialog = new AlertDialog.Builder(this)
									.SetTitle("Barcode Result")
									.SetMessage(format + "|" + value)
									.SetNeutralButton("OK", (sender, e) =>
									{
										dialog.Cancel();
									}).Create();
					dialog.Show();
				});
			});

			return new Java.Lang.String();
		}

        void ButtonRandom_Click(object sender, EventArgs e)
        {
            buttonRandom.PerformHapticFeedback(FeedbackConstants.KeyboardTap);
            var parentLayout = (ConstraintLayout)scanArea.Parent;
            var rand = new Random();

            var layoutParams = (ViewGroup.MarginLayoutParams)scanArea.LayoutParameters;

            if (scanAreaIsRectangle)
            {
                var moveUp = rand.Next(0, 100) % 2 == 0;

                if (isScanAreaCentered)
                {
                    if (moveUp)
                    {
                        layoutParams.BottomMargin = rand.Next(0, parentLayout.Height - scanArea.Height);
                        layoutParams.LeftMargin = rand.Next(0, parentLayout.Width - scanArea.Width);
                    }
                    else
                    {
                        layoutParams.TopMargin = rand.Next(0, parentLayout.Height - scanArea.Height);
                        layoutParams.RightMargin = rand.Next(0, parentLayout.Width - scanArea.Width);
                    }
                }
                else
                {
                    if (moveUp)
                    {
                        layoutParams.BottomMargin = rand.Next(0, parentLayout.Height - scanArea.Height);
                    }
                    else
                        layoutParams.TopMargin = rand.Next(0, parentLayout.Height - scanArea.Height);
                }
            }
            else
            {
                var moveUp = rand.Next(0, 100) % 2 == 0;

                if (moveUp)
                {
                    layoutParams.BottomMargin = rand.Next(0, parentLayout.Height - scanArea.Height);
                    layoutParams.LeftMargin = rand.Next(0, parentLayout.Width - scanArea.Width);
                }
                else
                {
                    layoutParams.TopMargin = rand.Next(0, parentLayout.Height - scanArea.Height);
                    layoutParams.RightMargin = rand.Next(0, parentLayout.Width - scanArea.Width);
                }
            }
            scanArea.LayoutParameters = layoutParams;
        }

        void ButtonDecrease_Click(object sender, EventArgs e)
        {
            buttonDecrease.PerformHapticFeedback(FeedbackConstants.KeyboardTap);
            var layout = scanArea.LayoutParameters;
            var parentLayout = (ConstraintLayout)scanArea.Parent;
            var height = layout.Height;
            var width = layout.Width;
            height = (int)(height * .9);
            width = (int)(width * .9);
            if (height > parentLayout.Height) height = parentLayout.Height;
            if (width > parentLayout.Width) width = parentLayout.Width;
            layout.Height = height;
            layout.Width = width;
            scanArea.LayoutParameters = layout;
        }

        void ButtonIncrease_Click(object sender, EventArgs e)
        {
            buttonIncrease.PerformHapticFeedback(FeedbackConstants.KeyboardTap);
            var layout = scanArea.LayoutParameters;
            var parentLayout = (ConstraintLayout)scanArea.Parent;
            var height = layout.Height;
            var width = layout.Width;
            height = (int)(height * 1.1);
            width = (int)(width * 1.1);
            if (height > parentLayout.Height) height = parentLayout.Height;
            if (width > parentLayout.Width) width = parentLayout.Width;
            layout.Height = height;
            layout.Width = width;
            scanArea.LayoutParameters = layout;
        }

        public int ConvertDpToPx(int dp)
        {
            var density = ApplicationContext.Resources
                                   .DisplayMetrics
                                   .Density;
            return (int)Math.Round(dp * density + .5f);
        }

        void ScanViewPosition_Click(object sender, EventArgs e)
        {
            scanViewPosition.PerformHapticFeedback(FeedbackConstants.KeyboardTap);
            isScanAreaCentered = scanViewPosition.Checked;

            if (scanAreaIsRectangle)
            {
                if (isScanAreaCentered) SetRectangleCentered();
                else SetRectangleFullWidth();
            }
            else if (isScanAreaCentered) SetSquareCentered();
            else SetSquareFullWidth();
        }

        void Square_Click(object sender, EventArgs e)
        {
            buttonSquare.PerformHapticFeedback(FeedbackConstants.KeyboardTap);
            if (scanViewPosition.Checked)
            {
                SetSquareCentered();
            }
            else
            {
                SetSquareFullWidth();
            }
        }

        void Rectangle_Click(object sender, EventArgs e)
        {
            buttonRectangle.PerformHapticFeedback(FeedbackConstants.KeyboardTap);
            if (scanViewPosition.Checked)
            {
                SetRectangleCentered();
            }
            else
            {
                SetRectangleFullWidth();
            }
        }

        void SetRectangleCentered()
        {
            var layout = (ViewGroup.MarginLayoutParams)scanArea.LayoutParameters;
            layout.MarginEnd = 0;
            layout.LeftMargin = 0;
            layout.RightMargin = 0;
            layout.TopMargin = 0;

            layout.Height = ConvertDpToPx(75);
            layout.Width = ConvertDpToPx(150);
            scanArea.LayoutParameters = layout;
            isScanAreaCentered = true;
            scanAreaIsRectangle = true;
        }

        void SetRectangleFullWidth()
        {
            var layout = (ViewGroup.MarginLayoutParams)scanArea.LayoutParameters;
            layout.MarginEnd = 0;
            layout.LeftMargin = 0;
            layout.RightMargin = 0;
            layout.TopMargin = 0;
            var parentLayout = (ConstraintLayout)scanArea.Parent;
            layout.Height = ConvertDpToPx(120);
            layout.Width = parentLayout.Width;
            scanArea.LayoutParameters = layout;
            isScanAreaCentered = false;
            scanAreaIsRectangle = true;
        }

        void SetSquareCentered()
        {
            var layout = (ViewGroup.MarginLayoutParams)scanArea.LayoutParameters;
            layout.MarginEnd = 0;
            layout.LeftMargin = 0;
            layout.RightMargin = 0;
            layout.TopMargin = 0;
            layout.Height = ConvertDpToPx(120);
            layout.Width = ConvertDpToPx(120);
            scanArea.LayoutParameters = layout;
            isScanAreaCentered = true;
            scanAreaIsRectangle = false;
        }

        void SetSquareFullWidth()
        {
            var layout = (ViewGroup.MarginLayoutParams)scanArea.LayoutParameters;
            layout.MarginEnd = 0;
            layout.LeftMargin = 0;
            layout.RightMargin = 0;
            layout.TopMargin = 0;
            var parentLayout = (ConstraintLayout)scanArea.Parent;
            if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Landscape)
            {
                layout.Height = parentLayout.Height;
                layout.Width = parentLayout.Height;
            }
            else
            {
                layout.Height = parentLayout.Width;
                layout.Width = parentLayout.Width;
            }
            scanArea.LayoutParameters = layout;
            isScanAreaCentered = false;
            scanAreaIsRectangle = false;
        }
    }
}


