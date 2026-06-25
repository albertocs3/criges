using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Customers;

public static class CustomerErrors
{
    public static readonly AppError ValidationFailed = new(
        "CUSTOMERS.VALIDATION_FAILED",
        "The customer request is invalid.");

    public static readonly AppError TaxIdAlreadyExists = new(
        "CUSTOMERS.TAXID_ALREADY_EXISTS",
        "The customer tax id already exists.");
}
