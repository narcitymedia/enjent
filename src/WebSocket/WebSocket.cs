using System;
using System.Text;

namespace NarcityMedia.Enjent
{
    /// <summary>
    /// Enumerates the possible OPCodes of a WebSocket frame as described in RFC 6455
    /// </summary>
    public enum WebSocketOPCode
    {
        /// <summary>
        /// Represents the continuation of a message that was sent over two WebSocket frames
        /// </summary>
        Continuation = 0x0,

        /// <summary>
        /// Indicates a text payload
        /// </summary>
        Text = 0x1,

        /// <summary>
        /// Indicates a binary payload
        /// </summary>
        Binary = 0x2,

        /// <summary>
        /// Indicates a closing frame
        /// </summary>
        Close = 0x8,

        /// <summary>
        /// Indicates a PING frame
        /// </summary>
        Ping = 0x9,

        /// <summary>
        /// Indicates a PONG frame
        /// </summary>
        Pong = 0xA
    }

    /// <summary>
    /// Represents WebSocket close codes as defined by the RFC 6455 specification
    /// </summary>
    public enum WebSocketCloseCode
    {
        /// <summary>
        /// Indicates a normal closure, meaning that the purpose for
        /// which the connection was established has been fulfilled.
        /// </summary>
        NormalClosure = 1000,

        /// <summary>
        /// Indicates that an endpoint is "going away", such as a server
        /// going down or a browser having navigated away from a page.
        /// </summary>
        GoingAway = 1001,
    
        /// <summary>
        /// Indicates that an endpoint is terminating the connection due
        /// to a protocol error.
        /// </summary>
        ProtocolError = 1002,

        /// <summary>
        /// Indicates that an endpoint is terminating the connection
        /// because it has received a type of data it cannot accept (e.g., an
        /// endpoint that understands only text data MAY send this if it
        /// receives a binary message).
        /// </summary>
        UnacceptableDataType = 1003,

        // 1004 is undefined

        /// <summary>
        /// Is a reserved value and MUST NOT be set as a status code in a
        /// Close control frame by an endpoint.  It is designated for use in
        /// applications expecting a status code to indicate that no status
        /// code was actually present.
        /// </summary>
        NoCloseCode = 1005,

        /// <summary>
        /// is a reserved value and MUST NOT be set as a status code in a
        /// Close control frame by an endpoint.  It is designated for use in
        /// applications expecting a status code to indicate that the
        /// connection was closed abnormally, e.g., without sending or
        /// receiving a Close control frame.
        /// </summary>
        AbnormalClose = 1006,

        /// <ummary>
        /// Indicates that an endpoint is terminating the connection
        /// because it has received data within a message that was not
        /// consistent with the type of the message (e.g., non-UTF-8 [RFC3629]
        /// data within a text message).
        /// </summary>
        InconsistentDataType = 1007,

        /// <summary>
        /// Indicates that an endpoint is terminating the connection
        /// because it has received a message that violates its policy.  This
        /// is a generic status code that can be returned when there is no
        /// other more suitable status code (e.g., 1003 or 1009) or if there
        /// is a need to hide specific details about the policy.
        /// </summary>
        PolicyViolation = 1008,

        /// <summary>
        /// Indicates that an endpoint is terminating the connection
        /// because it has received a message that is too big for it to
        /// process.
        /// </summary>
        MessageSizeExceeded = 1009,

        /// <summary>
        /// dicates that an endpoint (client) is terminating the
        /// connection because it has expected the server to negotiate one or
        /// more extension, but the server didn't return them in the response
        /// message of the WebSocket handshake.  The list of extensions that
        /// are needed SHOULD appear in the /reason/ part of the Close frame.
        /// Note that this status code is not used by the server, because it
        /// can fail the WebSocket handshake instead.
        /// </summary>
        ExtensionNegotiationFailure = 1010,

        /// <summary>
        /// ndicates that a server is terminating the connection because
        /// it encountered an unexpected condition that prevented it from
        /// fulfilling the request.
        /// </summary>
        UnexpectedCondition = 1011,

        // 1012 is undefined
        // 1013 is undefined
        // 1014 is undefined

        /// <summary>
        /// is a reserved value and MUST NOT be set as a status code in a
        /// Close control frame by an endpoint.  It is designated for use in
        /// applications expecting a status code to indicate that the
        /// connection was closed due to a failure to perform a TLS handshake
        /// (e.g., the server certificate can't be verified).
        /// </summary>
        TLSHandshakeFailure = 1015
    }

	/// <summary>
	/// Represents types of WebSocket payload data
	/// </summary>
	public enum WebSocketDataType { Text, Binary }

    /// <summary>
    /// Represents a general concept of a WebSocket frame described by the 
    /// WebSocket standard
    /// </summary>
    public abstract partial class WebSocketFrame
    {
        /// <summary>
        /// Indicates whether the current WebSocketFrame is the last one of a message
        /// </summary>
        public bool Fin;

		/// <summary>
        /// Indicates whether the current WebSocketFrame is masked.
        /// Frames comming from the client must be masked whereas frames sent from the server must NOT be masked
        /// </summary>
        public bool Masked;

		/// <summary>
		/// Represents the masking key used to mask the current frame if <see cref="Masked" /> is true
		/// </summary>
		/// <remarks>
		/// The <see cref="GetBytes" /> method ignores this field if <see cref="Masked" /> is false
		/// </remarks>
		public byte[] MaskingKey;

        /// <summary>
        /// The OPCode of the current WebSocketFrame
        /// </summary>
        public WebSocketOPCode OpCode;

		protected byte[] _payload;

		public byte[] Payload
		{
			get { return _payload; }
			set { this._payload = value; }
		}

		public WebSocketFrame(bool fin, byte[] payload, byte[] maskingKey) : this(fin, false, payload, maskingKey)
		{
			this.MaskingKey = new byte[4];
			CryptoRandomSingleton.Instance.GetNonZeroBytes(this.MaskingKey);
		}

        /// <summary>
        /// Initializes a new instance of the WebSocketFrame class
        /// </summary>
        /// <param name="fin">Indicates whether the current WebSocketFrame is the last one of a multi frame message</param>
        /// <param name="masked">Whether or not the current frame should be masked.true All frames sent by a client should be masked</param>
        /// <param name="payload">Payload of the current WebSocketFrame</param>
        /// <remarks>
        /// If true is passed in as the 'masked' parameter, <see cref="this.MaskingKey" />
        /// will be initialized to a byte array of length 4 which is derived from a secure source of randomness.
        /// Else, <see cref="this.MaskingKey" /> will be initialized to an empty byte array
        /// </remarks>
        public WebSocketFrame(bool fin, bool masked, byte[] payload, byte[] maskingKey)
        {
			if (maskingKey == null)
				throw new ArgumentNullException(nameof(maskingKey));
			if (maskingKey.Length != 4)
				throw new ArgumentException("A masking key should have a length of exactly 4 bytes", nameof(maskingKey));

            this.Fin = fin;
			this.Masked = masked;
			this._payload = payload ?? new byte[0];
			this.MaskingKey = maskingKey;
        }

        /// <summary>
        /// Returns the bytes representation of the data frame
        /// </summary>
        /// <returns>Returns the bytes that form the data frame</returns>
        public byte[] GetBytes()
        {
			// It's useful to determine the exact length of the frame beforehand
			// to avoid some potential memory re-allocation
			byte[] frameBytes = new byte[this.PredictFrameLength()];

            // First octet has 1 bit for FIN, 3 reserved, 4 for OP Code
            byte octet0 = (byte) ((this.Fin) ? 0b10000000 : 0b00000000);
            octet0 = (byte) ( octet0 | (byte) this.OpCode );

            byte octet1 = (byte) ((this.Masked) ? 0b10000000 : 0b00000000);
            octet1 = (byte) ( octet1 | ( this.Payload.Length <= 125 ? this.Payload.Length : this.Payload.Length <= ushort.MaxValue ? 126 : 127 ) );

            frameBytes[0] = octet0;
            frameBytes[1] = octet1;

			byte[] contentLengthBytes = this.GetContentLengthBytes();
			contentLengthBytes.CopyTo(frameBytes, 2);
			
			if (this.Masked && this.Payload.Length > 0)
			{
				this.MaskingKey.CopyTo(frameBytes, 2 + contentLengthBytes.Length);
			}

			int payloadWriteStart = 2 + contentLengthBytes.Length + ( this.Masked ? 4 : 0 );
			this.Payload.CopyTo(frameBytes, payloadWriteStart);

            return frameBytes;
        }

		/// <summary>
		/// Returns the number of bytes necessary to represent the current
		/// frame as a byte array
		/// </summary>
		/// <returns>
		/// The number of bytes
		/// </returns>
		protected int PredictFrameLength()
		{
			return (
				2
				+ ( this.Payload.Length <= 125 ? 0 : this.Payload.Length <= ushort.MaxValue ? 2 : 4 )
				+ ( this.Masked ? 4 : 0 )
				+ this.Payload.Length
			);
		}

		/// <summary>
		/// Returns an array of bytes that represent the length of to be inserted in a 
		/// WebSocket frame as the content length.
		/// The returned byte array will be of length:
		/// 0 if the current frame's content length is smaller than or equal to 125
		/// 2 if the current frame's content length is smaller than or equal to 65,535 (ushort)
		/// 4 if the current frame's content length is smaller than or equal to 4,294,967,295 (uint)
		/// </summary>
		/// <returns>An array of bytes that can be inserted in the current frame to represent the content length, if any</returns>
		/// <remark>The length of a frame is a uint hence will never be bigger</remark>
		protected byte[] GetContentLengthBytes()
		{
			byte[] contentLengthBytes;
			if (this.Payload.Length <= 125)
			{
				contentLengthBytes = new byte[0];
			}
            // Length must be encoded in network byte order
        	else if (this.Payload.Length <= ushort.MaxValue)
			{
				byte[] encodedLength = BitConverter.GetBytes(this.Payload.Length);
				contentLengthBytes = new byte[2] { encodedLength[1], encodedLength[0] };
			}
			else
			{
				byte[] encodedLength = BitConverter.GetBytes(this.Payload.Length);
				contentLengthBytes = new byte[4] { encodedLength[3], encodedLength[2], encodedLength[1], encodedLength[0] };
			}

			return contentLengthBytes;
		}

        /// <summary>
        /// Appends the payload to the header of the data frame
        /// </summary>
        /// <param name="header">Bytes of the frame header</param>
        /// <param name="message">Bytes of the frame message</param>
        /// <returns>All the bytes of the frame</returns>
        private byte[] AppendContentToHeader(byte[] header, byte[] message)
        {
            byte[] payload = new byte[header.Length + message.Length];
            header.CopyTo(payload, 0);
            message.CopyTo(payload, header.Length);

            return payload;
        }
    }

	public class WebSocketContinuationFrame : WebSocketFrame
	{
		public WebSocketContinuationFrame(bool fin, byte[] payload) : this(fin, payload, new byte[4])
		{}

        public WebSocketContinuationFrame(bool fin, byte[] payload, byte[] maskingKey) : base(fin, masked, payload, maskingKey)
		{
			this.OpCode = WebSocketOPCode.Continuation;
		}

		public static WebSocketContinuationFrame FromDataFrame(WebSocketDataFrame dataFrame)
		{
			if (dataFrame.Masked)
			{
				return new WebSocketContinuationFrame(dataFrame.Fin, dataFrame.Mas)
			}
			else
			{
				return new WebSocketContinuationFrame(dataFrame.Fin, dataFrame.Masked, dataFrame.Payload);
			}
		}
	}

	public abstract class WebSocketDataFrame : WebSocketFrame
	{
		public readonly WebSocketDataType DataType;

		public WebSocketDataFrame(bool fin, bool masked, byte[] payload, WebSocketDataType dataType) : base(fin, masked, payload)
		{
			this.DataType = dataType;
			this.OpCode = dataType == WebSocketDataType.Binary ? WebSocketOPCode.Binary : WebSocketOPCode.Text;
		}
	}

    /// <summary>
    /// Represents a WebSocket frame that contains binary data
    /// </summary>
    public class WebSocketBinaryFrame : WebSocketDataFrame
    {
		public WebSocketBinaryFrame() : this(new byte[0]) {}

		public WebSocketBinaryFrame(byte[] payload) : this(payload, false, false)
		{}

        /// <summary>
        /// Initializes a new instance of the SocketDataFrame class
        /// </summary>
        /// <param name="fin">Indicates whether the current SocketDataFrame is the last one of a multi frame message</param>
        /// <param name="masked">Indicates whether the current SocketDataFrame should be masked or not</param>
        /// <param name="payload">Payload of the current WebSocketFrame</param>
        /// <param name="dataType">The data type of the current SocketDataFrame</param>
        public WebSocketBinaryFrame(byte[] payload, bool fin, bool masked) : base(fin, masked, payload, WebSocketDataType.Binary)
        {}
    }

	public class WebSocketTextFrame : WebSocketDataFrame
	{
		new public readonly WebSocketDataType DataType = WebSocketDataType.Text;
		
		new public byte[] Payload
		{
			get { return this._payload; }
			set
			{
				this._plaintext = Encoding.UTF8.GetString(value);
				this._payload = value;
			}
		}

		private string _plaintext;

		public string Plaintext
		{
			get { return this._plaintext; }
			set
			{
				this._payload = Encoding.UTF8.GetBytes(value);
				this._plaintext = value;
			}
		}

		public WebSocketTextFrame() : this(String.Empty) {}

		public WebSocketTextFrame(string plaintext) : this(true, false, plaintext)
		{}

		public WebSocketTextFrame(bool fin, bool masked, string plaintext) : base(fin, masked, new byte[0], WebSocketDataType.Text)
		{
			this._plaintext = plaintext;
			this._payload = Encoding.UTF8.GetBytes(plaintext);
		}
	}

    /// <summary>
    /// Represents a WebSocket control frame as described in RFC 6455
    /// </summary>
    public abstract class WebSocketControlFrame : WebSocketFrame
    {
        /// <summary>
        /// Initializes a new instance of the SocketControlFrame class
        /// </summary>
        /// <param name="controlOpCode">The control OPCode of the current SocketControlFrame</param>
        public WebSocketControlFrame(WebSocketOPCode controlOpCode) : this(controlOpCode, new byte[0], false)
        {}

		/// <summary>
		/// Initializes a new instance of the SocketControlFrame class
		/// </summary>
		/// <param name="controlOpCode">The control OPCode of the current SocketControlFrame</param>
		/// <param name="masked">Whether or not the current control frame is masked</param>
		public WebSocketControlFrame(WebSocketOPCode controlOpCode, byte[] payload, bool masked) : base(true, masked, payload)
		{
			if (payload.Length > 125)
				throw new ArgumentException("A control frame's Payload of 125 bytes or less", nameof(payload));

			this.OpCode = controlOpCode;
		}
    }

    /// <summary>
    /// Represents a WebSocket Close Control Frame as defined by RFC 6455
    /// </summary>
    public class WebSocketCloseFrame : WebSocketControlFrame
    {
        private string? _closeReason;
        public WebSocketCloseCode CloseCode;

        /// <summary>
        /// (Optionnal) indicates a reason for the WebSocket connexion closing.
        /// This text is not necessarily 'human readable' and should ideally not
        /// be shown to the end user but may be useful for debugging.
        /// </summary>
        /// <value>The string representing the close reason</value>
        public string? CloseReason { get { return this._closeReason; } }

        public WebSocketCloseFrame() : this(WebSocketCloseCode.NormalClosure)
        {}

        public WebSocketCloseFrame(WebSocketCloseCode closeCode) : this(closeCode, String.Empty)
        {}

		public WebSocketCloseFrame(WebSocketCloseCode closeCode, string closeReason) : this(closeCode, closeReason, false)
		{}

		public WebSocketCloseFrame(WebSocketCloseCode closeCode, string closeReason, bool masked) : base(WebSocketOPCode.Close, new byte[0], masked)
		{
			this.SetCloseReason(closeCode, closeReason);
		}

		/// <summary>
		/// Sets the <see cref="CloseCode" /> and <see cref="CloseReason" /> of the current instance.
		/// </summary>
		/// <param name="closeCode">The WebSocket close code to set for the current instance</param>
		/// <param name="closeReason">The close reason to set as the current instance</param>
		/// <remarks>
		/// As per the WebSocket specification, it is mandatory to specify a close code if a close reason is present
		/// hence there is no overload of this mwethod that accepts only a close reason
		/// /<remarks>
		public void SetCloseReason(WebSocketCloseCode closeCode, string closeReason)
		{
            this._closeReason = closeReason ?? String.Empty;
            this.CloseCode = closeCode;
		}
    }

	public class WebSocketPingFrame : WebSocketControlFrame
	{
		public WebSocketPingFrame() : this(new byte[0])
		{}

		public WebSocketPingFrame(byte[] payload) : this(payload, false)
		{}

		public WebSocketPingFrame(byte[] payload, bool masked) : base(WebSocketOPCode.Ping, payload, masked)
		{}
	}

	public class WebSocketPongFrame : WebSocketControlFrame
	{
		public WebSocketPongFrame() : this(new byte[0])
		{}

		public WebSocketPongFrame(byte[] payload) : this(payload, false)
		{}

		public WebSocketPongFrame(byte[] payload, bool masked) : base(WebSocketOPCode.Pong, payload, masked)
		{}
	}

	internal static class NetworkByteOrderByteArrayExtensions
	{
		public static void EnsureNetworkByteOrder(this byte[] that)
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(that);
			}
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
        0x8 : Close
        0x9 : Ping
        0xA : Pong
 */
