using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;
using servicioCliente.AppUtils;
using servicioCliente.Models;
using servicioCliente.ServiceServer;
using static servicioCliente.AppUtils.Enums;

namespace servicioCliente.Encryptionlogic{
    public class RSAEncryption{
        public FileWriter fw;
        public RSAEncryption(){}
        ///Generation RSA Keys
        public string GeneratePubPrivKeys(string partnerKeys, string publicKeyFile)
        {
            RSAModel rsaModel = new RSAModel();
            FileWriter.WriteOnEvents(EventLevel.Info,"Verificando la existencia de la llave a generar.");
            if(KeysPartnerExists(partnerKeys,publicKeyFile)){
                DeleteKeysPartner(partnerKeys);
            }
            rsaModel = GenerateOwnkeyEncrypts(partnerKeys,publicKeyFile);
            return rsaModel.PublicKey;
        }
        /// <summary>
        /// Metodo que genera llaves publica y privada del algoritmo RSA
        /// </summary>
        /// <returns>Modelo con informacion de llaves generadas</returns>
        private RSAModel GenerateOwnkeyEncrypts(string partnerKeys,string pathPublicKey)
        {
            int keySize = FileWriter.parameters.Value.KeyRSASize;
            CspParameters cp = new CspParameters();
            RSAModel rsaModel = new RSAModel();
            try
            {
                cp.KeyContainerName = "OwnkeyEncrypts"+partnerKeys;
                RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider(keySize,cp);
                FileWriter.WriteOnEvents(EventLevel.Info,"Inicio proceso de creacion de llaves.");
                RSAParameters publicKey = cryptoServiceProvider.ExportParameters(false);
                //RSAParameters privateKey = cryptoServiceProvider.ExportParameters(true);
                //string publicKey = cryptoServiceProvider.ToXmlString(false);
                FileWriter.WriteOnEvents(EventLevel.Info,"Proceso de creacion de llaves RSA exitoso.");
                string publicKeyString = GetStringFromKey(publicKey,pathPublicKey);
                //string privateKeyString = GetStringFromKey(privateKey);
                rsaModel.PublicKey = publicKeyString;
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Error generando llaves RSA. "+ex.Message);
            }
            return rsaModel;
        }

        private string GetStringFromKey(RSAParameters publicKey, string pathPublicKey)
        {
            // var stringWriter = new System.IO.StringWriter();
            // var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            // xmlSerializer.Serialize(stringWriter, publicKey);

            XmlSerializer xs = new XmlSerializer(typeof(RSAParameters));
            TextWriter tw = new StreamWriter(@pathPublicKey);
            xs.Serialize(tw, publicKey);
            tw.Close();
            string xmlKey = File.ReadAllText(pathPublicKey);
            File.Delete(pathPublicKey);
            return xmlKey;
            //return stringWriter.ToString();
        }

        private RSAParameters GetParamsFromString(string publicKeyPath)
        {
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            FileStream fs = new FileStream(publicKeyPath, FileMode.Open);
            RSAParameters rsaParam;
            rsaParam = (RSAParameters) xmlSerializer.Deserialize(fs);
            fs.Close();
            return rsaParam;
        }
        /// <summary>
        /// Valida si las llaves solicitadas existen
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns>true = Existen;false = No existen</returns>
        public bool KeysPartnerExists(string containerName, string publicKeyFile){
            CspParameters cspParameters = new CspParameters{
                Flags = CspProviderFlags.UseExistingKey,
                KeyContainerName = "OwnkeyEncrypts"+containerName
            };
            try
            {
                RSACryptoServiceProvider RSAcsp = new RSACryptoServiceProvider(cspParameters);
                FileWriter.WriteOnEvents(EventLevel.Info,"La llave existe en el contenedor.");
                if(File.Exists(publicKeyFile+".xml")){
                    FileWriter.WriteOnEvents(EventLevel.Info,"La llave publica del receptor existe.");
                    return true;
                }else{
                    FileWriter.WriteOnEvents(EventLevel.Atention,"La llave publica del receptor no existe.");
                    DeleteKeysPartner(containerName);
                }
                return false;
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Atention,"La llave solicitada no existe. "+ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Elimina el contenedor con las llaves indicadas
        /// </summary>
        /// <param name="containerName"></param>
        /// <returns>true = Eliminado; false = Error</returns>
        public bool DeleteKeysPartner(string containerName){
            CspParameters cp = new CspParameters();  
            cp.KeyContainerName = "OwnkeyEncrypts"+containerName;  
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp);  
                rsa.PersistKeyInCsp = false;    
                rsa.Clear();
                FileWriter.WriteOnEvents(EventLevel.Atention,"Las llaves del contenedor "+containerName+" fueron eliminadas."); 
                return true;
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Error intentando eliminar las llaves de "+containerName+". "+ex.Message);
                return false;
            }
        }

        internal ResponseEncryptAESKey EncryptAESKey(byte[] privateKey, string publicKey)
        {
            string filePublic = publicKey+".xml";
            ResponseEncryptAESKey response = new ResponseEncryptAESKey();
            FileWriter.WriteOnEvents(EventLevel.Info,"Buscando llave publica en: "+filePublic);
            if(File.Exists(filePublic)){
                FileWriter.WriteOnEvents(EventLevel.Info,"Llave publica encontrada!!!.");
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                try
                {
                    FileWriter.WriteOnEvents(EventLevel.Info,"Leyendo contenido llave publica.");
                    FileWriter.WriteOnEvents(EventLevel.Info,"Importando llave para proceso de cifrado");
                    RSA.ImportParameters(GetParamsFromString(filePublic));
                    response.encryptedKey = RSA.Encrypt(privateKey,true);
                    FileWriter.WriteOnEvents(EventLevel.Info,"Llave simetrica cifrada de manera exitosa.!!");
                    response.resul = true;
                }
                catch (System.Exception ex)
                {
                    FileWriter.WriteOnEvents(EventLevel.Exception,"Error en el proceso de cifrado de llave AES. "+ex.Message);
                    response.resul = false;
                }
            }else{
                FileWriter.WriteOnEvents(EventLevel.Info,"No se encontro la llave publica para cifrado.");
                response.resul = false;
            }
            return response;
        }

        internal ResponseRSADecryption DecryptAESKey(byte[] encryptedKey, string containerName)
        {
            ResponseRSADecryption response = new ResponseRSADecryption{
                result = false
            };
            byte[] decryptedKey;

            CspParameters cspParameters = new CspParameters{
                Flags = CspProviderFlags.UseExistingKey,
                KeyContainerName = "OwnkeyEncrypts"+containerName
            };
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(FileWriter.parameters.Value.KeyRSASize,cspParameters); 
                RSAParameters publicKey = rsa.ExportParameters(false);
                RSAParameters privateKey = rsa.ExportParameters(true);
                FileWriter.WriteOnEvents(EventLevel.Info,"Inicio proceso de descifrado de llave AES."); 
                decryptedKey = rsa.Decrypt(encryptedKey,true);
                response.decryptedKey = decryptedKey;
                FileWriter.WriteOnEvents(EventLevel.Info,"Proceso de descifrado de llave AES finalizada correctamente");
                response.result = true;
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Error descifrando llave aes"+ex.Message);
            }
            return response;
        }
    }
}
