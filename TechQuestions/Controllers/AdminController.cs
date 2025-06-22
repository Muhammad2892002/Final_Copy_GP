using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechQuestions.Context;
using TechQuestions.DTOs;
using TechQuestions.Entites;
using TechQuestions.Vm;
using TechQuestions.Services;

namespace TechQuestions.Controllers
{
    public class AdminController : Controller
    {
        private readonly TechQuestionDbContext dbContext;
        private readonly IWebHostEnvironment _env;
        public AdminController(TechQuestionDbContext _dbContext, IWebHostEnvironment env)
        {
            dbContext = _dbContext;
            _env = env;
        }
        public async Task <IActionResult> JobTitleInExam() {
            var query=await (from job in dbContext.JobTitles
                       select job 
                       ).Where(x=>x.IsDeleted==false).ToListAsync();
            return View(query);
        
        
        }
        public async Task<IActionResult> Index()
        {
   
           
            var dashboard = new MainPageCounterDTO();
            dashboard.CustomerCount =  dbContext.Users.Where(x => x.IsDeleted == false
            && x.UserType == "Customer").Count();
            dashboard.QuestionCount = await dbContext.Questions.Where(x => x.IsDeleted == false).CountAsync();
            dashboard.JobTitlesCount = await dbContext.JobTitles.Where(x => x.IsDeleted == false).CountAsync();
            dashboard.ExamCount = await dbContext.Exams.Where(x => x.IsDeleted == false).CountAsync();

            var query = await dbContext.JobTitles
                      .Where(x => !x.IsDeleted)
                      .GroupBy(x => x.CategoryId)
                      .Select(group => new CategoryDTO
                      {
                          Id = group.Key,
                          Name = group.FirstOrDefault().Category
                      })
                      .ToListAsync();
            return View(Tuple.Create(query,dashboard));
        }
        public async Task<IActionResult> Users()
        {
            //Get All Teams
            var customers = await (from user in dbContext.Users
                                   where user.UserType == "Customer" && user.IsDeleted == false
                                   orderby user.CreationDate ascending
                                   select new SystemUserDTO
                                   {
                                       Id = user.Id,
                                       Image = user.Image == null ? "https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg" : user.Image,
                                       Name = user.FirstName + " " + user.LastName,
                                       Email = user.Email,
                                       JoinDate = user.CreationDate.ToShortDateString(),
                                       Nationality = user.Nationality,
                                       Phone = user.CountryCode + "-" + user.Phone
                                   }).ToListAsync();
            return View(customers);
        }
        public async Task<IActionResult> JobTitles()
        {
            var JobTitles = await (from JobTitle in dbContext.JobTitles
                                    select JobTitle)
                                    .ToListAsync();
            return View(JobTitles);
        }
        public async Task<IActionResult> CreateJobTitle()
        {
            var query = await dbContext.JobTitles
                      .Where(x => !x.IsDeleted)
                      .GroupBy(x => x.CategoryId)
                      .Select(group => new CategoryDTO
                      {
                          Id = group.Key,
                          Name = group.FirstOrDefault().Category
                      })
                      .ToListAsync();
            return View(query);
        }
        public async Task<IActionResult> UpdateJobTitle(int Id)
        {
            var categories = await dbContext.JobTitles
                     .Where(x => !x.IsDeleted)
                     .GroupBy(x => x.CategoryId)
                     .Select(group => new CategoryDTO
                     {
                         Id = group.Key,
                         Name = group.FirstOrDefault().Category
                     })
                     .ToListAsync();
            var query = await dbContext.JobTitles.FirstOrDefaultAsync(x => x.Id == Id);
            if (query != null)
            {

                return View(Tuple.Create(categories, query));
            }
            return RedirectToAction("JobTitles");

        }
        [HttpPost]
        public async Task<IActionResult> CreateJobTitle(string name, string desc,string category, IFormFile image)
        {
            JobTitle JobTitle = new JobTitle()
            {
                CreationDate = DateTime.Now,
                UpdateDate = null,
                IsDeleted = false,
                Description = desc,
                Title = name,
                Image = image == null ? "" : await UploadImageAndGetURL(image),
                Category=category
            };
            await dbContext.AddAsync(JobTitle);
            await dbContext.SaveChangesAsync();
            return RedirectToAction("JobTitles");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateJobTitle(int JobTitleId,string category, string title, string desc, IFormFile image)
        {
      
            var JobTitle = await dbContext.JobTitles.FirstOrDefaultAsync(x => x.Id == JobTitleId);
            if (JobTitle != null)
            {
                JobTitle.Description = desc;
                JobTitle.Title = title;
                JobTitle.Image = image == null ? JobTitle.Image : await UploadImageAndGetURL(image);
                JobTitle.UpdateDate = DateTime.Now;
                dbContext.Update(JobTitle);
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction("JobTitles");

        }
        [HttpGet]
        public async Task<IActionResult> DeleteJobTitle(int Id)
        {
            var query = await dbContext.JobTitles.FirstOrDefaultAsync(x => x.Id == Id);
            if (query != null)
            {
                query.IsDeleted = true;
                dbContext.Update(query);
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction("JobTitles");
        }
        public async Task<IActionResult> Questions()
        {
            ViewBag.isDeleted = false;
    
            VmQuestionPageModel Vmobject=new VmQuestionPageModel();
            Vmobject.Filter = new VmFilter();
            Vmobject.JobTitles=await(dbContext.JobTitles.Where(job=>job.IsDeleted== false).ToListAsync());
            ViewBag.JobsList = Vmobject.JobTitles;
            Vmobject.Questions = await (from Question in dbContext.Questions
                                  join JobTitle in dbContext.JobTitles
                                  on Question.JobTitleId equals JobTitle.Id
                                  where Question.IsDeleted == false
                                  orderby Question.CreationDate ascending
                                  select new QuestionDTO
                                  {
                                      Id = Question.Id,
                                      Body = Question.Body,
                                      Level = Question.Level,
                                      JobTitle = JobTitle.Title,
                                      MultiMediaPath = Question.MultiMediaPath,
                                      Type = Question.Type,
                                      Time = $"Since {Question.CreationDate.ToShortTimeString()}",
                                      IsDeleted = false
                                      
                                      
                                  }).ToListAsync();

            return View(Vmobject);
        }
        public async Task<IActionResult> QuestionDetails(int Id)
        {
            var Questions = await (from Question in dbContext.Questions
                                   join JobTitle in dbContext.JobTitles
                                   on Question.JobTitleId equals JobTitle.Id
                                   where Question.IsDeleted == false && Question.Id == Id
                                   orderby Question.CreationDate ascending
                                   select new QuestionDetailsDTO
                                   {
                                      JobTitle = JobTitle,
                                      Question = Question
                                   }).FirstOrDefaultAsync();
            if (Questions == null) {
                 Questions = await (from Question in dbContext.Questions
                                       join JobTitle in dbContext.JobTitles
                                       on Question.JobTitleId equals JobTitle.Id
                                       where Question.IsDeleted == true && Question.Id == Id
                                       orderby Question.CreationDate ascending
                                       select new QuestionDetailsDTO
                                       {
                                           JobTitle = JobTitle,
                                           Question = Question
                                       }).FirstOrDefaultAsync();


            }
            

            return View(Questions);
        }
        public async Task<IActionResult> CreateQuestion()
        {
            return View(await dbContext.JobTitles.Where(x => x.IsDeleted == false).ToListAsync());
        }
        public async Task<IActionResult> UpdateQuestion(int Id)
        {
            var query = await dbContext.Questions.FirstOrDefaultAsync(x => x.Id == Id);
            if (query != null)
            {

                return View(Tuple.Create<Question, List<JobTitle>>(query, await dbContext.JobTitles.ToListAsync()));
            }
            return RedirectToAction("Questions");

        }
        [HttpPost]
        public async Task<IActionResult> CreateQuestion(int jobTitleId, string name,
            string level, string body, IFormFile image,string ans1, string ans2
            , string ans3, string ans4, int corrcetIndex)
        {
            Question Question = new Question()
            {
                JobTitleId = jobTitleId,
                CreationDate = DateTime.Now,
                UpdateDate = null,
                IsDeleted = false,
                Type = name,
                Level = level,
                Body = body,
                MultiMediaPath = image == null ? "" : await UploadImageAndGetURL(image),
                Answer1 = ans1,
                Answer2= ans2,
                Answer3 = ans3,
                Answer4 = ans4
                
        };
            if (corrcetIndex == 0)
                Question.CorrectAnswer = ans1;
            if (corrcetIndex == 1)
                Question.CorrectAnswer = ans2;
            if (corrcetIndex == 2)
                Question.CorrectAnswer = ans3;
            if (corrcetIndex == 3)
                Question.CorrectAnswer = ans4;
            await dbContext.AddAsync(Question);
            await dbContext.SaveChangesAsync();
            return RedirectToAction("Questions");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateQuestion(int Id, int jobTitleId, string name,
            string level, string body, IFormFile image, string ans1, string ans2
            , string ans3, string ans4,int corrcetIndex)
        {
            var Question = await dbContext.Questions.FirstOrDefaultAsync(x => x.Id == Id);
            if (Question != null)
            {
                Question.JobTitleId = jobTitleId;
                Question.CreationDate = DateTime.Now;
                Question.UpdateDate = null;
                Question.IsDeleted = false;
                Question.Type = name;
                Question.Level = level;
                Question.Body = body;
                Question.MultiMediaPath = image == null ? Question.MultiMediaPath : await UploadImageAndGetURL(image);
                Question.Answer1 = ans1;
                Question.Answer2 = ans2;
                Question.Answer3 = ans3;
                Question.Answer4 = ans4;
                if (corrcetIndex == 0)
                    Question.CorrectAnswer = ans1;
                if (corrcetIndex == 1)
                    Question.CorrectAnswer = ans2;
                if (corrcetIndex == 2)
                    Question.CorrectAnswer = ans3;
                if (corrcetIndex == 3)
                    Question.CorrectAnswer = ans4;
                dbContext.Update(Question);
                await dbContext.SaveChangesAsync();

            }
            return RedirectToAction("Questions");
        }
        [HttpGet]
        public async Task<IActionResult> DeleteQuestion(int Id)
        {
            var query = await dbContext.Questions.FirstOrDefaultAsync(x => x.Id == Id);
            if (query != null)
            {
                query.IsDeleted = !query.IsDeleted;
                dbContext.Update(query);
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction("Questions");
        }

        public async Task<IActionResult> Testimonials()
        {
            //Testimonials
            var testimoials = await (from user in dbContext.Users
                                     join comment in dbContext.Testimoials
                                     on user.Id equals comment.CustomerId
                                     where  user.UserType == "Customer"
                                    
                                     orderby comment.CreationDate descending
                                     select new CommentDTO
                                     {
                                         Id = comment.Id,
                                         Comment = comment.Comment,
                                         Title = comment.Title,
                                         Date = comment.CreationDate.ToShortDateString(),
                                         IsDeleted = comment.IsDeleted,
                                         UserEmail = user.Email,
                                         UserName = user.FirstName + " " + user.LastName

                                     }).ToListAsync();
            return View(testimoials);
        }
        [HttpGet]
        public async Task<IActionResult> UpdateTestimonail(int Id, bool isApproved)
        {
            var query = await dbContext.Testimoials.FirstOrDefaultAsync(x => x.Id == Id);
            if (query != null)
            {
                query.IsDeleted = !isApproved;
                dbContext.Update(query);
                await dbContext.SaveChangesAsync();
            }
            return RedirectToAction("Testimonials");
        }
        public async Task<IActionResult> Exams(int IdJob)
        {
            Service1 service = new Service1();
         
            var Jobs = await dbContext.JobTitles.FirstOrDefaultAsync(jobs => jobs.Id == IdJob);


            var EasyScores = await (from exam in dbContext.Exams
                                    join users in dbContext.Users
                                    on exam.CustomerId equals users.Id
                                    where exam.ExamLevel == "Easy" && exam.JobTitle == Jobs.Title && exam.IsDeleted == false
                                    select new ChartsData {
                                        Scores = service.TurnMarkToInt(exam.Score),
                               
                                   
                                        Name = users.FirstName,
                                        Level = exam.ExamLevel



                                    }).ToListAsync();


            var MidScores = await (from exam in dbContext.Exams
                                    join users in dbContext.Users
                                    on exam.CustomerId equals users.Id
                                    where exam.ExamLevel == "Mid" && exam.JobTitle == Jobs.Title && exam.IsDeleted == false
                                   select new ChartsData
                                    {
                                        Scores = service.TurnMarkToInt(exam.Score),
                                        Name = users.FirstName,
                                        Level = exam.ExamLevel



                                    }).ToListAsync();

            var AdvancedScores = await (from exam in dbContext.Exams
                                   join users in dbContext.Users
                                   on exam.CustomerId equals users.Id
                                   where exam.ExamLevel == "Advanced" && exam.JobTitle == Jobs.Title && exam.IsDeleted == false
                                        select new ChartsData
                                   {
                                       Scores = service.TurnMarkToInt(exam.Score),
                                       Name = users.FirstName,
                                       Level = exam.ExamLevel



                                   }).ToListAsync();


            var query = new List<ExamDTO>();
            var Id = HttpContext.Session.GetInt32("Id");
            if (Id != null)
            {
                query = await (from exam in dbContext.Exams
                               join user in dbContext.Users
                               on exam.CustomerId equals user.Id
                               where exam.JobTitle==Jobs.Title && exam.IsDeleted == false
                               
                               select new ExamDTO
                               {
                                   Id = exam.Id,
                                   Customer = user.FirstName + " " + user.LastName,
                                   Title = exam.Title,
                                   Score = exam.Score,
                                   ExamDate = exam.CreationDate.ToShortDateString(),
                                   ExamDeadLine = exam.ExamDeadLine.ToShortDateString(),
                                   ExamLevel = exam.ExamLevel,
                                   QuestionCountInEasyLevel = exam.QuestionCountInEasyLevel,
                                   QuestionCountInHardLevel = exam.QuestionCountInHardLevel,
                                   QuestionCountInIntermidateLevel = exam.QuestionCountInIntermidateLevel,
                                   JobTitle = exam.JobTitle 
                               }).ToListAsync();
            }
            return View(Tuple.Create(query,EasyScores,MidScores,AdvancedScores));
        }
        
        public async Task<IActionResult> ExamQuestionDetails(int Id)
        {
            
            List<ExamQuestionDetailDTO> query=null;
            var order = await dbContext.Exams.FirstOrDefaultAsync(x => x.Id == Id);
            try
            {
                query = await (from item in dbContext.Exams
                               where item.IsDeleted == false && item.Id == Id
                               join ExamQuestion in dbContext.ExamQuestions on item.Id equals ExamQuestion.ExamId into eqGroup
                               from ExamQuestion in eqGroup.DefaultIfEmpty()
                               join Question in dbContext.Questions on ExamQuestion.QuestionId equals Question.Id into qGroup
                               from Question in qGroup.DefaultIfEmpty()
                               join Customer in dbContext.Users on item.CustomerId equals Customer.Id into uGroup
                               from Customer in uGroup.DefaultIfEmpty()
                               orderby item.UpdateDate descending
                               select new ExamQuestionDetailDTO
                               {
                                   Question = Question,
                                   User = Customer,
                                   Exam = item,
                                   ExamQuestion = ExamQuestion
                               }).ToListAsync();

            }
            catch (Exception ex) {
                

                Console.WriteLine(ex.ToString());
            
            }

            return View(query);
        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("Id");
            HttpContext.Session.Remove("ExamId");
            HttpContext.Session.Remove("Type");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Error()
        {
            ViewBag.ErrorTitle = "";
            ViewBag.Time = DateTime.Now.ToShortDateString();
            return View();
        }
        
        public async Task<string> UploadImageAndGetURL(IFormFile file)
        {
            string uploadFolder = Path.Combine(_env.WebRootPath, "Uploads");
            if (file == null || file.Length == 0)
            {
                throw new Exception("Please Enter Valid File");
            }
            string newFileURL2 = Guid.NewGuid().ToString() + "" + file.FileName;
            using (var inputFile = new FileStream(Path.Combine(uploadFolder, newFileURL2), FileMode.Create))
            {
                await file.CopyToAsync(inputFile);
            }
            return newFileURL2;
        }
        [HttpPost]
        public async Task<IActionResult> SearchQuastions(VmQuestionPageModel vmFilter) {

            VmQuestionPageModel Vmobject = new VmQuestionPageModel();
            Vmobject.Filter = vmFilter.Filter;
            vmFilter.JobTitles = await(dbContext.JobTitles.Where(job => job.IsDeleted == false).ToListAsync());
            ViewBag.JobsList = Vmobject.JobTitles;
            vmFilter.Questions= await(from Question in dbContext.Questions
                                       join JobTitle in dbContext.JobTitles
                                       on Question.JobTitleId equals JobTitle.Id
                                       where Question.IsDeleted==vmFilter.Filter.IsDeleted && Question.JobTitleId==vmFilter.Filter.TitleId && Question.Level==vmFilter.Filter.Type
                                       orderby Question.CreationDate ascending
                                       select new QuestionDTO
                                       {
                                           Id = Question.Id,
                                           Body = Question.Body,
                                           Level = Question.Level,
                                           JobTitle = JobTitle.Title,
                                           MultiMediaPath = null,
                                           Type = Question.Type,
                                           Time = $"Since {Question.CreationDate.ToShortTimeString()}",
                                           IsDeleted=Question.IsDeleted,
                                       }).ToListAsync();
            ViewBag.isDeleted = vmFilter.Filter.IsDeleted;
            return View("Questions",vmFilter);
        }
       
        public IActionResult ActiveQuastion(int Id) {
            var quastion=dbContext.Questions.FirstOrDefault(q => q.Id==Id);
            quastion.IsDeleted= false;
            quastion.UpdateDate = DateTime.Now;
            dbContext.SaveChanges();
            return RedirectToAction("Questions");
        
        
        }
     
    }
}
