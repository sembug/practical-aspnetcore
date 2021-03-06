using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System;
using Microsoft.Extensions.Hosting;

namespace PracticalAspNetCore
{
    public class JwtIssuerOptions
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public SigningCredentials SigningCredentials { get; set; }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = "SimpleServer";
                options.Audience = "http://localhost";
                options.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes("12345678901234567890")), SecurityAlgorithms.HmacSha256);

            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }

    public class HomeController : Controller
    {
        readonly IOptions<JwtIssuerOptions> _options;
        public HomeController(IOptions<JwtIssuerOptions> options)
        {
            _options = options;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return new ContentResult
            {
                Content =
    $@"<html>
    <body>
    <form action=""Home/Jwt"" method=""post"">
        <button type=""submit"">Get Token</button>
    </form>
    </body>
</html>",
                ContentType = "text/html"
            };
        }

        [HttpPost]
        public ActionResult Jwt()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Anne"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var option = _options.Value;

            var token = new JwtSecurityToken
            (
                issuer: option.Issuer,
                audience: option.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: option.SigningCredentials
            );

            var outputToken = new JwtSecurityTokenHandler().WriteToken(token);
            var content =
            $@"
<html>
<body>
    <strong>Content</strong>: {token}
    <br/><br/>
    <strong>Encoded Token</strong>: {outputToken}

    <hr />
    Copy the encoded token here to get the content of the token back
    <form action=""/Home/DecodeJwt"" method=""post"">
        <input type=""text"" name=""token"" />
        <button type=""submit"">Decode Token</button>
    </form>
</body>
</html>";

            return new ContentResult
            {
                Content = content,
                ContentType = "text/html"
            };
        }

        [HttpPost]
        public ActionResult DecodeJwt([FromForm] string token)
        {
            var jwt = new JwtSecurityToken(token);

            var content = $@"<html>
<body>
    <strong>Content</strong>: {jwt}
</body>
</html>";
            return new ContentResult
            {
                Content = content,
                ContentType = "text/html"
            };
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseStartup<Startup>()
                );
    }
}