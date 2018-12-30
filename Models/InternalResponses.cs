namespace servicioCliente.Models
{
    public class ResponseSignData{
        public bool result { get; set; }
        public byte[]  signData { get; set; }
    }

    public class ResponseEncryptAES{
        public bool result{get;set;}
        public string privateKey { get; set; }
        public byte[] encryptedData { get; set; }
    }
}