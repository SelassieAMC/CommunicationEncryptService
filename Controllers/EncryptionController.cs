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
using servicioCliente.EncryptionLogic;

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
            //Take origin as destination, this methos is called for server keys
            string filePublicKey = parameters.Value.FilesOutput+parameters.Value.PrivKeyFile+infoClients.userNameOrigin;
            if(RSAencr.KeysPartnerExists(infoClients.userNameOrigin, filePublicKey)){
                return Ok(new {Result= true});
            }
            else{
                return Ok(new{Result = false});
            }
        }
        /// <summary>
        /// Funcion E - Cifrado de mensaje, envio a servidor de mensaje cifrado en AES, firma del mensaje, llave simetrica cifrada y firma de identificacion
        /// </summary>
        /// <param name="interactModel">Modelo de mensaje y datos</param>
        /// <returns>Transaccion satisfactoria o fallida</returns>
        public bool EncryptMessage(InteractionModel interactModel){
            //Generate url's file
            string filePublicKey = parameters.Value.FilesOutput+parameters.Value.PrivKeyFile+interactModel.userNameDestination;
            //Initialize models and classes
            RSAEncryption rsaEncrypt = new RSAEncryption();
            RSASigning rsaSigning = new RSASigning(interactModel.userNameDestination);
            AESEncryption aesEncryption = new AESEncryption(parameters.Value.KeyAESSize);
            ResponseSignData responseSign = new ResponseSignData();
            ResponseSignData responseSignId = new ResponseSignData();
            ResponseEncryptAES responseAES = new ResponseEncryptAES();
            ResponseEncryptAESKey responseAESKey = new ResponseEncryptAESKey();
            //Busca llave publica RSA destino
            if(rsaEncrypt.KeysPartnerExists(interactModel.userNameDestination,filePublicKey)){
                FileWriter.WriteOnEvents(EventLevel.Info,"Llaves RSA para cifrado encontradas.");
                FileWriter.WriteOnEvents(EventLevel.Info,"Iniciando firmado de mensaje.");
                //Firma hash de mensaje con RSA
                responseSign = rsaSigning.signData(interactModel.mensaje);
                if(responseSign.result){
                    //Cifrado de mensaje
                    if(aesEncryption.generateProperties()){
                        responseAES = aesEncryption.EncryptMessage(interactModel.mensaje);
                        if(!responseAES.result){
                            FileWriter.WriteOnEvents(EventLevel.Error,"Error en el proceso de cifrado de mensaje, verifique los eventos previos.");
                            return false;
                        }
                    }
                }else{
                    FileWriter.WriteOnEvents(EventLevel.Error,"Falla en intento de firma de mensaje, verificar logs anteriores.");
                    return false;
                }
            }else{
                FileWriter.WriteOnEvents(EventLevel.Error,
                    "Imposible cifrar mensaje, llaves RSA para origen:"+
                    interactModel.userNameOrigin+"\tdestino:"+interactModel.userNameDestination+"no encontradas");
                    return false;
            }   
            //Cifra llave AES
            if(responseAES.privateKey != null){
                FileWriter.WriteOnEvents(EventLevel.Info,"Iniciando proceso de cifrado llaves AES con RSA");
                responseAESKey = rsaEncrypt.EncryptAESKey(responseAES.privateKey,filePublicKey);
            }else{
                FileWriter.WriteOnEvents(EventLevel.Error,"Error en cifrado llave AES con RSA, no existe la llave de AES.");
                return false;
            }
            //Generate de sign for server identification
            responseSignId = rsaSigning.signData(interactModel.userNameOrigin+interactModel.userNameDestination);
            if(!responseSignId.result){
                FileWriter.WriteOnEvents(EventLevel.Error,"Falla en intento de firma de identificacion contra servidor, verificar logs anteriores.");
                return false;
            }
            //Call the server service and send the data model
            ServerRequest server = new ServerRequest(parameters.Value.EndpointServer,parameters.Value.SendFirstMessage);
            SendMessageModel sendFirstMessage = new SendMessageModel{
                encryptedMessage = responseAES.encryptedData,
                encryptSignature = responseSign.signData,
                encryptedKey = responseAESKey.encryptedKey,
                idSignature = responseSignId.signData,
                initVector = responseAES.InitVector
            };
            try
            {
                HttpStatusCode resultCode = new HttpStatusCode();
                Task<HttpStatusCode> response = server.SendMessage(sendFirstMessage);
                response.ContinueWith(task=>{
                    resultCode = task.Result;
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
                //Check the response values, if isn´t success set false
                if(resultCode.Equals(404)){
                    FileWriter.WriteOnEvents(EventLevel.Atention,"Intento de envio no satisfactorio. resultCode:"+ resultCode);
                    return false;
                }
                else{
                    FileWriter.WriteOnEvents(EventLevel.Info,"Llave enviada de forma satisfactoria. resultCode:"+ resultCode);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Excepcion en intento de envio de mensaje a servidor. "+ ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Funcion F - Recibe mensaje del servidor
        /// </summary>
        /// <param name="messageModel"></param>
        /// <returns></returns>
        public IActionResult ReceiveMessage(SendMessageModel messageModel){
            //Descifra llave AES

            //Descifra mensaje

            //Verifica firma

            //Muestra mensaje

            //confirma respuesta
            return Ok();
        }
    }
}