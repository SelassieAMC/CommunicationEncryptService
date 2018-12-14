
using System.Security.Cryptography;
using servicioCliente.AppUtils;
using static servicioCliente.AppUtils.Enums;

namespace servicioCliente.EncryptionLogic
{
    public class Container
    {
        public static bool GenKey_SaveInContainer(string ContainerName)  
        {  
            bool result = false;
            CspParameters cp = new CspParameters();  
            cp.KeyContainerName = ContainerName;  
            try
            { 
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp); 
                FileWriter.WriteOnEvents(EventLevel.Info, "Se ha almacenado la llave en el contenedor.");
                result = true; 
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Error en GenKey_SaveInContainer. "+ex.Message);
                result = false;
            }
            return result;
        }  
        /// <summary>
        /// Metodo que obtiene una llave del contenedor
        /// </summary>
        /// <param name="ContainerName">Nombre de la llave</param>
        public static string GetKeyFromContainer(string ContainerName)  
        {  
            try
            {
                CspParameters cp = new CspParameters();  
                cp.KeyContainerName = ContainerName;  
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp);  
                FileWriter.WriteOnEvents(EventLevel.Info, "Se ha leido una llave del contenedor.");
                return rsa.ToXmlString(true);
            }
            catch (System.Exception ex)
            {
                FileWriter.WriteOnEvents(EventLevel.Exception,"Error en GenKey_SaveInContainer. "+ex.Message);
                return "false";
            } 
        }  
        /// <summary>
        /// Metodo que elimina una llave almacenada en el contenedor.
        /// </summary>
        /// <param name="ContainerName">Nombre de la llave</param>
        public static void DeleteKeyFromContainer(string ContainerName)  
        {  
            // Create the CspParameters object and set the key container   
            // name used to store the RSA key pair.  
            CspParameters cp = new CspParameters();  
            cp.KeyContainerName = ContainerName;  
    
            // Create a new instance of RSACryptoServiceProvider that accesses  
            // the key container.  
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp);  
    
            // Delete the key entry in the container.  
            rsa.PersistKeyInCsp = false;  
    
            // Call Clear to release resources and delete the key from the container.  
            rsa.Clear();  
        }  
    }
}