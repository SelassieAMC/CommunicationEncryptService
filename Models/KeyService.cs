namespace servicioCliente.Models
{
    public class KeyService{
        public int id { get; set; }
        public string originIp { get; set; }
        public string destinationIp { get; set; }
        public string keySignature { get; set; }
        public string keyEncrypt { get; set; }
    }    
}
