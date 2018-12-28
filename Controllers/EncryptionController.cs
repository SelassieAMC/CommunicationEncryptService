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
using servicioCliente.SignLogic;

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
        public IActionResult GeneratekeyEncrypts(InfoClients infoclient){
            //Serialize and store the request info
            var jsonObj = JsonConvert.SerializeObject(infoclient);
            FileWriter.WriteOnEvents(EventLevel.Info,"Request en GeneratekeyEncrypts ObjKeyService recibido "+ string.Join(", ",jsonObj));
            //Generate the own RSA Keys
            RSAEncryption rsaEncryption = new RSAEncryption();
            string filePublicKey = parameters.Value.FilesOutput+parameters.Value.PrivKeyFile+infoclient.userNameDestination;
            infoclient.keyEncrypt = rsaEncryption.GeneratePubPrivKeys(infoclient.userNameDestination,filePublicKey);
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
                if(resultCode.Equals(404)){
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
                FileWriter.WriteOnEvents(EventLevel.Exception,"Excepcion en GeneratekeyEncrypts. "+ ex.Message);
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
                        parameters.Value.PrivKeyFile+infoClients.userNameDestination,infoClients.keyEncrypt);
            FileWriter.WriteOnEvents(EventLevel.Info,
                        "Llave publica de "+ infoClients.userNameDestination+" almacenada.");
            //Delete and generate the own RSA Keys
            infoClients.keyEncrypt="";
            RSAEncryption rsaEncryption = new RSAEncryption();
            RSASigning rsaSigning = new RSASigning();
            string filePublicKey = parameters.Value.FilesOutput+parameters.Value.PrivKeyFile+infoClients.userNameDestination;
            infoClients.keyEncrypt = rsaEncryption.GeneratePubPrivKeys(infoClients.userNameDestination,filePublicKey);
            if(infoClients.keyEncrypt !=""){
                FileWriter.WriteOnEvents(EventLevel.Info,"Devolviendo llaves generadas.");
                return Ok(infoClients);
            }else{
                FileWriter.WriteOnEvents(EventLevel.Error,"Error generando llaves.");
                return BadRequest(infoClients);
            }
        }
        /// <summary>
        /// Funcion C - Recibe y Guarda Llave Publica
        /// </summary>
        /// <param name="infoClients"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult StoreKey(InfoClients infoClients){
            //Write Event
            var jsonObj = JsonConvert.SerializeObject(infoClients);
            FileWriter.WriteOnEvents(EventLevel.Info,
                        "Request en RequestStoreKeys ObjKeyService recibido "+ string.Join(", ",jsonObj));
            //Check data
            if(infoClients.keyEncrypt == ""){
                FileWriter.WriteOnEvents(EventLevel.Error,"Llave recibida vacia, no se puede almacenar.");
                return Ok(new {mensaje="Llave vacia."});
            }
            //Store partner's public key
            FileWriter.WriteOnFile(parameters.Value.FilesOutput,
                        parameters.Value.PubKeyFile+infoClients.userNameOrigin,infoClients.keyEncrypt);
            FileWriter.WriteOnEvents(EventLevel.Info,
                        "Llave publica de "+ infoClients.userNameOrigin+" almacenada.");
            return Ok();
        }
        /// <summary>
        /// Funcion D - Valida existencia llave del usuario origen
        /// </summary>
        /// <param name="infoClients"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ValidateUserKey(InfoClients infoClients){
            //Write Event
            var jsonObj = JsonConvert.SerializeObject(infoClients);
            FileWriter.WriteOnEvents(EventLevel.Info,
                        "Request en ValidateUserKey ObjKeyService recibido "+ string.Join(", ",jsonObj));
            RSAEncryption RSAencr = new RSAEncryption();
            //Check if the key container exist
            string filePublicKey = parameters.Value.FilesOutput+parameters.Value.PrivKeyFile+infoClients.userNameDestination;
            if(RSAencr.KeysPartnerExists(infoClients.userNameOrigin, filePublicKey)){
                return Ok(new {Result= true});
            }
            else{
                return Ok(new{Result = false});
            }
        }

        public void EncryptMessage(InteractionModel interactModel){
            
        }
    }
}