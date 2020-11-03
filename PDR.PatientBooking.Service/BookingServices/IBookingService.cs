using System;
using System.Threading.Tasks;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;

namespace PDR.PatientBooking.Service.BookingServices
{
    public interface IBookingService
    {
        Task<PatientAppointmentResponse> GetPatientNextAppointmentAsync(long identificationNumber);

        Task AddBookingAsync(AddBookingRequest request);

        Task CancelBookingAsync(Guid bookingId);
    }
}