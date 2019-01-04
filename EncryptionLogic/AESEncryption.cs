using System;
using System.IO;
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

        public AESEncryption(){
            aes = new AesCryptoServiceProvider();
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
                ResponseEncryptAES response = new ResponseEncryptAES{
                    result = false
                };
                byte[] encryptedMessage;
                //Inicia proceso para cifrado
                FileWriter.WriteOnEvents(EventLevel.Info,"Iniciando proceso de cifrado.");
                try
                {
                    if(aes.Key == null || aes.IV == null || mensaje == ""){
                        FileWriter.WriteOnEvents(EventLevel.Error,"Uno o mas de los argumentos para cifrado aes invalidos.");
                    }
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key,aes.IV);

                    using(MemoryStream msEncrypt = new MemoryStream()){
                          using(CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)){
                              using(StreamWriter swEncrypt = new StreamWriter(csEncrypt)){
                                //write the message to the stream
                                FileWriter.WriteOnEvents(EventLevel.Info,"Escribiendo mensaje en el bloque de flujo.");
                                swEncrypt.Write(mensaje);
                              }
                            encryptedMessage = msEncrypt.ToArray();
                            response.encryptedData = encryptedMessage;
                            response.result = true;
                            response.privateKey = aes.Key;
                            response.InitVector = aes.IV;
                          }
                    }
                }
                catch (System.Exception ex)
                {
                    FileWriter.WriteOnEvents(EventLevel.Exception,"Excepcion en intento de cifrado. "+ex.Message);
                }
                return response;
        }

        internal ResponseAESDecryption DecryptMessage(SendMessageModel messageModel, byte[] decryptedKey)
        {
            ResponseAESDecryption response = new ResponseAESDecryption{
                result = false
            };
            // Create an AesCryptoServiceProvider object with the specified key and IV.
            using(AesCryptoServiceProvider aesDecrypt = new AesCryptoServiceProvider()){
                FileWriter.WriteOnEvents(EventLevel.Info,"Inicio proceso de descifrado de mensaje.");
                aesDecrypt.Key = decryptedKey;
                aesDecrypt.IV = messageModel.initVector;
                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesDecrypt.CreateDecryptor(aesDecrypt.Key,aesDecrypt.IV);
                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(messageModel.encryptedMessage))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream and place them in a string.
                            response.decryptedMessage = srDecrypt.ReadToEnd();
                            response.result = true;
                            FileWriter.WriteOnEvents(EventLevel.Info,"Proceso de descifrado de mensaje finalizado correctamente");
                        }
                    }
                }
            }
            return response;
        }
    }
}