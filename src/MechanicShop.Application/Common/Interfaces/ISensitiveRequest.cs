namespace MechanicShop.Application.Common.Interfaces;

// Marks a request that carries secrets (e.g. passwords). Pipeline behaviors log only its name, never its contents.
public interface ISensitiveRequest;
