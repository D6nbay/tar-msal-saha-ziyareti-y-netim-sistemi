namespace SahaZiyaret.Api.Dtos;

public class FarmerRequest{
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
}
//istemcinin bize gönderdiği şey, ID ve CreatedAt otomatik oluşturulduğu için burada yok.