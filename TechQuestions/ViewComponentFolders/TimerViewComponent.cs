

using Microsoft.AspNetCore.Mvc;

namespace TechQuestions.ViewComponentFolder
{
    public class TimerViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(DateTime endTime) {
            TimeSpan duration=endTime-DateTime.Now;
            Console.WriteLine($"The Time is {duration}");
            return View("Timer",duration);
        
        
        }
    }
}
