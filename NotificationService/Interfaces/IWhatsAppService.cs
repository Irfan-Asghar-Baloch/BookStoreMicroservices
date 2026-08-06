namespace NotificationService.Interfaces
{
    public interface IWhatsAppService
    {
       // Task SendAsync(string phoneNumber, string message);
     
            Task SendTemplateAsync( string phoneNumber,  string customerName,  string purchaseType, string orderNumber, string productName, string deliveryDate);
        }
    
}

