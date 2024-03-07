namespace MVCtest3d.Database.DatabaseModels
{
    public class ListingModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Price { get; set; }
        public int Year { get; set; }
        public int Horsepower { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Created {  get; set; }
        public string Location { get; set; }
    }
}
