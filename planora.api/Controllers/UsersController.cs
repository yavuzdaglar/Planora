using Microsoft.AspNetCore.Mvc;
using Planora.Application.Dtos.UserDtos;
using Planora.Application.Interfaces;

namespace Planora.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var users = _userService.GetAll();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var user = _userService.GetById(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public IActionResult Add(UserAddDto userAddDto)
    {
        _userService.Add(userAddDto);
        return Ok("Kullanıcı eklendi.");
    }

    [HttpPut]
    public IActionResult Update(UserUpdateDto userUpdateDto)
    {
        _userService.Update(userUpdateDto);
        return Ok("Kullanıcı güncellendi.");
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _userService.Delete(id);
        return Ok("Kullanıcı silindi.");
    }
}