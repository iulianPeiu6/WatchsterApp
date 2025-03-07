﻿using MediatR;
using Watchster.Application.Models;

namespace Watchster.Application.Features.Commands
{
    public class GenerateAndSaveResetPasswordCodeCommand : IRequest<ResetPasswordCodeResult>
    {
        public string Email { get; set; }
        public string Endpoint { get; set; }
    }
}
