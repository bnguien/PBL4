using Common.Enums;
using Common.Models;

namespace Common.Networking
{

    public sealed class SystemInfoRequest : BasePacket
    {
        public SystemInfoRequest()
        {
            PacketType = PacketType.SystemInfoRequest;
        }

        public bool IncludeHardware { get; set; } = true;
        public bool IncludeNetwork { get; set; } = true;
        public bool IncludeSoftware { get; set; } = true;
    }



//-------------------------------------------//


    public sealed class SystemInfoResponse : BasePacket
    {
        public SystemInfoResponse()
        {
            PacketType = PacketType.SystemInfoResponse;
        }

        public ResponseStatusType Status { get; set; } = ResponseStatusType.Unknown;
        public SystemInfoModel? Payload { get; set; }
        public int? PayloadSizeBytes { get; set; }
        public string? ErrorMessage { get; set; }
    }
}


