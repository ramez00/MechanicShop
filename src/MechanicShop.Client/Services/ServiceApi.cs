using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
namespace MechanicShop.Client.Services;
using MechanicShop.Client.Models;
using MechanicShop.Contracts.Requests.Customers;
using MechanicShop.Contracts.Requests.WorkOrders;
using MechanicShop.Contracts.Responses;

public sealed class ServiceApi(HttpClient httpClient, TimeZoneService timeZoneService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly TimeZoneService _timeZoneService = timeZoneService;


    private readonly HttpClient _httpClient = httpClient;

    public Task<ApiResult<TResponse>> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(() => new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);

    public Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(() => CreateJsonRequest(HttpMethod.Post, requestUri, body), cancellationToken);

    public Task<ApiResult> PostAsync<TRequest>(string requestUri, TRequest body, CancellationToken cancellationToken = default) =>
        AsVoidResult(SendAsync<object?>(() => CreateJsonRequest(HttpMethod.Post, requestUri, body), cancellationToken));

    public Task<ApiResult> PostAsync(string requestUri, CancellationToken cancellationToken = default) =>
        AsVoidResult(SendAsync<object?>(() => new HttpRequestMessage(HttpMethod.Post, requestUri), cancellationToken));

    public Task<ApiResult<TResponse>> PutAsync<TRequest, TResponse>(string requestUri, TRequest body, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(() => CreateJsonRequest(HttpMethod.Put, requestUri, body), cancellationToken);

    public Task<ApiResult> PutAsync<TRequest>(string requestUri, TRequest body, CancellationToken cancellationToken = default) =>
        AsVoidResult(SendAsync<object?>(() => CreateJsonRequest(HttpMethod.Put, requestUri, body), cancellationToken));

    public Task<ApiResult> PutAsync(string requestUri, CancellationToken cancellationToken = default) =>
        AsVoidResult(SendAsync<object?>(() => new HttpRequestMessage(HttpMethod.Put, requestUri), cancellationToken));

    public Task<ApiResult> DeleteAsync(string requestUri, CancellationToken cancellationToken = default) =>
        AsVoidResult(SendAsync<object?>(() => new HttpRequestMessage(HttpMethod.Delete, requestUri), cancellationToken));

    public async Task<ApiResult<List<CustomerModel>>> GetCustomersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/customers");

            if (response.IsSuccessStatusCode)
            {
                var customers = await response.Content.ReadFromJsonAsync<List<CustomerModel>>();
                return ApiResult<List<CustomerModel>>.Success(customers ?? []);
            }

            return await HandleErrorResponseAsync<List<CustomerModel>>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<List<CustomerModel>>(ex, "Failed to retrieve customers");
        }
    }
    public async Task<ApiResult<CustomerModel>> CreateCustomerAsync(CreateCustomerRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/customers", request);

            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadFromJsonAsync<CustomerModel>();

                if (customer is null)
                {
                    return ApiResult<CustomerModel>.Failure("Customer response was null.");
                }

                return ApiResult<CustomerModel>.Success(customer);
            }

            return await HandleErrorResponseAsync<CustomerModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<CustomerModel>(ex, "Failed to create customer.");
        }
    }

    public async Task<ApiResult> RelocateWorkOrderAsync(Guid workOrderId, RelocateWorkOrderRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/WorkOrders/{workOrderId}/relocation", request);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to relocate work order timing for {workOrderId}");
        }
    }


    public async Task<ApiResult> UpdateWorkOrderStateAsync(Guid workOrderId, UpdateWorkOrderStateRequest statusUpdateRequest)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/WorkOrders/{workOrderId}/state", statusUpdateRequest);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to update work order state for {workOrderId}");
        }
    }

    public async Task<ApiResult> UpdateWorkOrderRepairTasksAsync(Guid workOrderId, ModifyRepairTaskRequest updateWorkOrderRepairTasks)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/WorkOrders/{workOrderId}/repair-task", updateWorkOrderRepairTasks);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to update work order repair tasks for {workOrderId}");
        }
    }

     public async Task<ApiResult> UpdateWorkOrderLaborAsync(Guid workOrderId, AssignLaborRequest laborUpdateRequest)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/WorkOrders/{workOrderId}/labor", laborUpdateRequest);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to update work order labor for {workOrderId}");
        }
    }

    public async Task<ApiResult> CreateWorkOrderAsync(CreateWorkOrderRequest workOrderRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/WorkOrders", workOrderRequest);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, "Failed to create work order");
        }
    }
     public async Task<ApiResult<CustomerModel>> GetCustomerAsync(Guid customerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/customers/{customerId}");

            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadFromJsonAsync<CustomerModel>();
                return ApiResult<CustomerModel>.Success(customer!);
            }

            return await HandleErrorResponseAsync<CustomerModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<CustomerModel>(ex, $"Failed to retrieve customer {customerId}");
        }
    }

    public async Task<ApiResult<List<VehicleModel>>> GetVehiclesByCustomerIdAsync(Guid customerId)
    {
        try
        {
            var customerResult = await GetCustomerAsync(customerId);

            if (!customerResult.IsSuccess)
            {
                return ApiResult<List<VehicleModel>>.Failure(
                    customerResult.ErrorMessage,
                    customerResult.ErrorDetail,
                    customerResult.StatusCode,
                    customerResult.ValidationErrors);
            }

            var vehicles = customerResult.Data?.Vehicles ?? [];
            return ApiResult<List<VehicleModel>>.Success(vehicles);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<List<VehicleModel>>(ex, $"Failed to retrieve vehicles for customer {customerId}");
        }
    }


     // Repair Task methods
    public async Task<ApiResult<List<RepairTaskModel>>> GetRepairTasksAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/repair-tasks");

            if (response.IsSuccessStatusCode)
            {
                var repairTasks = await response.Content.ReadFromJsonAsync<List<RepairTaskModel>>();
                return ApiResult<List<RepairTaskModel>>.Success(repairTasks ?? []);
            }

            return await HandleErrorResponseAsync<List<RepairTaskModel>>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<List<RepairTaskModel>>(ex, "Failed to retrieve repair tasks");
        }
    }


    public async Task<ApiResult> DeleteWorkOrderAsync(Guid workOrderId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/WorkOrders/{workOrderId}");

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to delete work order {workOrderId}");
        }
    }

     public async Task<ApiResult<ScheduleModel>> GetDailySchedule(DateOnly date, Guid? laborId = null)
    {
        try
        {
            var url = $"api/v1/workorders/schedule/{date:yyyy-MM-dd}";
            if (laborId.HasValue)
            {
                url += $"?laborId={laborId.Value}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var tz = await _timeZoneService.GetLocalTimeZoneAsync();

            request.Headers.Add("X-TimeZone", tz);

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None)
                                            .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var schedule = await response.Content.ReadFromJsonAsync<ScheduleModel>().ConfigureAwait(false);

                if (schedule is null)
                {
                    return ApiResult<ScheduleModel>.Failure("Schedule data is null");
                }

                try
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Montreal");

                    foreach (var slot in schedule.Spots.SelectMany(s => s.Slots))
                    {
                        slot.StartAt = TimeZoneInfo.ConvertTime(slot.StartAt, timeZone);
                        slot.EndAt = TimeZoneInfo.ConvertTime(slot.EndAt, timeZone);
                    }
                }
                catch (TimeZoneNotFoundException)
                {
                    return ApiResult<ScheduleModel>.Failure("Time zone 'America/Montreal' not found on this system.");
                }

                return ApiResult<ScheduleModel>.Success(schedule);
            }

            return await HandleErrorResponseAsync<ScheduleModel>(response).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<ScheduleModel>(ex, $"Failed to retrieve schedule for {date}")
                         .ConfigureAwait(false);
        }
    }

    public async Task<ApiResult<List<LaborModel>>> GetLaborsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/labors");

            if (response.IsSuccessStatusCode)
            {
                var labors = await response.Content.ReadFromJsonAsync<List<LaborModel>>();
                return ApiResult<List<LaborModel>>.Success(labors ?? []);
            }

            return await HandleErrorResponseAsync<List<LaborModel>>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<List<LaborModel>>(ex, "Failed to retrieve labors");
        }
    }


    public async Task<ApiResult<OperatingHoursResponse>> GetOperateHoursAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/settings/operating-hours");

            if (response.IsSuccessStatusCode)
            {
                var operateHours = await response.Content.ReadFromJsonAsync<OperatingHoursResponse>();

                return ApiResult<OperatingHoursResponse>.Success(operateHours!);
            }

            return await HandleErrorResponseAsync<OperatingHoursResponse>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<OperatingHoursResponse>(ex, "Failed to retrieve operating hours");
        }
    }

    public async Task<ApiResult> DeleteCustomerAsync(Guid customerId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/customers/{customerId}");

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to delete customer {customerId}");
        }
    }

     public async Task<ApiResult<CustomerModel>> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/customers/{customerId}", request);

            if (response.IsSuccessStatusCode)
            {
                var customer = await response.Content.ReadFromJsonAsync<CustomerModel>();
                return ApiResult<CustomerModel>.Success(customer!);
            }

            return await HandleErrorResponseAsync<CustomerModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<CustomerModel>(ex, $"Failed to update customer {customerId}");
        }
    }


    public async Task<ApiResult<byte[]>> GetInvoicePdfAsync(Guid invoiceId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/invoices/{invoiceId}/pdf");

            if (response.IsSuccessStatusCode)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                return ApiResult<byte[]>.Success(pdfBytes);
            }

            return await HandleErrorResponseAsync<byte[]>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<byte[]>(ex, $"Failed to retrieve PDF for invoice {invoiceId}");
        }
    }
    public async Task<ApiResult<byte[]>> GetBytesAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ToFailureAsync<byte[]>(response, cancellationToken);
            }

            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            return ApiResult<byte[]>.Success(data);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ApiResult<byte[]>.Failure(ex.Message);
        }
    }

    private static Task<ApiResult> HandleErrorResponseAsync(HttpResponseMessage response) =>
        HandleErrorResponseAsync<object>(response)
            .ContinueWith(t =>
                ApiResult.Failure(
                    t.Result.ErrorMessage,
                    t.Result.ErrorDetail,
                    t.Result.StatusCode,
                    t.Result.ValidationErrors));

    public async Task<ApiResult> SettleInvoice(Guid invoiceId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/v1/invoices/{invoiceId}/payments", null);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success();
            }

            return await HandleErrorResponseAsync(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync(ex, $"Failed to settle invoice {invoiceId}");
        }
    }
    public async Task<ApiResult<InvoiceModel>> GetInvoiceAsync(Guid invoiceId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/invoices/{invoiceId}");

            if (response.IsSuccessStatusCode)
            {
                var invoice = await response.Content.ReadFromJsonAsync<InvoiceModel>();
                return ApiResult<InvoiceModel>.Success(invoice!);
            }

            return await HandleErrorResponseAsync<InvoiceModel>(response);
        }
        catch (Exception ex)
        {
            return await HandleExceptionAsync<InvoiceModel>(ex, $"Failed to retrieve invoice {invoiceId}");
        }
    }

    private static Task<ApiResult<T>> HandleExceptionAsync<T>(Exception ex, string message) =>
      Task.FromResult(ex switch
      {
          HttpRequestException => ApiResult<T>.Failure($"Network error occurred. {message}"),
          TaskCanceledException => ApiResult<T>.Failure($"Request timed out. {message}"),
          _ => ApiResult<T>.Failure($"An unexpected error occurred. {message}")
      });

    private static Task<ApiResult> HandleExceptionAsync(Exception ex, string message) =>
        HandleExceptionAsync<object>(ex, message).ContinueWith(t =>
            ApiResult.Failure(
                t.Result.ErrorMessage,
                t.Result.ErrorDetail,
                t.Result.StatusCode,
                t.Result.ValidationErrors));
    private async Task<ApiResult<TResponse>> SendAsync<TResponse>(Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken)
    {
        try
        {
            using var request = requestFactory();
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await ToFailureAsync<TResponse>(response, cancellationToken);
            }

            if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
            {
                return ApiResult<TResponse>.Success(default!);
            }

            var data = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);

            return ApiResult<TResponse>.Success(data!);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ApiResult<TResponse>.Failure(ex.Message);
        }
    }

    private static async Task<ApiResult> AsVoidResult(Task<ApiResult<object?>> resultTask)
    {
        var result = await resultTask;

        return result.IsSuccess
            ? ApiResult.Success()
            : ApiResult.Failure(result.ErrorMessage, result.ErrorDetail, result.StatusCode, result.ValidationErrors);
    }

    private static async Task<ApiResult<TResponse>> ToFailureAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var validationProblem = await TryReadAsync<ValidationProblemDetailsDto>(response, cancellationToken);

            if (validationProblem?.Errors is { Count: > 0 })
            {
                return ApiResult<TResponse>.Failure(
                    validationProblem.Title ?? "Validation failed.",
                    validationProblem.Detail,
                    statusCode,
                    validationProblem.Errors);
            }
        }

        var problem = await TryReadAsync<ProblemDetailsDto>(response, cancellationToken);

        return ApiResult<TResponse>.Failure(
            problem?.Title ?? response.ReasonPhrase ?? "Request failed.",
            problem?.Detail,
            statusCode);
    }


    private static async Task<T?> TryReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken) where T : class
    {
        if (response.Content.Headers.ContentLength is null or 0)
        {
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static HttpRequestMessage CreateJsonRequest<TRequest>(HttpMethod method, string requestUri, TRequest body) =>
        new(method, requestUri) { Content = JsonContent.Create(body, options: JsonOptions) };


    private class ProblemDetailsDto
    {
        public string? Title { get; set; }

        public string? Detail { get; set; }

        public int? Status { get; set; }
    }

    private sealed class ValidationProblemDetailsDto : ProblemDetailsDto
    {
        public Dictionary<string, string[]>? Errors { get; set; }
    }

        private static string GetFriendlyErrorMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Invalid request. Please check your input and try again.",
            HttpStatusCode.Unauthorized => "You are not authorized to perform this action.",
            HttpStatusCode.Forbidden => "You don't have permission to perform this action.",
            HttpStatusCode.NotFound => "The requested resource was not found.",
            HttpStatusCode.Conflict => "The operation conflicts with the current state of the resource.",
            HttpStatusCode.UnprocessableEntity => "The request contains invalid data.",
            HttpStatusCode.InternalServerError => "A server error occurred. Please try again later.",
            HttpStatusCode.BadGateway => "Service temporarily unavailable. Please try again later.",
            HttpStatusCode.ServiceUnavailable => "Service temporarily unavailable. Please try again later.",
            HttpStatusCode.GatewayTimeout => "The request timed out. Please try again.",
            _ => "An error occurred while processing your request."
        };
    }

    private static async Task<ApiResult<T>> HandleErrorResponseAsync<T>(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();

        try
        {
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, options: new() { PropertyNameCaseInsensitive = true });

            if (problemDetails is not null)
            {
                return ApiResult<T>.Failure(
                    message: problemDetails.Title ?? "An error occurred",
                    detail: problemDetails.Detail ?? "Error",
                    statusCode: problemDetails.Status ?? (int)response.StatusCode,
                    validationErrors: problemDetails.Errors);
            }

            return ApiResult<T>.Failure(
                message: GetFriendlyErrorMessage(response.StatusCode),
                detail: content,
                statusCode: (int)response.StatusCode);
        }
        catch (JsonException)
        {
            return ApiResult<T>.Failure(
                message: GetFriendlyErrorMessage(response.StatusCode),
                detail: content,
                statusCode: (int)response.StatusCode);
        }
    }

}
