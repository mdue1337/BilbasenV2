namespace MVCtest3d.Hubs.Model
{
    public class ChatMessageModel
    {
        public int Id { get; set; }         
        public int ChatRoomId { get; set; } 
        public int SenderId { get; set; }   
        public string Message { get; set; } 
        public DateTime Timestamp { get; set; }
    }
}
