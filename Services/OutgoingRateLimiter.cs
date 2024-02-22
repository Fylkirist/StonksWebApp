namespace StonksWebApp.Services;

public class OutgoingRateLimiter
{
    private static OutgoingRateLimiter? _instance;
    public static OutgoingRateLimiter Instance => _instance ??= new OutgoingRateLimiter();
    private int _finnhubRequests;
    private int _secRequests;

    public OutgoingRateLimiter()
    {
        _finnhubRequests = 0;
        _secRequests = 0;
        ResetLimiter();
    }

    private async Task ResetLimiter()
    {
        while (true)
        {
            _finnhubRequests = 0;
            _secRequests = 0;
            Thread.Sleep(1000);
        }
    }

    public void WaitForFinnhubLimiter()
    {
        while (_finnhubRequests >= 29)
        {
            Thread.Sleep(300);
        }

        _finnhubRequests++;
        return;
    }

    public void WaitForSECLimiter()
    {
        while (_secRequests >= 10)
        {
            Thread.Sleep(300);
        }

        _secRequests++;
        return;
    }
}