namespace XP.Injection
{
  public interface IFactory<out T>
  {
    T Get();
  }
}