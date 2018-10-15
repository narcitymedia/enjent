using System;
using System.Numerics;
using System.Collections.Generic;

namespace NarcityMedia.Net
{
    /// <summary>
    /// Represents a general concept of a WebSocket frame described by the 
    /// WebSocket standard
    /// </summary>
    abstract class SocketFrame
    {
        public enum OPCode
        {
            Continuation = 0x0,
            Text = 0x1,
            Binary = 0x2,
            Ping = 0x9,
            Pong = 0xA
        }

        protected bool fin;
        protected bool masked;
        protected byte[] data;
        protected byte opcode;
        protected byte contentLength;

        public SocketFrame(bool fin, bool masked, byte length)
        {
            this.fin = fin;
            this.masked = masked;
            this.contentLength = length;
        }

        /// <summary>
        /// Initializes the OPCode of the frame, the frame must decide it's own opcode
        /// </summary>
        /// <remarks>Derived classes MUST override</remarks>
        protected abstract void InitOPCode();

        /// <summary>
        /// Returns the bytes representation of the data frame
        /// </summary>
        /// <returns>Returns the bytes that form the data frame</returns>
        public byte[] GetBytes()
        {
            byte[] frameHeader = new byte[2];

            // First octet - 1 bit for FIN, 3 reserved, 4 for OP Code
            byte octet0 = (byte) ((this.fin) ? 0b10000000 : 0b00000000);
            octet0 = (byte) (octet0 | this.opcode);

            byte octet1 = (byte) ((this.masked) ? 0b10000000 : 0b00000000);
            octet1 = (byte) (octet1 | this.contentLength);
            // octet1 = (byte) (octet1 | this.contentLength);

            frameHeader[0] = octet0;
            frameHeader[1] = octet1;
            
            byte[] finalBytes = AppendMessageToBytes(frameHeader, this.data);

            Console.WriteLine("OCTETS : ");
            for (int i = 0; i < finalBytes.Length; i++)
            {
                Console.WriteLine(Convert.ToString(finalBytes[i], 2));
            }

            return finalBytes;
        }

        /// <summary>
        /// Appends the payload to the header of the data frame
        /// </summary>
        /// <param name="header">Bytes of the frame header</param>
        /// <param name="message">Bytes of the frame message</param>
        /// <returns>All the bytes of the frame</returns>
        private byte[] AppendMessageToBytes(byte[] header, byte[] message)
        {
            byte[] payload = new byte[header.Length + message.Length];
            header.CopyTo(payload, 0);
            message.CopyTo(payload, header.Length);

            // Removing trailing zeroes
            int i = payload.Length - 1;
            while (payload[i] == 0)
            {
                i--;
            }

            byte[] trimmed = new byte[i + 1];
            Array.Copy(payload, trimmed, i + 1);

            return trimmed;
        }
    }

    /// <summary>
    /// Represents a WebSocket frame that contains data
    /// </summary>
    class SocketDataFrame : SocketFrame
    {
        public enum DataFrameType { Text, Binary }
        public DataFrameType DataType;

        public SocketDataFrame(bool fin, bool masked, byte length,
                                DataFrameType dataType,
                                byte[] message) : base(fin, masked, length)
        {
            this.DataType = dataType;
            this.InitOPCode();
            this.data = message;
        }

        protected override void InitOPCode()
        {
            if (this.DataType == DataFrameType.Text) this.opcode = (byte) SocketFrame.OPCode.Text;
            else this.opcode = (byte) SocketFrame.OPCode.Binary;
        }
    }

    /// <summary>
    /// Represents an application message that is to be sent via WebSocket.
    /// A message is composed of frames
    /// </summary>
    /// <remarks>This class only support messages that can fit in a single frame for now</remarks>
    class SocketMessage
    {
        // Start values at value 1 to avoid sending empty application data
        public enum ApplicationMessageCode { Greeting = 1, FetchNOtifications, FetchCurrentArticle, FetchComments }
        public SocketDataFrame.DataFrameType MessageType = SocketDataFrame.DataFrameType.Binary;

        public ushort AppMessageCode
        {
            get { return this.appMessageCode; }
            set {
                this.appMessageCode = value;
                this.contentLength = MinimumPayloadSize();
            }
        }

        // WS standard allows 7 bits to represent message length
        private byte contentLength;
        private ushort appMessageCode;

        public SocketMessage(ApplicationMessageCode code)
        {
            this.AppMessageCode = (ushort) code;
        }

        /// <summary>
        /// Returns the Websocket frames that compose the current message, as per
        /// the websocket standard
        /// </summary>
        /// <remarks>The method currently supports only 1 frame messages</remarks>
        /// <returns>A List containing the frames of the message</returns>
        public List<SocketFrame> GetFrames()
        {
            byte[] payload = BitConverter.GetBytes((int)this.appMessageCode);
            List<SocketFrame> frames = new List<SocketFrame>(1);
            frames.Add(new SocketDataFrame(true, false, this.contentLength, this.MessageType, payload));

            return frames;
        }

        /// <summary>
        /// Returns a byte representing the minimum number of bytes needed to represent the AppMessageCode
        /// </summry>
        private byte MinimumPayloadSize()
        {
            byte minSize = 1;
            if ((ushort) this.appMessageCode >= 256) minSize = 2;

            return minSize;
        }
    }
}

/*

    0                   1                   2                   3
    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    +-+-+-+-+-------+-+-------------+-------------------------------+
    |F|R|R|R| opcode|M| Payload len |    Extended payload length    |
    |I|S|S|S|  (4)  |A|     (7)     |             (16/64)           |
    |N|V|V|V|       |S|             |   (if payload len==126/127)   |
    | |1|2|3|       |K|             |                               |
    +-+-+-+-+-------+-+-------------+ - - - - - - - - - - - - - - - +
    |     Extended payload length continued, if payload len == 127  |
    + - - - - - - - - - - - - - - - +-------------------------------+
    |                               |Masking-key, if MASK set to 1  |
    +-------------------------------+-------------------------------+
    | Masking-key (continued)       |          Payload Data         |
    +-------------------------------- - - - - - - - - - - - - - - - +
    :                     Payload Data continued ...                :
    + - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
    |                     Payload Data continued ...                |
    +---------------------------------------------------------------+

    OPCODES
        0x0 : Continuation
        0x1 : Text (UTF-8)
        0x2 : Binary
        0x9 : Ping
        0xA : Pong
 */