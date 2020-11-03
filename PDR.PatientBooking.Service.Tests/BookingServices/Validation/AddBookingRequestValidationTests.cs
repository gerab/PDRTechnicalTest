using System;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;

namespace PDR.PatientBooking.Service.Tests.BookingServices.Validation
{
    [TestFixture]
    public class AddBookingRequestValidationTests
    {
        private IFixture _fixture;

        private PatientBookingContext _context;

        private AddBookingRequestValidator _addBookingRequestValidator;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _fixture = new Fixture();

            //Prevent fixture from generating from entity circular references
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _addBookingRequestValidator = new AddBookingRequestValidator(
                _context
            );
        }

        private void SetupMockDefaults()
        {
        }

        static object[] _pastTimeCases =
        {
            new object[] { DateTime.Now.AddMinutes(-1) },
            new object[] { DateTime.Now.AddHours(-1) },
            new object[] { DateTime.Now.AddDays(-1) },
            new object[] { DateTime.Parse("1970-01-01") },
        };

        private static object[] _conflictedCases =
        {
            new object[] { DateTime.Today.AddDays(2).AddMinutes(15), DateTime.Today.AddDays(2).AddMinutes(45) }, // between
            new object[] { DateTime.Today.AddDays(2).AddHours(-1), DateTime.Today.AddDays(2).AddHours(2) }, // overlap all
            new object[] { DateTime.Today.AddDays(2).AddMinutes(15), DateTime.Today.AddDays(2).AddHours(2) }, // overlap end
            new object[] { DateTime.Today.AddDays(2).AddMinutes(-15), DateTime.Today.AddDays(2).AddMinutes(15) }, // overlap start
        };

        [Test]
        public void ValidateRequest_DoesNotFail()
        {
            // arrange
            var request = GetValidRequest();
            AddAnOrderToDb();

            // act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            // assert
            res.PassedValidation.Should().BeTrue();
            res.Errors.Should().BeEmpty();
        }

        [Test, TestCaseSource(nameof(_pastTimeCases))]
        public void ValidateRequest_DateIsInThePast_ReturnsFailedValidationResult(DateTime pastDate)
        {
            // arrange
            var request = GetValidRequest();
            request.StartTime = pastDate;

            // act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            // assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain(AddBookingRequestValidator.AppointmentStartTimeIsInThePastMessage);
        }

        [Test]
        public void ValidateRequest_StartDateIsAfterEndDate_ReturnsFailedValidationResult()
        {
            // arrange
            var request = GetValidRequest();
            request.StartTime = request.EndTime.AddMinutes(3);

            // act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            // assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain(AddBookingRequestValidator.AppointmentStartIsAfterEndMessage);
        }

        [Test, TestCaseSource(nameof(_conflictedCases))]
        public void ValidateRequest_DoctorHasAnAppointment_ReturnsFailedValidationResult(DateTime startTime, DateTime endTime)
        {
            // arrange
            var dbOrder = AddAnOrderToDb();
            var request = GetValidRequest();
            request.StartTime = startTime;
            request.EndTime = endTime;
            request.DoctorId = dbOrder.DoctorId;

            // act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            // assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain(AddBookingRequestValidator.DoctorHasAnAppointmentAlreadyMessage);
        }

        private AddBookingRequest GetValidRequest()
        {
            var request = _fixture.Create<AddBookingRequest>();
            request.StartTime = DateTime.Today.AddDays(1);
            request.EndTime = DateTime.Today.AddDays(1).AddHours(1);
            return request;
        }

        private Order AddAnOrderToDb()
        {
            var existingOrder = _fixture
                .Build<Order>()
                .With(x => x.StartTime, DateTime.Today.AddDays(2))
                .With(x => x.EndTime, DateTime.Today.AddDays(2).AddHours(1))
                .Create();

            _context.Add(existingOrder);
            _context.SaveChanges();

            return existingOrder;
        }
    }
}