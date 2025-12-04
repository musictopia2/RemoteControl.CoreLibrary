namespace RemoteControl.CoreLibrary.Helpers;
public static class Extensions
{
    extension( HubConnection hub)
    {
        public HubConnection RegisterGenericMethod<T>(string method, Action<T> action)
        {
            hub.On<string>(method, async item =>
            {
                T value = await js1.DeserializeObjectAsync<T>(item);
                action.Invoke(value);
            });
            return hub;
        }
    }   
}