using Dapper;
using MVCtest3d.Database.DatabaseModels;
using System.Data;
using System.Data.SQLite;

namespace MVCtest3d.Database
{
    public class DatabaseConnection
    {
        private string ConnectionString;

        public DatabaseConnection(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<UserModel> GetUsers()
        {
            using (IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "SELECT * FROM Users";
                var output = cnn.Query<UserModel>(sql);
                return output.ToList();
            }
        }

        public void CreateUser(UserModel user)
        {
            using(IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "INSERT INTO Users (Name, Age) VALUES (@Name, @Age)";
                cnn.Execute(sql, new {Name = user.Name, Age = user.Age});
            }
        }
    }
}
