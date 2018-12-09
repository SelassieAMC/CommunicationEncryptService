using Microsoft.Extensions.Options;
using servicioCliente.Models;
using System;
using System.IO;
using static servicioCliente.AppUtils.Enums;

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
                if(!Directory.Exists(@path)){
                    Directory.CreateDirectory(@path);
                    WriteOnEvents(EventLevel.Info,"Path "+path+" creado.");
                }
                using (StreamWriter file = new StreamWriter(@path+fileName, false))
                {
                    file.WriteLine(message);
                    WriteOnEvents(EventLevel.Info,"Escritura llave privada exitosa!!");
                    operationSucces = true;
                }
            }
            catch (System.Exception ex)
            {
                WriteOnEvents(EventLevel.Exception,"Excepcion en FileWriter\t"+ex.Message);
            }
            return operationSucces;
        }

        public void WriteOnEvents(EventLevel eventLevel,string message){
            string pathLogs = parameters.Value.FilesOutput+parameters.Value.LogEventsFile;
            try
            {
                using (StreamWriter file = new StreamWriter(@pathLogs, true))
                {
                    string tabs = (eventLevel == EventLevel.Info || eventLevel == EventLevel.Error)?"\t\t":"\t";
                    file.WriteLine(eventLevel+tabs+DateTime.Now+"\t"+message);
                }
            }
            catch (System.Exception)
            {
                //do nothing, there isn't way to save this event
            }
        }
    }
}