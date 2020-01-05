using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketFrm
{
    public interface IClientUINotifier
    {
        void HandleDisplayTextServerMessage(DisplayTextServerMessage displayTextServerMessage);
        void HandleRegisterIDResultServerMessage(RegisterIdResultServerMessage registerIdResultServerMessage);
        void HandleClientAvailabilityNotificationServerMessage(ClientAvailabilityNotificationServerMessage clientAvailabilityNotificationServerMessage);
        void HandleTransmitToPeerResultServerMessage(TransmitToPeerResultServerMessage transmitToPeerResultServerMessage);
        void HandleTransmitToPeeServerMessage(TransmitToPeerServerMessage transmitToPeerServerMessage);
    }
}
