using System.IO;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos;
using api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    /// <summary>
    /// Controller used for marketing stuff
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MarketingController : ControllerBase
    {
        private readonly Db _context;
        private readonly IEmailSender _emailSender;

        private readonly IHostingEnvironment _environment;

        /// <summary>
        /// Constructor
        /// </summary>
        public MarketingController(Db context, IEmailSender emailSender, IHostingEnvironment environment)
        {
            _context = context;
            _emailSender = emailSender;
            _environment = environment;
        }

        /// <summary>
        /// Operation used to register a user for the newsletter
        /// </summary>
        /// <param name="clientDto"></param>
        [Route("newsletter")]
        [HttpPost]
        public async Task<ActionResult> Newsletter([FromBody] ProductClientDto clientDto)
        {
            if (string.IsNullOrWhiteSpace(clientDto.email))
            {
                return BadRequest("Must specify at least the email address");
            }
            ProductClient prodCli = _context.ProductClients.FirstOrDefault(pc => pc.Email == clientDto.email);
            if (prodCli != null)
            {
                if (prodCli.IsSubscribed == false)
                {
                    prodCli.IsSubscribed = true;
                    _context.SaveChanges();
                }
            }
            else
            {
                string email = clientDto.email;
                string userName = string.IsNullOrWhiteSpace(clientDto.name) ? email : clientDto.name;
                prodCli = new ProductClient() { Email = email, Name = userName, IsSubscribed = clientDto.isSubscribed };
                _context.ProductClients.Add(prodCli);
                _context.SaveChanges();
            }

            string path = Path.Combine(_environment.WebRootPath, "mail/newsletter_subscr.html");
            string newsletterMailContent = System.IO.File.ReadAllText(path);

            await _emailSender.SendEmailAsync(
                    clientDto.email,
                    "Thanks for joining the PlantHydra community",
                    newsletterMailContent);
            return Ok("ok");
        }

        /// <summary>
        /// Action used to unsubscribe an existing user
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [Route("unsubscribe")]
        [HttpPost]
        public ActionResult Unsubscribe([FromBody] string email)
        {
            ProductClient prodCli = _context.ProductClients.FirstOrDefault(pc => pc.Email == email);
            if (prodCli == null)
            {
                return BadRequest("The email address was not in our database");
            }

            prodCli.IsSubscribed = false;
            _context.SaveChanges();
            return Ok();
        }
    }
}