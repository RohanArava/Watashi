using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtpNet;
using QRCoder;
using Watashi.Data;
using Watashi.Models;
using Watashi.Models.UserInfo;
using Watashi.ViewModels;

namespace Watashi.Controllers;

public class UserController(WatashiDbContext db) : Controller
{
    private readonly WatashiDbContext _db = db;

    [HttpGet("/signup")]
    public IActionResult Signup() => View();

    [HttpPost("/signup")]
    public async Task<IActionResult> Signup(SignupViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (await _db.Users.AnyAsync(u => u.Username == model.Username))
        {
            ModelState.AddModelError("Username", "Username already exists.");
            return View(model);
        }

        var secret = KeyGeneration.GenerateRandomKey(20);
        var totpSecret = Base32Encoding.ToString(secret);

        var user = new User
        {
            Username = model.Username.Trim(),
            DisplayName = model.DisplayName?.Trim(),
            TotpSecret = totpSecret
        };

        var recoveryCodes = Enumerable
            .Range(0, 10)
            .Select(_ => new RecoveryCode
            {
                Code = Guid.NewGuid().ToString("N")[..10],
                IsUsed = false,
                User = user
            })
            .ToList();

        user.RecoveryCodes = recoveryCodes;

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        string issuer = "Watashi";
        string otpUri =
            $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(user.Username)}"
            + $"?secret={user.TotpSecret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(otpUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var qrCodeBytes = qrCode.GetGraphic(20);
        string qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);
        string qrCodeImageUrl = $"data:image/png;base64,{qrCodeBase64}";

        return View(
            "SignupSuccess",
            new SignupSuccessViewModel
            {
                Username = user.Username,
                TotpSecret = totpSecret,
                RecoveryCodes = [.. recoveryCodes.Select(rc => rc.Code)],
                QrCodeImageUrl = qrCodeImageUrl
            }
        );
    }

    [HttpGet("/login")]
    public IActionResult Login() => View();

    [HttpPost("/login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

        if (user == null)
        {
            ModelState.AddModelError("Username", "Invalid username.");
            return View(model);
        }

        var totp = new Totp(Base32Encoding.ToBytes(user.TotpSecret));
        var code = model.Code.Trim();

        if (totp.VerifyTotp(code, out _, new VerificationWindow(2, 2)))
        {
            return View("LoginSuccess", user);
        }

        ModelState.AddModelError("Code", "Invalid TOTP.");
        return View(model);
    }

    [HttpGet("/recover")]
    public IActionResult Recover() => View();

    [HttpPost("/recover")]
    public async Task<IActionResult> Recover(RecoverViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _db
            .Users.Include(u => u.RecoveryCodes)
            .FirstOrDefaultAsync(u => u.Username == model.Username);

        if (user == null)
        {
            ModelState.AddModelError("Username", "Invalid username or recovery code.");
            return View(model);
        }

        var input = model.Code.Trim();

        var recoveryCode = user.RecoveryCodes.FirstOrDefault(rc =>
            !rc.IsUsed && rc.Code.Equals(input, StringComparison.OrdinalIgnoreCase)
        );

        if (recoveryCode == null)
        {
            ModelState.AddModelError("Code", "Invalid or already used recovery code.");
            return View(model);
        }

        recoveryCode.IsUsed = true;

        if (user.RecoveryCodes.All(rc => rc.IsUsed))
        {
            var newCodes = Enumerable
                .Range(0, 10)
                .Select(_ => new RecoveryCode
                {
                    Code = Guid.NewGuid().ToString("N")[..10],
                    IsUsed = false,
                    User = user
                })
                .ToList();

            user.RecoveryCodes = newCodes;

            await _db.SaveChangesAsync();

            return View(
                "RecoverySuccess",
                new RecoverySuccessViewModel
                {
                    Username = user.Username,
                    NewRecoveryCodes = newCodes.Select(rc => rc.Code).ToList()
                }
            );
        }

        await _db.SaveChangesAsync();
        return View("LoginSuccess", user);
    }
}
