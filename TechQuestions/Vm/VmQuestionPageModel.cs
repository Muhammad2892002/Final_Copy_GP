using TechQuestions.DTOs;
using TechQuestions.Entites;

namespace TechQuestions.Vm
{
    public class VmQuestionPageModel
    {
        public List<QuestionDTO> Questions { get; set; }
        public List<JobTitle> JobTitles { get; set; }
        public VmFilter Filter { get; set; }
    }
}
