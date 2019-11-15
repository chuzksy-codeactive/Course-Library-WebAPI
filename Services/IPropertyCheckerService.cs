namespace Library.API.Services
{
    public interface IPropertyCheckerService
    {
        bool TypeHasProperties<T> (string fields);
    }
}
