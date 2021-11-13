namespace RemoteControl.CoreLibrary.Helpers;
public static class Extensions
{
    public static HubConnection RegisterGenericMethod<T>(this HubConnection hub, string method, Action<T> action)
    {
        hub.On<string>(method, async item =>
        {
            T value = await js.DeserializeObjectAsync<T>(item);
            action.Invoke(value);
        });
        return hub;
    }
}