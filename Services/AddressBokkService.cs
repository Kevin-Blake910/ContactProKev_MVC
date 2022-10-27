using ContactProKev_MVC.Data;
using ContactProKev_MVC.Models;
using ContactProKev_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContactProKev_MVC.Services
{
    public class AddressBookService : IAddressBookService
    {
        private readonly ApplicationDbContext _context;

        public AddressBookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddContactToCategoryAsync(int categoryId, int contactId)
        {
            try
            {
                //Check to se if contact is already in the category
                if (!await IsContactInCategory(categoryId,contactId))
                {
                //If not... Add the Category to the Contact's collection of Categories
                    Contact? contact = await _context.Contacts.FindAsync(contactId);
                    Category? category = await _context.Categories.FindAsync(categoryId);
                    if (contact != null && category != null)
                    {
                        category.Contacts.Add(contact);
                        await _context.SaveChangesAsync();
                    }

                }     
                
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsContactInCategory(int categoryId, int contactId)
        {
            Contact? contact = await _context.Contacts.FindAsync(contactId);

            bool isinCategory = await _context.Categories
                                        .Include(c => c.Contacts)
                                        .Where(c => c.Id == categoryId && c.Contacts.Contains(contact!))
                                        .AnyAsync();

            return isinCategory;
        }
    }
}
