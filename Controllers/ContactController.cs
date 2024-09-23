using System;
using System.Linq;
using System.Web.Mvc;
using ContactAppProject.Data;
using ContactAppProject.Models;

namespace ContactAppProject.Controllers
{
    [Authorize(Roles = "Staff")]
    public class ContactController : Controller
    {
        // GET: Contact
        public ActionResult Index()
        {
            return View();
        }


        //public ActionResult GetAllContacts()
        //{
        //    if (Session["UserId"] == null)
        //    {
        //        return RedirectToAction("Login", "User");
        //    }
        //    Guid userId = (Guid)Session["UserId"]; // Retrieve userId from the session
        //    using (var session = NHibernateHelper.CreateSession())
        //    {
        //        // Eager load User and Contacts to avoid lazy loading outside the session
        //        var user = session.Query<User>()
        //                          .FetchMany(u => u.Contacts) // Ensure Contacts are fetched eagerly
        //                          .FirstOrDefault(u => u.Id == userId);


        //        var contacts = user.Contacts.Select(c => new Contact
        //        {
        //            Id = c.Id,
        //            FirstName = c.FirstName,
        //            LastName = c.LastName,
        //            IsActive = c.IsActive
        //        }).ToList();



        //        return Json(contacts, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult GetAllContacts()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            Guid userId = (Guid)Session["UserId"]; // Retrieve userId from the session
            using (var session = NHibernateHelper.CreateSession())
            {
                // Fetch contacts directly by UserId
                var contacts = session.Query<Contact>()
                                      .Where(c => c.User.Id == userId)
                                      .Select(c => new Contact
                                      {
                                          Id = c.Id,
                                          FirstName = c.FirstName,
                                          LastName = c.LastName,
                                          IsActive = c.IsActive
                                      })
                                      .ToList();

                return Json(contacts, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddContact(Contact contact)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "User");
            }

            Guid userId = (Guid)Session["UserId"]; // Retrieve userId from the session
            using (var session = NHibernateHelper.CreateSession())
            {
                using (var txn = session.BeginTransaction())
                {
                    var user = session.Query<User>().FirstOrDefault(u => u.Id == userId);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found" }, JsonRequestBehavior.AllowGet);
                    }
                    contact.User = user;

                    session.Save(contact);
                    txn.Commit();
                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult GetContact(Guid id)
        {
            using (var session = NHibernateHelper.CreateSession())
            {
                var contact = session.Get<Contact>(id);
                if (contact == null)
                {
                    return Json(new { success = false, message = "Contact not found" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new
                {
                    success = true,
                    contact = new
                    {
                        contact.Id,
                        contact.FirstName,
                        contact.LastName
                    }
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EditContact(Contact contact)
        {
            using (var session = NHibernateHelper.CreateSession())
            {
                using (var txn = session.BeginTransaction())
                {
                    var existingContact = session.Get<Contact>(contact.Id);
                    if (existingContact == null)
                    {
                        return Json(new { success = false, message = "Contact not found" });
                    }

                    existingContact.FirstName = contact.FirstName;
                    existingContact.LastName = contact.LastName;

                    session.Update(existingContact);
                    txn.Commit();

                    return Json(new { success = true });
                }
            }
        }

        [HttpPost]
        public ActionResult UpdateIsActiveStatus(Guid contactId, bool isActive)
        {
            using (var session = NHibernateHelper.CreateSession())
            {
                using (var txn = session.BeginTransaction())
                {
                    var contact = session.Get<Contact>(contactId);
                    if (contact != null)
                    {
                        contact.IsActive = isActive;
                        session.Update(contact);
                        txn.Commit();
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Contact not found" });
                    }
                }
            }

        }

    }
}
