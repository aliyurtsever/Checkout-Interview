# Payment Gateway API - Design Considerations & Assumptions

This document outlines the key design considerations and assumptions made during the implementation of this payment gateway API as an offline take-home exercise.

## Key Design Considerations

### 1. Separation of Concerns

The solution is organized into distinct layers, each with a single responsibility:

- **Controllers**: Handle HTTP requests/responses, routing, and status code management
- **Validators**: Input validation using FluentValidation, separated from business logic
- **BankIntegration**: External API communication isolated in a dedicated folder
- **Repository**: Data persistence abstraction (currently in-memory, replaceable with database)

This separation ensures maintainability, testability, and allows independent development of each layer.

### 2. Dependency Injection & SOLID Principles

All dependencies are injected through constructor injection:

- `IPaymentsService` - Bank integration abstraction (Dependency Inversion Principle)
- `IValidator<CardPaymentRequest>` - Validation abstraction
- `IMapper` - Object mapping abstraction
- `PaymentsRepository` - Data access abstraction

This approach follows SOLID principles, particularly the Dependency Inversion Principle, making the code testable and loosely coupled.

### 3. Validation Strategy - FluentValidation

**FluentValidation** was chosen for input validation because:

- **Readability**: Declarative, fluent syntax makes validation rules easy to understand
- **Testability**: Each validation rule can be tested independently (60+ test cases)
- **Separation**: Validation logic is separated from business logic
- **Maintainability**: Rules are centralized and easy to modify
- **Extensibility**: Supports async validation and custom validators

All validation rules are comprehensively tested, covering edge cases like invalid card numbers, expired cards, invalid currencies, etc.

### 4. Object Mapping - AutoMapper

**AutoMapper** is used to:

- **Reduce Boilerplate**: Eliminates repetitive mapping code
- **Prevent Errors**: Reduces manual mapping mistakes
- **Centralize Logic**: All mappings defined in one place (`PaymentMappingProfile`)
- **Maintainability**: Model changes require updates in one location only

### 5. Error Handling Strategy

- **Validation Errors**: Return `Rejected` status without calling the bank (invalid input)
- **Bank Service Errors**: Logged and handled gracefully, payment marked as `Declined`
- **Exception Handling**: Try-catch blocks in critical paths (bank service calls)
- **Structured Logging**: All errors logged with context using `ILogger` for debugging

### 6. Testing Strategy

- **Unit Tests**: Fast, isolated tests for validators (60+ test cases covering all validation rules)
- **Integration Tests**: End-to-end tests using `WebApplicationFactory` to test the full application stack
- **Real Services**: Integration tests use real `PaymentsService` (no mocks) for realistic testing
- **Test Pyramid**: Many unit tests (fast), fewer integration tests (slower but comprehensive)

This approach ensures both correctness of individual components and the overall system behavior.

### 7. Configuration Management

- Bank service URL configurable via `appsettings.json`
- Environment-specific settings supported
- Configuration externalized for easy deployment across environments

### 8. Bank Integration Isolation

The `BankIntegration` folder isolates all external bank service dependencies:

- **Abstraction**: `IPaymentsService` interface defines the contract
- **Implementation**: `PaymentsService` handles HTTP communication
- **Isolation**: Changes to bank API don't affect domain logic
- **Testability**: Easy to mock for unit tests, real implementation for integration tests

### 9. Constants for Error Messages

Error messages are centralized in `Constants/ErrorMessages.cs`:

- **DRY Principle**: Single source of truth for error messages
- **Consistency**: Same error message used everywhere
- **Maintainability**: Easy to update messages

## Assumptions

### Business Logic Assumptions

1. **Payment Status Mapping**:
   - Bank returns `authorized: true` → Payment status is `Authorized`
   - Bank returns `authorized: false` → Payment status is `Declined`
   - Validation fails → Payment status is `Rejected` (no bank call made)
   - Bank returns 503 Service Unavailable → Payment is treated as `Declined` (bank rejection)

2. **Card Number Masking**:
   - Only last 4 digits are stored and returned (`CardNumberLastFour`)
   - Card number is masked in responses for security compliance

3. **Payment Storage**:
   - Only `Authorized` and `Declined` payments are stored in the repository
   - `Rejected` payments are not stored (invalid input, no bank interaction occurred)
   - This reduces storage requirements and maintains data quality

4. **Bank Simulator Behavior** (based on provided simulator):
   - Card number ending in odd digit (1, 3, 5, 7, 9) → Returns `authorized: true`
   - Card number ending in even digit (2, 4, 6, 8) → Returns `authorized: false`
   - Card number ending in 0 → Returns 503 Service Unavailable (treated as Declined)
   - Missing required fields → Returns 400 Bad Request (treated as Declined)

## Summary

This implementation prioritizes:

1. **Clean Architecture**: Separation of concerns, SOLID principles
2. **Testability**: Comprehensive test coverage (60+ unit tests, integration tests)
3. **Maintainability**: Clear structure, dependency injection, centralized configuration
4. **Production Readiness**: While simplified for assessment, architecture supports production enhancements

The solution demonstrates understanding of software engineering best practices while acknowledging the trade-offs made for an assessment context.
