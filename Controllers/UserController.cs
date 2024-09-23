using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using ContactAppProject.Data;
using ContactAppProject.Models;
using ContactAppProject.ViewModels;
using NHibernate.Linq;

/*
 * This UserController class manages user-related functionalities for the ContactAppProject.
 * It provides methods for user login, registration, logout, and management, restricted to admin roles.
 * 
 * Key features include:
 * - Login: Authenticates users, checks admin status, and sets authentication cookies.
 * - Register: Allows new user registrations, with password hashing and role assignment.
 * - Logout: Signs out the user and redirects to the login page.
 * - ViewStaffs: Displays a list of non-admin users with their contacts.
 * - ViewAdmins: Displays a list of admin users.
 * - EditUser: Retrieves and updates user information, including role assignment.
 * - UpdateIsActiveStatus: Updates the active status of a user and returns a JSON response.
 */

namespace ContactAppProject.Controllers
{
    //restrict access to users with the "Admin" role
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginVM loginVM)
        {
            using (var session = NHibernateHelper.CreateSession())
            {
                using (var txn = session.BeginTransaction())
                {
                    //fetch user based on the provided username
                    var getUser = session.Query<User>().SingleOrDefault(u => u.UserName == loginVM.UserName);
                    if (getUser != null)
                    {
                        if (getUser.IsActive)
                        {
                            //if (user.IsAdmin)
                            //{
                            //verify password and check if the user is an admin
                            if (BCrypt.Net.BCrypt.Verify(loginVM.Password, getUser.Password))
                            {
                                FormsAuthentication.SetAuthCookie(loginVM.UserName, true);
                                Session["UserId"] = getUser.Id;
                                return RedirectToAction("ViewStaffs");
                            }
                        }
                        else
                        {
                            if (BCrypt.Net.BCrypt.Verify(loginVM.Password, getUser.Password))
                            {
                                FormsAuthentication.SetAuthCookie(loginVM.UserName, true);
                                Session["UserId"] = getUser.Id;
                                return RedirectToAction("Index", "Contact");
                            }
                            //}
                        }
                    }
                    ModelState.AddModelError("", "UserName/Password doesn't exists");
                    return View();
                }
            }
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(User user, string password)
        {
            //hash the user password
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            using (var session = NHibernateHelper.CreateSession())
            {
                using (var txn = session.BeginTransaction())
                {
                    //assign role based on admin status
                    user.Role.User = user;
                    user.IsActive = true;
                    if (user.IsAdmin)
                    {
                        user.Role.RoleName = "Admin";
                    }
                    else
                    {
                        user.Role.RoleName = "Staff";
                    }
                    session.Save(user);
                    txn.Commit();
                    return RedirectToAction("ViewStaffs");
                }
            }
        }

        //logs out the current user and redirects to the login page
        [Authorize(Roles = "Admin, Staff")]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        public ActionResult ViewStaffs()
        {
            using (var session = NHibernateHelper.CreateSession())
            {
                var staffs = session.Query<User>().FetchMany(u => u.Contacts).Where(u => u.IsAdmin == false).ToList();
                return View(staffs);
            }
        }

        public ActionResult ViewAdmins()
        {
            using (var session = NHibernateHelper.CreateSession())
            {
                var admins = session.Query<User>().Where(u => u.IsAdmin == true).ToList();
                return View(admins);
            }
        }

        public ActionResult EditUser(Guid userId)
        {
            using (var session = NHibernateHelper.CreateSession())
            {
                var fetchUser = session.Get<User>(userId);
                return View(fetchUser);
            }
        }

        [HttpPost]
        public ActionResult EditUser(User user)
        {
            using (var session = NHibernateHelper.CreateSession())
            {
                using (var txn = session.BeginTransaction())
                {
                    user.Role.User = user;
                    if (user.IsAdmin)
                        user.Role.RoleName = "Admin";

                    else
                        user.Role.RoleName = "Staff";
                    session.Update(user);
                    txn.Commit();
                    return RedirectToAction("ViewStaffs");
                }
            }

        }


        public ActionResult ViewContacts(Guid userId)
        {
            TempData["userId"] = userId;
            using (var session = NHibernateHelper.CreateSession())
            {
                var contacts = session.Query<Contact>().Where(c => c.User.Id == userId).ToList();
                return View(contacts);
            }
        }

        public ActionResult ViewContactDetails(Guid contactId)
        {
            TempData["contactId"] = contactId;
            using (var session = NHibernateHelper.CreateSession())
            {
                var contactDetails = session.Query<ContactDetails>().Where(c => c.Contact.Id == contactId).ToList();
                return View(contactDetails);
            }
        }


        [HttpPost]
        public ActionResult UpdateIsActiveStatus(Guid userId, bool isActive)
        {
            using (var session = NHibernateHelper.CreateSession())
            {
                using (var txn = session.BeginTransaction())
                {
                    var user = session.Get<User>(userId);
                    if (user != null)
                    {
                        user.IsActive = isActive;
                        session.Update(user);
                        txn.Commit();
                    }
                }
            }
            return Json(new { success = true });
        }
    }
}