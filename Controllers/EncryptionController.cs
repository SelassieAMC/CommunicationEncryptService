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
            string filePublicKey = parameters.Value.FilesOutput+parameters.Value.PubKeyFile+infoclient.userNameDestination+infoclient.userNameOrigin+".xml";
            infoclient.keyEncrypt = rsaEncryption.GeneratePubPrivKeys(infoclient.userNameDestination+infoclient.userNameOrigin,filePublicKey);
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
                ServerRequest callPartner = new ServerRequest(parameters.Value.EndpointServer,parameters.Value.RequestKeyPartner,parameters.Value.GetRequest);
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
                        parameters.Value.PubKeyFile+infoClients.userNameOrigin+infoClients.userNameDestination,infoClients.keyEncrypt);
            FileWriter.WriteOnEvents(EventLevel.Info,
                        "Llave publica de "+ infoClients.userNameOrigin+infoClients.userNameDestination+" almacenada.");
            //Delete and generate the own RSA Keys
            infoClients.keyEncrypt="";
            RSAEncryption rsaEncryption = new RSAEncryption();
            string filePublicKey = parameters.Value.FilesOutput+parameters.Value.PubKeyFile+infoClients.userNameOrigin+infoClients.userNameDestination;
            infoClients.keyEncrypt = rsaEncryption.GeneratePubPrivKeys(infoClients.userNameOrigin+infoClients.userNameDestination,filePublicKey);
            //Inverting usernames origin-destination
            string auxUserName = infoClients.userNameOrigin;
            infoClients.userNameOrigin = infoClients.userNameDestination;
            infoClients.userNameDestination = auxUserName;
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
                        "Request en StoreKey ObjKeyService recibido "+ string.Join(", ",jsonObj));
            //Check data
            if(infoClients.keyEncrypt == ""){
                FileWriter.WriteOnEvents(EventLevel.Error,"Llave recibida vacia, no se puede almacenar.");
                return Ok(new {mensaje="Llave vacia."});
            }
            //Store partner's public key
            FileWriter.WriteOnFile(parameters.Value.FilesOutput,
                        parameters.Value.PubKeyFile+infoClients.userNameOrigin+infoClients.userNameDestination,infoClients.keyEncrypt);
            FileWriter.WriteOnEvents(EventLevel.Info,
                        "Llave publica "+ infoClients.userNameOrigin+infoClients.userNameDestination+" almacenada.");
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
            string filePublicKey = parameters.Value.FilesOutput+parameters.Value.PubKeyFile+infoClients.userNameDestination+infoClients.userNameOrigin;
            if(RSAencr.KeysPartnerExists(infoClients.userNameDestination+infoClients.userNameOrigin, filePublicKey)){
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
        [HttpPost]
        public IActionResult EncryptMessage(InteractionModel interactModel){
            //Generate url's file
            string filePublicKey = parameters.Value.FilesOutput+parameters.Value.PubKeyFile+interactModel.userNameDestination+interactModel.userNameOrigin;
            //Initialize models and classes
            SendMessageModel sendFirstMessage = new SendMessageModel();
            RSAEncryption rsaEncrypt = new RSAEncryption();
            RSASigning rsaSigning = new RSASigning(interactModel.userNameDestination+interactModel.userNameOrigin);
            AESEncryption aesEncryption = new AESEncryption(parameters.Value.KeyAESSize);
            ResponseSignData responseSign = new ResponseSignData();
            ResponseSignData responseSignId = new ResponseSignData();
            ResponseEncryptAES responseAES = new ResponseEncryptAES();
            ResponseEncryptAESKey responseAESKey = new ResponseEncryptAESKey();
            // Looking for partner RSA public key 
            if(rsaEncrypt.KeysPartnerExists(interactModel.userNameDestination+interactModel.userNameOrigin,filePublicKey)){
                FileWriter.WriteOnEvents(EventLevel.Info,"Llaves RSA para cifrado encontradas.");
                FileWriter.WriteOnEvents(EventLevel.Info,"Iniciando firmado de mensaje.");
                //Sign data with RSA Private Key
                responseSign = rsaSigning.signData(interactModel.mensaje);
                if(responseSign.result){
                    //Encrypt Message
                    if(aesEncryption.generateProperties()){
                        responseAES = aesEncryption.EncryptMessage(interactModel.mensaje);
                        if(!responseAES.result){
                            FileWriter.WriteOnEvents(EventLevel.Error,"Error en el proceso de cifrado de mensaje, verifique los eventos previos.");
                            return BadRequest(sendFirstMessage);
                        }
                    }
                }else{
                    FileWriter.WriteOnEvents(EventLevel.Error,"Falla en intento de firma de mensaje, verificar logs anteriores.");
                    return BadRequest(sendFirstMessage);
                }
            }else{
                FileWriter.WriteOnEvents(EventLevel.Error,
                    "Imposible cifrar mensaje, llaves RSA para origen:"+
                    interactModel.userNameOrigin+"\tdestino:"+interactModel.userNameDestination+" no encontradas");
                    return BadRequest(sendFirstMessage);
            }   
            //Encrypt AES Key
            if(responseAES.privateKey != null){
                FileWriter.WriteOnEvents(EventLevel.Info,"Iniciando proceso de cifrado llaves AES con RSA");
                responseAESKey = rsaEncrypt.EncryptAESKey(responseAES.privateKey,filePublicKey);
            }else{
                FileWriter.WriteOnEvents(EventLevel.Error,"Error en cifrado llave AES con RSA, no existe la llave de AES.");
                return BadRequest(sendFirstMessage);
            }
            //Generate de sign for server identification
            //responseSignId = rsaSigning.signData(interactModel.userNameOrigin+interactModel.userNameDestination);
            //if(!responseSignId.result){
            //     FileWriter.WriteOnEvents(EventLevel.Error,"Falla en intento de firma de identificacion contra servidor, verificar logs anteriores.");

            //     return BadRequest(sendFirstMessage);
            // }
            //Call the server service and send the data model
            //ServerRequest server = new ServerRequest(parameters.Value.EndpointServer,parameters.Value.SendFirstMessage,parameters.Value.GetRequest);
            
                sendFirstMessage.encryptedMessage = responseAES.encryptedData;
                sendFirstMessage.encryptSignature = responseSign.signData;
                sendFirstMessage.encryptedKey = responseAESKey.encryptedKey;
                sendFirstMessage.idSignature = responseSignId.signData;
                sendFirstMessage.initVector = responseAES.InitVector;
                sendFirstMessage.userNameOrigin = interactModel.userNameOrigin;
                sendFirstMessage.userNameDestination = interactModel.userNameDestination;
            
            FileWriter.WriteOnEvents(EventLevel.Info,"Solicitud de envio de llave exitoso.");
            return Ok(sendFirstMessage);
        }
        /// <summary>
        /// Funcion F - Recibe mensaje del servidor
        /// </summary>
        /// <param name="messageModel"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ReceiveMessage(SendMessageModel messageModel){
            string filePublicKey = parameters.Value.FilesOutput+parameters.Value.PubKeyFile+messageModel.userNameDestination+messageModel.userNameOrigin;
            RSAEncryption rsaEncryption = new RSAEncryption();
            AESEncryption aesEncryption = new AESEncryption();
            RSASigning rsaSigning = new RSASigning();
            
            //Decrypt symmetric key
            ResponseRSADecryption rsaDecryptResponse = new ResponseRSADecryption();
            rsaDecryptResponse = rsaEncryption.DecryptAESKey(messageModel.encryptedKey, messageModel.userNameDestination+messageModel.userNameOrigin);
            if(!rsaDecryptResponse.result){
                FileWriter.WriteOnEvents(EventLevel.Error,"Error descifrando llave AES con RSA.");
                return BadRequest(new {result = false});
            }
            //Decrypt Message
            ResponseAESDecryption responseAESDecryption = new ResponseAESDecryption();
            responseAESDecryption = aesEncryption.DecryptMessage(messageModel,rsaDecryptResponse.decryptedKey);
            if(!responseAESDecryption.result){
                FileWriter.WriteOnEvents(EventLevel.Error,"Error descifrando mensaje con AES.");
                return BadRequest(new {result = false});
            }
            //Validate Sign
            if(!rsaSigning.validateSignAndHash(responseAESDecryption.decryptedMessage,messageModel.encryptSignature,filePublicKey)){ 
                FileWriter.WriteOnEvents(EventLevel.Atention,"La información recibida es corrupta.");
                return BadRequest(new {result = false});
            }
            //Muestra mensaje
            return Ok(new{mensaje = responseAESDecryption.decryptedMessage});
        }
    }
}