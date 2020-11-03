using PDR.PatientBooking.Data.Models;

namespace PDR.PatientBookingApi.Extensions
{
    public static class OrderExtensions
    {
        public static SurgeryType GetSurgeryType(this Order order)
        {
            return order.Patient.Clinic.SurgeryType;
        }
    }
}
