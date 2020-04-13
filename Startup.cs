using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shop.Data;
using System.Text;
using Shop;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.ResponseCompression;
using System.Linq;
using Microsoft.OpenApi.Models;

namespace Shop
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) // Servicoes aspnetcore e externos que a apl vai utilizar
        {
            services.AddCors(); 
            // Comprime o JSON em ZIP
            services.AddResponseCompression(options => 
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults
                                    .MimeTypes
                                    .Concat(new[] { "application/json"});
            });
            // services.AddResponseCaching();
            services.AddControllers();

            // Criacao da chave de autenticacao
            var key = Encoding.ASCII.GetBytes(Settings.Secret);
            services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }

            ).AddJwtBearer(x => 
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                }
            );



            /* services.AddDbContext<DataContext>(
                opt => opt.UseSqlServer(Configuration
                .GetConnectionString("connectionString")));
            */

            services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Database"));

            services.AddSwaggerGen(c => c
                        .SwaggerDoc("v1", new OpenApiInfo 
                        {
                            Title = "Shop Api",
                            Version = "v1"
                        }));

            //
            
            //

            // Dependecy Injection - Add Scopped usa o Context em memoria
            // Um datacontext por requisicao
            // Desliga a conexao quando acaba a requisicao
             

            // AddTransient = Gera um data context novo sempre que requisitado
            // AddSingleton = Cria uma instancia do Context por aplicacao
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline. 
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) // Como e opcoes do servicoes utilizaremos
        {
            if (env.IsDevelopment()) // Verifica se o ambiente é de dev
            {
                app.UseDeveloperExceptionPage();
            }

            // Forca a API responder ao método HTTPS
            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c => c
                            .SwaggerEndpoint("/swagger/v1/swagger.json", "Shop API V1"));
            // Padrao de rotas do ASPNET MVC
            app.UseRouting();
            // Permite chamadas do localhost em tempo de dev
            app.UseCors(
                x => x
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            app.UseAuthentication();
            app.UseAuthorization();

            // Mapeamento de endpoints (URL / Como o usuário acessa o servidor (endpoints))
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
