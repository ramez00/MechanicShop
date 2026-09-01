using System.Net.Mail;
using MechanicShop.Domain.Common;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.Customers.cars;

namespace MechanicShop.Domain.Customers;

public class Customer : AuditableEntity
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    private readonly List<Car> _cars = [];
    public IEnumerable<Car> Cars => _cars.AsReadOnly();

    private Customer() { } // For EF Core

    private Customer(Guid id, string name, string email, string phoneNumber,List<Car> cars) : base(id)
    {
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        _cars = cars;
    }

    public static Result<Customer> Create(Guid id, string name, string email, string phoneNumber, List<Car> cars)
    {
        if (string.IsNullOrWhiteSpace(name))
            return CustomerErrors.NameRequired;

         if (string.IsNullOrWhiteSpace(email))
            return CustomerErrors.EmailRequired;

        try
        {
            _ = new MailAddress(email);
        }       
        catch
        {
            return CustomerErrors.EmailInvalid;
        }

        return new Customer(id, name, email, phoneNumber, cars);
    }

    public Result<Updated> Update(string name, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            return CustomerErrors.NameRequired;

        if (string.IsNullOrWhiteSpace(email))
            return CustomerErrors.EmailRequired;

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            return CustomerErrors.EmailInvalid;
        }

        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;

        return Result.updated;
    }


    public Result<Updated> UpsertParts(List<Car> incomingCar)
    {
        
        _cars.RemoveAll(existingCar => incomingCar.All(c => c.Id != existingCar.Id));

        foreach (var incoming in incomingCar)
        {
            var existingCar = _cars.FirstOrDefault(c => c.Id == incoming.Id);

            if (existingCar is null)
                _cars.Add(incoming);     
            else
            {
                var updatedCarResult = existingCar.Update(incoming.Make, incoming.Model, incoming.Year, incoming.LicensePlate);
                
                if (updatedCarResult.IsError)
                    return updatedCarResult.Errors!;
            }
        }

        return Result.updated;
    }

}