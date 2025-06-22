
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechQuestions.Entites
{
    public class ExamQuestion : MainEntity
    {
        public int ExamId { get; set; }
        public int QuestionId { get; set; }
        public int? AnswerId { get; set; }
        public string AnswerBody { get; set; }
        public bool IsCorrect { get; set; }
        public float Mark { get; set; }
    }
}
