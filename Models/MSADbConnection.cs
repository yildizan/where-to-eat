namespace MSAWeb.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class MSADbConnection : DbContext
    {
        public MSADbConnection()
            : base("name=MSADbConnection")
        {
        }

        public virtual DbSet<Period> Periods { get; set; }
        public virtual DbSet<RatingHistory> RatingHistories { get; set; }
        public virtual DbSet<Restaurant> Restaurants { get; set; }
        public virtual DbSet<SuggestionHistory> SuggestionHistories { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
