using Microsoft.AspNetCore.Mvc;
using Planora.Application.Dtos.BlockDtos;
using Planora.Application.Interfaces;

namespace Planora.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BlocksController : ControllerBase
{
    private readonly IBlockService _blockService;

    public BlocksController(IBlockService blockService)
    {
        _blockService = blockService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var blocks = _blockService.GetAll();
        return Ok(blocks);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var block = _blockService.GetById(id);
        if (block == null) return NotFound();
        return Ok(block);
    }

    [HttpGet("date/{date:datetime}")]
    public IActionResult GetByDate(DateTime date)
    {
        var blocks = _blockService.GetByDate(date);
        return Ok(blocks);
    }

    [HttpGet("range")]
    public IActionResult GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var blocks = _blockService.GetByDateRange(startDate, endDate);
        return Ok(blocks);
    }

    [HttpPost]
    public IActionResult Add(BlockAddDto blockAddDto)
    {
        try
        {
            _blockService.Add(blockAddDto);
            return Ok("Blok eklendi.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    public IActionResult Update(BlockUpdateDto blockUpdateDto)
    {
        try
        {
            _blockService.Update(blockUpdateDto);
            return Ok("Blok güncellendi.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/status")]
    public IActionResult UpdateStatus(int id, [FromBody] Planora.Entities.BlockStatus status)
    {
        try
        {
            _blockService.UpdateStatus(id, status);
            return Ok("Blok durumu güncellendi.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _blockService.Delete(id);
        return Ok("Blok silindi.");
    }
}