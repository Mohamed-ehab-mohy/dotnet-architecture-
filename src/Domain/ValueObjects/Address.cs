namespace Domain.ValueObjects;

public class Address : IEquatable<Address>
{
    public string Street { get; }
    public string City { get; }
    public string Country { get; }
    public string PostalCode { get; }

    public Address(string street, string city, string country, string postalCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        Street = street;
        City = city;
        Country = country;
        PostalCode = postalCode;
    }

    public bool Equals(Address? other)
    {
        if (other is null) return false;
        return Street == other.Street &&
               City == other.City &&
               Country == other.Country &&
               PostalCode == other.PostalCode;
    }

    public override bool Equals(object? obj) => Equals(obj as Address);

    public override int GetHashCode() => HashCode.Combine(Street, City, Country, PostalCode);

    public static bool operator ==(Address? left, Address? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Address? left, Address? right) =>
        !(left == right);
}
