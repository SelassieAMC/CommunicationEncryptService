namespace servicioCliente.Models{
    public class RSAModel{
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public string[] EncryptedData { get; set; }
    }

    public class AESModel{
        public string PrivateKey { get; set; }
        public string[] EncryptedData { get; set; }
    }

    public class SignatureModel{
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string Hash { get; set; }
    }
}