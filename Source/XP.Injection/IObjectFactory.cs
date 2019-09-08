namespace XP.Injection
{
  public interface IObjectFactory
  {
    IFactory GetOrCreate();
  }
}