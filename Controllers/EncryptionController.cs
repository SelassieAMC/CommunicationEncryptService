using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using servicioCliente.AppUtils;
using servicioCliente.Encryptionlogic;
using servicioCliente.Models;
using servicioCliente.ServiceServer;
using static servicioCliente.AppUtils.Enums;

namespace servicioCliente.Controllers
{
    [Route("CommunicationManager/[controller]/{action}")]
    [ApiController]
    public class EncryptionController : ControllerBase
    {
        private readonly IOptions<ParametersModel> parameters;
        public EncryptionController(IOptions<ParametersModel> config){
            parameters = config;
            FileWriter.parameters = config;
        }
        /// <summary>
        /// Funcion A - Genera Primera Llave
        /// </summary>
        /// <param name="infoclient"></param>
        /// <returns></returns>
        [HttpPost] 
        public IActionResult GenerateRSAKeys(InfoClients infoclient){
            //Serialize and store the request info
            var jsonObj = JsonConvert.SerializeObject(infoclient);
            FileWriter.WriteOnEvents(EventLevel.Info,"Request en GenerateRSAKeys ObjKeyService recibido "+ string.Join(", ",jsonObj));
            //Generate the own RSA Keys
            RSAEncryption rsaEncryption = new RSAEncryption();
            infoclient.RSAKey = rsaEncryption.GeneratePubPrivKeys(infoclient.userDestino);
            //Call the server service to send my public key to my partner 
            if(SendMyPublicKey(infoclient)){
                FileWriter.WriteOnEvents(EventLevel.Info,"Proceso de generacion y envio de llaves realizado de forma correcta.");
                return Ok("Proceso de generacion y envio de llaves realizado de forma correcta.");
            }
            FileWriter.WriteOnEvents(EventLevel.Atention,"Ocurrieron errores en el proceso, verifique e intente nuevamente.");
            return BadRequest("Ocurrieron errores en el proceso, verifique el log e intente nuevamente.");
        } 
        public bool SendMyPublicKey(InfoClients infoClients){
            bool result = false;
            FileWriter.WriteOnEvents(EventLevel.Info,"Llamado al servidor para entrega de llave privada.");
            try
            {
                ServerRequest callPartner = new ServerRequest(parameters.Value.EndpointServer,parameters.Value.RequestKeyPartner);
                HttpStatusCode resultCode = new HttpStatusCode();
                Task<HttpStatusCode> response = callPartner.RequestPartnerKey(infoClients);
                response.ContinueWith(task=>{
                    resultCode = task.Result;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
                //Check the response values, if isn´t success set false
                if(!resultCode.Equals(200)){
                    FileWriter.WriteOnEvents(EventLevel.Atention,"Respuesta no satisfactoria. resultCode:"+ resultCode);
                    result = false;
                }
                else{
                    FileWriter.WriteOnEvents(EventLevel.Info,"Llave enviada de forma satisfactoria. resultCode:"+ resultCode);
                    result = true;
                }
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Excepcion en GenerateRSAKeys. "+ ex.Message);
                result = false;
            }
            return result;
        }
        /// <summary>
        /// Funcion B - Recibe llave, elimina anterior y genera llave
        /// </summary>
        /// <param name="infoClients"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult RequestStoreKeys(InfoClients infoClients){
            //Write Event
            var jsonObj = JsonConvert.SerializeObject(infoClients);
            FileWriter.WriteOnEvents(EventLevel.Info,
                        "Request en RequestStoreKeys ObjKeyService recibido "+ string.Join(", ",jsonObj));
            //Store partner's public key
            FileWriter.WriteOnFile(parameters.Value.FilesOutput,
                        parameters.Value.PrivKeyFile+infoClients.userOrigen,infoClients.RSAKey);
            FileWriter.WriteOnEvents(EventLevel.Info,
                        "Llave publica de"+ infoClients.userOrigen+"almacenada.");
            //Generate the own RSA Keys
            RSAEncryption rsaEncryption = new RSAEncryption();
            infoClients.RSAKey = rsaEncryption.GeneratePubPrivKeys(infoClients.userOrigen);
            return Ok(infoClients);
        }
        /// <summary>
        /// Funcion C - Recibe y Guarda Llave
        /// </summary>
        /// <param name="infoClients"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult StoreKey(InfoClients infoClients){
            return Ok();
        }
        /// <summary>
        /// Funcion D - Valida existencia llave del usuario origen
        /// </summary>
        /// <param name="infoClients"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ValidateUserKey(InfoClients infoClients){
            return Ok();
        }
    }
}