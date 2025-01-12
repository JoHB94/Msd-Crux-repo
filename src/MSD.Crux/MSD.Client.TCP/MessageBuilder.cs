using System.Text;
using Microsoft.Extensions.Configuration;
using MSD.Crux.Common;
using MSD.Crux.Core.Models;

namespace MSD.Client.TCP;

public static class MessageBuilder
{
    public static byte[] CreateVisionTypeFrame(byte frameType, VisionCum visionCum)
    {
        int payloadSize = 50;
        int totalSize = 6 + payloadSize;
        byte[] message = new byte[totalSize];

        message[0] = frameType;
        BitConverter.GetBytes((ushort)payloadSize).CopyTo(message, 1);
        message[3] = 1;
        message[4] = 1;
        message[5] = 0;

        Encoding.ASCII.GetBytes(visionCum.LineId.PadRight(4, '\0')).CopyTo(message, 6);
        BitConverter.GetBytes(new DateTimeOffset(visionCum.Time).ToUnixTimeSeconds()).CopyTo(message, 10);
        Encoding.ASCII.GetBytes(visionCum.LotId?.PadRight(20, '\0') ?? new string('\0', 20)).CopyTo(message, 18);
        Encoding.ASCII.GetBytes(visionCum.Shift?.PadRight(4, ' ') ?? "    ").CopyTo(message, 38);
        BitConverter.GetBytes((visionCum.EmployeeNumber ?? 0)).CopyTo(message, 42);
        BitConverter.GetBytes(visionCum.Total).CopyTo(message, 50);

        return message;
    }

    public static byte[] CreateJwtTypeFrame(byte frameType, User user, IConfiguration configuration)
    {
        CruxClaim cruxClaimClaims = new()
        {
            LoginId = user.LoginId ?? string.Empty,
            EmployeeName = user.Name,
            EmployeeNumber = user.EmployeeNumber,
            Roles = user.Roles ?? string.Empty,
        };

        string token = JwtHelper.GenerateToken(cruxClaimClaims.ToClaims(), configuration);

        byte[] payload = Encoding.UTF8.GetBytes(token);

        if (payload.Length < 200 || payload.Length > 1024)
        {
            throw new ArgumentException($"JWT 길이는 200~1024 bytes 사이어여합니다. 현재 길이는: {payload.Length}");
        }

        int totalLength = 6 + payload.Length;
        byte[] message = new byte[totalLength];

        message[0] = frameType;
        // 페이로드 길이 2Byte: 토큰길이는 700글자 이상이고 1 byte가 표현 가능한 숫자는 0~255이 한계이므로 길이에대한 숫자는 2Byte(ushort) 표현해야한다.
        BitConverter.GetBytes((ushort)payload.Length).CopyTo(message, 1);
        message[3] = 1;
        message[4] = 1;
        message[5] = 0;

        Array.Copy(payload, 0, message, 6, payload.Length);

        return message;
    }
}