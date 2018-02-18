using MSAWeb.Models;

namespace MSAWeb.Classes
{
    public sealed class DatabaseManager
    {
        private static readonly MSADbConnection instance = new MSADbConnection();

        private DatabaseManager() { }

        public static MSADbConnection Instance
        {
            get
            {
                return instance;
            }
        }
    }
}