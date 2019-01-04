using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using servicioCliente.AppUtils;
using servicioCliente.Models;
using static servicioCliente.AppUtils.Enums;

namespace servicioCliente.SignLogic
{
    public class RSASigning
    {
        protected bool state = false;
        protected RSACryptoServiceProvider rsaCryptoServ;
        public RSASigning(string containerName){
            CspParameters cspParameters = new CspParameters{
                Flags = CspProviderFlags.UseExistingKey,
                KeyContainerName = "OwnkeyEncrypts"+containerName
            };
            findPrivateKey(cspParameters);
        }
        public RSASigning(){}
        private void findPrivateKey(CspParameters rsaParameters)
        {
            try
            {
                rsaCryptoServ = new RSACryptoServiceProvider(rsaParameters);
                FileWriter.WriteOnEvents(EventLevel.Info,"Llave para firmas encontrada.");
                state= true;
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"La llave para firmas no existe. "+ex.Message);
            }
        }

        public ResponseSignData signData(string msn)
        {
            ResponseSignData response = new ResponseSignData();
            byte[] signedMsn;
            if(state){
                signedMsn = HashAndSignMessage(msn);
                response.signData = signedMsn;
                response.result = true;
            }else{
                response.result = false;
            }

            return response;
        }

        private byte[] HashAndSignMessage(string msn)
        {
            byte[] encryptedData = null;
            ASCIIEncoding ByteConverter = new ASCIIEncoding();
            try
            {
                byte[] originalMessage = ByteConverter.GetBytes(msn);
                encryptedData = rsaCryptoServ.SignData(originalMessage,new SHA512CryptoServiceProvider());
                FileWriter.WriteOnEvents(EventLevel.Info,"Firma de mensaje exitoso ");
                rsaCryptoServ.Dispose();
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Falla firmando data. "+ex.Message);
                rsaCryptoServ.Dispose();
            }
            return encryptedData;
        }

        internal bool validateSignAndHash(string decryptedMessage,byte[] signature,string publicKey)
        {
            bool result = false;
            RSACryptoServiceProvider rsaCSP = new RSACryptoServiceProvider();
            ASCIIEncoding ByteConverter = new ASCIIEncoding();
            try
            {
                byte[] decryptMessage = ByteConverter.GetBytes(decryptedMessage);
                FileWriter.WriteOnEvents(EventLevel.Info,"Leyendo contenido llave publica para verificacion de firmas.");
                string xmlKey = File.ReadAllText(publicKey);
                FileWriter.WriteOnEvents(EventLevel.Info,"Importando llave para proceso de verificacion de firma");
                rsaCSP.FromXmlString(xmlKey);
                result = rsaCSP.VerifyData(decryptMessage,new SHA512CryptoServiceProvider(),signature);
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Error en el proceso de validacion de firma. "+ex.Message);
                result = false;
            }
            return result;
        }
    }
}