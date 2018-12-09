using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using servicioCliente.AppUtils;
using servicioCliente.Encryptionlogic;
using servicioCliente.Models;
using static servicioCliente.AppUtils.Enums;

namespace servicioCliente.Controllers
{
    [Route("CommunicationManager/[controller]/{action}")]
    [ApiController]
    public class EncryptionController : ControllerBase
    {
        private readonly IOptions<ParametersModel> parameters;
        FileWriter fw;
        public EncryptionController(IOptions<ParametersModel> config){
            parameters = config;
            fw = new FileWriter(parameters);
        }

        [HttpPost]
        public string GenerateOwnRSAKey(KeyService keyService){
            //Store the key's partner
            var jsonObj = JsonConvert.SerializeObject(keyService);
            // fw.WriteOnEvents(EventLevel.Atention,"ObjKeyService recibido "+ JsonConvert.DeserializeObject(jsonObj));
            fw.WriteOnEvents(EventLevel.Atention,"ObjKeyService recibido "+ string.Join(", ",jsonObj));
            //Generate own keys for RSA Encryption
            RSAEncryption rsaEncryption = new RSAEncryption();
            RSAModel rsaModel = new RSAModel();
            rsaModel = rsaEncryption.GeneratePubPrivKeys(parameters);
            //return the publicKey
            return rsaModel.PublicKey;
        }

        public string RequestPartnerKeys(KeyService keyService){
            bool partnerOk=false;
            //Call Server Service for Get partner Keys

            //if the answer of the server is partner online, return keys, else wait for the answer
            if(partnerOk){
                return "keysOk";
            }
            else{
                return "Partner is offline";
            }       
        }
    }
}