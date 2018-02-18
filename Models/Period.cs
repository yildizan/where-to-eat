namespace MSAWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("msa.Periods")]
    public partial class Period
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Period()
        {
            RatingHistories = new HashSet<RatingHistory>();
            SuggestionHistories = new HashSet<SuggestionHistory>();
        }

        public long Id { get; set; }

        public int Length { get; set; }

        public int? Count { get; set; }

        public bool IsActive { get; set; }

        public bool IsPending { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RatingHistory> RatingHistories { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SuggestionHistory> SuggestionHistories { get; set; }
    }
}
