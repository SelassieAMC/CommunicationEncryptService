using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using servicioCliente.AppUtils;
using servicioCliente.Models;
using static servicioCliente.AppUtils.Enums;

namespace servicioCliente.ServiceServer
{
    public class ServerRequest
    {
        private static readonly HttpClient client = new HttpClient();
        public int count = 0;
        string method = "";
        public ServerRequest(string baseURI, string method){
            try
            {
                DefineCallParams(baseURI,method);
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Excepcion seteando parametros de petición. "+ex.Message);
                DefineCallParams(baseURI,method);
                client.CancelPendingRequests();
                if(count==4) return;
                FileWriter.WriteOnEvents(EventLevel.Atention,"Reintentando seteo numero "+(++count));
            }
        }
        /// <summary>
        /// Define param for making the call to the server
        /// </summary>
        /// <param name="baseURI"></param>
        /// <param name="method"></param>
        private void DefineCallParams(string baseURI, string method)
        {
            FileWriter.WriteOnEvents(EventLevel.Info,"Inicio peticion a servidor "+baseURI+method.Substring(1));
            client.BaseAddress = new Uri(baseURI);
            this.method = method;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<HttpStatusCode> RequestPartnerKey(InfoClients infoClients){
            try
            {
                string uriComplete = client.BaseAddress+method.Substring(1);
                var myRequest = (HttpWebRequest)WebRequest.Create(uriComplete);
                var responseInfo = (HttpWebResponse)myRequest.GetResponse();
                if(responseInfo.StatusCode == HttpStatusCode.OK){
                    HttpResponseMessage response = await client.PostAsJsonAsync(method,infoClients);
                    response.EnsureSuccessStatusCode();
                    return response.StatusCode;
                }else{
                    FileWriter.WriteOnEvents(EventLevel.Error,"El servidor del servicio no responde. StatusCode:"+responseInfo.StatusCode);
                    return responseInfo.StatusCode;
                }
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Error,"Servidor no disponible. "+ex.Message);
                return HttpStatusCode.NotFound;
            } 
        }

        public async Task<HttpStatusCode> SendMessage(SendMessageModel modelRequest){
            try
            {
                string uriComplete = client.BaseAddress+method.Substring(1);
                var myRequest = (HttpWebRequest)WebRequest.Create(uriComplete);
                var responseInfo = (HttpWebResponse)myRequest.GetResponse();
                if(responseInfo.StatusCode == HttpStatusCode.OK){
                    HttpResponseMessage response = await client.PostAsJsonAsync(method,modelRequest);
                    response.EnsureSuccessStatusCode();
                    return response.StatusCode;
                }else{
                    FileWriter.WriteOnEvents(EventLevel.Error,"El servidor del servicio no responde. StatusCode:"+responseInfo.StatusCode);
                    return responseInfo.StatusCode;
                }
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Error,"Servidor no disponible. "+ex.Message);
                return HttpStatusCode.NotFound;
            }
        }
    }
}