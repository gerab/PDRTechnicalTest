using System;

namespace PDR.PatientBooking.Service.BookingServices.Responses
{
    public class PatientAppointmentResponse
    {
        public Guid Id { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public long DoctorId { get; set; }
    }
}