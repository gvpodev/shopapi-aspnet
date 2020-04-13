using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Shop.Controllers
{
    [Route("products")]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public async Task<ActionResult<List<Product>>> Get(
            [FromServices]DataContext context
        )
        {
            var products = await context.Products
                            .Include(x => x.Category)
                            .AsNoTracking()
                            .ToListAsync();
            
            return Ok(products);
        }
        
        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetById(
            int id,
            [FromServices]DataContext context
        )
        {
            var product = await context
                            .Products
                            .Include(x => x.Category)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id == id);
            
            return Ok(product);
        }

        [HttpGet]
        [Route("categories/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> GetByCategory(
            int id,
            [FromServices]DataContext context
        )
        {
            var products = await context
                            .Products
                            .Include(x => x.Category)
                            .AsNoTracking()
                            .Where(x => x.CategoryId == id)
                            .ToListAsync();
            
            return Ok(products);
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> Post(
            [FromBody]Product model,
            [FromServices]DataContext context
        )
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                context.Products.Add(model);
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch (System.Exception)
            {
                
                return BadRequest(new { message = "Não foi possível criar a categoria."});
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> Put(
            int id,
            [FromBody]Product model,
            [FromServices]DataContext context
        )
        {
            if(model.Id != id)
                return BadRequest(new { message = "Produto não encontrado"});

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Entry<Product>(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch(DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Esse produto já foi atualizado"});
            }
            catch(Exception)
            {
                return BadRequest(new { message = "Não foi possível atualizar o produto"});
            }

        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> Delete(
            int id,
            [FromServices]DataContext context
        )
        {
            try{
                var product = context.Products.FirstOrDefault(x => x.Id == id);
                context.Products.Remove(product);
                await context.SaveChangesAsync();
                return Ok(new { message = "Produto removido com sucesso"});
            }
            catch(Exception)
            {
                return BadRequest(new { message = "Não foi possível remover o produto."});
            }
        }
    }
}