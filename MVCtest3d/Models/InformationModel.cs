using MVCtest3d.Database.DatabaseModels;
using MVCtest3d.Hubs.Model;

namespace MVCtest3d.Models
{
    public class InformationModel
    {
        public UserModel User { get; set; }
        public List<ListingModel> BuyHistoryListings { get; set; }
        public List<ListingModel> UserListings {  get; set; }
        public List<ChatRoomModel> Chats { get; set; }
    }
}
