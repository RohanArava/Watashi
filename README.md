# Watashi (私)

A demo OpenID Connect Identity Provider built with ASP.NET Core

## Implemented Features

- Client registration and management
  - Partial Dynamic Client Registration support
  - Dynamic client id and secret generation
  - Client metadata consistent with DCR
  - Update and delete operations that follow DCRM
- User registration and Authentication
  - User registration with unique usernames
  - Login with TOTP-based authentication
  - Cookie-based session management
- OAuth 2.0 Authorization Code Flow
  - `/connect/authorize` endpoint
  - User consent screen
  - `/connect/token` endpoint
    - Issues access token and ID token
    - Signed using RSA with JWK exposure
  

This is not a complete implementation of OAuth 2.0 and OIDC specifications.  
This implements the commonly used authorization code flow. Other flows are skipped for now.  
Some parts of the implementation do not follow the specification exactly, but the function of the application is essentially the same.  

## Planned Features

- Better discovery support (/.wellknown endpoints)
- Follow DCR specifications more closely
- PKCE support for public clients
- Refresh token support
- `/user-info` endpoint
- Support more user data and granular scoping

## Running the project

Update the config at [`./appsettings.json`](./appsettings.json)

Install dependencies:
```
dotnet restore
```
Apply db migrations:
```
dotnet ef database update
```
Run the server:
```
dotnet run
```

Visit: http://localhost:5233 and follow the user login or sign up flow. 

For client registration, as there is no demo client application for now, send requests from Postman or Thunder client following the code at [ClientController.cs](./Controllers/ClientController.cs).

Do the same for OAuth flows as well.

Example flow:
- Register a client via POST `/connect/register`
- User registers via `/user/signup`
- Client initiates auth request to `/connect/authorize`
- User logs in and grants consent
- Authorization code is issued
- Client exchanges code at `/connect/token` for access and ID tokens
- Tokens can be verified using public key at .well-known/jwks.json