using Dapper;
using MVCtest3d.Database.DatabaseModels;
using System.Data;
using System.Data.SQLite;
using System.Net;
using System.Net.Mail;
using MVCtest3d.Other;

namespace MVCtest3d.Database
{
    public class DatabaseConnection
    {
        private string ConnectionString;
        private string mail;
        private string password;

        public DatabaseConnection(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
            mail = configuration["Credentials:mail"];
            password = configuration["Credentials:password"];
        }

        private static Random random = new Random();

        public void CreateUser(string recipientEmail, UserModel user)
        {
            using(IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "SELECT COUNT(*) FROM User WHERE Email = @Email";
                var exists = cnn.QueryFirstOrDefault<int>(sql, new { Email = recipientEmail });
                if(exists == 1)
                {
                    throw new Exception("E-mail is already in use");
                }
            }

            string verificationCode = random.Next(0, 10).ToString();

            while (verificationCode.Length < 9)
            {
                verificationCode += random.Next(0, 10);
            }

            // https://learn.microsoft.com/en-us/answers/questions/1167393/send-email-form-gmail-account-using-c
            using (var client = new SmtpClient())
            {
                client.Host = "smtp.gmail.com";
                client.Port = 587;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(mail, password);
                using (var message = new MailMessage(
                    from: new MailAddress(mail),
                    to: new MailAddress(recipientEmail)
                    ))
                {
                    message.Subject = "Verfication";
                    message.Body = "Your onetime password is: " + verificationCode;

                    client.Send(message);
                }
            }

            using (IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "INSERT INTO User (Name, Age, Password, Email, Activated) VALUES (@Name, @Age, @Password, @Email, @Activated)";
                cnn.Execute(sql, new { Name = user.Name, Age = user.Age, Password = verificationCode, Email = recipientEmail, Activated = false });
            }
        }

        public UserModel LoginUser(string email, string password)
        {
            if(password.Length != 9 && !password.All(char.IsDigit))
            {
                password = EncryptionHelper.ComputeSha256Hash(password);
            }

            using(IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "SELECT * FROM User WHERE Email = @Email AND Password = @Password";
                UserModel output = cnn.QueryFirstOrDefault<UserModel>(sql, new { Email = email, Password = password });

                if(output == null)
                {
                    throw new Exception("User not found");
                }
                else
                {
                    return output;
                }
            }
        }

        public void UpdatePassword(string newPassword, int id)
        {
            var password = EncryptionHelper.ComputeSha256Hash(newPassword);

            using(IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "UPDATE User SET Password = @Password WHERE User.Id = @Id";
                cnn.Execute(sql, new {Password = password, Id = id});
            }
        }

        public void CreateListing(ListingModel model, int userId)
        {
            using (IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "INSERT INTO Listing (User.Id, Price, Year, Horsepower, Brand, Model, Created) VALUES (@User, @Price, @Year, @Horsepower, @Brand, @Model, @Created)";
                cnn.Execute(sql, new { User = userId, Price = model.Price, Year = model.Year, Horsepower = model.Horsepower, Brand = model.Brand, Model = model.Model, Created = model.Created });
            }
        }
    }
}
