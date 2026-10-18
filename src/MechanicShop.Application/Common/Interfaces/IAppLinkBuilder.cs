namespace MechanicShop.Application.Common.Interfaces;

// Builds absolute links into the client app for use in emails.
public interface IAppLinkBuilder
{
    string EmailConfirmationLink(string userId, string token);
}
