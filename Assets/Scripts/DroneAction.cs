using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class DroneAction : MonoBehaviour
{
    public int MavId;

    byte[] buf = new byte[1024];
    MAVLink.MavlinkParse mavlinkParse = new MAVLink.MavlinkParse();
    Socket sock;
    IPEndPoint myproxy;

    private void Awake()
    {
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
        {
            Blocking = false
        };
        sock.Bind(new IPEndPoint(IPAddress.Any, 17800 + MavId));
        myproxy = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 17500);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetVelcoty(Vector3 vel)
    {
        var msg = new MAVLink.mavlink_set_position_target_local_ned_t()
        {
            coordinate_frame = 1,
            type_mask = 3527,
            vx = -vel.x,
            vy = vel.z,
            vz = 0
        };
        byte[] data = mavlinkParse.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.SET_POSITION_TARGET_LOCAL_NED, msg);
        sock.SendTo(data, myproxy);
    }

    public void SetTarget(Vector3 tgt)
    {
        var msg = new MAVLink.mavlink_set_position_target_local_ned_t()
        {
            coordinate_frame = 1,
            type_mask = 3576,
            x = -tgt.x,
            y = tgt.z,
            z = -tgt.y
        };
        byte[] data = mavlinkParse.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.SET_POSITION_TARGET_LOCAL_NED, msg);
        sock.SendTo(data, myproxy);
    }

    // Update is called once per frame
    void Update()
    {
        while (sock.Available > 0)
        {
            int recvBytes = 0;
            try
            {
                recvBytes = sock.Receive(buf);
            }
            catch (SocketException e)
            {
                Debug.LogWarning(e.Message);
                break;
            }
            if (recvBytes > 0)
            {
                byte[] msg_buf = new byte[recvBytes];
                System.Buffer.BlockCopy(buf, 0, msg_buf, 0, recvBytes);
                MAVLink.MAVLinkMessage msg = mavlinkParse.ReadPacket(msg_buf);
                if (msg != null)
                {
                    switch (msg.msgid)
                    {
                        case (uint)MAVLink.MAVLINK_MSG_ID.STATUSTEXT:
                            {
                                var status_txt = (MAVLink.mavlink_statustext_t)msg.data;
                                //Debug.Log(System.Text.Encoding.ASCII.GetString(status_txt.text));
                                break;
                            }
                        case (uint)MAVLink.MAVLINK_MSG_ID.HEARTBEAT:
                            {
                                if (msg.sysid == MavId)
                                {
                                }
                                break;
                            }
                        case (uint)MAVLink.MAVLINK_MSG_ID.ATT_POS_MOCAP:
                            {
                                var att_pos = (MAVLink.mavlink_att_pos_mocap_t)msg.data;
                                //convert from optitrack motive coordinate system to unity coordinate system
                                var pos = new Vector3(-att_pos.x, att_pos.y, att_pos.z);
                                var rot = new Quaternion(-att_pos.q[1], att_pos.q[2], att_pos.q[3], -att_pos.q[0]);
                                transform.SetPositionAndRotation(pos, rot);
                                break;
                            }
                    }
                }
            }
        }
    }
}
