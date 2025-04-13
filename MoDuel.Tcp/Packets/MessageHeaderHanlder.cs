using MoDuel.Shared;
using System.Buffers.Binary;

namespace MoDuel.Tcp.Packets;

/// <summary>
/// Handles incoming tcp messages, dealing with their size and system messages.
/// </summary>
internal class MessageHeaderHandler {

    /// <summary>
    /// The header is only a single int.
    /// </summary>
    private static readonly int HeaderSize = sizeof(int);

    /// <summary>
    /// Add the message size header to the provided data.
    /// </summary>
    /// <returns>The data prepended by the header.</returns>
    public static byte[] AddHeader(byte[] data) {

        byte[] fullMessage = new byte[HeaderSize + data.Length];
        byte[] size = BitConverter.GetBytes(data.Length);

        Buffer.BlockCopy(size, 0, fullMessage, 0, HeaderSize);
        Buffer.BlockCopy(data, 0, fullMessage, HeaderSize, data.Length);
        return fullMessage;
    }

    public static byte[] AddSystemHeader(SystemMessageHeader type, byte[] data) {
        byte[] fullMessage = new byte[HeaderSize + data.Length];
        byte[] ping = BitConverter.GetBytes((int)type);
        Buffer.BlockCopy(ping, 0, fullMessage, 0, HeaderSize);
        Buffer.BlockCopy(data, 0, fullMessage, HeaderSize, data.Length);
        return fullMessage;
    }

    /// <summary>
    /// Read the header value from the data.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="remainingPayload"></param>
    /// <returns></returns>
    public static int ReadHeader(ReadOnlySpan<byte> data, ref int pointer) {

        if (data.Length < HeaderSize) {
            return (int)SystemMessageHeader.Error;
        }
        var result = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(pointer, HeaderSize));
        pointer += HeaderSize;
        return result;
    }
    /// <summary>
    /// The buffer to store the current message being read.
    /// </summary>
    private byte[] currentMessage = [];
    /// <summary>
    /// The size of the current message being read.
    /// </summary>
    private int currentMessageSize = 0;
    /// <summary>
    /// The index in the <see cref="currentMessage"/> that bytes have been inserted up to.
    /// </summary>
    private int currentMessagePointer = 0;

    private SystemMessageHeader CurrentMessageHeader = SystemMessageHeader.Standard;

    public MessageHeaderHandler() { }

    /// <summary>
    /// Parse the incoming <paramref name="data"/> for messages return true when a message is received,
    /// </summary>
    /// <param name="data">The data to parse.</param>
    /// <param name="messages">The set of messages the data contains or completed.</param>
    /// <returns>True if there is a full message was read into <paramref name="messages"/>.</returns>
    public bool HandleIncomingData(ReadOnlySpan<byte> data, out List<(SystemMessageHeader, byte[])> messages) {

        // The set of all messages that were completed upon receiving the data.
        messages = [];

        // The pointer to read the current data.
        int dataPointer = 0;

        while (dataPointer < data.Length) {

            // If we are not in the middle of a message read the header.
            if (currentMessageSize == 0) {
                var header = ReadHeader(data, ref dataPointer);

                // No header information available due to size.
                if (header == -1) {
                    break;
                }

                // Handle system messages.
                if (header < 0) {
                    CurrentMessageHeader = (SystemMessageHeader)header;
                    switch (CurrentMessageHeader) {
                        case SystemMessageHeader.Standard:
                            currentMessageSize = 0;
                            break;
                        case SystemMessageHeader.Error:
                            currentMessageSize = 0;
                            break;
                        case SystemMessageHeader.Ping:
                            currentMessageSize = PingPacket.GetSize();
                            break;
                    }
                }
                else {
                    CurrentMessageHeader = SystemMessageHeader.Standard;
                    currentMessageSize = header;
                }

                currentMessage = new byte[currentMessageSize];
                Logger.Log(LogTypes.MessageSize, "Current Message Size: " + header);
            }

            // Take until the end of the data or end of the message.
            int bytesToTake = int.Min(data.Length - dataPointer, currentMessageSize - currentMessagePointer);

            Buffer.BlockCopy(data.ToArray(), dataPointer, currentMessage, currentMessagePointer, bytesToTake);
            currentMessagePointer += bytesToTake;
            dataPointer += bytesToTake;

            if (currentMessagePointer == currentMessageSize) {
                currentMessagePointer = 0;
                currentMessageSize = 0;
                messages.Add((CurrentMessageHeader, currentMessage));
            }

        }

        return messages.Count != 0;

    }


}
