using System.Configuration;

namespace euPATHS.AppCode
{
    public sealed class DBConnection
    {
        //Connection string name from configuration file.
        //const string DEFAULT_CONNECTION_KEY = "eupaths_local";
        const string DEFAULT_CONNECTION_KEY = "eupaths_prod";
        /// <summary>
        /// Gets the connection string from configuration.
        /// </summary>
        /// <returns>The connection string</returns>
        public static string DefaultConnection
        {
            get
            {
                return ConfigurationManager.ConnectionStrings[DEFAULT_CONNECTION_KEY].ConnectionString;
            }
        }
    }
}