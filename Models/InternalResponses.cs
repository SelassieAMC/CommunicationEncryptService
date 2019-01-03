namespace servicioCliente.Models
{
    public class ResponseSignData{
        public bool result { get; set; }
        public byte[]  signData { get; set; }
    }

    public class ResponseEncryptAES{
        public bool result{get;set;}
        public byte[] privateKey { get; set; }
        public byte[] encryptedData { get; set; }
        public byte[] InitVector { get; set; }
    }

    public class ResponseEncryptAESKey{
        public bool resul { get; set; }
        public byte[] encryptedKey { get; set; }
        public byte[] initVector { get; set; }
    }
}