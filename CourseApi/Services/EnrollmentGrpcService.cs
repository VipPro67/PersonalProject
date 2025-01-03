using Grpc.Core;
using CourseApi.Protos;
using CourseApi.Repositories;

namespace CourseApi.Services
{
    public class EnrollmentGrpcService : Protos.EnrollmentService.EnrollmentServiceBase
    {
        private readonly IEnrollmentRepository _enrollmentRepository;

        public EnrollmentGrpcService(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public override async Task<CheckStudentEnrollmentResponse> CheckStudentEnrollment(
            CheckStudentEnrollmentRequest request, ServerCallContext context)
        {
            var isEnrolled = await _enrollmentRepository.IsStudentHasEnrollmentAsync(request.StudentId);
            return new CheckStudentEnrollmentResponse { IsEnrolled = isEnrolled };
        }
    }
}