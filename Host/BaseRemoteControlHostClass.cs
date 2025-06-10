namespace RemoteControl.CoreLibrary.Host;
public abstract class BaseRemoteControlHostClass : IAsyncDisposable
{
    protected string Title { get; }
    protected HubConnection Hub;
    public BaseRemoteControlHostClass(IRemoteControlEndPoint endPoint, ITitle title)
    {
        Hub = new HubConnectionBuilder()
         .WithUrl(endPoint.EndPointAddress)
         .WithAutomaticReconnect()
         .Build();
        Title = title.Title;
    }
    private async Task StartAsync()
    {
        await Hub.StartAsync();
    }
    private bool _didInit;
    public async Task InitializeAsync()
    {
        if (_didInit)
        {
            return;
        }
        await StartAsync();
        Hub!.On("Hosting", HostingAsync);
        Hub.On("NewClient", () => NewClient.InvokeAsync());
        RegisterCustomMethods();
        await RegisterAsync(); //this is server side.  so needs to register obviously as well.
        _didInit = true;
    }
    public Func<Task>? NewClient { get; set; }
    protected virtual Task HostingAsync() { return Task.CompletedTask; }
    protected virtual void RegisterCustomMethods() { } //defaults with nothing.  but can register any other custom method that is needed.
    protected async Task SendCustomDataAsync<T>(string method, T value)
    {
        if (_didInit == false)
        {
            //this means at the beginning, if the remote control was not initialised, at least won't attempt it.
            return;
        }
        string data = await js1.SerializeObjectAsync(value);
        try
        {
            await Hub!.SendAsync("HostSendClientDataAsync", Title, method, data);
        }
        catch (Exception)
        {

            //if you can't send it, ignore (if possible).
        }


    }
    private async Task RegisterAsync()
    {
        await Hub!.SendAsync("HostInitAsync", Title);
    }
    public async ValueTask DisposeAsync()
    {
        await Hub!.SendAsync("HostDisconnectAsync", Title); //this way even tablets can act as host.
        await Hub.DisposeAsync();
        GC.SuppressFinalize(this); // Fix for CA1816
    }
}