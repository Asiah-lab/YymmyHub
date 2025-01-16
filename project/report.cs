using Microsoft.EntityFrameworkCore;
namespace project
{
    [Keyless]
    public class report
    {
        public string custname { get; set; }
        public int total { get; set; }
    }
}