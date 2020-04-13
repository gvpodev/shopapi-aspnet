using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Shop.Services;

namespace Shop.Controllers
{
    [Route("users")]
    public class UserController : Controller
    {   
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<User>>> Get([FromServices]DataContext context)
        {
            var users = await context
                        .Users
                        .AsNoTracking()
                        .ToListAsync();
            return users;
        }

        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Post(
            [FromBody]User model,
            [FromServices]DataContext context
        )
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                model.Role = "employee";
                context.Users.Add(model);
                await context.SaveChangesAsync();
                model.Password = "";
                return model;
            }
            catch (System.Exception)
            {
                return BadRequest(new { message = "Não foi possível criar o usuário"});
            }
        }
    
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Authenticate(
            [FromBody]User model,
            [FromServices]DataContext context
        )
        {
            var user = await context.Users
                .AsNoTracking()
                .Where(x => x.UserName == model.UserName 
                    && x.Password == model.Password)
                .FirstOrDefaultAsync();
            
            if(user == null)
                return NotFound(new { message = "Usuário ou senha inválidos"});
            
            var token = TokenService.GenerateToken(user);

            user.Password = "";
            return new 
            {
                user = user,
                token = token
            };
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> Put(
            [FromBody]User model,
            [FromServices]DataContext context,
            int id
        )
        {
            if(!ModelState.IsValid)
                return BadRequest(new { message = "Não foi possível atualizar o usuário"});

            if(model.Id != id)
                return NotFound(new { message = "Usuário não encontrando"});
            try
            {
                context.Entry(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok("Usuário atualizado com sucesso");
            }
            catch (System.Exception)
            {
                return BadRequest(new { message = "Não foi possível atualizar o usuário"});
            }
            
            
        }
    }
}