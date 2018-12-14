using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using servicioCliente.Models;

namespace servicioCliente.ServiceServer
{
    public class PartnerCalls
    {
        private static readonly HttpClient client = new HttpClient();
        string method = "";
        public PartnerCalls(string baseURI, string method){
            client.BaseAddress = new Uri(baseURI);
            this.method = method;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<string> RequestPartnerKey(KeyService keyService){
            HttpResponseMessage response = await client.PostAsJsonAsync(method,keyService);
            response.EnsureSuccessStatusCode();
            return response.StatusCode.ToString();
        }
    }
}