using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SML.DAL.Repositories {

    public abstract class BaseRepository {
        protected readonly string _connectionString = "Server=tcp:smldbserver.database.windows.net,1433;Initial Catalog=SML_db;Persist Security Info=False;User ID=maxsnax;Password=$yMMetry21;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        protected BaseRepository() {
            _connectionString = ConfigurationManager.ConnectionStrings["SML_db-connection"].ToString();
        }

        protected SqlConnection GetConnection() {
            return new SqlConnection(_connectionString);
        }
    }
    
}
