using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Service.Enums;
using PDR.PatientBooking.Service.Exceptions;
using PDR.PatientBooking.Service.Validation;

namespace PDR.PatientBooking.Service.Tests.BookingServices
{
    [TestFixture]
    public class BookingServiceTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private PatientBookingContext _context;
        private Mock<IAddBookingRequestValidator> _validator;

        private BookingService _bookingService;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            //Prevent fixture from generating circular references
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _validator = _mockRepository.Create<IAddBookingRequestValidator>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _bookingService = new BookingService(
                _context,
                _validator.Object
            );
        }

        private void SetupMockDefaults()
        {
            _validator.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>()))
                .Returns(new PdrValidationResult(true));
        }

        [Test]
        public async Task AddClinic_ValidatesRequest()
        {
            // arrange
            var request = _fixture.Create<AddBookingRequest>();

            // act
            await _bookingService.AddBookingAsync(request);

            // assert
            _validator.Verify(x => x.ValidateRequest(request), Times.Once);
        }

        [Test]
        public void AddBooking_ValidatorFails_ThrowsDomainException()
        {
            // arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());

            _validator.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>())).Returns(failedValidationResult);

            // act & assert
            _bookingService.Awaiting(x => x.AddBookingAsync(_fixture.Create<AddBookingRequest>())).Should()
                .Throw<DomainException>().And.ErrorCode.Should().Be(ErrorCode.BadRequest);
        }

        [Test]
        public async Task AddBooking_AddsBookingToContextWithGeneratedId()
        {
            // arrange
            var request = _fixture.Create<AddBookingRequest>();

            var expected = new Order
            {
                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
            };

            // act
            await _bookingService.AddBookingAsync(request);

            // assert
            _context.Order.Should().ContainEquivalentOf(expected, options => options.Excluding(clinic => clinic.Id));
        }

        [Test]
        public void GetPatientNextAppointment_NoAppointments_ThrowsDomainException()
        {
            // arrange
            var patientId = _fixture.Create<long>();

            // act
            var res = _bookingService.Awaiting(x => x.GetPatientNextAppointmentAsync(patientId))
                .Should().Throw<DomainException>().And;

            // assert
            res.ErrorCode.Should().Be(ErrorCode.NotFound);
            res.Message.Should().Be($"A patient with number '{patientId}' doesn't exist.");
        }

        [Test]
        public void GetPatientNextAppointment_NoFutureAppointments_ThrowsDomainException()
        {
            // arrange
            var dbOrder = AddOrderToDb(DateTime.Today.AddDays(-1));

            // act & assert
            var res = _bookingService.Awaiting(x => x.GetPatientNextAppointmentAsync(dbOrder.PatientId))
                .Should().Throw<DomainException>().And;

            // assert
            res.ErrorCode.Should().Be(ErrorCode.NotFound);
            res.Message.Should().Be("No future appointment were found for the patient.");
        }

        [Test]
        public void CancelBooking_AlreadyCancelledAppointments_ThrowsDomainException()
        {
            // arrange
            var dbOrder = AddOrderToDb(DateTime.Today.AddDays(1), isCancelled: true);

            // act
            var res = _bookingService.Awaiting(x => x.CancelBookingAsync(dbOrder.Id))
                .Should().Throw<DomainException>().And;

            // assert
            res.ErrorCode.Should().Be(ErrorCode.NotFound);
            res.Message.Should().Be($"A booking with id '{dbOrder.Id}' doesn't exist or already cancelled.");
        }

        [Test]
        public void CancelBooking_NoFutureAppointments_ThrowsDomainException()
        {
            // arrange
            var dbOrder = AddOrderToDb(DateTime.Today.AddDays(-1));

            // act
            var res = _bookingService.Awaiting(x => x.CancelBookingAsync(dbOrder.Id))
                .Should().Throw<DomainException>().And;

            // assert
            res.ErrorCode.Should().Be(ErrorCode.BadRequest);
            res.Message.Should().Be("The past booking cannot be updated.");
        }

        private Order AddOrderToDb(DateTime startDate, bool isCancelled = false)
        {
            var order = _fixture.Build<Order>()
                .With(x => x.StartTime, startDate)
                .With(x => x.IsCancelled, isCancelled)
                .Create();
            _context.Order.Add(order);
            _context.SaveChanges();
            return order;
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }
    }
}