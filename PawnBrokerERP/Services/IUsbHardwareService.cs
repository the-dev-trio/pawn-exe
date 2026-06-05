namespace PawnBrokerERP.Services;

public interface IUsbHardwareService
{
    string? GetCurrentUsbSerial();
    bool IsRunningFromUsb();
}
