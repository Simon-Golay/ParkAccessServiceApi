using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[Route("api/calendar")]
[ApiController]
public class CalendarController : ControllerBase
{
    private readonly EventStoreService _eventStoreService;

    public CalendarController(EventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    [HttpGet("events")]
    public ActionResult<IEnumerable<EventData>> GetEvents()
    {
        var events = _eventStoreService.GetAllEvents();

        return Ok(events);
    }

    [HttpGet("nextevent")]
    public ActionResult<IEnumerable<EventData>> GetNextEvent()
    {
        var events = _eventStoreService.GetNextEvent();

        return Ok(events);
    }
}
