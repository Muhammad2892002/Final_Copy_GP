namespace TechQuestions.Services
{
    public class Service1
    {
        public int TurnMarkToInt(string marks)
        {
            if (marks == "")
            {
                return 0;

            }
            else
            {
                string var1 = marks.Split('/')[0];
                int var2;
               
                bool intMarks = int.TryParse(var1,out var2);
                return (var2);
            }






        }
        public bool iscorrect(int corectAnswerid, int AnswerId) {
            if (corectAnswerid == AnswerId)
            {
                return true;
            }
            else {
                return false;
            
            }
        
        }
    }
}
