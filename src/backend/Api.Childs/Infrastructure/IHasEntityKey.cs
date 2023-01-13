namespace Api.Childs.Infrastructure;

public interface IHasEntityKey
{
    string Key { get; }
    public string KeyName { get; }
}