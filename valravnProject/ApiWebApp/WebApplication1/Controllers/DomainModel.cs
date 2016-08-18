using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using System.Net;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json;


        public class Valravn
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }
            public string DeviceId { get; set; }
            public string Action { get; set; }
            public string ServicedBy { get; set; }
            public string AirTemperature{ get; set; }
            public string Humidity { get; set; }
            public string Hydration{ get; set; }



        public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

    public class MsgToConnector
    {
        public string summary { get; set; }
        public string title { get; set; }
        public ConnectorSections[] sections { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);

        }
    }

    public class ConnectorSections
    {
        public string title { get; set; }
        public ConnectorFacts[] facts { get; set; }
        public ConnectorImages[] images { get; set; }
    }

    public class ConnectorImages
    {
        public string image { get; set; }
    }

    public class ConnectorFacts
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class UWPReturnMsg
    {
        public int StatusCode { get; set; }
    }





    }