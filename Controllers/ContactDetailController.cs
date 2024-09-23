using System;
using System.Linq;
using System.Web.Mvc;
using ContactAppProject.Data;
using ContactAppProject.Models;

namespace ContactAppProject.Controllers
{
    [Authorize(Roles = "Staff")]
    public class ContactDetailController : Controller
    {
        // GET: ContactDetail
        public ActionResult Index(Guid contactId)
        {
            TempData["ContactId"] = contactId;
            return View();
        }

        public ActionResult GetContactDetails(int page, int rows, string sidx, string sord, string searchString)
        {
            //int authorId = (int)TempData.Peek("authorId");

            Guid contactId = (Guid)TempData.Peek("ContactId");
            using (var session = NHibernateHelper.CreateSession())
            {
                //var contactDetails = session.Query<ContactDetails>().Where(cd => cd.Contact.Id == contactId).ToList();
                var contactDetails = session.Query<ContactDetails>()
                   .Where(cd => cd.Contact.Id == contactId)
                   .OrderBy(cd => cd.Number) // Default order
                   .ToList();
                //Get total count of records(for pagination)
                int totalCount = contactDetails.Count();
                //Calculate total pages
                int totalPages = (int)Math.Ceiling((double)totalCount / rows);


                var jsonData = new
                {
                    total = totalPages,
                    page,
                    records = totalCount,
                    rows = (from detail in contactDetails
                            orderby sidx + " " + sord
                            select new
                            {
                                cell = new string[]
                                {
                                    detail.Id.ToString(),
                              detail.Number.ToString(),
                              detail.Email                                }
                            }).Skip((page - 1) * rows).Take(rows).ToArray()
                };

                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult AddContactDetail(ContactDetails contactDetail)
        {
            Guid contactId = (Guid)TempData.Peek("ContactId");
            using (var session = NHibernateHelper.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var contact = session.Query<Contact>().SingleOrDefault(c => c.Id == contactId);
                    contactDetail.Contact = contact;
                    session.Save(contactDetail);
                    transaction.Commit();
                    return Json(new { success = true, message = "Contact Detail added successfully" });
                }
            }
        }
        public ActionResult DeleteContactDetail(Guid id)
        {
            using (var session = NHibernateHelper.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var contactDetail = session.Query<ContactDetails>().FirstOrDefault(cd => cd.Id == id);
                    session.Delete(contactDetail);
                    transaction.Commit();
                    return Json(new { success = true, message = "Contact Detail Deleted Successfully" });
                }
            }
        }

        public ActionResult EditContactDetail(ContactDetails contactDetails)
        {
            using (var session = NHibernateHelper.CreateSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var existingContactDetail = session.Query<ContactDetails>().FirstOrDefault(cd => cd.Id == contactDetails.Id);
                    if (existingContactDetail != null)
                    {
                        existingContactDetail.Number = contactDetails.Number;
                        existingContactDetail.Email = contactDetails.Email;
                        session.Update(existingContactDetail);
                        transaction.Commit();
                        return Json(new { success = true, message = "Contact Detail Edited Successfully." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Contact Detail not found." });
                    }
                }

            }
        }
    }
}