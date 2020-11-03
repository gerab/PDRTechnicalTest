using System;
using System.Linq;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class AddBookingRequestValidator : IAddBookingRequestValidator
    {
        private readonly PatientBookingContext _context;
        public const string AppointmentTimeIsInThePastMessage = "An appointment time must be in the future.";
        public const string AppointmentIsInThePastMessage = "An appointment start date must be less than an end date.";
        public const string DoctorHasAnAppointmentAlreadyMessage = "Doctor already has an appointment for a specified time frame.";

        public AddBookingRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(AddBookingRequest request)
        {
            var result = new PdrValidationResult(true);

            AppointmentIsInThePast(request, ref result);
            AppointmentEndIsAfterStart(request, ref result);
            DoctorAlreadyHasAnAppointment(request, ref result);

            return result;
        }

        private void AppointmentIsInThePast(AddBookingRequest request, ref PdrValidationResult result)
        {
            var isAppointmentTimeInThePast = request.StartTime <= DateTime.Now;

            if (isAppointmentTimeInThePast)
            {
                result.PassedValidation = false;
                result.Errors.Add(AppointmentTimeIsInThePastMessage);
            }
        }

        private void AppointmentEndIsAfterStart(AddBookingRequest request, ref PdrValidationResult result)
        {
            var isAppointmentEndNotAfterStart = request.EndTime <= request.StartTime;

            if (isAppointmentEndNotAfterStart)
            {
                result.PassedValidation = false;
                result.Errors.Add(AppointmentIsInThePastMessage);
            }
        }

        private void DoctorAlreadyHasAnAppointment(AddBookingRequest request, ref PdrValidationResult result)
        {
            var doctorAlreadyHasAnAppointment = _context.Order
                .Any(x => x.DoctorId == request.DoctorId &&
                        (x.StartTime <= request.StartTime && request.EndTime <= x.EndTime // between
                        || request.StartTime <= x.StartTime && x.EndTime <= request.EndTime // overlap all
                        || x.StartTime <= request.StartTime && x.EndTime <= request.EndTime // overlap end
                        || request.StartTime <= x.StartTime && request.EndTime <= x.EndTime) // overlap beginning
                );

            if (doctorAlreadyHasAnAppointment)
            {
                result.PassedValidation = false;
                result.Errors.Add(DoctorHasAnAppointmentAlreadyMessage);
            }
        }
    }
}