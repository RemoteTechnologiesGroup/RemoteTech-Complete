namespace RemoteTech.Common.Interfaces
{
    public interface IInstanciable<T>  where T : class
    {
        T GetInstance();
    }
}
