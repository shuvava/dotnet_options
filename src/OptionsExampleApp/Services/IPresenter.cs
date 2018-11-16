namespace OptionsExampleApp.Services
{
    public interface IPresenter
    {
        string Serialize<T>(T obj);
    }
}
