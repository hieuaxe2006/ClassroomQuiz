using System;
using System.Text;
using UnityEngine;

/// <summary>
/// Lớp tĩnh chịu trách nhiệm chuyển đổi (Serialize/Deserialize) giữa dữ liệu packet và mảng byte nhị phân.
/// Định dạng gói tin trên đường truyền mạng TCP:
/// [4-byte Độ dài payload + type (int)][2-byte Loại packet (PacketType - ushort)][Dữ liệu JSON dạng UTF-8]
/// </summary>
public static class PacketSerializer
{
    /// <summary>
    /// Đóng gói loại packet và chuỗi JSON thành mảng byte để gửi qua NetworkStream.
    /// Cấu trúc: [4-byte length][2-byte type][payload UTF-8]
    /// </summary>
    /// <param name="type">Loại packet</param>
    /// <param name="json">Chuỗi JSON chứa nội dung packet</param>
    /// <returns>Mảng byte sẵn sàng truyền qua TCP socket</returns>
    public static byte[] Serialize(PacketType type, string json)
    {
        // TODO: Chuyển đổi chuỗi json sang UTF8 byte array: Encoding.UTF8.GetBytes(json ?? "")
        // TODO: Tính tổng chiều dài (2 byte type + số byte payload) hoặc chiều dài toàn gói
        // TODO: Ghi 4 byte length (BitConverter.GetBytes(length)) theo BigEndian/LittleEndian
        // TODO: Ghi 2 byte packet type (BitConverter.GetBytes((ushort)type))
        // TODO: Nối các mảng byte lại thành một mảng hoàn chỉnh và trả về

        return Array.Empty<byte>();
    }

    /// <summary>
    /// Giải mã mảng byte nhận được từ socket thành loại packet và chuỗi nội dung JSON.
    /// </summary>
    /// <param name="data">Mảng byte dữ liệu nhận được từ mạng</param>
    /// <returns>Tuple chứa (PacketType type, string json)</returns>
    public static (PacketType type, string json) Deserialize(byte[] data)
    {
        // TODO: Kiểm tra độ dài dữ liệu hợp lệ (tối thiểu header)
        // TODO: Đọc 2 byte type để ép kiểu về PacketType: (PacketType)BitConverter.ToUInt16(...)
        // TODO: Đọc các byte còn lại của payload và giải mã chuỗi JSON: Encoding.UTF8.GetString(...)
        // TODO: Có thể dùng JsonUtility hoặc Json parser tùy chọn để deserialize sang object cụ thể

        return (default, string.Empty);
    }
}
