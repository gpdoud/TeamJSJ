using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealJSJDatabase.Models;

namespace RealJSJDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {

        public static string NEW = "NEW";
        public static string MODIFIED = "MODIFIED";
        public static string APPROVED = "APPROVED";
        public static string REJECTED = "REJECTED";
        public static string REVIEW = "REVIEW";
        public static string PAID = "PAID";
        

        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Expenses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
        {
            return await _context.Expenses.Include(x => x.Employee).ToListAsync();
        }

        // GET: api/Expenses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetExpense(int id)
        {
            var expense = await _context.Expenses.Include(x => x.ExpenseLines)!
                                                .ThenInclude(x => x.Item)
                                            .Include(x => x.Employee)
                                            .SingleOrDefaultAsync(x => x.Id == id);

            if (expense == null)
            {
                return NotFound();
            }

            return expense;
        }

        [HttpGet("approved")]
        public async Task<ActionResult<IEnumerable<Expense>>> GetApprovedExpenses()
        {
            return await _context.Expenses.Include(x => x.Employee).Where(x => x.Status == APPROVED).ToListAsync();
        }

        [HttpGet("review")]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpensesInReview()
        {
            return await _context.Expenses.Include(x => x.Employee).Where(x => x.Status == REVIEW).ToListAsync();
        }


        // PUT: api/Expenses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpense(int id, Expense expense)
        {
            if (id != expense.Id)
            {
                return BadRequest();
            }

            _context.Entry(expense).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPut("pay/{expenseId}")]
        public async Task<IActionResult> PayExpense(int expenseId)
        {
            
            var exp = await _context.Expenses.FindAsync(expenseId);
            if (exp is null)
            {
                return NotFound();
            }
            if (exp.Status != APPROVED)
            {
                return BadRequest();
            }
          
            exp.Status = PAID;
            
            var empl = await _context.Employees.FindAsync(exp.EmployeeId);
            
            if (empl is null)
            {
                throw new Exception("Employee Id does not exist");
            }
            empl.ExpensesDue -= exp.Total;
            empl.ExpensesPaid += exp.Total;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveExpense(int id, Expense expense)
        {
            if (expense.Status == APPROVED)
            {
                return BadRequest();
            }
            expense.Status = APPROVED;
            var fred = await PutExpense(id, expense);
            await UpdateEmployeeExpenseDueAndPaid(expense.EmployeeId);
            return fred;
        }

        [HttpPut("reject/{id}")]
        public async Task<IActionResult> RejectExpense(int id, Expense expense)
        {
            expense.Status = REJECTED;
            var fred = await PutExpense(id, expense);
            await UpdateEmployeeExpenseDueAndPaid(expense.EmployeeId);
            return fred;
        }

        [HttpPut("review/{id}")]
        public async Task<IActionResult> ReviewExpense(int id, Expense expense)
        {
            var prevStatus = expense.Status;
            expense.Status = (expense.Total <= 75) ? APPROVED : REVIEW;
            var fred = await PutExpense(id, expense);
            await UpdateEmployeeExpenseDueAndPaid(expense.EmployeeId);
            return fred;
        }

        // POST: api/Expenses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Expense>> PostExpense(Expense expense)
        {
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExpense", new { id = expense.Id }, expense);
        }

        // DELETE: api/Expenses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }

        private async Task<IActionResult> UpdateEmployeeExpenseDueAndPaid(int employeeId)
        {
            var empX = await _context.Employees.FindAsync(employeeId);
            if(empX == null)
            {
                throw new Exception("Employee Id does not exist");
            }
            empX.ExpensesDue = (from e in _context.Expenses
                                join el in _context.ExpenseLines
                                    on e.Id equals el.ExpenseId
                                join i in _context.Items
                                   on el.ItemId equals i.Id
                                where e.Status == APPROVED && e.EmployeeId == empX.Id
                                select new
                                {
                                    Subtotal = el.Quantity * i.Price
                                }).Sum(x => x.Subtotal);

            empX.ExpensesPaid = (from e in _context.Expenses
                                 join el in _context.ExpenseLines
                                     on e.Id equals el.ExpenseId
                                 join i in _context.Items
                                    on el.ItemId equals i.Id
                                 where e.Status == PAID && e.EmployeeId == empX.Id
                                 select new
                                 {
                                     Subtotal = el.Quantity * i.Price
                                 }).Sum(x => x.Subtotal);

            await _context.SaveChangesAsync();
            return Ok();




        }

    }
}
