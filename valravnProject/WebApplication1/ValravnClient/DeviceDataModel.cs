using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValravnClient
{
    public class DeviceDataModel
    {
        public string MessageType { get; set; }
        public string Device_ID { get; set; }
        public string status { get; set; }
        public float AirTemperature { get; set; }
        public string humidity { get; set; }
        public float hydration { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string ImageUrl { get; set; }
        public string DeviceResponses_MetaData { get; set; }
        public string SubmissionTime { get; set; }
        public string Expiry { get; set; }
    }
}
