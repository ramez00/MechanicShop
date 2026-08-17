using MechanicShop.Application.Features.Customers.Dtos;

namespace MechanicShop.Application.customers.Mappers;

public static class CustomerMapper
{
    public static CustomerDto ToDto(this Customer customer)
    {
        return new CustomerDto(
            customer.Id,
            customer.Name!,
            customer.PhoneNumber!,
            customer.Email!,
            customer.Cars?.Select(car => car.ToDto()).ToList()
        );
    }

    public static List<CustomerDto> ToDtoList(this IEnumerable<Customer> customers)
    {
        return customers.Select(customer => customer.ToDto()).ToList();
    }
}