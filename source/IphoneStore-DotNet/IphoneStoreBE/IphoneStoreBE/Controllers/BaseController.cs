using IphoneStoreBE.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IphoneStoreBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected IActionResult HandleResponse(ResponseResult result)
        {
            return result.Success ? Ok(result) : BadRequest(result);
        }

        protected IActionResult HandleResponse<T>(ResponseResult<T> result)
        {
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
