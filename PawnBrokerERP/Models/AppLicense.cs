namespace PawnBrokerERP.Models;

public class AppLicense
{
    public int Id { get; set; } = 1;
    public string UsbSerialNumber { get; set; } = string.Empty;
    public bool IsInitialized { get; set; } = false;
    public DateTime InitializedAt { get; set; } = DateTime.UtcNow;
    public string ShopName { get; set; } = string.Empty;
}
