using System.Net;

namespace Pufzi.Services.EmailTemplates;

public static class AuthEmailTemplates
{
    public static string EmailConfirmation(
        string firstName,
        string confirmationUrl)
    {
        var safeFirstName = WebUtility.HtmlEncode(firstName);
        var safeConfirmationUrl = WebUtility.HtmlEncode(confirmationUrl);

        return $"""
            <div style="
                font-family: Arial, sans-serif;
                max-width: 600px;
                margin: 0 auto;
                padding: 32px;
            ">
                <h1>Bine ai venit în Pufzi! 🐾</h1>

                <p>Salut {safeFirstName},</p>

                <p>
                    Contul tău Pufzi a fost creat.
                    Confirmă adresa de email pentru
                    a-ți activa contul.
                </p>

                <p style="margin: 32px 0;">
                    <a
                        href="{safeConfirmationUrl}"
                        style="
                            background: #222;
                            color: white;
                            padding: 12px 20px;
                            text-decoration: none;
                            border-radius: 8px;
                            display: inline-block;
                        ">
                        Confirmă adresa de email
                    </a>
                </p>

                <p>
                    Linkul este valabil 24 de ore.
                </p>

                <p>
                    Dacă nu tu ai creat acest cont,
                    poți ignora acest email.
                </p>
            </div>
            """;
    }

    public static string PasswordReset(
        string firstName,
        string resetUrl)
    {
        var safeFirstName = WebUtility.HtmlEncode(firstName);
        var safeResetUrl = WebUtility.HtmlEncode(resetUrl);

        return $"""
            <div style="
                font-family: Arial, sans-serif;
                max-width: 600px;
                margin: 0 auto;
                padding: 32px;
            ">
                <h1>Resetare parolă Pufzi 🐾</h1>

                <p>Salut {safeFirstName},</p>

                <p>
                    Am primit o cerere pentru resetarea
                    parolei contului tău Pufzi.
                </p>

                <p style="margin: 32px 0;">
                    <a
                        href="{safeResetUrl}"
                        style="
                            background: #222;
                            color: white;
                            padding: 12px 20px;
                            text-decoration: none;
                            border-radius: 8px;
                            display: inline-block;
                        ">
                        Resetează parola
                    </a>
                </p>

                <p>
                    Linkul este valabil o oră.
                </p>

                <p>
                    Dacă nu ai solicitat resetarea parolei,
                    poți ignora acest email.
                </p>
            </div>
            """;
    }
}