﻿namespace RemoteControl.CoreLibrary.Client;
public abstract class BaseRemoteControlClientClass : IAsyncDisposable
{
    protected string Title { get; }
    protected HubConnection Hub;
    public BaseRemoteControlClientClass(IRemoteControlEndPoint endPoint, ITitle title)
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
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public async ValueTask DisposeAsync()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    {
        await Hub!.DisposeAsync();
    }
    private bool _didInit;
    public async Task InitializeAsync()
    {
        if (_didInit)
        {
            return;
        }
        await StartAsync();
        Hub!.On("HostDisconnected", Disconnected);
        Hub!.On("Failed", Failed);
        RegisterCustomMethods();
        await RegisterAsync(); //this is server side.  so needs to register obviously as well.
        _didInit = true;
    }
    private async Task RegisterAsync()
    {
        await Hub!.SendAsync("ClientInitAsync", Title);
    }
    protected virtual void Disconnected()
    {
        HostStateInfo?.Invoke($"Host Not Connected As Of {DateTime.Now} ");
    }
    protected virtual void Failed()
    {
        HostStateInfo?.Invoke($"Failed to send message to host on {DateTime.Now}");
    }
    public Action<string>? HostStateInfo { get; set; }
    protected virtual void RegisterCustomMethods() { } //defaults with nothing.  but can register any other custom method that is needed.
    protected async Task SendSimpleActionAsync(string method)
    {
        await Hub!.SendAsync("ClientInvokeSimpleActionAsync", Title, method);
    }
    protected async Task SendStringActionAsync(string method, string args)
    {
        await Hub!.SendAsync("ClientInvokeComplexActionAsync", Title, method, args);
    }
    protected async Task SendComplexActionAsync(string method, object payLoad)
    {
        string data = await js1.SerializeObjectAsync(payLoad);
        await Hub!.SendAsync("ClientInvokeComplexActionAsync", Title, method, data);
    }
}