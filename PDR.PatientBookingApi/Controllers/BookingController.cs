using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Enums;
using PDR.PatientBooking.Service.Exceptions;
using PDR.PatientBookingApi.Extensions;

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

        private static MyOrderResult UpdateLatestBooking(List<Order> bookings2, int i)
        {
            var latestBooking = new MyOrderResult
            {
                Id = bookings2[i].Id,
                DoctorId = bookings2[i].DoctorId,
                StartTime = bookings2[i].StartTime,
                EndTime = bookings2[i].EndTime,
                PatientId = bookings2[i].PatientId,
                SurgeryType = (int) bookings2[i].GetSurgeryType()
            };

            return latestBooking;
        }

        private class MyOrderResult
        {
            public Guid Id { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public long PatientId { get; set; }
            public long DoctorId { get; set; }
            public int SurgeryType { get; set; }
        }
    }
}