namespace SafeVaultApi.DTOs

{
    public record LoginDto(
        string Username, 
        string Password
    );
}