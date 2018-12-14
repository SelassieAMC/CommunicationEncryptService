using System;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using servicioCliente.AppUtils;
using servicioCliente.Models;
using static servicioCliente.AppUtils.Enums;

namespace servicioCliente.Encryptionlogic{
    public class RSAEncryption{
        public FileWriter fw;
        public RSAEncryption(){}
        ///Generation RSA Keys
        public string GeneratePubPrivKeys(IOptions<ParametersModel> param )
        {
            fw = new FileWriter(param);
            RSAModel rsaModel = new RSAModel();
            rsaModel = GenerateOwnRSAKeys(param);
            try
            {
                //Writing private key for RSA Encryption
                if(fw.WriteOnFile(param.Value.FilesOutput,param.Value.PrivKeyFile,rsaModel.PrivateKey)){
                    //Encrypt private key file
                    
                }else{
                    fw.WriteOnEvents(EventLevel.Error,"Error en la creacion del archivo de llave privada propia.");
                }
            }
            catch (System.Exception ex)
            {
                fw.WriteOnEvents(EventLevel.Exception,"Excepcion en RSAEncryption.GeneratePubPrivKeys\t"+ex.Message);
            }
            return rsaModel.PublicKey;
        }
        /// <summary>
        /// Metodo que genera llaves publica y privada del algoritmo RSA
        /// </summary>
        /// <returns>Modelo con informacion de llaves generadas</returns>
        private RSAModel GenerateOwnRSAKeys(IOptions<ParametersModel> param)
        {
            int keySize = param.Value.KeyRSASize;
            RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider(keySize);
            RSAModel rsaModel = new RSAModel();
            try
            {
                RSAParameters publicKey = cryptoServiceProvider.ExportParameters(false);
                RSAParameters privateKey = cryptoServiceProvider.ExportParameters(true);

                string publicKeyString = GetStringFromKey(publicKey);
                string privateKeyString = GetStringFromKey(privateKey);
                rsaModel.PrivateKey = privateKeyString;
                rsaModel.PublicKey = publicKeyString;
            }
            catch (System.Exception ex)
            {
                fw.WriteOnEvents(EventLevel.Exception,"Error generando llaves RSA. "+ex.Message);
            }
            return rsaModel;
        }

        private string GetStringFromKey(RSAParameters publicKey)
        {
            var stringWriter = new System.IO.StringWriter();
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xmlSerializer.Serialize(stringWriter, publicKey);
            return stringWriter.ToString();
        }
    }
}
