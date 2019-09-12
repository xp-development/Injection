namespace XP.Injection
{
  public interface IFactory<T> : IFactory
  {
  }

  public interface IFactory
  {
    object Get();
  }
}