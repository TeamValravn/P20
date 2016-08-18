using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using Newtonsoft.Json;
using System.Web;


namespace ValravnClient
{
    public class Valravn
    {
        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string Action { get; set; }
        public string ServicedBy { get; set; }
        public string AirTemperature { get; set; }
        public string Humidity { get; set; }
        public string Hydration { get; set; }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
            
        }
    }
 
    


}
