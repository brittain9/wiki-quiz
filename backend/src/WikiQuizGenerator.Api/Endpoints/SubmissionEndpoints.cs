using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Mappers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WikiQuizGenerator.Api;

// Deprecated: moved to UserEndpoints. Keeping file for clarity if referenced elsewhere.
public static class SubmissionEndpoints
{
    public static void MapSubmissionEndpoints(this WebApplication app)
    {
        // No-op; endpoints moved to UserEndpoints under /api/user
    }
}
