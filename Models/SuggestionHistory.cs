namespace MSAWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("msa.SuggestionHistory")]
    public partial class SuggestionHistory
    {
        public long Id { get; set; }

        public long RestaurantId { get; set; }

        public long PeriodId { get; set; }

        public int? OrderNo { get; set; }

        public virtual Period Period { get; set; }

        public virtual Restaurant Restaurant { get; set; }
    }
}
