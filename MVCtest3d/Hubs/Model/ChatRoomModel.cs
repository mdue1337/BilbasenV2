namespace MVCtest3d.Hubs.Model
{
    public class ChatRoomModel
    {
        public int Id { get; set; }            
        public int UseroneId { get; set; }     
        public int UsertwoId { get; set; }
        public List<ChatMessageModel> Messages { get; set; }
    }
}
