using Microsoft.Extensions.Options;
using servicioCliente.Models;
using System;
using System.Globalization;
using System.IO;
using static servicioCliente.AppUtils.Enums;

namespace servicioCliente.AppUtils
{
    public class FileWriter
    {
        public static IOptions<ParametersModel> parameters;
        // public FileWriter(IOptions<ParametersModel> param){
        //     parameters = param;
        // }
        public static bool WriteOnFile(string path,string fileName,string message){
            bool operationSucces = false;
            try
            {
                if(!Directory.Exists(@path)){
                    Directory.CreateDirectory(@path);
                    WriteOnEvents(EventLevel.Info,"Path "+path+" creado.");
                }
                using (StreamWriter file = new StreamWriter(@path+fileName+".xml", false))
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

        public static void WriteOnEvents(EventLevel eventLevel,string message){
            string pathLogs = parameters.Value.FilesOutput+parameters.Value.LogEventsFile;
            try
            {
                //string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                using (StreamWriter file = new StreamWriter(@pathLogs, true))
                {
                    string tabs = (eventLevel == EventLevel.Info || eventLevel == EventLevel.Error)?"\t\t":"\t";
                    file.WriteLine(eventLevel+tabs+DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") +"\t\t"+message);
                }
            }
            catch (System.Exception)
            {
                //do nothing, there isn't way to save this event
            }
        }
    }
}