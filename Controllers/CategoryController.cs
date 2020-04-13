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
    // Endpint = URL
    // http://localhost:5000/categories
    // https://localhost:5001/categories
    // https://meuapp.azurewesites.net/
    // Sempre a mesma rota, por padrao (REST)
    // Só muda o verbo
    [Route("v1/categories")]
    public class CategoryController : ControllerBase
    {
        [HttpGet] // Padrao, porém usar por boas práticas/expressividade
        [Route("")]
        [AllowAnonymous]
        [ResponseCache(VaryByHeader = "User-Agent", 
            Location = ResponseCacheLocation.Any,
            Duration = 30)]
        public async Task<ActionResult<List<Category>>> Get(
            [FromServices]DataContext context
        )
        {
            // AsNoTracking não traz infos adicionais e faz uma leitura rápida
            // ToList deve ser no final
            var categories = await context.Categories.AsNoTracking().ToListAsync();
            return Ok(categories);
            
        }

        [HttpGet] // Padrao, porém usar por boas práticas/expressividade
        [Route("{id:int}")] // Mapeamento / : = restrição 
        [AllowAnonymous]
        public async Task<ActionResult<Category>> GetById(
            int id,
            [FromServices]DataContext context
        )
        {
        var category = await context.Categories.AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Id == id);
        return Ok(category); 
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<List<Category>>> Post(
            [FromBody]Category model, // Busca a categoria no corpo da requisicao
            [FromServices]DataContext context // Busca o context dos servicos
        ) 

        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            
            try
            {
                // context.DbSet.Add(variável)
                context.Categories.Add(model);
                await context.SaveChangesAsync(); // Persistencia de dados. Gera ID automático
                return Ok(model);
            }
            catch (System.Exception)
            {
                
                return BadRequest(new { message = "Não foi possível criar a categoria"});
            }
            
        }

        [HttpPut] 
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<List<Category>>> Put(
            int id,
            [FromBody]Category model,
            [FromServices]DataContext context 
        )
        {
            // Verificar se o id informado é o mesmo do modelo
            if(model.Id != id)
                return NotFound(new { message = "Categoria não encontrada"});

            // Verifica se os dados sao validos
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Entry<Category>(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Esse registro já foi atualizado"});
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível atualizar sua categoria."});
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<List<Category>>> Delete(
            int id,
            [FromServices]DataContext context
        )
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if(category == null)
                return NotFound(new { message = "Categoria não encontrada."});
            
            try
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                return Ok(new { message = "Categoria removida com sucesso."});
            }
            catch (System.Exception)
            {
                
                return BadRequest(new { message = "Não foi possível remover a categoria selecionada."});
            }
        }
    }
}