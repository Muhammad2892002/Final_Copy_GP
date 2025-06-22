using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechQuestions.Entites;

namespace TechQuestions.Entites
{
    public class Exam : MainEntity
    {
        public string Title { get; set; }
        public DateTime ExamDate { get; set; }
        public DateTime ExamDeadLine { get; set; }
        public string JobTitle { get; set; }
        public string ExamLevel { get; set; }
        public int QuestionCountInEasyLevel { get; set; }
        public int QuestionCountInHardLevel { get; set; }
        public int QuestionCountInIntermidateLevel { get; set; }
        public int  CustomerId { get; set; }
        public string? Score { get; set; }
    }
}
