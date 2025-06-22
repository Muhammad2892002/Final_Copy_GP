using System.Diagnostics;
using TechQuestions.Models;
using Microsoft.AspNetCore.Mvc;
using TechQuestions.Context;
using TechQuestions.DTOs;
using Microsoft.EntityFrameworkCore;
using TechQuestions.Entites;
using Microsoft.AspNetCore.Identity.Data;
using TechQuestions.Vm;

namespace TechQuestions.Controllers
{
    public class HomeController : Controller
    {
       public static bool DoesnotUserExist = false;
        public static bool flag = false;
        public static  User _user2 ;
        private readonly TechQuestionDbContext dbContext;
        public static int customerid;

        public HomeController(TechQuestionDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            //Statsics 
            var dashboard = new MainPageCounterDTO();
            dashboard.CustomerCount = dbContext.Users.Where(x => x.IsDeleted == false
            && x.UserType == "Customer").Count();
            dashboard.QuestionCount = dbContext.Questions.Where(x => x.IsDeleted == false).Count();
            dashboard.JobTitlesCount = dbContext.JobTitles.Where(x => x.IsDeleted == false).Count();
            dashboard.ExamCount = dbContext.Exams.Where(x => x.IsDeleted == false).Count();

            var query = await dbContext.JobTitles
                       .Where(x => !x.IsDeleted) 
                       .GroupBy(x => x.CategoryId) 
                       .Select(group => new CategoryDTO
                       {
                           Id = group.Key,
                           Name = group.FirstOrDefault().Category ,
                           VacancyCount = dbContext.JobTitles.Where(x => x.IsDeleted == false && x.CategoryId == group.Key).Count()
                       })
                       .ToListAsync();


            var query2 = await (from comment in dbContext.Testimoials
                         join user in dbContext.Users
                         on comment.CustomerId equals user.Id
                         where comment.IsDeleted == false
                         && user.IsDeleted == false
                         && user.UserType == "Customer"
                         select new CommentDTO
                         {
                             Id = comment.Id,   
                             IsDeleted = comment.IsDeleted,
                             Comment = comment.Comment,
                             Date = comment.CreationDate.ToShortDateString(),
                             Image = user.Image,
                             Title = comment.Title, 
                             UserEmail = user.Email,
                             UserName = user.FirstName + " " + user.LastName
                         }).ToListAsync();
            return View(Tuple.Create(dashboard,query,query2));
        }
        public async Task<IActionResult> JobTitles()
        {
            var titles = dbContext.JobTitles.Where(x => x.IsDeleted == false).
                Select(x=> new JobTitleDto
                {
                    Id= x.Id,
                    Description = x.Description,    
                    Image = x.Image,    
                    Title = x.Title,
                    CategoryId = x.CategoryId,
                    Time = $"{x.CreationDate.ToShortDateString()}"
                })
                .ToList();
            var query = await dbContext.JobTitles
                      .Where(x => !x.IsDeleted)
                      .GroupBy(x => x.CategoryId)
                      .Select(group => new CategoryDTO
                      {
                          Id = group.Key,
                          Name = group.FirstOrDefault().Category,
                          VacancyCount = dbContext.JobTitles.Where(x => x.IsDeleted == false && x.CategoryId == group.Key).Count()
                      })
                      .ToListAsync();
            return View(Tuple.Create(titles,query));
        }
        public async Task<IActionResult> Error()
        {
            ViewBag.ErrorTitle = "";
            ViewBag.Time = DateTime.Now.ToShortDateString();
            return View();
        }
        public async Task<IActionResult> Login()
        {
            ViewBag.emailDoesntExist = false;
            ViewBag.FlagResetpass = false;
            return View();
        }
        public async Task<IActionResult> Register()
        {
            ViewBag.EmailDoesExist = flag;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
          
        
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email
            && x.Password == password && x.IsDeleted == false);
            if (user == null)
            {
               

                var query = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (query != null)
                {

                    if (query.IsDeleted)
                        ViewBag.ErrorTitle = "Your Account Has Been Suspend";

                    else
                    {
                    
                        ViewBag.FlagResetpass = true;
                        ViewBag.emailDoesntExist = false;


                        _user2 = query;
                        return View();
                    }

                }
                else
                {
                    ViewBag.FlagResetpass = false;
                    //doesntEmailExist = true;
                    ViewBag.emailDoesntExist = true;
                    return View();

                    //    ViewBag.ErrorTitle = "User Not Found";
                    //}
                    //ViewBag.ReturnController = "Login";
                    //ViewBag.ReturnAction = "Home";
                    //ViewBag.Time = DateTime.Now.ToShortDateString();
                    //return View("Error");
                }
                return View();
            }
            else
            {
                HttpContext.Session.SetInt32("Id", user.Id);
                if (user.UserType == "Admin")
                {

                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    customerid = user.Id;


                    return RedirectToAction("Index", "Customer");
                }

            }
        }
        public IActionResult ResetPassword() {
            VMResetPass obj = new VMResetPass();
            obj.Email=_user2.Email;
            return View(obj);
            
            return View();
        
        
        }
        [HttpPost]
        public IActionResult ResetPassword(VMResetPass vmobject)
        {
            User user = dbContext.Users.FirstOrDefault(x => x.Id == _user2.Id);
            if (vmobject.Password == vmobject.ConfirmPassword) {
                user.Password=vmobject.Password;
               
            
            }
            dbContext.SaveChanges();
            
            


            return RedirectToAction("Index");


        }
        [HttpPost]
        public async Task<IActionResult> Register(string firstname, string lastname,
            string email, string password, string nationality, string phone, string countrycode)
        {
            flag = false;
            var user = dbContext.Users.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                User cutomer = new User()
                {
                    FirstName = firstname,
                    LastName = lastname,
                    Email = email,
                    Password = password,
                    Nationality = nationality,
                    CountryCode = countrycode,
                    CreationDate = DateTime.Now,
                    UpdateDate = null,
                    Image = null,
                    Phone = phone,
                    UserType = "Customer",
                    IsDeleted = false,
                };
                await dbContext.Users.AddAsync(cutomer);
                await dbContext.SaveChangesAsync();
                if (cutomer.Id > 0)
                    return RedirectToAction("Login", "Home");
                else
                {
                    ViewBag.ReturnController = "Register";
                    ViewBag.ReturnAction = "Home";
                    ViewBag.Time = DateTime.Now.ToShortDateString();
                    ViewBag.ErrorTitle = "Failed To Register New Customer";
                    return RedirectToAction("Error");
                }
            }
            else {
                flag = true;
                return RedirectToAction("Register");
            
            }
        }
        public IActionResult ConfirmAccountToResetPassword()
        {
            ViewBag.DoesUserExist = DoesnotUserExist;
            return View();


        }
        [HttpPost]
        public async Task<IActionResult> CheckAccountExistince(Vm.VMInfoToResetPass data)
        {
            var GoTo = "";
            var user = await  dbContext.Users.FirstOrDefaultAsync(usersData => usersData.FirstName == data.FirstName &&
            usersData.LastName == data.LastName && usersData.Phone == data.PhoneNumber);
            if (user != null)
            {
                
                ViewBag.Email = user.Email;
                GoTo = "ResetPassword";

            }
            else
            {
                DoesnotUserExist = true;

                GoTo ="ConfirmAccountToResetPassword";
            }
            return RedirectToAction(GoTo); 
            
            
            
           


        }
    }
}
