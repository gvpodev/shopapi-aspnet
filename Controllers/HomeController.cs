using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shop.Data;
using Shop.Models;


// Popular o banco no modo de dev
namespace Shop.Controllers
{
    [Route("v1")]
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("")]
        public async Task<ActionResult<dynamic>> Get(
            [FromServices]DataContext context
        )
        {
            var employee = new User { Id = 1, UserName = "emp1", Password = "emp1", Role = "employee" };
            var manager = new User { Id = 2, UserName = "man1", Password = "man1", Role = "manager" };
            var category = new Category { Id = 1, Title = "Informática"};
            var product = new Product { Id = 1, Category = category, Title = "Mouse", Price = 299, Description = "Mouse gamer"};
            
            context.Users.Add(employee);
            context.Users.Add(manager);
            context.Categories.Add(category);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            return Ok(new {
                message = "Dados configurados"
            });
        }
    }
}
