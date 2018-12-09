using System;
using Microsoft.Extensions.Options;
using servicioCliente.AppUtils;
using servicioCliente.Models;
using static servicioCliente.AppUtils.Enums;

namespace servicioCliente.Encryptionlogic{
    public class RSAEncryption{
        public RSAEncryption(){}
        ///Generation RSA Keys
        public RSAModel GeneratePubPrivKeys(IOptions<ParametersModel> param )
        {
            FileWriter fw = new FileWriter(param);
            RSAModel rsaModel = new RSAModel();
            rsaModel.PublicKey = "llavePublicaRSA";
            rsaModel.PrivateKey = "llavePrivadaRSA";
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
            return rsaModel;
        }
    }
}
