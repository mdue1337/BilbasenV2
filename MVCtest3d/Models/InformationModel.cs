using MVCtest3d.Database.DatabaseModels;

namespace MVCtest3d.Models
{
    public class InformationModel
    {
        public UserModel User { get; set; }
        public List<ListingModel> BuyHistoryListings { get; set; }
        public List<ListingModel> UserListings {  get; set; }
    }
}
