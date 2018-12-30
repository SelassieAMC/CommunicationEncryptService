namespace servicioCliente.Models
{
    public class ParametersModel
    {
        public string FilesOutput { get; set; }
        public string PubKeyFile { get; set; }
        public string PrivKeyFile { get; set; }
        public string LogEventsFile { get; set; }
        public string EndpointServer { get; set; }
        public string RequestKeyPartner { get; set; }
        public int KeyRSASize { get; set; }
        public int KeyAESSize { get; set; }
    }
}