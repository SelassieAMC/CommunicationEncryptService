using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using servicioCliente.Encryptionlogic;
using servicioCliente.Models;

namespace servicioCliente.Controllers
{
    [Route("CommunicationManager/[controller]/{action}")]
    [ApiController]
    public class EncryptionController : ControllerBase
    {
        private readonly IOptions<ParametersModel> Parameters;
        public EncryptionController(IOptions<ParametersModel> config){
            Parameters = config;
        }

        [HttpPost]
        public string GenerateOwnRSAKey(KeyService keyService){
            //Store the key's partner
            //Generate own keys for RSA Encryption
            RSAEncryption rsaEncryption = new RSAEncryption();
            RSAModel rsaModel = new RSAModel();
            rsaModel = rsaEncryption.GeneratePubPrivKeys(Parameters);
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