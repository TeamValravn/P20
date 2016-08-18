using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;
using Windows.UI.Notifications;
using NotificationsExtensions.Toasts;
using System.Xml.Linq;

namespace ValravnClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Global constants.
        private const string DeviceConnectionString = "HostName=ValravnIOTHub1.azure-devices.net;DeviceId=MJSSP3;SharedAccessKeyName=iothubowner;SharedAccessKey=1ejHdx+Go7WV8LUh4dgdlTAxXwgr44v/fhyQV5hKf34=";

        // Global variables.
        public int CurrentUserID = 123456;
        public MessageDialog LoggingDialog = new MessageDialog("");
        public ReceiveMessage MessageQueue = new ReceiveMessage();
        public ObservableCollection<DeviceDataModel> DeviceAlerts = new ObservableCollection<DeviceDataModel>();
        public DeviceDataModel CurrentDevice = new DeviceDataModel();

        public MainPage()
        {
            this.InitializeComponent();

            //Hook event.
            MessageQueue.MessageRecieved += MessageQueue_MessageRecieved;

            //Initialize UI.
            buttonSubmitReplace.IsEnabled = false;
            buttonSubmitRepair.IsEnabled = false;
            buttonSubmitIgnore.IsEnabled = false;

            //Wait for new alerts.
            GetMessages();
        }

        public static Windows.Data.Xml.Dom.XmlDocument CreateToast()
        {
            var xDoc = new XDocument(
            new XElement("toast",
            new XElement("visual",
            new XElement("binding", new XAttribute("template", "ToastGeneric"),
            new XElement("text", "Moisture Device Alert"),
            new XElement("text", "A new device alert has arrived. Please inspect the sensor listed in the Valravn Sensor Maintenance control panel.")
            )
            )
            )
            );

            var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDoc.LoadXml(xDoc.ToString());
            return xmlDoc;
        }

        //Poll for messages.
        private async void GetMessages()
        {
            //Connect to client.
            try
            {
                DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString);

                if (deviceClient == null)
                {
                    LoggingDialog.Content = "Failed to connect to DeviceClient.";
                    await LoggingDialog.ShowAsync();
                }
                else
                {
                    await MessageQueue.ReceiveCommands(deviceClient);
                }
            }
            catch (Exception ex)
            {
                LoggingDialog.Content = String.Format("Error in GetMessages(): {0}", ex.Message);
                await LoggingDialog.ShowAsync();
            }
        }

        //Event handlers.
        private void MessageQueue_MessageRecieved(object sender, EventArgs e)
        {
            var xmdock = CreateToast();
            var toast = new ToastNotification(xmdock);

            var notifi = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            notifi.Show(toast);

            //Add device alert to collection.
            DeviceAlerts.Add((e as MessageRecievedEventArgs).CurrentDevice);
            listViewDeviceAlerts.ItemsSource = DeviceAlerts;
        }

        private void listViewDeviceAlerts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            if (listViewDeviceAlerts.SelectedItem == null) return;

            //Capture current device.
            CurrentDevice = (DeviceDataModel)listViewDeviceAlerts.SelectedItem;

            //Populate device status and details.
            textBlockDevice.Text = "Device ID: " + CurrentDevice.Device_ID.ToString();
            textBlockLocation.Text = "Location: " + CurrentDevice.longitude.ToString() + "," + CurrentDevice.latitude.ToString();
            textBlockStatus.Text = "Status: " + CurrentDevice.status.ToString();
            textBlockAirTemperature.Text = "Temperature: " + CurrentDevice.AirTemperature.ToString();
            textBlockHumidity.Text = "Humidity: " + CurrentDevice.humidity.ToString();
            textBlockHydration.Text = "Hydration: " + CurrentDevice.hydration.ToString();

            //Enable UI and set status.
            buttonSubmitReplace.IsEnabled = true;
            buttonSubmitRepair.IsEnabled = true;
            buttonSubmitIgnore.IsEnabled = true;
            textBlockMaintenanceStatus.Text = "Maintenance review needed for Device ID " + CurrentDevice.Device_ID.ToString() + ".";
        }

        private void buttonSubmitReplace_Click(object sender, RoutedEventArgs e)
        {
            SubmitAction("Replace");
        }

        private void buttonSubmitRepair_Click(object sender, RoutedEventArgs e)
        {
            SubmitAction("Repair");
        }

        private void buttonSubmitIgnore_Click(object sender, RoutedEventArgs e)
        {
            SubmitAction("Ignore");
        }

        //Internal methods.
        private async void SubmitAction(string anAction)
        {
   
            try
            {
                Valravn msgtodocdb = new Valravn { Id = "D", DeviceId = CurrentDevice.Device_ID, Action = anAction, ServicedBy = Convert.ToString(CurrentUserID), AirTemperature = Convert.ToString(CurrentDevice.AirTemperature), Humidity= Convert.ToString(CurrentDevice.humidity), Hydration=Convert.ToString(CurrentDevice.hydration) };

                HttpClient apiclient = new HttpClient();

                apiclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                var json = "=" + JsonConvert.SerializeObject(msgtodocdb);
                var apiurl = "http://valravnapiapp.azurewebsites.net/api/values/";

                var bodycontent = new StringContent(json, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

                await apiclient.PostAsync(apiurl, bodycontent);

                //Notify user maintenance request submitted and remove and clear current device.
                textBlockMaintenanceStatus.Text = anAction + " maintenance request submitted for " + CurrentDevice.Device_ID.ToString();
                DeviceAlerts.Remove(CurrentDevice);
                CurrentDevice = new DeviceDataModel();

                //Reinitialize UI.
                textBlockDevice.Text = "Device ID: ";
                textBlockLocation.Text = "Location: ";
                textBlockStatus.Text = "Status: ";
                textBlockAirTemperature.Text = "Temperature: ";
                textBlockHumidity.Text = "Humidity: ";
                textBlockHydration.Text = "Hydration: ";
                buttonSubmitReplace.IsEnabled = false;
                buttonSubmitRepair.IsEnabled = false;
                buttonSubmitIgnore.IsEnabled = false;
            }
            catch (Exception ex)
            {
                LoggingDialog.Content = String.Format("Error in GetMessages(): {0}", ex.Message);
                await LoggingDialog.ShowAsync();
            }
        }
    }
}
