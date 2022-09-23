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
    public class ExpenseLinesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpenseLinesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ExpenseLines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExpenseLine>>> GetExpenseLines()
        {
            return await _context.ExpenseLines.ToListAsync();
        }

        // GET: api/ExpenseLines/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpenseLine>> GetExpenseLine(int id)
        {
            var expenseLine = await _context.ExpenseLines.FindAsync(id);

            if (expenseLine == null)
            {
                return NotFound();
            }

            return expenseLine;
        }

        // PUT: api/ExpenseLines/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpenseLine(int id, ExpenseLine expenseLine)
        {
            if (id != expenseLine.Id)
            {
                return BadRequest();
            }

            _context.Entry(expenseLine).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseLineExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            await RecalculateExpenseTotal(expenseLine.ExpenseId);
            return NoContent();
        }

        // POST: api/ExpenseLines
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ExpenseLine>> PostExpenseLine(ExpenseLine expenseLine)
        {
            _context.ExpenseLines.Add(expenseLine);
            await _context.SaveChangesAsync();
            await RecalculateExpenseTotal(expenseLine.ExpenseId);
            return CreatedAtAction("GetExpenseLine", new { id = expenseLine.Id }, expenseLine);
        }

        // DELETE: api/ExpenseLines/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpenseLine(int id)
        {
            var expenseLine = await _context.ExpenseLines.FindAsync(id);
            if (expenseLine == null)
            {
                return NotFound();
            }

            _context.ExpenseLines.Remove(expenseLine);
            await _context.SaveChangesAsync();
            await RecalculateExpenseTotal(expenseLine.ExpenseId);
            return NoContent();
        }

        private bool ExpenseLineExists(int id)
        {
            return _context.ExpenseLines.Any(e => e.Id == id);
        }

        //PRIVATE: RecalculateExpenseTotal(expenseId) Integrated into the methods of expenseline; not available for calling outside the class

        private async Task<IActionResult> RecalculateExpenseTotal(int expenseId) {
            var expense = await _context.Expenses.FindAsync(expenseId);

            if (expense == null) {
                throw new Exception(
                    "Expense does not exist.");
                }

            expense.Total = (from el in _context.ExpenseLines
                             join i in _context.Items
                                 on el.ItemId equals i.Id
                             where expenseId == el.ExpenseId
                             select new {
                                 ProdTotal = el.Quantity * i.Price
                             }).Sum(x => x.ProdTotal);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
