using System;
using System.Collections.Generic;
using System.Linq;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using System.Web.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;


namespace WebApplication1.Controllers
{
    public class ValuesController : ApiController
    {
        //GET api/values

        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            Valravn msg = new Valravn { Id = "1", DeviceId = "somedevice", Action = "Replace", ServicedBy = "John" };

            string valravnmsg = JsonConvert.SerializeObject(msg);

            return valravnmsg;
            //return "value";
        }

        // POST api/values
        public async void Post([FromBody]string msg)
        {
            //Task<HttpResponseMessage> 
            try
            {
                //Send msg to DocumentDB
                Random rnd = new Random();
                int yr = rnd.Next(1500, 2100); // creates a number between 1 and 12
                int month = rnd.Next(1, 13); // creates a number between 1 and 12
                int dice = rnd.Next(1, 7);   // creates a number between 1 and 6
                int card = rnd.Next(52);     // creates a number between 0 and 51

                var uid = "D" + Convert.ToString(yr) + Convert.ToString(month) + Convert.ToString(dice) + Convert.ToString(card);

                Valravn valravnmsg = JsonConvert.DeserializeObject<Valravn>(msg);
                valravnmsg.Id = uid;

                SendMsgtoConnector(valravnmsg.DeviceId, valravnmsg.ServicedBy, valravnmsg.AirTemperature, valravnmsg.Humidity, valravnmsg.Hydration);

                if (String.IsNullOrEmpty(valravnmsg.Id) == false)
                {
                    DocDbHandler cl = new DocDbHandler();
                    await cl.PostUWPToDocDb(valravnmsg);
                }

                //Send msg to Office Connector
                

                //return new HttpResponseMessage { StatusCode=HttpStatusCode.OK };
            }
            catch (Exception)
            {
                //return new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest};
            }
        }

        private async void SendMsgtoConnector(string devid, string servicedby, string airtemp, string humidity, string hyrdation)
        {
            try
            {
                using (HttpClient apiclient = new HttpClient())
                { 
                    apiclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var connectorurl = "https://outlook.office365.com/webhook/c01489c6-e2d6-4328-8dec-dc65f69739c1@72f988bf-86f1-41af-91ab-2d7cd011db47/779857a1-f3f0-4326-9bd8-4fce05376fca/4daffac6fdce41a3ba06593051a08867/79a60ca2-8603-4917-9e51-0690a7c49ac0";

                    ConnectorImages[] img = { new ConnectorImages { image = @"http://www.thzy8.com//images/I042093_03.jpg" } };

                    ConnectorFacts[] fact = { new ConnectorFacts {  name="Air Temperature:", value= airtemp }, new ConnectorFacts { name = "Humidity:", value = humidity }, new ConnectorFacts { name = "Hydration:", value = hyrdation } };

                    ConnectorSections[] sec = { new ConnectorSections { title = "Facts", facts = fact}, new ConnectorSections { title = "Images", images = img } };

                    MsgToConnector msg = new MsgToConnector
                    {
                        summary = DateTime.Now.ToString() + ": with the sensor image.",
                        //title = "Device Id:"+ devid + " AirTemperature: " + airtemp + " Humidity: " + humidity + " Hydration: " + hyrdation + " To be serviced By: " + servicedby,
                        title = "Stats about the device",
                        sections = sec
                    };

                    var msgjson = JsonConvert.SerializeObject(msg);

                    var connectorcontent = new StringContent(msgjson, System.Text.Encoding.UTF8, "application/json");

                    await apiclient.PostAsync(connectorurl, connectorcontent);
                }
            }
            catch (Exception)
                { }
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
