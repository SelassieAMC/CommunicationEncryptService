using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        private readonly FileWriter fw;
        public EncryptionController(IOptions<ParametersModel> config){
            parameters = config;
            fw = new FileWriter(parameters);
        }

        [HttpPost]
        public IActionResult GenerateClientRSAKey(KeyService keyService){
            //Store the key's partner
            var jsonObj = JsonConvert.SerializeObject(keyService);
            // fw.WriteOnEvents(EventLevel.Atention,"ObjKeyService recibido "+ JsonConvert.DeserializeObject(jsonObj));
            fw.WriteOnEvents(EventLevel.Atention,"ObjKeyService recibido "+ string.Join(", ",jsonObj));
            fw.WriteOnFile(parameters.Value.FilesOutput,parameters.Value.PubKeyFile,keyService.keyEncrypt);
            //Generate own keys for RSA Encryption
            RSAEncryption rsaEncryption = new RSAEncryption();
            string publicKey = rsaEncryption.GeneratePubPrivKeys(parameters);
            //Fill object to response request
            KeyService response = new KeyService{
                originIp = "own ip address",
                destinationIp = keyService.originIp,
                keyEncrypt = publicKey
            };
            //return the publicKey
            return Ok(response);
        }

        [HttpPost]
        public IActionResult RequestPartnerKeys(KeyService keyService){
            bool partnerOk=false;
            fw.WriteOnEvents(EventLevel.Info,"Llamado por llaves al servidor");
            //Call Server Service for Get partner Keys
            string keyPartner = "";
            PartnerCalls callPartner = new PartnerCalls(parameters.Value.EndpointServer,parameters.Value.RequestKeyPartner);
            var response = callPartner.RequestPartnerKey(keyService);
            response.ContinueWith(task=>{
                keyPartner = task.Result;
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            //if the answer of the server is partner online, return keys, else wait for the answer
            partnerOk = keyPartner!=""?true:false;
            if(partnerOk){
                return Ok(partnerOk);
            }
            else{
                return NotFound(partnerOk);
            }       
        }
    }
}