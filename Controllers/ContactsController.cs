using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContactProKev_MVC.Data;
using ContactProKev_MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ContactProKev_MVC.Enums;
using ContactProKev_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ContactProKev_MVC.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;
        private readonly IEmailSender _emailSender;

        public ContactsController(ApplicationDbContext context, 
            UserManager<AppUser> userManager,
            IImageService imageService,
            IAddressBookService addressBookService,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;
            _emailSender = emailSender;
        }

        // GET: Contacts ----------------can do more then one include
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User);

            List<Contact> contacts = await _context.Contacts
                                                   .Where(c => c.AppUserId == userId)
                                                   .Include(c => c.AppUser)
                                                   .Include(c => c.Categories)
                                                   .ToListAsync();

            List<Category> userCategories = await _context.Categories.Where(c => c.AppUserID == userId).ToListAsync();

            ViewData["CategoryId"] = new SelectList(userCategories, "Id", "Name");
            return View(contacts);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EmailContact(int? id)
        {

            string appUserId = _userManager.GetUserId(User);
            Contact? contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);

            if (contact == null)
            {
                return NotFound();
            }

            EmailData emailData = new EmailData()
            {
                EmailAddress = contact.Email,
                FirstName = contact.FirstName,
                LastName = contact.LastName
            };

            EmailContactViewModels viewModels = new EmailContactViewModels()
            {
                Contact = contact,
                EmailData = emailData
            };

            return View(viewModels);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailContact(EmailContactViewModels viewModels)
        {
            if (ModelState.IsValid)
            {
                string swalMessage = string.Empty;

                try
                {
                    await _emailSender.SendEmailAsync(viewModels.EmailData!.EmailAddress, viewModels.EmailData.EmailSubject, viewModels.EmailData.EmailBody);
                    swalMessage = "Success: Email Sent";
                    return RedirectToAction("Index", "Contacts", new { swalMessage });

                }
                catch (Exception)
                {
                    swalMessage = "Error: Email Send Failed";
                    return RedirectToAction("Index", "Contacts", new { swalMessage });
                    throw;
                }
            }
            return View();
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create --------------getting enum list**
        [Authorize]
        public async Task<IActionResult> Create()
        {
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            //TODO: Categories Drop Down
            string userID = _userManager.GetUserId(User);
            List<Category> categories = await _context.Categories
                                                      .Where(c => c.AppUserID == userID)
                                                      .ToListAsync();
            ViewData["CategoryList"] = new MultiSelectList(categories,"Id","Name");

            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,ImageFile")] Contact contact, List<int> categoryList)
        {
            ModelState.Remove("AppUserId");
            
            if (ModelState.IsValid)
            {
                contact.AppUserId = _userManager.GetUserId(User);
                contact.Created = DateTime.UtcNow;

                if(contact.BirthDate != null)
                {
                    contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                }

                //Check whether a file/image has been selected
                //if ImageFile is NOT null set the ImageData property - Convert file to byte[]
                //if ImageFile is NOT null set the ImageType property - Use the file extension as the value
                if (contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();

                //TODO: Use the list of category Ids to...
                //1. Find the associated Category
                //2. Add the Category to the Collection of Categories for the current Contact
                foreach(int categoryId in categoryList)
                {
                    await _addressBookService.AddContactToCategoryAsync(categoryId,contact.Id);
                }



                return RedirectToAction(nameof(Index));
            }
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            string userID = _userManager.GetUserId(User);
            List<Category> categories = await _context.Categories
                                                      .Where(c => c.AppUserID == userID)
                                                      .ToListAsync();
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name");

            return View(contact);
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            string appUserId = _userManager.GetUserId(User);
            
            Contact? contact = await _context.Contacts
                                             .Where(c => c.Id == id && c.AppUserId == appUserId)
                                             .Include(c => c.Categories)
                                             .FirstOrDefaultAsync();
            if (contact == null)
            {
                return NotFound();
            }

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            //TODO: Add a categories List

            List<Category> categories = (await _addressBookService.GetAppUserCategoriesAsync(appUserId)).ToList();
            List<int> categoryIds = contact.Categories.Select(c=>c.Id).ToList();       
            
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", categoryIds);

            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageFile,ImageData,ImageType")] Contact contact, List<int> categoryList)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contact.Created = DateTime.SpecifyKind(contact.Created, DateTimeKind.Utc);

                    if (contact.BirthDate != null)
                    {
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                    }

                    //Check whether a file/image has been selected
                    //if ImageFile is NOT null set the ImageData property - Convert file to byte[]
                    //if ImageFile is NOT null set the ImageType property - Use the file extension as the value
                    if (contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }


                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    //TODO: ADD categories Code
                    //Remove currecnt categories
                    await _addressBookService.RemoveAllContactCategoriesAsync(contact.Id);

                    //Add Selected categories to the contacts
                    await _addressBookService.AddContactToCategoiesAsync(categoryList, contact.Id);


                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserId);

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            

            List<Category> categories = (await _addressBookService.GetAppUserCategoriesAsync(contact.AppUserId!)).ToList();
            List<int> categoryIds = contact.Categories.Select(c => c.Id).ToList();

            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", categoryIds);

            return View(contact);                     

        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contacts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contacts'  is null.");
            }
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
          return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
