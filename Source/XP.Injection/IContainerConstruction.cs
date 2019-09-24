namespace XP.Injection
{
  public interface IContainerConstruction
  {
    IFactoryBuilder<T> GetOrAddFactoryBuilder<T>();
  }
}