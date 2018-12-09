using Microsoft.Extensions.Options;
using servicioCliente.Models;
using System.IO;

namespace servicioCliente.AppUtils
{

    public class FileWriter
    {
        private readonly IOptions<ParametersModel> parameters;
        public FileWriter(IOptions<ParametersModel> param){
            parameters = param;
        }
        public bool WriteOnFile(string path,string fileName,string message){
            bool operationSucces = false;
            try
            {
                if(!Directory.Exists(path)){
                    Directory.CreateDirectory(path);
                    WriteOnEvents("Info\tPath "+path+" creado.");
                }
                using (StreamWriter file = new StreamWriter(@path+fileName, true))
                {
                    file.WriteLine(message);
                }
            }
            catch (System.Exception ex)
            {
                WriteOnEvents("Exception\tExcepcion en FileWriter\t"+ex.Message);
            }
            return operationSucces;
        }

        public void WriteOnEvents(string message){
            string pathLogs = parameters.Value.FilesOutput+parameters.Value.LogEventsFile;
            try
            {
                // if(!File.Exists(@pathLogs)){
                //     using(FileStream fs = File.Create(@pathLogs)){
                //     //do nothing, only create the file
                //     }
                // }
                using (StreamWriter file = new StreamWriter(@pathLogs, true))
                {
                    file.WriteLine(message);
                }
            }
            catch (System.Exception)
            {
                //do nothing, there isn't way to save this event
            }


        }
    }
}