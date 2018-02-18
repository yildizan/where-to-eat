using MSAWeb.Models;

namespace MSAWeb.Classes
{
    public class Comparer
    {
        public Restaurant Item;
        public double Total = 0;
        public double Divider = 0;
        public double Average
        {
            get { return Total / Divider; }
        }
    }
}