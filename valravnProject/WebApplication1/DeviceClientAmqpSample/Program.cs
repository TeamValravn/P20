using DeviceClientAmqpSample;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Client.Samples
{
    class Program
    {


        // String containing Hostname, Device Id & Device Key in one of the following formats:
        //  "HostName=<iothub_host_name>;DeviceId=<device_id>;SharedAccessKey=<device_key>"
        //  "HostName=<iothub_host_name>;CredentialType=SharedAccessSignature;DeviceId=<device_id>;SharedAccessSignature=SharedAccessSignature sr=<iot_host>/devices/<device_id>&sig=<token>&se=<expiry_time>";
        private const string DeviceConnectionString = "HostName=ValravnIOTHub1.azure-devices.net;DeviceId=MJSSP3;SharedAccessKeyName=iothubowner;SharedAccessKey=1ejHdx+Go7WV8LUh4dgdlTAxXwgr44v/fhyQV5hKf34=";

        private static int MESSAGE_COUNT = 5;

        static void Main(string[] args)
        {
            try
            {
                DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString);

                if (deviceClient == null)
                {
                    Console.WriteLine("Failed to create DeviceClient!");
                }
                else
                {
                    SendEvent(deviceClient).Wait();
                    //ReceiveCommands(deviceClient).Wait();
                }

                Console.WriteLine("Exited!\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in sample: {0}", ex.Message);
                Console.WriteLine("Hit any key continue...");
                Console.ReadLine();
            }
        }

        static async Task SendEvent(DeviceClient deviceClient)
        {

            var lines = File.ReadAllLines("inputdata.csv");
            
			int countRecords=0;

            foreach (var line in lines)
            {
                int fieldCount = 0;

                var fields = line.Split(',');
                var deviceInfo = new DeviceDataModel();
                deviceInfo.MessageType = "FromDevice";
                deviceInfo.Device_ID = fields[fieldCount++];
                deviceInfo.status = fields[fieldCount++];
                deviceInfo.AirTemperature = Convert.ToSingle(fields[fieldCount++]);
                deviceInfo.humidity = Convert.ToSingle(fields[fieldCount++]);
                deviceInfo.hydration = Convert.ToSingle(fields[fieldCount++]);
                deviceInfo.latitude = Convert.ToSingle(fields[fieldCount++]);
                deviceInfo.longitude = Convert.ToSingle(fields[fieldCount++]);
                deviceInfo.SubmissionTime = DateTime.Now.ToUniversalTime().ToString();

                String input = JsonConvert.SerializeObject(deviceInfo);

                Message eventMessage = new Message(Encoding.UTF8.GetBytes(input));

                Console.WriteLine($"\t{DateTime.Now.ToLocalTime()}> Sending message{countRecords}: Data: [{input}]");

                await deviceClient.SendEventAsync(eventMessage);
				
				countRecords++;

            }

            Console.WriteLine("Device sending {0} messages to IoTHub...\n", MESSAGE_COUNT);

            //for (int count = 0; count < MESSAGE_COUNT; count++)
            //{
            
            //    deviceInfo.Device_ID = "MJSPSP3";
            //    deviceInfo.status = "good";
            //    deviceInfo.AirTemperature = 80.0f;
            //    deviceInfo.latitude = 45.35f;
            //    deviceInfo.longitude = 67.34f;
            //    deviceInfo.SubmissionTime = DateTime.Now.ToLocalTime().ToString();
            //    deviceInfo.Expiry = DateTime.Now.AddDays(1).ToLocalTime().ToString();

            //    String input = JsonConvert.SerializeObject(deviceInfo);

            //    Message eventMessage = new Message(Encoding.UTF8.GetBytes(input));

            //    Console.WriteLine("\t{0}> Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), count, input);

            //    await deviceClient.SendEventAsync(eventMessage);

            //}
        }

        static async Task ReceiveCommands(DeviceClient deviceClient)
        {
            Console.WriteLine("\nDevice waiting for commands from IoTHub...\n");
            Message receivedMessage;
            string messageData;

            while (true)
            {
                receivedMessage = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(1));
                
                if (receivedMessage != null)
                {
                    messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    Console.WriteLine("\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData);

                    int propCount = 0;
                    foreach (var prop in receivedMessage.Properties)
                    {
                        Console.WriteLine("\t\tProperty[{0}> Key={1} : Value={2}", propCount++, prop.Key, prop.Value);
                    }

                    await deviceClient.CompleteAsync(receivedMessage);
                }
            }
        }
    }
}
