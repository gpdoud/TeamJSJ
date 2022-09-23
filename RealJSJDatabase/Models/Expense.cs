using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealJSJDatabase.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [StringLength(80)]
        public string Description { get; set; }


        [StringLength(10)]
        public string Status { get; set; } = "NEW";


        [Column(TypeName = "Decimal (11,2)")]
        public decimal Total { get; set; } = 0;


        public int EmployeeId { get; set; }             //Fk
        public virtual Employee? Employee { get; set; }

        public virtual ICollection<ExpenseLine>? ExpenseLines { get; set; }


    }
}
