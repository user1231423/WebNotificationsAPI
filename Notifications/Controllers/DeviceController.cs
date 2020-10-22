using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notifications.Data;
using Notifications.DTO;
using Notifications.Models;
using PoTT.Cloud.Data.Common;

namespace Notifications.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public DeviceController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var devices = await _context.Devices.ToListAsync();
                return Ok(devices);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            try
            {
                var device = await _context.Devices.FirstOrDefaultAsync(x => x.Id == id);
                return Ok(device);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(DeviceDTO createDevice)
        {
            try
            {
                var device = new Device()
                {
                    Name = createDevice.Name,
                    PushAuth = createDevice.PushAuth,
                    PushEndpoint = createDevice.PushEndpoint,
                    PushP256DH = createDevice.PushP256DH,
                    UserId = createDevice.UserId
                };

                if(_context.Devices.Any(x => x.PushAuth == device.PushAuth))
                {
                    return BadRequest("This device is already registered");
                }

                _context.Devices.Add(device);

                await _context.SaveChangesAsync();

                return Ok(device);

            }catch(Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("{id}")]
        public async Task<ActionResult> Update(int id, DeviceDTO updateDevice)
        {
            try
            {
                var old = await this._context.Devices.FirstOrDefaultAsync(x => x.Id == id);

                var updateDTO = new Device()
                {
                    Name = updateDevice.Name,
                    PushAuth = updateDevice.PushAuth,
                    PushEndpoint = updateDevice.PushEndpoint,
                    PushP256DH = updateDevice.PushP256DH,
                    UserId = updateDevice.UserId
                };

                var update = new Device();

                update.BuildUpdateObj(updateDTO, old);

                old.UpdateModifiedFields(update);

                this._context.Entry(old).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return Ok(old);

            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var device = await _context.Devices.FirstOrDefaultAsync(x => x.Id == id);

                _context.Devices.Remove(device);

                await _context.SaveChangesAsync();

                return Ok(device);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
