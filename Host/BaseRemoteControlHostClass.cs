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
    protected async Task SendCustomDataAsync(string method, object value)
    {
        string data = await js.SerializeObjectAsync(value);
        await Hub!.SendAsync("HostSendClientDataAsync", Title, method, data);
    }
    private async Task RegisterAsync()
    {
        await Hub!.SendAsync("HostInitAsync", Title);
    }
    public ValueTask DisposeAsync()
    {
        return Hub.DisposeAsync();
    }
}