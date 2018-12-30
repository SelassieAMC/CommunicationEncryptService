using System;
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
    }
}