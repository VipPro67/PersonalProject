using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Resources;

public static class ResourceKey
{
    #region Objects
    public const string Password = "Password";
    public const string Username = "Username";
    public const string FullName = "FullName";
    public const string Email = "Email";
    public const string Address = "Address";
    public const string DateOfBirth = "DateOfBirth";
    public const string User = "User";
    public const string Logout = "Logout";
    public const string RefreshToken = "RefreshToken";
    public const string Token = "Token";
    public const string Login = "Login";
    public const string Registration = "Registration";
    public const string Validation = "Validation";
    #endregion Objects

    #region Status
    public const string Error = "Error";
    public const string Successful = "Successful";
    public const string Failed = "Failed";
    public const string NotFound = "NotFound";
    public const string Created = "Created";
    public const string Updated = "Updated";
    public const string Deleted = "Deleted";
    public const string Unauthenticated = "Unauthenticated";
    public const string Unauthorized = "Unauthorized";
    public const string InvalidToken = "InvalidToken";
    #endregion Status

    #region Validations
    public const string Required = "Required";
    public const string Invalid = "Invalid";
    public const string MustContainUppercase = "MustContainUppercase";
    public const string MustContainLowercase = "MustContainLowercase";
    public const string MustContainSpecialCharacter = "MustContainSpecialCharacter";
    public const string MustContainDigit = "MustContainDigit";
    public const string MaxLength = "MaxLength";
    public const string MinLength = "MinLength";
    public const string Alphanumeric = "Alphanumeric";

    #endregion Validations

}
