﻿using Api.Infrastructure.DomainEvents;

namespace Api.Domain.Core;

[DomainEvent("FamilyMemberCreated")]
public record FamilyMemberCreatedDomainEvent(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime Birthdate,
    string? AspNetUserId) : IDomainEvent;