using System;
using System.Security.Cryptography;
using servicioCliente.AppUtils;
using servicioCliente.Models;
using static servicioCliente.AppUtils.Enums;

namespace servicioCliente.EncryptionLogic
{
    public class AESEncryption
    {
        protected int keySize;
        public AesCryptoServiceProvider aes;
        public AESEncryption(int keySize){
                aes = new AesCryptoServiceProvider();
            this.keySize = keySize;
        }

        public bool generateProperties(){
            try
            {
                FileWriter.WriteOnEvents(EventLevel.Info,"Seteando propiedades de algoritmo AES.");
                aes.KeySize = keySize;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                aes.GenerateKey();
                aes.GenerateIV();
                return true;
            }
            catch (System.Exception ex)
            {   
                FileWriter.WriteOnEvents(EventLevel.Exception,"Error intentando setear propiedades. "+ex.Message);
                return false;
            }
        }

        internal ResponseEncryptAES EncryptMessage(string mensaje)
        {
                ResponseEncryptAES response = new ResponseEncryptAES();
                byte[] encryptedMessage;
                //Inicia proceso para cifrado
                

                return response;
        }
    }
}