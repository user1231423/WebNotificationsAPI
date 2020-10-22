using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Notifications.Data;
using Notifications.DTO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using WebPush;

namespace Notifications.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public NotificationsController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpGet("Keys")]
        public async Task<ActionResult> GetApplicationServerKey()
        {
            try
            {
                var keys = WebPush.VapidHelper.GenerateVapidKeys();

                return Ok(new { ServerKey = ConfigurationManager.AppSettings["publicKey"], keys.PublicKey, keys.PrivateKey });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("{userId}")]
        public ActionResult Send(int userId,[FromBody] PayloadDTO payloadDTO)
        {
            try
            {
                var devices = _context.Devices.Where(x => x.UserId == userId).ToList();

                Microsoft.Extensions.Primitives.StringValues payload = JsonConvert.SerializeObject(payloadDTO, Formatting.None, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                devices.ForEach(device =>
                {
                    string vapidPublicKey = ConfigurationManager.AppSettings["publicKey"];
                    string vapidPrivateKey = ConfigurationManager.AppSettings["privateKey"];

                    var pushSubscription = new PushSubscription(device.PushEndpoint, device.PushP256DH, device.PushAuth);
                    var vapidDetails = new VapidDetails("mailto:" + ConfigurationManager.AppSettings["subject"], vapidPublicKey, vapidPrivateKey);

                    var webPushClient = new WebPushClient();

                    try
                    {
                        webPushClient.SendNotification(pushSubscription, payload, vapidDetails);
                    }
                    catch (Exception)
                    {
                        
                    }

                });

                return Ok();

            }
            catch (Exception e) {
                return BadRequest(e.Message.ToString());
            }
        }
    }
}
