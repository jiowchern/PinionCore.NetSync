﻿using PinionCore.Network;
using System;

using PinionCore.Remote;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace PinionCore.NetSync.Web
{
    public class WebPeer : IStreamable
    {
        private readonly IStreamable Tcp_;
        public readonly Network.Tcp.Peer Peer;
        readonly Stream ReverseStream_;
        int _ReceiveSize;
        
        readonly IStreamable Stream_;

        public WebPeer(Network.Tcp.Peer peer)
        {
            _ReceiveSize = 0;
            ReverseStream_ = new Stream();
            Stream_ = ReverseStream_;
            Peer = peer;
            Tcp_ = peer;
        }

        IWaitableValue<int> IStreamable.Receive(byte[] buffer, int offset, int count)
        {
            return Receive(buffer, offset, count).ToWaitableValue();
        }

        IWaitableValue<int> IStreamable.Send(byte[] buffer, int offset, int count)
        {
            return Send(buffer, offset, count).ToWaitableValue();
        }

        public async Task<int> Receive(byte[] buffer, int offset, int count)
        {
            if(_ReceiveSize > 0)
            {
                var receiveSize = await Stream_.Receive(buffer, offset, count);
                _ReceiveSize -= receiveSize;
                UnityEngine.Debug.Log($"receiveSize={receiveSize} _ReceiveSize:{_ReceiveSize}");
                return receiveSize;
            }

            while (true)
            {
                // Read the initial 2-byte header
                byte[] header = await ReadExactly(2);
                
                byte b0 = header[0];
                byte b1 = header[1];

                bool fin = (b0 & 0b10000000) != 0;
                int opcode = b0 & 0b00001111;
                bool isMasked = (b1 & 0b10000000) != 0;
                

                if (!isMasked)
                {
                    throw new InvalidOperationException("Received unmasked frame from client.");
                }

                // Determine payload length
                ulong payloadLength = (ulong)(b1 & 0b01111111);
                if(payloadLength == 0)
                {
                    UnityEngine.Debug.Log($"payloadLength == 0 header=[{b0},{b1}]");
                }
                
                if (payloadLength == 126)
                {
                    byte[] extendedLength = await ReadExactly(2);
                    payloadLength = BinaryPrimitives.ReadUInt16BigEndian(extendedLength);
                }
                else if (payloadLength == 127)
                {
                    byte[] extendedLength = await ReadExactly(8);
                    payloadLength = BinaryPrimitives.ReadUInt64BigEndian(extendedLength);
                }

                if (payloadLength > int.MaxValue)
                {
                    throw new InvalidOperationException("Payload length too large.");
                }

                // Read masking key
                byte[] maskingKey = await ReadExactly(4);
                
                // Read and unmask payload data
                int payloadLen = (int)payloadLength;
                byte[] payloadData = await ReadExactly(payloadLen);
                if (opcode == 0x08)
                {
                    await SendPong(header);
                    continue;
                }
                if (opcode != 0x02)
                    continue;

                for (int i = 0; i < payloadLen; i++)
                {
                    payloadData[i] ^= maskingKey[i % 4];
                }

                var sendSize = await ReverseStream_.Push(payloadData, 0, payloadData.Length);
                _ReceiveSize += sendSize;
                var receiveSize = await Stream_.Receive(buffer, offset, count);
                _ReceiveSize -= receiveSize;
                UnityEngine.Debug.Log($"sendSize={sendSize} receiveSize={receiveSize} _ReceiveSize:{_ReceiveSize}");
                return receiveSize;
            }

            return 0;
        }

        public async Task<int> Send(byte[] buffer, int offset, int count)
        {
            // Construct the WebSocket frame
            var frame = new List<byte>();

            // FIN and Opcode (binary frame)
            frame.Add(0b10000010); // FIN = 1, Opcode = 2 (binary)

            // Payload Length
            if (count <= 125)
            {
                frame.Add((byte)count);
            }
            else if (count <= ushort.MaxValue)
            {
                frame.Add(126);
                byte[] lengthBytes = new byte[2];
                BinaryPrimitives.WriteUInt16BigEndian(lengthBytes, (ushort)count);
                frame.AddRange(lengthBytes);
            }
            else
            {
                frame.Add(127);
                byte[] lengthBytes = new byte[8];
                BinaryPrimitives.WriteUInt64BigEndian(lengthBytes, (ulong)count);
                frame.AddRange(lengthBytes);
            }

            // Add the payload data
            frame.AddRange(new ArraySegment<byte>(buffer, offset, count));

            // Send the frame
            byte[] frameBytes = frame.ToArray();
            int totalSent = 0;
            while (totalSent < frameBytes.Length)
            {
                int sent = await Tcp_.Send(frameBytes, totalSent, frameBytes.Length - totalSent);
                if (sent == 0)
                {
                    throw new InvalidOperationException("Connection closed while sending data.");
                }
                totalSent += sent;
            }

            return totalSent;
        }

        // Helper method to read an exact number of bytes
        private async Task<byte[]> ReadExactly(int count)
        {
            byte[] buffer = new byte[count];
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = await Tcp_.Receive(buffer, totalRead, count - totalRead);
                if (read == 0)
                {
                    throw new InvalidOperationException("Connection closed while reading data.");
                }
                totalRead += read;
            }
            return buffer;
        }
        private async Task SendPong(byte[] pingHeaderBuffer)
        {
            List<byte> pongFrame = new List<byte>();

            // FIN and Opcode for Pong (0xA)
            pongFrame.Add((byte)(pingHeaderBuffer[0] & 0b11110000 | 0xA));

            // Payload length (same as Ping frame, which should typically be 0)
            int payloadLength = pingHeaderBuffer[1] & 0b01111111;
            pongFrame.Add((byte)payloadLength);

            // No payload in typical Ping/Pong frames, but if there was, add it here
            if (payloadLength > 0)
            {
                byte[] payloadData = new byte[payloadLength];
                await Tcp_.Receive(payloadData, 0, payloadLength);
                pongFrame.AddRange(payloadData);
            }

            UnityEngine.Debug.Log("Sending Pong frame");
            // Send the Pong frame
            await Tcp_.Send(pongFrame.ToArray(), 0, pongFrame.Count);
        }
        // Helper method to discard frames
        private async Task DiscardRemainingFrames(bool fin)
        {
            while (!fin)
            {
                // Read and discard the header
                byte[] header = await ReadExactly(2);
                byte b0 = header[0];
                byte b1 = header[1];

                fin = (b0 & 0b10000000) != 0;
                bool isMasked = (b1 & 0b10000000) != 0;
                ulong payloadLength = (ulong)(b1 & 0b01111111);

                if (payloadLength == 126)
                {
                    byte[] extendedLength = await ReadExactly(2);
                    payloadLength = BinaryPrimitives.ReadUInt16BigEndian(extendedLength);
                }
                else if (payloadLength == 127)
                {
                    byte[] extendedLength = await ReadExactly(8);
                    payloadLength = BinaryPrimitives.ReadUInt64BigEndian(extendedLength);
                }

                if (isMasked)
                {
                    await ReadExactly(4); // Discard masking key
                }

                // Discard payload
                while (payloadLength > 0)
                {
                    int chunkSize = (int)Math.Min(payloadLength, 1024);
                    await ReadExactly(chunkSize);
                    payloadLength -= (ulong)chunkSize;
                }
            }
        }
    }
}