using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MissionPlanner.HIL
{
    public class MatLab : Hil, IDisposable
    {
        Socket SimulatorRECV;
        UdpClient MatLabSEND;
        EndPoint Remote = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));
        public override void SetupSockets(int recvPort, int SendPort, string simIP)
        {
            // setup receiver
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, recvPort);

            SimulatorRECV = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);

            SimulatorRECV.Bind(ipep);

            UpdateStatus(-1, "Listerning on port UDP " + recvPort + " (sim->planner)\n");


            // setup sender
            MatLabSEND = new UdpClient(simIP, SendPort);

            UpdateStatus(-1, "Sending to port UDP " + SendPort + " (planner->sim)\n");

            UpdateStatus(-1, "Sent xplane settings\n");
        }
        public override void Shutdown()
        {
            try
            {
                SimulatorRECV.Close();
            }
            catch
            {
            }
            try
            {
                MatLabSEND.Close();
            }
            catch
            {
            }
        }
        public override void GetFromSim()
        {

        }
        public override void SendToSim()
        {

        }
        public override void SendToAP(MAVLinkInterface port, MAVState mav)
        {

        }
        public override void GetFromAP()
        {

        }
        public void Dispose()
        {
            if (SimulatorRECV != null)
                SimulatorRECV.Dispose();
        }
    }
}
