using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[Route("api/calendar")]
[ApiController]
public class CalendarController : ControllerBase
{
    private readonly EventStoreService _eventStoreService;
    private readonly ParkingStoreService _pargingStoreService;

    public CalendarController(EventStoreService eventStoreService, ParkingStoreService parkingStoreService)
    {
        _eventStoreService = eventStoreService;
        _pargingStoreService = parkingStoreService;
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

    [HttpGet("parkings")]
    public ActionResult<IEnumerable<EventData>> GetParkings()
    {
        var events = _pargingStoreService.GetAllParkings();
        return Ok(events);
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
        ParkingData parking = _pargingStoreService.GetParkingByName(name);
        if (parking == null)
        {
            return NotFound("Parking not found.");
        }

        _pargingStoreService.DeleteParking(parking);
        return Ok("Parking deleted successfully.");
    }
}
