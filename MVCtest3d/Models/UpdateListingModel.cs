using MVCtest3d.Database.DatabaseModels;

namespace MVCtest3d.Models
{
    public class UpdateListingModel
    {
        public ListingModel ListingModel { get; set; }
        public List<PictureModel> PictureModel { get; set; }
    }
}
