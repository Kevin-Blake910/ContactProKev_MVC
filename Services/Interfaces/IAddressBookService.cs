namespace ContactProKev_MVC.Services.Interfaces
{
    public interface IAddressBookService
    {
        public Task AddContactToCategoryAsync(int categoryId, int contactId);
        Task<bool> IsContactInCategory(int categoryId, int contactId);
    }
}
