using ContactProKev_MVC.Models;

namespace ContactProKev_MVC.Services.Interfaces
{
    public interface IAddressBookService
    {
        public Task AddContactToCategoryAsync(int categoryId, int contactId);

        //Add method: Add to a list of  CategoryIds
        public Task AddContactToCategoiesAsync (IEnumerable<int> categoryIds, int contactId);

        public Task<bool> IsContactInCategory(int categoryId, int contactId);
        public Task<IEnumerable<Category>> GetAppUserCategoriesAsync(string appUserId);

        //Add method to remove from all Categories
        public Task RemoveAllContactCategoriesAsync(int contactId);
    }
}
