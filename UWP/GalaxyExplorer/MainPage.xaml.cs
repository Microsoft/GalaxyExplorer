// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using UnityPlayer;
using Windows.UI.Input;

namespace GalaxyExplorer
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private WinRTBridge.WinRTBridge _bridge;
		
		private SplashScreen splash;
		private Rect splashImageRect;
		private WindowSizeChangedEventHandler onResizeHandler;
		private InputHandling inputHandler = new InputHandling();

		public MainPage()
		{
			this.InitializeComponent();
			NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;

			AppCallbacks appCallbacks = AppCallbacks.Instance;
			// Setup scripting bridge
			_bridge = new WinRTBridge.WinRTBridge();
			appCallbacks.SetBridge(_bridge);

			bool isWindowsHolographic = false;

#if UNITY_HOLOGRAPHIC
			// If application was exported as Holographic check if the deviceFamily actually supports it,
			// otherwise we treat this as a normal XAML application
			string deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
			isWindowsHolographic = String.Compare("Windows.Holographic", deviceFamily) == 0;
#endif

			if (isWindowsHolographic)
			{
				appCallbacks.InitializeViewManager();
			}
			else
			{
				appCallbacks.RenderingStarted += () => { RemoveSplashScreen(); };

				appCallbacks.SetKeyboardTriggerControl(this);
				appCallbacks.SetSwapChainPanel(GetSwapChainPanel());
				appCallbacks.SetCoreWindowEvents(Window.Current.CoreWindow);
				appCallbacks.InitializeD3DXAML();

				splash = ((App)App.Current).splashScreen;
				GetSplashBackgroundColor();
				OnResize();
				onResizeHandler = new WindowSizeChangedEventHandler((o, e) => OnResize());
				Window.Current.SizeChanged += onResizeHandler;
			}
		}

	/// <summary>
	/// Invoked when this page is about to be displayed in a Frame.
	/// </summary>
	/// <param name="e">Event data that describes how this page was reached.  The Parameter
	/// property is typically used to configure the page.</param>
	protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			splash = (SplashScreen)e.Parameter;
			OnResize();
		}

		private void OnResize()
		{
			if (splash != null)
			{
				splashImageRect = splash.ImageLocation;
				PositionImage();
			}
		}

		private void PositionImage()
		{
			ExtendedSplashImage.SetValue(Canvas.LeftProperty, splashImageRect.X);
			ExtendedSplashImage.SetValue(Canvas.TopProperty, splashImageRect.Y);
			ExtendedSplashImage.Height = splashImageRect.Height;
			ExtendedSplashImage.Width = splashImageRect.Width;
		}

		private async void GetSplashBackgroundColor()
		{
			try
			{
				StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///AppxManifest.xml"));
				string manifest = await FileIO.ReadTextAsync(file);
				int idx = manifest.IndexOf("SplashScreen");
				manifest = manifest.Substring(idx);
				idx = manifest.IndexOf("BackgroundColor");
				if (idx < 0)  // background is optional
					return;
				manifest = manifest.Substring(idx);
				idx = manifest.IndexOf("\"");
				manifest = manifest.Substring(idx + 1);
				idx = manifest.IndexOf("\"");
				manifest = manifest.Substring(0, idx);
				int value = 0;
				bool transparent = false;
				if (manifest.Equals("transparent"))
					transparent = true;
				else if (manifest[0] == '#') // color value starts with #
					value = Convert.ToInt32(manifest.Substring(1), 16) & 0x00FFFFFF;
				else
					return; // at this point the value is 'red', 'blue' or similar, Unity does not set such, so it's up to user to fix here as well
				byte r = (byte)(value >> 16);
				byte g = (byte)((value & 0x0000FF00) >> 8);
				byte b = (byte)(value & 0x000000FF);

				await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.High, delegate()
				{
					byte a = (byte)(transparent ? 0x00 : 0xFF);
					ExtendedSplashGrid.Background = new SolidColorBrush(Color.FromArgb(a, r, g, b));
				});
			}
			catch (Exception)
			{ }
		}

		public SwapChainPanel GetSwapChainPanel()
		{
			return DXSwapChainPanel;
		}

		public void RemoveSplashScreen()
		{
			DXSwapChainPanel.Children.Remove(ExtendedSplashGrid);
			if (onResizeHandler != null)
			{
				Window.Current.SizeChanged -= onResizeHandler;
				onResizeHandler = null;
			}
		}

		protected override Windows.UI.Xaml.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new UnityPlayer.XamlPageAutomationPeer(this);
		}

        private void DXSwapChainPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint releasePoint = e.GetCurrentPoint(null);

            inputHandler.PointerOrSingleFingerReleased(releasePoint.Position.X, releasePoint.Position.Y);
        }

        private void DXSwapChainPanel_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint releasePoint = e.GetCurrentPoint(null);

            inputHandler.PointerMoved(releasePoint.Position.X, releasePoint.Position.Y);
        }
    }
}
