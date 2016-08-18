using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceClientAmqpSample
{
    public class DeviceDataModel
    {
        public string MessageType;
        public string Device_ID;
        public string status;
        public float AirTemperature;
        public float humidity;
        public float hydration;
        public float latitude;
        public float longitude;
        public string ImageUrl;
        public string DeviceResponses_MetaData;
        public string SubmissionTime;
        public string Expiry;

    }
}
