using System;
using servicioCliente.Models;

namespace servicioCliente.Encryptionlogic{
    public class RSAEncryption{
        public RSAEncryption(){}
        ///Generation RSA Keys
        public RSAModel GeneratePubPrivKeys()
        {
            RSAModel rsaModel = new RSAModel();
            rsaModel.PublicKey = "llavePublicaRSA";
            rsaModel.PrivateKey = "llavePrivadaRSA";
            try
            {
                //Write data on file
            }
            catch (System.Exception ex)
            {
                //Write Event on log
            }
            return rsaModel;
        }
    }
}
