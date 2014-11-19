//-----------------------------------------------------------------------------
// Filename: RTPVP8Header.cs
//
// Description: Represents the RTP header to use for a VP8 encoded payload as per
// http://tools.ietf.org/html/draft-ietf-payload-vp8-12.
//
//
// History:
// 11 Nov 2014	Aaron Clauson	Created.
//
// License: 
// Aaron Clauson
//-----------------------------------------------------------------------------

using System;

namespace SIPSorcery.Net
{
     /// <summary>
    /// Exmaples of size Payload Header size calculations:
    /// 
    /// For length of first parition 54: S0 = 4, S1 = 0x32, S2 = 0.
    /// For length of first parition 1777: S0 = 1, S1 = 0xde, S2 = 0.
    /// 
     /// </summary>
    public class RTPVP8Header
    {
        // Payload Descriptor Fields.
        public bool ExtendedControlBitsPresent;     // Indicated whether extended control bits are present.
        public bool NonReferenceFrame;              // When set indicates the frame can be discarded wihtout affecting any other frames.
        public bool StartOfVP8Partition;            // Should be set when the first payload octet is the start of a new VP8 partition.
        public byte PartitionIndex;                 // Denotes the VP8 partition index that the first payload octet of the packet belongs to.
        public bool IsPictureIDPresent;
        public byte PictureID;

        // Payload Header Fields.
        public int FirstPartitionSize;              // The size of the first partition in bytes is calculated from the 19 bits in Size0, SIze1 & Size2 as: size = Size0 + (8 x Size1) + (2048 8 Size2).
        public bool ShowFrame;
        public int VersionNumber;
        public bool IsKeyFrame;

        private int _length = 0;
        public int Length
        {
            get { return _length;  }
        }

        public RTPVP8Header()
        {

        }

        public static RTPVP8Header GetVP8Header(byte[] rtpPayload)
        {
            RTPVP8Header vp8Header = new RTPVP8Header();
            int payloadHeaderStartIndex = 1;

            // First byte of payload descriptor.
            vp8Header.ExtendedControlBitsPresent = ((rtpPayload[0] >> 7) & 0x01) == 1;
            vp8Header.StartOfVP8Partition = ((rtpPayload[0] >> 4) & 0x01) == 1;
            vp8Header._length = 1;
            
            // Is second byye being used.
            if(vp8Header.ExtendedControlBitsPresent)
            {
                vp8Header.IsPictureIDPresent = ((rtpPayload[1] >> 7) & 0x01) == 1;
                vp8Header._length = 2; 
                payloadHeaderStartIndex = 2;
            }

            // Is third byte being used.
            if(vp8Header.IsPictureIDPresent)
            {
                vp8Header.PictureID = rtpPayload[2];
                vp8Header._length = 3;
                payloadHeaderStartIndex = 3;
            }

            // Payload header only on first packet in frame.
            if (vp8Header.StartOfVP8Partition)
            {
                byte s0 = (byte)((rtpPayload[payloadHeaderStartIndex] >> 5) & 0x07);
                vp8Header.ShowFrame = (byte)((rtpPayload[payloadHeaderStartIndex] >> 4) & 0x01) == 1;
                vp8Header.IsKeyFrame = (rtpPayload[payloadHeaderStartIndex] & 0x01) == 0;
                byte s1 = rtpPayload[payloadHeaderStartIndex + 1];
                byte s2 = rtpPayload[payloadHeaderStartIndex + 2];
                vp8Header.FirstPartitionSize = s0 + (8 * s1) + (2048 * s2);

                vp8Header._length += 3;
            }

            return vp8Header;
        }

        public byte[] GetBytes()
        {
            if (!StartOfVP8Partition)
            {
                if (IsPictureIDPresent)
                {
                    _length = 3;
                    return new byte[] { 0x80, 0x80, PictureID };
                }
                else
                {
                    // No partition header on continuation packets.
                    _length = 1;
                    return new byte[] { 0x00 };
                }
            }
            else
            {
                byte[] payloadDescriptor = null;

                if (IsPictureIDPresent)
                {
                    _length = 3;
                    payloadDescriptor = new byte[] { 0x90, 0x80, PictureID };
                }
                else
                {
                    _length = 1;
                    payloadDescriptor = new byte[] { 0x10 };
                }

                byte s2 = (byte)(FirstPartitionSize / 2048);
                byte s1 = (byte)((FirstPartitionSize - (s2 * 2048)) / 8);
                byte s0 = (byte)(((FirstPartitionSize - (s2 * 2048) - (s1 * 8)) << 5) & 0xe0);

                byte firstBytePH = (byte)(s0 + ((ShowFrame) ? 0x10 : 0x00));
                firstBytePH = (byte)(firstBytePH + ((IsKeyFrame) ? 0x00 : 0x01));

                byte[] payloadHeader = new byte[] { firstBytePH, s1, s2 };

                byte[] vp8Header = new byte[payloadDescriptor.Length + payloadHeader.Length];

                Buffer.BlockCopy(payloadDescriptor, 0, vp8Header, 0, payloadDescriptor.Length);
                Buffer.BlockCopy(payloadHeader, 0, vp8Header, payloadDescriptor.Length, payloadHeader.Length);

                _length += 3;

                return vp8Header;
            }
        }
    }
}
