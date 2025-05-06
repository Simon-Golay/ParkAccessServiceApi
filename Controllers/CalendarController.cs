using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkAccessServiceApi.Class;

[Authorize(Policy = "RequireParkingManagerRole")]
[Route("api/calendar")]
[ApiController]
public class CalendarController : ControllerBase
{
    private readonly EventStoreService _eventStoreService;
    private readonly ParkingStoreService _pargingStoreService;
    private readonly HistoryStoreService _historyStoreService;

    public CalendarController(EventStoreService eventStoreService, ParkingStoreService parkingStoreService, HistoryStoreService historyStoreService)
    {
        _eventStoreService = eventStoreService;
        _pargingStoreService = parkingStoreService;
        _historyStoreService = historyStoreService;
    }

    [HttpGet("events")]
    public ActionResult<IEnumerable<EventData>> GetEvents()
    {
        var events = _eventStoreService.GetAllEvents();

        return Ok(events);
    }

    [HttpGet("parkings")]
    public ActionResult<IEnumerable<EventData>> GetParkings()
    {
        var parkings = _pargingStoreService.GetAllParkings();
        return Ok(parkings);
    }

    [HttpGet("history")]
    public ActionResult<IEnumerable<History>> GetHistory()
    {
        var history = _historyStoreService.GetHistory();
        return Ok(history);
    }

    [HttpPost("addevent")]
    public ActionResult AddParking([FromBody] EventData newEvent)
    {
        _eventStoreService.AddNewEvent(newEvent);
        return Ok("Event added successfully.");
    }

    [HttpPost("addparking")]
    public ActionResult AddParking([FromBody] ParkingData newParking)
    {
        _pargingStoreService.AddNewParking(newParking);
        return Ok("Parking added successfully.");
    }

    [HttpPost("addhistory")]
    public async Task<IActionResult> AddHistory([FromBody] string description)
    {
        await _historyStoreService.AddNewHistory(description);
        return Ok("History added successfully.");
    }

    [HttpDelete("deleteevent/{name}")]
    public ActionResult DeleteEvent(string name)
    {
        var eventToDelete = _eventStoreService.GetEventByName(name);
        if (eventToDelete == null)
        {
            return NotFound("Event not found.");
        }

        _eventStoreService.DeleteEvent(eventToDelete);
        return Ok("Event deleted successfully.");
    }

    [HttpDelete("deleteparking/{name}")]
    public ActionResult DeleteParking(string name)
    {
        ParkingData? parking = _pargingStoreService.GetParkingByName(name);
        if (parking == null)
        {
            return NotFound("Parking not found.");
        }

        _pargingStoreService.DeleteParking(parking);
        return Ok("Parking deleted successfully.");
    }

    [HttpDelete("deletehistory")]
    public ActionResult DeleteHistory()
    {
        _historyStoreService.DeleteHistory();
        return Ok("History deleted successfully");
    }
}
