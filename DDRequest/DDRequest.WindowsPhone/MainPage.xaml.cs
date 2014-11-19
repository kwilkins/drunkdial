using System;
using System.Linq;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Windows.Devices.Geolocation;

using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace DDRequest
{
    using System.Text;
    using System.Threading.Tasks;

    using Windows.ApplicationModel.Chat;
    using Windows.ApplicationModel.Contacts;
    using Windows.UI.Popups;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Media.Imaging;


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ContactGridViewModel contactGridViewModel;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            contactGridViewModel = ContactGridViewModel.Instance;
            ContactGrid.DataContext = contactGridViewModel;
            ContactGrid.ItemsSource = contactGridViewModel.DataSource;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private async void CustomMessageButton_Click(object sender, RoutedEventArgs e)
        {
            this.ActivateProgressRing();

            const string CustomMessage = "We're having a good time, come join us! - {0}";

            var mapQuery = await this.CreateMapQuery();
            Debug.WriteLine("Map Query: " + mapQuery);
            this.SendLocationSms(CustomMessage, mapQuery);

            DeactivateProgressRing();
        }

        private async Task<string> CreateMapQuery()
        {
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

            return mapQuery;
        }

        private void ActivateProgressRing()
        {
            CustomMessageButton.IsEnabled = false;
            ContactGrid.IsEnabled = false;
            AddContact.IsEnabled = false;

            this.ProgressRing.IsActive = true;
        }

        private void DeactivateProgressRing()
        {
            this.ProgressRing.IsActive = false;

            ContactGrid.IsEnabled = true;
            CustomMessageButton.IsEnabled = true;
            AddContact.IsEnabled = true;
        }

        public void SendLocationSms(string message, string mapQuery)
        {
            SendLocationSms(message, null, mapQuery);
        }

        public void SendLocationSms(string message, string recipient, string mapQuery)
        {
            var sms = new ChatMessage
            {
                Body = String.Format(message, new Uri(mapQuery))
            };
            if (recipient != null)
            {
                sms.Recipients.Add(recipient);
            }

            ChatMessageManager.ShowComposeSmsMessageAsync(sms);
        }

        private async void AddContact_Click(object sender, RoutedEventArgs e)
        {
            if (contactGridViewModel.DataSource.Count >= 6)
            {
                var messageDialog = new MessageDialog("You can only pin 6 contacts at a time");

                await messageDialog.ShowAsync();

                return;
            }

            var contactPicker = new ContactPicker();
            contactPicker.DesiredFieldsWithContactFieldType.Add(ContactFieldType.PhoneNumber);
            var pickedContact = await contactPicker.PickContactAsync();

            if (pickedContact == null)
            {
                return;
            }

            Debug.WriteLine("pickedContact: {0}({1})", pickedContact.DisplayName, pickedContact.Id);

            if (contactGridViewModel.DataSource.Any(c => c.Id.Equals(pickedContact.Id)))
            {
                var messageDialog = new MessageDialog(pickedContact.DisplayName + " is already pinned");

                await messageDialog.ShowAsync();

                return;
            }

            if (!pickedContact.Phones.Any() || pickedContact.Phones.All(c => c.Kind != ContactPhoneKind.Mobile))
            {
                var messageDialog = new MessageDialog("Please assign a mobile number for " + pickedContact.DisplayName + " before pinning them");

                await messageDialog.ShowAsync();

                return;
            }

            contactGridViewModel.DataSource.Add(new DDContact()
            {
                Name = pickedContact.FirstName,
                Id = pickedContact.Id,
                Phone = pickedContact.Phones.First(c => c.Kind == ContactPhoneKind.Mobile).Number
            });

            contactGridViewModel.SaveContacts();
        }

        private async void ContactGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var phoneNumber = ((DDContact)e.ClickedItem).Phone;

            this.ActivateProgressRing();

            var mapQuery = await this.CreateMapQuery();
            Debug.WriteLine("Map Query: " + mapQuery);
            this.SendLocationSms("Hey, could you come pick me up? Don't think I should be driving - {0}", phoneNumber, mapQuery);

            DeactivateProgressRing();
        }

        private void ContactGridElement_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            var senderElement = sender as FrameworkElement;
            var flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

    }
}
