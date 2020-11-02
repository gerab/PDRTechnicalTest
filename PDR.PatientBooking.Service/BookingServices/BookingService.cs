using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Service.Exceptions;

namespace PDR.PatientBooking.Service.BookingServices
{
    public class BookingService : IBookingService
    {
        private readonly PatientBookingContext _context;
        private readonly IAddBookingRequestValidator _validator;

        public BookingService(PatientBookingContext context, IAddBookingRequestValidator validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<PatientAppointmentResponse> GetPatientNextAppointmentAsync(long identificationNumber)
        {
            // todo: add validation
            var bookings = await _context.Order.OrderBy(x => x.StartTime).ToListAsync();

            if (bookings.All(x => x.Patient.Id != identificationNumber))
            {
                // return StatusCode(502) seems to be not correct
                // notify clients to expect correct http status code
                throw DomainException.NotFound($"A patient with number '{identificationNumber}' doesn't exist.");
            }

            var bookings2 = bookings.Where(x => x.PatientId == identificationNumber).ToList();

            if (!bookings2.Any(x => x.StartTime > DateTime.Now))
            {
                // return StatusCode(502) seems to be not correct
                throw DomainException.NotFound("No future appointment were found for the patient.");
            }

            var bookings3 = bookings2.Where(x => x.StartTime > DateTime.Now).ToList();

            return new PatientAppointmentResponse
            {
                Id = bookings3.First().Id,
                DoctorId = bookings3.First().DoctorId,
                StartTime = bookings3.First().StartTime,
                EndTime = bookings3.First().EndTime
            };
        }

        public Task AddBookingAsync(AddBookingRequest request)
        {
            // todo: add validation
            /*
             var bookingId = new Guid();
            var bookingStartTime = booking.StartTime;
            var bookingEndTime = booking.EndTime;
            var bookingPatientId = booking.PatientId;
            var bookingPatient = _context.Patient.FirstOrDefault(x => x.Id == booking.PatientId);
            var bookingDoctorId = booking.DoctorId;
            var bookingDoctor = _context.Doctor.FirstOrDefault(x => x.Id == booking.DoctorId);
            var bookingSurgeryType = _context.Patient.FirstOrDefault(x => x.Id == bookingPatientId)?.Clinic.SurgeryType ?? 0;

            var myBooking = new Order
            {
                Id = bookingId,
                StartTime = bookingStartTime,
                EndTime = bookingEndTime,
                PatientId = bookingPatientId,
                DoctorId = bookingDoctorId,
                Patient = bookingPatient,
                Doctor = bookingDoctor,
                SurgeryType = (int)bookingSurgeryType
            };

            await _context.Order.AddAsync(myBooking);
            await _context.SaveChangesAsync();

            return StatusCode(200);
             */

            throw new NotImplementedException();
        }
    }
}