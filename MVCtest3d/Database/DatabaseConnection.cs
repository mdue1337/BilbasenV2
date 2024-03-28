using Dapper;
using MVCtest3d.Database.DatabaseModels;
using System.Data;
using System.Data.SQLite;
using System.Net;
using System.Net.Mail;
using MVCtest3d.Other;
using System.Collections.Generic;
using MVCtest3d.Hubs.Model;

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

        public string GenerateVerifactionCode()
        {
            string code = random.Next(0, 10).ToString();

            while (code.Length < 9)
            {
                code += random.Next(0, 10);
            }

            return code;
        }

        public void SendEmail(string recipientEmail, string verificationCode)
        {
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
        }

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

            string verificationCode = GenerateVerifactionCode();

            SendEmail(recipientEmail, verificationCode);

            using (IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "INSERT INTO User (Name, Age, Password, Email, Activated) VALUES (@Name, @Age, @Password, @Email, @Activated)";
                cnn.Execute(sql, new { Name = user.Name, Age = user.Age, Password = verificationCode.ToString(), Email = recipientEmail, Activated = false });
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

        public UserModel GetUser(int id)
        {
            // '??' tjekker om venstresiden er null, hvis den er så smider den en exception.

            using(IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "SELECT * FROM User WHERE Id = @Id";
                UserModel user = cnn.QueryFirstOrDefault<UserModel>(sql, new { Id = id });
                return user ?? throw new Exception("User not found");
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

        public void ResetPassword(string email)
        {
            using(IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "SELECT User.Id FROM User WHERE Email = @Email";
                int? id = cnn.QueryFirstOrDefault<int?>(sql, new {Email = email});

                if(id is null) // check for default value.
                {
                    return; // bare stop, dog skal brugeren ikke vide at der ikke findes en bruger med den mail.
                }

                string VCode = GenerateVerifactionCode();

                string sql2 = "UPDATE User SET Password = @Password WHERE User.Id = @Id; UPDATE User SET Activated = False WHERE User.Id = @Id";
                cnn.Execute(sql2, new { Password = VCode, Id = id});

                SendEmail(email, VCode);
            }
        }

        public void ActivateAccount(int id)
        {
            using(IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "UPDATE User SET Activated = True WHERE User.Id = @Id";
                cnn.Execute(sql, new {Id = id});
            }
        }

        public int CreateListing(ListingModel model)
        {
            using (IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "INSERT INTO Listing ('User.Id', Price, Year, Horsepower, Brand, Model, Created, Location, Status) VALUES (@User, @Price, @Year, @Horsepower, @Brand, @Model, @Created, @Location, @Status)";
                cnn.Execute(sql, new { User = model.UserId, Price = model.Price, Year = model.Year, Horsepower = model.Horsepower, Brand = model.Brand, Model = model.Model, Created = model.Created, Location = model.Location, Status = 1});

                string getListingId = "SELECT Id FROM Listing ORDER BY Id DESC LIMIT 1;";
                int ListingId = cnn.QueryFirst<int>(getListingId);

                return ListingId;
            }
        }

        public ListingModel GetSpecificListing(int id)
        {
            using(IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = @"SELECT Id, ""User.Id"" AS UserId, Price, Year, Horsepower, Brand, Model, Created, Location, Status FROM Listing WHERE Listing.Id = @Id";
                ListingModel l = cnn.QueryFirstOrDefault<ListingModel>(sql, new {Id = id}) ?? throw new Exception();
                return l;
            }
        }

        public List<ListingModel> GetAllListing()
        {
            using (IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = @"SELECT Id, ""User.Id"" AS UserId, Price, Year, Horsepower, Brand, Model, Created, Location, Status FROM Listing";
                List<ListingModel> l = cnn.Query<ListingModel>(sql).ToList();
                return l;
            }
        }

        public void InsertPicture(byte[] img, int listingId)
        {
            using(IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
    
                using(IDbTransaction transaction = cnn.BeginTransaction())
                {
                    try
                    {
                        string sql1 = "INSERT INTO Picture (data) VALUES (@Data);  SELECT last_insert_rowid()";
                        int PictureId = cnn.ExecuteScalar<int>(sql1, new { Data = img });
                        string sql2 = "INSERT INTO ListingPicture ('Listing.Id', 'Picture.Id') VALUES (@ListingId, @PictureId);";
                        cnn.Execute(sql2, new { ListingId = listingId, PictureId = PictureId });

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }
            }
        }

        public List<PictureModel> GetListingPictures(int ListingId)
        {
            using(IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "SELECT * FROM ListingPicture INNER JOIN Picture ON ListingPicture.'Picture.Id' = Picture.Id WHERE ListingPicture.'Listing.Id' = @Id";
                List<PictureModel> pictures = cnn.Query<PictureModel>(sql, new { Id = ListingId }).ToList();
                return pictures;
            }
        }

        public void PurchaseListing(int listingId, int userId)
        {
            int sellerId = GetSpecificListing(listingId).Id;

            if (sellerId == userId)
            {
                throw new InvalidOperationException();
            }

            using (IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();

                using(IDbTransaction transaction = cnn.BeginTransaction())
                {
                    try
                    {
                        string sql = "INSERT INTO BuyHistory ('User.Id', 'Listing.Id') VALUES (@UId, @LId);";
                        cnn.Execute(sql, new { UId = userId, LId = listingId });

                        string sql2 = "UPDATE Listing SET Status = 0 WHERE Id = @LId";
                        cnn.Execute(sql2, new { LId = listingId });

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }
            }
        }

        public List<BuyHistory> getUserHistory(int userId)
        {
            using(IDbConnection cnn =  new SQLiteConnection(ConnectionString)) 
            {
                string sql = "SELECT Id, `User.Id` AS UserId, `Listing.Id` AS ListingId FROM BuyHistory WHERE `User.Id` = @Id";
                List<BuyHistory> output = cnn.Query<BuyHistory>(sql, new { Id = userId }).ToList();
                return output;
            }
        }

        public int ConnectChatRoomId(int userone, int usertwo)
        {
            using( IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                int RoomId;
                string sql = "SELECT Id FROM ChatRoom WHERE (UseroneId = @userone OR UseroneId = @usertwo) AND (UsertwoId = @userone OR UsertwoId = @usertwo)";
                int? ChatId = cnn.QueryFirstOrDefault<int?>(sql, new { userone = userone, usertwo = usertwo});

                if (ChatId == null)
                {
                    RoomId = CreateChatHub(userone, usertwo);
                }
                else
                {
                    RoomId = (int)ChatId;
                }

                return RoomId;
            }
        }

        public int CreateChatHub(int userId, int recieverId)
        {
            using (IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "INSERT INTO ChatRoom (UseroneId, UsertwoId) VALUES (@User1, @User2)";
                cnn.Execute(sql, new {User1 = userId, User2 = recieverId});

                string sql2 = "SELECT Id FROM ChatRoom ORDER BY Id DESC LIMIT 1";
                int RoomId = cnn.QueryFirstOrDefault<int>(sql2);

                return RoomId;
            }
        }

        public List<ChatMessageModel> GetChatMessage(int chatRoomId)
        {
            using(IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "SELECT * FROM ChatMessage WHERE ChatRoomId = @Id";
                List<ChatMessageModel> messages = cnn.Query<ChatMessageModel>(sql, new { Id = chatRoomId }).ToList();
                return messages;
            }
        }

        public void ChatSendMessage(int chatRoomId, int senderId, string message)
        {
            using (IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "INSERT INTO ChatMessage (ChatRoomId, SenderId, Message, Timestamp) VALUES (@ChatId, @Id, @Message, @TP)";
                DateTime date = DateTime.Now;
                cnn.Execute(sql, new { ChatId = chatRoomId, Id = senderId, Message = message, TP = date });
            }
        }

        public List<ChatRoomModel> GetChats(int userId)
        {
            using( IDbConnection cnn = new SQLiteConnection(ConnectionString))
            {
                string sql = "SELECT * FROM ChatRoom WHERE (UseroneId = @Id OR UsertwoId = @Id)";
                List<ChatRoomModel> chats = cnn.Query<ChatRoomModel>(sql, new { Id = userId }).ToList();
                return chats;
            }
        }
    }
}