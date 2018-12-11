using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using servicioCliente.Models;

namespace servicioCliente.ServiceServer
{
    public class PartnerCalls
    {
        HttpClient client = new HttpClient();
        public PartnerCalls(string baseURI, string method){
            client.BaseAddress = new System.Uri(baseURI+method);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<string> RequestPartnerKey(CancellationToken cancellationToken, KeyService keyService){
            await Task.Delay(3000);
            var jsonObj = JsonConvert.SerializeObject(keyService);
            cancellationToken.ThrowIfCancellationRequested();
            var response = client.PostAsJsonAsync<string>(client.BaseAddress,jsonObj,cancellationToken);
            //HttpResponseMessage response = client.PostAsync(client.BaseAddress.ToString(),keyService);
            response.Wait();

            var result = response.Result;
            if(result.IsSuccessStatusCode){
                return JsonConvert.DeserializeObject<string>(result.ToString());
            }
            return new string("Error");
        }
    }
}