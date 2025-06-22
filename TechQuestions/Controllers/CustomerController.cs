using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Options;
using TechQuestions.Context;
using TechQuestions.DTOs;
using TechQuestions.Entites;
using TechQuestions.Services;
using TechQuestions.Vm;


namespace TechQuestions.Controllers
{
    public class CustomerController : Controller
    {
        public static bool DoesExamExist=false;
        
        public List<string> LeavingEraliar=new List<string>();
        public static Exam ifLeavingExamEarly = new Exam();
        static int counter = 0;
        static bool FlagError=false ;
       static string ErrorText ="";
       static DateTime DealLineExam;
        static int time;

        public int cId=HomeController.customerid;
        private readonly TechQuestionDbContext dbContext;
        public static List<Question> currentExam = new List<Question>();
        public static List<string> userAnswer = new List<string>();
        public static int examIndexPointer = 0;
        
       
     
        public CustomerController(TechQuestionDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Error = false;
            ViewBag.ErrorFlag = "no meassage";

            if (counter < 2) { 

            ViewBag.Error =ErrorText ;
            ViewBag.ErrorFlag = FlagError;
            }
            if (counter == 1) {
              counter++;
            }
            
            
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
                           Name = group.FirstOrDefault().Category,
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
            return View(Tuple.Create(dashboard, query, query2));
        }
        public async Task<IActionResult> JobTitles()
        {
         
            
            var titles = dbContext.JobTitles.Where(x => x.IsDeleted == false).
                Select(x => new JobTitleDto
                {
                    Id = x.Id,
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
            return View(Tuple.Create(titles, query));
        }
        public async Task<IActionResult> Error()
        {
            ViewBag.ErrorTitle = "";
            ViewBag.Time = DateTime.Now.ToShortDateString();
            return View();
        }
        public async Task<IActionResult> GetExam()
        {
            int questionNumber = 48;
        
            List<JobTitle> newObj=new List<JobTitle>();
            var query=dbContext.JobTitles.ToList();
            foreach (var item in query) {
                var query2=dbContext.Questions.Where(qua=>qua.JobTitleId==item.Id).Count();
                if (query2 > 48) {
                    newObj.Add(item);
                
                }
            
            }
      

            return View(newObj);
        }
        public async Task<IActionResult> GetExamByJobTitle(int Id)
        {
         
           
  return View(dbContext.JobTitles.Where(x => x.Id == Id).ToList());

            
            
          
        }
        [HttpPost]
        public async Task<IActionResult> RequesitExam(string level, int jobTitleid)
        {
            var jobTitle = dbContext.JobTitles.FirstOrDefault(x => x.Id == jobTitleid);
            var Id = HttpContext.Session.GetInt32("Id");
            int minutes = 0;
            int questionEasy = 0, questionMid = 0, questionAdvance = 0;
            List<Question> advanceoptions;
            List<Question> midoptions;
            List<Question> easyoptions;
            ViewBag.ErrorFlag = false;
            int mark_Condition = 0;
            if (currentExam != null && userAnswer != null)
            {
                userAnswer.Clear();
                currentExam.Clear();
            }
            Service1 service1=new Service1();

            int advancedQuestionNumbers = 17;
        
            int numaricalGrade = 0;
            var queryQEasy = dbContext.Questions.Where(Q => Q.JobTitleId == jobTitleid && Q.Level == "Easy" && Q.IsDeleted==false).ToList();
            var queryQMid = dbContext.Questions.Where(Q => Q.JobTitleId == jobTitleid && Q.Level == "Mid" && Q.IsDeleted == false).ToList(); ;
            var queryQAdvanced = dbContext.Questions.Where(Q => Q.JobTitleId == jobTitleid && Q.Level == "Advanced" && Q.IsDeleted == false).ToList(); 
            var queryQNumsEasy = queryQEasy.Count();
            var queryQNumsMid = queryQMid.Count();
            var queryQNumsAdvanced = queryQAdvanced.Count();
            var job =   dbContext.JobTitles.Where(x => x.Id == jobTitleid).FirstOrDefault();
            var exams =   dbContext.Exams.Where(x => x.CustomerId == cId && x.ExamLevel == level && x.JobTitle==job.Title && x.IsDeleted==false).ToList();
            if (level=="Mid" )
            {
                var gradesEasy =await dbContext.Exams.FirstOrDefaultAsync(x => x.CustomerId == cId && x.ExamLevel == "Easy" && x.IsDeleted==false && x.JobTitle==jobTitle.Title );
                if (gradesEasy == null)
                {
                    mark_Condition = service1.TurnMarkToInt("");
                }
                else
                {
                    mark_Condition = service1.TurnMarkToInt(gradesEasy.Score);
                }
            }
            if (level == "Advanced") {
                var gradesMid = await dbContext.Exams.Where(x => x.CustomerId == cId && x.ExamLevel == "Mid" && x.IsDeleted == false && x.JobTitle == jobTitle.Title).FirstOrDefaultAsync();
                if (gradesMid == null)
                {
                    mark_Condition = service1.TurnMarkToInt("");
                }
                else
                {
                    mark_Condition = service1.TurnMarkToInt(gradesMid.Score);
                }
            }
            if (level == "Easy" && queryQNumsEasy >= 30)
            {
                counter = 0;

                FlagError = true;
                counter++;
                foreach (var item in exams)
                {
                    numaricalGrade = service1.TurnMarkToInt(item.Score);
                    if (item.ExamLevel == "Easy" && numaricalGrade >= 20)
                    {
                        ErrorText = "You cant take Easy Exam You Passed";

                        return RedirectToAction("Index");

                    }
                  

                }

                if (exams.Count != 0)
                {
                    foreach (var item in exams)
                    {
                        item.IsDeleted = true;
                    }
                    dbContext.UpdateRange(exams);
                    dbContext.SaveChanges();
                }
                ViewBag.Timer = minutes;
                    minutes = 30;
                    questionEasy = 30;
               CustomerController.time=minutes;
                Exam exam = new Exam()
                {
                    CreationDate = DateTime.Now,
                    CustomerId = (int)Id,
                    ExamDate = DateTime.Now,
                    ExamLevel = level,
                    ExamDeadLine = DateTime.Now.AddMinutes(minutes),
                    JobTitle = jobTitle != null ? jobTitle.Title : "",
                    QuestionCountInEasyLevel = questionEasy,
                    QuestionCountInIntermidateLevel = questionMid,
                    QuestionCountInHardLevel = questionAdvance,
                    IsDeleted = false,
                    Score = "",
                    Title = jobTitle.Title + $"  {level}  Exam",
                    UpdateDate = null

                };
                CustomerController.DealLineExam = exam.ExamDeadLine;
                dbContext.Add(exam);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("ExamId", exam.Id);
                Random rndm = new Random();
                easyoptions = queryQEasy.OrderBy(q => rndm.Next()).Take(questionEasy).ToList();
                midoptions = queryQMid.OrderBy(q => rndm.Next()).Take(questionMid).ToList();
                advanceoptions = queryQAdvanced.OrderBy(q => rndm.Next()).Take(questionAdvance).ToList();

                currentExam.AddRange(easyoptions);
                currentExam.AddRange(midoptions);
                currentExam.AddRange(advanceoptions);
                foreach (var item in currentExam)
                {
                    var examoptions = new ExamQuestion
                    {
                        QuestionId = item.Id,
                        ExamId = exam.Id,
                        IsDeleted = false,
                        AnswerBody = "",
                        AnswerId = item.Id,
                        IsCorrect = false,
                        Mark = 0,
                        UpdateDate = null,
                        CreationDate = DateTime.Now
                    };

                    await dbContext.AddAsync(examoptions);
                    await dbContext.SaveChangesAsync();
                }

                return Redirect("ExamContent");



            }
            else if (level == "Mid" && queryQNumsEasy >= 12 && queryQNumsMid >= 18 && mark_Condition>=20 )
            {
                counter = 0;
                FlagError = true;
                counter++;
                foreach (var item in exams)
                {
                    numaricalGrade = service1.TurnMarkToInt(item.Score);
                   
                     if (item.ExamLevel == "Mid" && numaricalGrade >= 17)
                    {
                        ErrorText = "You cant take Mid Exam You Passed";
                        return RedirectToAction("Index");

                    }
            
                }
                if (exams.Count != 0)
                {
                    foreach (var item in exams)
                    {
                        item.IsDeleted = true;
                    }
                    dbContext.UpdateRange(exams);
                    dbContext.SaveChanges();
                }
                ViewBag.Timer = minutes;
                    minutes = 45;
    CustomerController.time=minutes;
                    questionEasy = 12;
                    questionMid = 18;

                Exam exam = new Exam()
                {
                    CreationDate = DateTime.Now,
                    CustomerId = (int)Id,
                    ExamDate = DateTime.Now,
                    ExamLevel = level,
                    ExamDeadLine = DateTime.Now.AddMinutes(minutes),
                    JobTitle = jobTitle != null ? jobTitle.Title : "",
                    QuestionCountInEasyLevel = questionEasy,
                    QuestionCountInIntermidateLevel = questionMid,
                    QuestionCountInHardLevel = questionAdvance,
                    IsDeleted = false,
                    Score = "",
                    Title = jobTitle.Title + $"  {level}  Exam",
                    UpdateDate = null

                };
                CustomerController.DealLineExam = exam.ExamDeadLine;
                dbContext.Add(exam);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("ExamId", exam.Id);
                Random rndm = new Random();
                easyoptions = queryQEasy.OrderBy(q => rndm.Next()).Take(questionEasy).ToList();
                midoptions = queryQMid.OrderBy(q => rndm.Next()).Take(questionMid).ToList();
                advanceoptions = queryQAdvanced.OrderBy(q => rndm.Next()).Take(questionAdvance).ToList();

                currentExam.AddRange(easyoptions);
                currentExam.AddRange(midoptions);
                currentExam.AddRange(advanceoptions);
                foreach (var item in currentExam)
                {
                    var examoptions = new ExamQuestion
                    {
                        QuestionId = item.Id,
                        ExamId = exam.Id,
                        IsDeleted = false,
                        AnswerBody = "",
                        AnswerId = item.Id,
                        IsCorrect = false,
                        Mark = 0,
                        UpdateDate = null,
                        CreationDate = DateTime.Now
                    };

                    await dbContext.AddAsync(examoptions);
                    await dbContext.SaveChangesAsync();
                }

                return Redirect("ExamContent");





            }
            else if (level == "Advanced" && queryQNumsEasy >= 5 && queryQNumsMid >= 8 && queryQNumsAdvanced >= 17 && mark_Condition>=17) {
                counter = 0;

                FlagError = true;
                counter++;
                foreach (var item in exams)
                {
                    numaricalGrade = service1.TurnMarkToInt(item.Score);
                   
                     if ( numaricalGrade >= 18)
                    {
                        ErrorText = "You cant take Advanced Exam You Passed";
                        return RedirectToAction("Index");

                    }

                }

                if (exams.Count != 0)
                {
                    foreach (var item in exams)
                    {
                        item.IsDeleted = true;
                    }
                    dbContext.UpdateRange(exams);
                    dbContext.SaveChanges();
                }
            ViewBag.Timer=minutes;
                minutes = 60;
                questionEasy = 5;
                questionMid = 8;
                questionAdvance = 17;
             CustomerController.time=minutes;



                Exam exam = new Exam()
                {
                    CreationDate = DateTime.Now,
                    CustomerId = (int)Id,
                    ExamDate = DateTime.Now,
                    ExamLevel = level,
                    ExamDeadLine = DateTime.Now.AddMinutes(minutes),
                    JobTitle = jobTitle != null ? jobTitle.Title : "",
                    QuestionCountInEasyLevel = questionEasy,
                    QuestionCountInIntermidateLevel = questionMid,
                    QuestionCountInHardLevel = questionAdvance,
                    IsDeleted = false,
                    Score = "",
                    Title = jobTitle.Title + $"  {level}  Exam",
                    UpdateDate = null

                };
                CustomerController.DealLineExam = exam.ExamDeadLine;
                dbContext.Add(exam);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("ExamId", exam.Id);
                Random rndm = new Random();
                easyoptions = queryQEasy.OrderBy(q => rndm.Next()).Take(questionEasy).ToList();
                midoptions = queryQMid.OrderBy(q => rndm.Next()).Take(questionMid).ToList();
                advanceoptions = queryQAdvanced.OrderBy(q => rndm.Next()).Take(questionAdvance).ToList();

                currentExam.AddRange(easyoptions);
                currentExam.AddRange(midoptions);
                currentExam.AddRange(advanceoptions);
                foreach (var item in currentExam)
                {
                    var examoptions = new ExamQuestion
                    {
                        QuestionId = item.Id,
                        ExamId = exam.Id,
                        IsDeleted = false,
                        AnswerBody = "",
                        AnswerId = item.Id,
                        IsCorrect = false,
                        Mark = 0,
                        UpdateDate = null,
                        CreationDate = DateTime.Now
                    };

                    await dbContext.AddAsync(examoptions);
                    await dbContext.SaveChangesAsync();
                }

                return Redirect("ExamContent");
            }

            else {
                counter = 0;
                 
                FlagError = true;
                counter++;
                
                if (mark_Condition < 20 && level=="Mid" && queryQNumsEasy>=12 && queryQNumsMid>=18)
                {
                    ErrorText = "You cant take this exam pass The Easy First";
                    return RedirectToAction("Index");
                }
               else if (mark_Condition < 17 && level == "Advanced" && queryQNumsEasy>=5 && queryQNumsMid>=8 && queryQNumsAdvanced>=17) {
                    ErrorText = "You cant take this exam pass The Mid First";

                    return RedirectToAction("Index");
                }
               else if (level=="Easy"&& queryQNumsEasy < 30) {
                    ErrorText = "You cant take Easy Exam exam Please Contact The Admin";
                    RedirectToAction("Index");
                
                }
               else if (level == "Mid" && queryQNumsEasy < 12 && queryQNumsMid<18)
                {
                    ErrorText = "You cant take Mid Exam exam Please Contact The Admin";
                    RedirectToAction("Index");

                }
               else if (level == "Advanced" && queryQNumsEasy < 5 && queryQNumsMid<8 && queryQNumsAdvanced<17)
                {
                    ErrorText = "You cant take Easy Exam exam Please Contact The Admin";
                    RedirectToAction("Index");

                }


            }
            return RedirectToAction("Index");

        
        }
        public async Task<IActionResult> ExamContent()
        {
            ViewBag.userId=cId;
            ViewBag.Timer = CustomerController.time;
            DealLineExam.AddMinutes(1);
            TimeOnly time = TimeOnly.FromDateTime(DealLineExam);

            ViewBag.DeadLine = DealLineExam;


            ViewBag.Index = examIndexPointer;
            ViewBag.Text = "Next";
            return View(currentExam.ElementAt(0));
         
        }
        [HttpPost]
        public async Task<IActionResult> IncreaseExamIndex(int id, string Answer)
        {
            DateTime timeCondition = DateTime.Now;
            if (timeCondition < DealLineExam)
            {
                TimeOnly time = TimeOnly.FromDateTime(DealLineExam);

                ViewBag.DeadLine = DealLineExam;



                ViewBag.Timer = CustomerController.time;



                userAnswer.Insert(examIndexPointer, Answer);
                LeavingEraliar.Add(Answer);
                if (userAnswer.Count() == currentExam.Count())
                {
                    return Redirect("SubmitExam");
                }
                if ((examIndexPointer + 3) > currentExam.Count())
                {
                    ++examIndexPointer;

                    ViewBag.Index = examIndexPointer;
                    ViewBag.IsFinish = true;
                    ViewBag.Text = "Submit And Finish";
                    userAnswer.Count();
                    return View("ExamContent", currentExam.ElementAt(examIndexPointer));

                }
                else
                {
                    ++examIndexPointer;

                    ViewBag.Index = examIndexPointer;
                    ViewBag.IsFinish = false;
                    ViewBag.Text = "Next";
                    userAnswer.Count();
                    return View("ExamContent", currentExam.ElementAt(examIndexPointer));

                }
            }
            else {
                return RedirectToAction("SubmitExam");
            
            }

        }
        public async Task<IActionResult> SubmitExam()
        {
            DateTime timeCondition = DateTime.Now;
            if (userAnswer.Count()!=30)
            {
                while (userAnswer.Count < currentExam.Count)
                {
                    userAnswer.Add(""); 
                }
               
            }
            int result = 0;
            var examInfo = dbContext.Exams.Where(recored => recored.Id == HttpContext.Session.GetInt32("ExamId")).SingleOrDefault();
            if (examInfo == null)
            {

                return RedirectToAction("Index");
            }
            else
            {

                examIndexPointer = 0;
                int j = 0;
                foreach (var item in currentExam)
                {
                    var examQuestion = dbContext.ExamQuestions.FirstOrDefault(x => x.ExamId == examInfo.Id
                    && x.QuestionId == item.Id);
                    if (examQuestion != null)
                    {
                        examQuestion.AnswerBody = userAnswer.ElementAt(j);
                        examQuestion.AnswerId = 1;
                        if (userAnswer.ElementAt(j) == currentExam.ElementAt(j).CorrectAnswer
                            && !string.IsNullOrEmpty(userAnswer.ElementAt(j))
                            && !string.IsNullOrWhiteSpace(userAnswer.ElementAt(j)))
                        {
                            
                            result++;
                            examQuestion.Mark = 1;
                        }
                        else
                        {
                            examQuestion.Mark = 0;
                        }
                        dbContext.Update(examQuestion);
                        dbContext.SaveChanges();
                        j++;
                    }
                }
                examInfo.Score = result+"/"+currentExam.Count();
                dbContext.Update(examInfo);
              
                dbContext.SaveChanges();
                return RedirectToAction("MyExamHistory");
            }

        }
        public async Task<IActionResult> MyExamHistory()
        {

           
            var Id = HttpContext.Session.GetInt32("Id");
            var isHaveTestimonial = await dbContext.Testimoials.AnyAsync(x => x.CustomerId == Id);

            var exams = dbContext.Exams.Where(x => x.CustomerId == Id && x.IsDeleted==false)
                .ToList();
         
          ViewBag.IsHaveTest = isHaveTestimonial;
            return View(exams);
        }
        public async Task<IActionResult> CreateTestimonial()
        {
         
           
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateTestimonial(string title,string comment)
        {
            var Id = HttpContext.Session.GetInt32("Id");
            var AllTestimonial = await dbContext.Testimoials.Where(obj => obj.Id == Id).ToListAsync();
            foreach (var item in AllTestimonial) { 
                item.IsDeleted = true;
            }

            dbContext.UpdateRange(AllTestimonial);
            dbContext.SaveChanges();
            Testimoial testimoial = new Testimoial();
            testimoial.Title = title;
            testimoial.Comment = comment;   
            testimoial.CreationDate = DateTime.Now;
            testimoial.IsDeleted = false;
            testimoial.CustomerId = (int)Id;

            dbContext.Add(testimoial);
            dbContext.SaveChanges();

            return RedirectToAction("MyExamHistory");
        }
        public IActionResult EditAccount( ) {
            ViewBag.EmailCheck = DoesExamExist;
            var Id = HttpContext.Session.GetInt32("Id");
            var user = (from userdb in dbContext.Users
                        select new VMEditAccount()
                        {
                            Id=userdb.Id,
                            FirstName = userdb.FirstName,
                            LastName = userdb.LastName,
                            Email = userdb.Email,
                            CountryCode = userdb.CountryCode,
                            Image = userdb.Image,
                            Nationality = userdb.Nationality,
                            Phone = userdb.Phone,



                        }).FirstOrDefault(x=>x.Id==Id);
            return View(user);
            }
        [HttpPost]
        public IActionResult EditAccount(VMEditAccount obj) {

      
            var userCheck = dbContext.Users.FirstOrDefault(x=>x.Email==obj.Email);
            if (userCheck == null || cId==userCheck.Id)
            {
                DoesExamExist = false;
                ViewBag.EmailCheck = DoesExamExist;
                var Id = HttpContext.Session.GetInt32("Id");
                var User = dbContext.Users.FirstOrDefault(x => x.Id == Id);
                User.FirstName = obj.FirstName;
                User.LastName = obj.LastName;
                User.Email = obj.Email;
                User.CountryCode = obj.CountryCode;
                User.Image = obj.Image;
                User.Nationality = obj.Nationality;
                User.Phone = obj.Phone;
                User.UpdateDate = DateTime.Now;
                dbContext.SaveChanges();



                return RedirectToAction("Index", "Home");
            }
            else {
                DoesExamExist = true;
                ViewBag.EmailCheck = DoesExamExist;
                return View("EditAccount",obj);
            
            }
     
        
        }
        public async Task<IActionResult> Logout()
        {
            FlagError=false;
            
            HttpContext.Session.Remove("Id");
            HttpContext.Session.Remove("Type");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult LeavingEarly() {
          
            
            var examInfo = dbContext.Exams.Where(recored => recored.Id == HttpContext.Session.GetInt32("ExamId")).SingleOrDefault();
            int result = 0;
            Service1 service=new Service1();
            examIndexPointer = 0;
            int j = 0;
            foreach (var item in currentExam)
            {
                var examQuestion = dbContext.ExamQuestions.FirstOrDefault(x => x.ExamId == examInfo.Id
                && x.QuestionId == item.Id);
                if (examQuestion != null)
                {
                    examQuestion.AnswerBody = userAnswer.ElementAt(j);
                    examQuestion.AnswerId = 1;
                    if (userAnswer.ElementAt(j) == currentExam.ElementAt(j).CorrectAnswer
                        && !string.IsNullOrEmpty(userAnswer.ElementAt(j))
                        && !string.IsNullOrWhiteSpace(userAnswer.ElementAt(j)))
                    {
                        result++;
                        examQuestion.Mark = 1;
                    }
                    else
                    {
                        examQuestion.Mark = 0;
                    }
                    dbContext.Update(examQuestion);
                    dbContext.SaveChanges();
                    j++;
                }
            }
            examInfo.Score = result + "/" + currentExam.Count();
            dbContext.Update(examInfo);

            dbContext.SaveChanges();
            return RedirectToAction("MyExamHistory");


        }
       



    }
}
