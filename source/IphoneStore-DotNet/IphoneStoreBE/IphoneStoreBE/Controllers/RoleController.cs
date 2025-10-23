using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IphoneStoreBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IphoneStoreContext _context;

        public RoleController(IphoneStoreContext context)
        {
            _context = context;
        }

        // GET: api/role
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .Select(r => new RoleDto
                    {
                        Id = r.Id,
                        RoleName = r.RoleName
                    })
                    .ToListAsync();

                Console.WriteLine($"✅ Returning {roles.Count} roles");
                return Ok(ResponseResult<List<RoleDto>>.SuccessResult(roles, "Lấy danh sách roles thành công"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading roles: {ex.Message}");
                return BadRequest(ResponseResult.Fail($"Lỗi: {ex.Message}"));
            }
        }
    }

    // DTO để trả về cho frontend
    public class RoleDto
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}