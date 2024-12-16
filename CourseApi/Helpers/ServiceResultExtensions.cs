using Microsoft.AspNetCore.Mvc;

namespace CourseApi.Helpers
{
    public static class ServiceResultExtensions
    {
        public static IActionResult ToActionResult(this ControllerBase controller, ServiceResult result)
        {
            switch (result.Type)
            {
                case ResultType.Ok:
                    {
                        return controller.Ok(new SuccessResponse(200, result.Message, result.Data));
                    }
                case ResultType.BadRequest:
                    {
                        return controller.BadRequest(new ErrorResponse(400, result.Message, result.Data));
                    }
                case ResultType.NotFound:
                    {
                        return controller.NotFound(new ErrorResponse(404, result.Message, result.Data));
                    }
                case ResultType.Unauthorized:
                    {
                        return controller.Unauthorized(new ErrorResponse(401, result.Message, result.Data));
                    }
                case ResultType.InternalServerError:
                    {
                        return controller.StatusCode(500, new ErrorResponse(500, result.Message, result.Data));
                    }
                default:
                    {
                        return controller.StatusCode(500, new ErrorResponse(500, "An unexpected error occurred", null));
                    }
            }

        }
    }
}