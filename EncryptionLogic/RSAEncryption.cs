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
        public string GeneratePubPrivKeys(string partnerKeys)
        {
            RSAModel rsaModel = new RSAModel();
            rsaModel = GenerateOwnRSAKeys(partnerKeys);
            try
            {
                //No necesito guardar la llave privada,windows lo hace
                //Writing private key for RSA Encryption
                // if(FileWriter.WriteOnFile(FileWriter.parameters.Value.FilesOutput,
                //         FileWriter.parameters.Value.PrivKeyFile,
                //         rsaModel.PrivateKey)){
                //     FileWriter.WriteOnEvents(EventLevel.Info,"Creacion del archivo de llave privada exitoso.");
                //     //Encrypt private key file
                //     //Seguimos aqui...                    
                // }else{
                //     FileWriter.WriteOnEvents(EventLevel.Error,"Error en la creacion del archivo de llave privada propia.");
                // }
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Excepcion en RSAEncryption.GeneratePubPrivKeys\t"+ex.Message);
            }
            return rsaModel.PublicKey;
        }
        /// <summary>
        /// Metodo que genera llaves publica y privada del algoritmo RSA
        /// </summary>
        /// <returns>Modelo con informacion de llaves generadas</returns>
        private RSAModel GenerateOwnRSAKeys(string partnerKeys)
        {
            int keySize = FileWriter.parameters.Value.KeyRSASize;
            CspParameters cp = new CspParameters();
            cp.KeyContainerName = "OwnRSAKeys"+partnerKeys;

            RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider(keySize,cp);
            RSAModel rsaModel = new RSAModel();
            try
            {
                FileWriter.WriteOnEvents(EventLevel.Info,"Inicio proceso de creacion de llaves.");
                // RSAParameters publicKey = cryptoServiceProvider.ExportParameters(false);
                // RSAParameters privateKey = cryptoServiceProvider.ExportParameters(true);
                string publicKey = cryptoServiceProvider.ToXmlString(false);
                //string privateKey = cryptoServiceProvider.ToXmlString(true); //No necesito tocar la privada
                FileWriter.WriteOnEvents(EventLevel.Info,"Proceso de creacion de llaves RSA exitoso.");
                // string publicKeyString = GetStringFromKey(publicKey);
                // string privateKeyString = GetStringFromKey(privateKey);
                rsaModel.PrivateKey = publicKey;
                //rsaModel.PublicKey = privateKey;
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Error generando llaves RSA. "+ex.Message);
            }
            return rsaModel;
        }

        // private string GetStringFromKey(RSAParameters publicKey)
        // {
        //     var stringWriter = new System.IO.StringWriter();
        //     var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
        //     xmlSerializer.Serialize(stringWriter, publicKey);
        //     return stringWriter.ToString();
        // }
    }
}
