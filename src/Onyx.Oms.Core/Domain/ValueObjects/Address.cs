namespace Onyx.Oms.Core.Domain.ValueObjects;

public record Address(
    string Street,
    string City,
    string District,
    string State,
    string PostalCode,
    string Country)
{
    public bool IsEmpty => 
        string.IsNullOrWhiteSpace(Street) && 
        string.IsNullOrWhiteSpace(City) &&
        string.IsNullOrWhiteSpace(District) &&
        string.IsNullOrWhiteSpace(State) &&
        string.IsNullOrWhiteSpace(PostalCode) &&
        string.IsNullOrWhiteSpace(Country);

    public static Address Empty => new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    public override string ToString()
    {
        if (IsEmpty) return string.Empty;
        
        // Basic formatting, can be improved based on locale
        var parts = new[] { Street, City, State, PostalCode, Country }
            .Where(s => !string.IsNullOrWhiteSpace(s));
            
        return string.Join(", ", parts);
    }
}
