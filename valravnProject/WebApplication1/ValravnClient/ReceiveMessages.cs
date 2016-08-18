using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ValravnClient
{
    public class ReceiveMessage
    {
        public MessageDialog LoggingDialog = new MessageDialog("");
        public DeviceDataModel CurrentDevice = new DeviceDataModel();

        public async Task ReceiveCommands(DeviceClient deviceClient)
        {
            Message receivedMessage;
            string messageData;

            while (true)
            {
                receivedMessage = await deviceClient.ReceiveAsync();

                if (receivedMessage != null)
                {
                    //Get message.
                    messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                    //Deserialize message.
                    this.CurrentDevice = JsonConvert.DeserializeObject<DeviceDataModel>(messageData);

                    //Raise event.
                    OnMessageRecieved(new MessageRecievedEventArgs() { CurrentDevice = this.CurrentDevice });

                    //Acknowledge message received.
                    await deviceClient.CompleteAsync(receivedMessage);
                }

                //  Note: In this sample, the polling interval is set to 
                //  10 seconds to enable you to see messages as they are sent.
                //  To enable an IoT solution to scale, you should extend this //  interval. For example, to scale to 1 million devices, set 
                //  the polling interval to 25 minutes.
                //  For further information, see
                //  https://azure.microsoft.com/documentation/articles/iot-hub-devguide/#messaging
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }

        public event EventHandler MessageRecieved;

        protected virtual void OnMessageRecieved(EventArgs e)
        {
            EventHandler handler = MessageRecieved;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public delegate void MessageRecievedEventHandler(MessageRecievedEventArgs e);
    }

    public class MessageRecievedEventArgs : EventArgs
    {
        public DeviceDataModel CurrentDevice { get; set; }
    }
}
