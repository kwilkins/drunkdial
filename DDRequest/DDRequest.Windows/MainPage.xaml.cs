using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DDRequest
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Windows.Devices.Geolocation;
    using Windows.System;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void GetLocationButton_Click(object sender, RoutedEventArgs e)
        {
            GetLocationButton.IsEnabled = false;
            this.ProgressRing.IsActive = true;

            var geo = new Geolocator();
            Geoposition pos = null;

            try
            {
                pos = await geo.GetGeopositionAsync();

                Debug.WriteLine("Latitude: " + pos.Coordinate.Latitude.ToString() + " Longitude: " + pos.Coordinate.Longitude.ToString() + " Accuracy: " + pos.Coordinate.Accuracy.ToString());
            }
            catch (System.UnauthorizedAccessException)
            {
                // Failed - location capability not set in manifest, or user blocked location access at run-time.
            }
            catch (TaskCanceledException)
            {
                // Cancelled or timed-out.
            }

            var mapQuery = String.Format("http://www.bing.com/maps/?rtp=~pos.{0}_{1}_I%27m+Here",
                pos.Coordinate.Latitude.ToString(),
                pos.Coordinate.Longitude.ToString());

            Debug.WriteLine("Map Query: " + mapQuery);

            this.sendLocationEmail(mapQuery);

            this.ProgressRing.IsActive = false;
            GetLocationButton.IsEnabled = true;
        }

        public async void sendLocationEmail(string mapQuery)
        {
            await Launcher.LaunchUriAsync(new Uri(String.Format("mailto:{0}?subject={1}&body={2}", "dummy@foo.com", "Pick-me-up-please", mapQuery)));
        }
    }
}
