using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealJSJDatabase.Models {

    public class ExpenseLine {

        public int Id { get; set; }

        public int Quantity { get; set; } = 1;


        public int ExpenseId { get; set; }
        public virtual Expense? Expense { get; set; }

        public int ItemId { get; set; }
        public virtual Item? Item { get; set; }



    }
}
