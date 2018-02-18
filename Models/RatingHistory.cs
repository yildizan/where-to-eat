namespace MSAWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("msa.RatingHistory")]
    public partial class RatingHistory
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PeriodId { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long RestaurantId { get; set; }

        public int? Taste { get; set; }

        public int? Speed { get; set; }

        public int? Service { get; set; }

        public virtual Period Period { get; set; }

        public virtual Restaurant Restaurant { get; set; }

        public virtual User User { get; set; }
    }
}
