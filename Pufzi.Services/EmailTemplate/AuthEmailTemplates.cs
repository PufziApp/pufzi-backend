using System.Net;

namespace Pufzi.Services.EmailTemplates;

public static class AuthEmailTemplates
{
    public static string EmailConfirmation(
        string firstName,
        string confirmationUrl)
    {
        var safeFirstName =
            WebUtility.HtmlEncode(firstName);

        var safeConfirmationUrl =
            WebUtility.HtmlEncode(confirmationUrl);

        return $$"""
        <!DOCTYPE html>
        <html lang="ro">
        <head>
            <meta charset="UTF-8">
            <meta
                name="viewport"
                content="width=device-width, initial-scale=1.0"
            >
            <title>Confirmă contul Pufzi</title>
        </head>

        <body style="
            margin: 0;
            padding: 0;
            background-color: #f7f5f2;
            font-family: Arial, Helvetica, sans-serif;
            color: #242424;
        ">
            <table
                role="presentation"
                width="100%"
                cellspacing="0"
                cellpadding="0"
                border="0"
                style="
                    background-color: #f7f5f2;
                    padding: 40px 16px;
                "
            >
                <tr>
                    <td align="center">

                        <table
                            role="presentation"
                            width="100%"
                            cellspacing="0"
                            cellpadding="0"
                            border="0"
                            style="
                                max-width: 600px;
                                background-color: #ffffff;
                                border-radius: 24px;
                                overflow: hidden;
                                border: 1px solid #ece8e3;
                            "
                        >
                            <!-- Header -->
                            <tr>
                                <td
                                    align="center"
                                    style="
                                        padding: 36px 32px 24px 32px;
                                        background-color: #fff8f2;
                                    "
                                >
                                    <div
                                        style="
                                            font-size: 30px;
                                            font-weight: 800;
                                            color: #f47b35;
                                            letter-spacing: -1px;
                                        "
                                    >
                                        Pufzi
                                    </div>

                                    <div
                                        style="
                                            margin-top: 8px;
                                            font-size: 14px;
                                            color: #77716c;
                                        "
                                    >
                                        Tot ce are nevoie animalul tău,
                                        într-un singur loc.
                                    </div>
                                </td>
                            </tr>

                            <!-- Content -->
                            <tr>
                                <td
                                    style="
                                        padding: 40px 40px 32px 40px;
                                    "
                                >
                                    <h1
                                        style="
                                            margin: 0 0 16px 0;
                                            font-size: 26px;
                                            line-height: 1.25;
                                            color: #242424;
                                            text-align: center;
                                        "
                                    >
                                        Bine ai venit în Pufzi! 🐾
                                    </h1>

                                    <p
                                        style="
                                            margin: 0 auto 12px auto;
                                            max-width: 460px;
                                            font-size: 16px;
                                            line-height: 1.6;
                                            color: #68635f;
                                            text-align: center;
                                        "
                                    >
                                        Salut
                                        <strong
                                            style="color: #242424;"
                                        >
                                            {{safeFirstName}}
                                        </strong>!
                                    </p>

                                    <p
                                        style="
                                            margin: 0 auto 28px auto;
                                            max-width: 460px;
                                            font-size: 16px;
                                            line-height: 1.6;
                                            color: #68635f;
                                            text-align: center;
                                        "
                                    >
                                        Contul tău Pufzi a fost creat
                                        cu succes. Mai ai un singur pas:
                                        confirmă adresa de email pentru
                                        a-ți activa contul.
                                    </p>

                                    <!-- Info card -->
                                    <table
                                        role="presentation"
                                        width="100%"
                                        cellspacing="0"
                                        cellpadding="0"
                                        border="0"
                                        style="
                                            margin-bottom: 30px;
                                            background-color: #faf8f5;
                                            border-radius: 16px;
                                            border: 1px solid #eee9e4;
                                        "
                                    >
                                        <tr>
                                            <td
                                                align="center"
                                                style="padding: 20px;"
                                            >
                                                <div
                                                    style="
                                                        margin-bottom: 8px;
                                                        font-size: 13px;
                                                        color: #8a847e;
                                                        text-transform: uppercase;
                                                        letter-spacing: 1px;
                                                        font-weight: 600;
                                                    "
                                                >
                                                    Următorul pas
                                                </div>

                                                <div
                                                    style="
                                                        display: inline-block;
                                                        padding: 8px 16px;
                                                        background-color: #fff0e5;
                                                        border-radius: 999px;
                                                        color: #e86825;
                                                        font-size: 15px;
                                                        font-weight: 700;
                                                    "
                                                >
                                                    Confirmă adresa de email
                                                </div>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- CTA -->
                                    <table
                                        role="presentation"
                                        width="100%"
                                        cellspacing="0"
                                        cellpadding="0"
                                        border="0"
                                    >
                                        <tr>
                                            <td align="center">
                                                <a
                                                    href="{{safeConfirmationUrl}}"
                                                    style="
                                                        display: inline-block;
                                                        padding: 15px 30px;
                                                        background-color: #f47b35;
                                                        color: #ffffff;
                                                        text-decoration: none;
                                                        border-radius: 12px;
                                                        font-size: 16px;
                                                        font-weight: 700;
                                                    "
                                                >
                                                    Confirmă adresa de email
                                                </a>
                                            </td>
                                        </tr>
                                    </table>

                                    <p
                                        style="
                                            margin: 28px 0 0 0;
                                            font-size: 13px;
                                            line-height: 1.6;
                                            color: #918b85;
                                            text-align: center;
                                        "
                                    >
                                        Linkul de confirmare este valabil
                                        timp de
                                        <strong
                                            style="color: #68635f;"
                                        >
                                            24 de ore
                                        </strong>.
                                    </p>
                                </td>
                            </tr>

                            <!-- Divider -->
                            <tr>
                                <td style="padding: 0 40px;">
                                    <div
                                        style="
                                            height: 1px;
                                            background-color: #eeeae6;
                                        "
                                    ></div>
                                </td>
                            </tr>

                            <!-- Fallback link -->
                            <tr>
                                <td
                                    style="
                                        padding: 28px 40px;
                                    "
                                >
                                    <p
                                        style="
                                            margin: 0 0 10px 0;
                                            font-size: 13px;
                                            line-height: 1.6;
                                            color: #918b85;
                                        "
                                    >
                                        Dacă butonul nu funcționează,
                                        copiază linkul de mai jos în
                                        browser:
                                    </p>

                                    <p
                                        style="
                                            margin: 0;
                                            font-size: 12px;
                                            line-height: 1.6;
                                            word-break: break-all;
                                        "
                                    >
                                        <a
                                            href="{{safeConfirmationUrl}}"
                                            style="
                                                color: #f47b35;
                                                text-decoration: none;
                                            "
                                        >
                                            {{safeConfirmationUrl}}
                                        </a>
                                    </p>
                                </td>
                            </tr>

                            <!-- Footer -->
                            <tr>
                                <td
                                    align="center"
                                    style="
                                        padding: 24px 40px 32px 40px;
                                        background-color: #faf8f5;
                                    "
                                >
                                    <p
                                        style="
                                            margin: 0 0 6px 0;
                                            font-size: 13px;
                                            color: #77716c;
                                        "
                                    >
                                        Nu ai creat tu acest cont?
                                    </p>

                                    <p
                                        style="
                                            margin: 0;
                                            font-size: 12px;
                                            line-height: 1.5;
                                            color: #a19b95;
                                        "
                                    >
                                        Poți ignora acest email în
                                        siguranță. Contul nu va fi activat
                                        fără confirmarea adresei de email.
                                    </p>
                                </td>
                            </tr>
                        </table>

                        <p
                            style="
                                margin: 20px 0 0 0;
                                font-size: 12px;
                                color: #a19b95;
                                text-align: center;
                            "
                        >
                            © {{DateTime.UtcNow.Year}} Pufzi
                        </p>

                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
    }

    public static string PasswordReset(
        string firstName,
        string resetUrl)
    {
        var safeFirstName =
            WebUtility.HtmlEncode(firstName);

        var safeResetUrl =
            WebUtility.HtmlEncode(resetUrl);

        return $$"""
        <!DOCTYPE html>
        <html lang="ro">
        <head>
            <meta charset="UTF-8">
            <meta
                name="viewport"
                content="width=device-width, initial-scale=1.0"
            >
            <title>Resetare parolă Pufzi</title>
        </head>

        <body style="
            margin: 0;
            padding: 0;
            background-color: #f7f5f2;
            font-family: Arial, Helvetica, sans-serif;
            color: #242424;
        ">
            <table
                role="presentation"
                width="100%"
                cellspacing="0"
                cellpadding="0"
                border="0"
                style="
                    background-color: #f7f5f2;
                    padding: 40px 16px;
                "
            >
                <tr>
                    <td align="center">

                        <table
                            role="presentation"
                            width="100%"
                            cellspacing="0"
                            cellpadding="0"
                            border="0"
                            style="
                                max-width: 600px;
                                background-color: #ffffff;
                                border-radius: 24px;
                                overflow: hidden;
                                border: 1px solid #ece8e3;
                            "
                        >
                            <!-- Header -->
                            <tr>
                                <td
                                    align="center"
                                    style="
                                        padding: 36px 32px 24px 32px;
                                        background-color: #fff8f2;
                                    "
                                >
                                    <div
                                        style="
                                            font-size: 30px;
                                            font-weight: 800;
                                            color: #f47b35;
                                            letter-spacing: -1px;
                                        "
                                    >
                                        Pufzi
                                    </div>

                                    <div
                                        style="
                                            margin-top: 8px;
                                            font-size: 14px;
                                            color: #77716c;
                                        "
                                    >
                                        Tot ce are nevoie animalul tău,
                                        într-un singur loc.
                                    </div>
                                </td>
                            </tr>

                            <!-- Content -->
                            <tr>
                                <td
                                    style="
                                        padding: 40px 40px 32px 40px;
                                    "
                                >
                                    <h1
                                        style="
                                            margin: 0 0 16px 0;
                                            font-size: 26px;
                                            line-height: 1.25;
                                            color: #242424;
                                            text-align: center;
                                        "
                                    >
                                        Resetarea parolei 🔐
                                    </h1>

                                    <p
                                        style="
                                            margin: 0 auto 12px auto;
                                            max-width: 460px;
                                            font-size: 16px;
                                            line-height: 1.6;
                                            color: #68635f;
                                            text-align: center;
                                        "
                                    >
                                        Salut
                                        <strong
                                            style="color: #242424;"
                                        >
                                            {{safeFirstName}}
                                        </strong>!
                                    </p>

                                    <p
                                        style="
                                            margin: 0 auto 28px auto;
                                            max-width: 460px;
                                            font-size: 16px;
                                            line-height: 1.6;
                                            color: #68635f;
                                            text-align: center;
                                        "
                                    >
                                        Am primit o cerere pentru
                                        resetarea parolei contului tău
                                        Pufzi. Folosește butonul de mai
                                        jos pentru a alege o parolă nouă.
                                    </p>

                                    <!-- Info card -->
                                    <table
                                        role="presentation"
                                        width="100%"
                                        cellspacing="0"
                                        cellpadding="0"
                                        border="0"
                                        style="
                                            margin-bottom: 30px;
                                            background-color: #faf8f5;
                                            border-radius: 16px;
                                            border: 1px solid #eee9e4;
                                        "
                                    >
                                        <tr>
                                            <td
                                                align="center"
                                                style="padding: 20px;"
                                            >
                                                <div
                                                    style="
                                                        margin-bottom: 8px;
                                                        font-size: 13px;
                                                        color: #8a847e;
                                                        text-transform: uppercase;
                                                        letter-spacing: 1px;
                                                        font-weight: 600;
                                                    "
                                                >
                                                    Securitatea contului
                                                </div>

                                                <div
                                                    style="
                                                        display: inline-block;
                                                        padding: 8px 16px;
                                                        background-color: #fff0e5;
                                                        border-radius: 999px;
                                                        color: #e86825;
                                                        font-size: 15px;
                                                        font-weight: 700;
                                                    "
                                                >
                                                    Creează o parolă nouă
                                                </div>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- CTA -->
                                    <table
                                        role="presentation"
                                        width="100%"
                                        cellspacing="0"
                                        cellpadding="0"
                                        border="0"
                                    >
                                        <tr>
                                            <td align="center">
                                                <a
                                                    href="{{safeResetUrl}}"
                                                    style="
                                                        display: inline-block;
                                                        padding: 15px 30px;
                                                        background-color: #f47b35;
                                                        color: #ffffff;
                                                        text-decoration: none;
                                                        border-radius: 12px;
                                                        font-size: 16px;
                                                        font-weight: 700;
                                                    "
                                                >
                                                    Resetează parola
                                                </a>
                                            </td>
                                        </tr>
                                    </table>

                                    <p
                                        style="
                                            margin: 28px 0 0 0;
                                            font-size: 13px;
                                            line-height: 1.6;
                                            color: #918b85;
                                            text-align: center;
                                        "
                                    >
                                        Din motive de securitate, linkul
                                        este valabil timp de
                                        <strong
                                            style="color: #68635f;"
                                        >
                                            o oră
                                        </strong>.
                                    </p>
                                </td>
                            </tr>

                            <!-- Divider -->
                            <tr>
                                <td style="padding: 0 40px;">
                                    <div
                                        style="
                                            height: 1px;
                                            background-color: #eeeae6;
                                        "
                                    ></div>
                                </td>
                            </tr>

                            <!-- Fallback link -->
                            <tr>
                                <td
                                    style="
                                        padding: 28px 40px;
                                    "
                                >
                                    <p
                                        style="
                                            margin: 0 0 10px 0;
                                            font-size: 13px;
                                            line-height: 1.6;
                                            color: #918b85;
                                        "
                                    >
                                        Dacă butonul nu funcționează,
                                        copiază linkul de mai jos în
                                        browser:
                                    </p>

                                    <p
                                        style="
                                            margin: 0;
                                            font-size: 12px;
                                            line-height: 1.6;
                                            word-break: break-all;
                                        "
                                    >
                                        <a
                                            href="{{safeResetUrl}}"
                                            style="
                                                color: #f47b35;
                                                text-decoration: none;
                                            "
                                        >
                                            {{safeResetUrl}}
                                        </a>
                                    </p>
                                </td>
                            </tr>

                            <!-- Footer -->
                            <tr>
                                <td
                                    align="center"
                                    style="
                                        padding: 24px 40px 32px 40px;
                                        background-color: #faf8f5;
                                    "
                                >
                                    <p
                                        style="
                                            margin: 0 0 6px 0;
                                            font-size: 13px;
                                            color: #77716c;
                                        "
                                    >
                                        Nu ai solicitat resetarea parolei?
                                    </p>

                                    <p
                                        style="
                                            margin: 0;
                                            font-size: 12px;
                                            line-height: 1.5;
                                            color: #a19b95;
                                        "
                                    >
                                        Poți ignora acest email în
                                        siguranță. Parola contului tău
                                        nu va fi modificată.
                                    </p>
                                </td>
                            </tr>
                        </table>

                        <p
                            style="
                                margin: 20px 0 0 0;
                                font-size: 12px;
                                color: #a19b95;
                                text-align: center;
                            "
                        >
                            © {{DateTime.UtcNow.Year}} Pufzi
                        </p>

                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
    }
}