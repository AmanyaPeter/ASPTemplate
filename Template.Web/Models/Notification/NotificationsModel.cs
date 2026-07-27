namespace Template.Web.Models.Notification
{
    public class NotificationsModel
    {
        public List<NotificationItemViewModel> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
        public string? Filter { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public class NotificationItemViewModel
    {
        public Guid Id { get; set; }
        public required string Subject { get; set; }
        public required string Message { get; set; }
        public string? Icon { get; set; }
        public string? TimeAgo { get; set; }
        public string? LinkUrl { get; set; }
        public bool IsRead { get; set; }
    }
}
