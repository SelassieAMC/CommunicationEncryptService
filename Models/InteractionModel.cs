namespace servicioCliente.Models
{
    public class InteractionModel
    {
        public string userNameOrigin { get; set; }
        public string userNameDestination { get; set; }
        public string mensaje { get; set; }
    }

    public class SendMessageModel
    {
        public string userNameOrigin { get; set; }
        public string userNameDestination { get; set; }
        public byte[] encryptedMessage { get; set; }
        public byte[] encryptSignature { get; set; }
        public byte[] encryptedKey { get; set; }
        public byte[] initVector { get; set; }
        public byte[]  idSignature { get; set; }
    }
}