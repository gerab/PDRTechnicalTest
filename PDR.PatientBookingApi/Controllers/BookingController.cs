using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Enums;
using PDR.PatientBooking.Service.Exceptions;

namespace PDR.PatientBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("patient/{identificationNumber}/next")]
        public async Task<IActionResult> GetPatientNextAppointment(long identificationNumber)
        {
            try
            {
                var result = await _bookingService.GetPatientNextAppointmentAsync(identificationNumber);
                return Ok(result);
            }
            catch (DomainException ex) when (ex.ErrorCode == ErrorCode.NotFound)
            {
                return NotFound(ex.Message);
            }
            catch (DomainException ex) when (ex.ErrorCode == ErrorCode.BadRequest)
            {
                return BadRequest(ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddBooking(AddBookingRequest booking)
        {
            try
            {
                await _bookingService.AddBookingAsync(booking);
                return Ok(); // or NoContent();
            }
            catch(DomainException ex) when (ex.ErrorCode == ErrorCode.NotFound)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPatch("cancel")]
        public async Task<IActionResult> CancelBooking(Guid bookingId)
        {
            try
            {
                await _bookingService.CancelBookingAsync(bookingId);
                return Ok(); // or NoContent();
            }
            catch(DomainException ex) when (ex.ErrorCode == ErrorCode.NotFound)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
    }
}