using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryClassLibrary.IO
{
    public class Packet
    {
        private Object _data { get; set; }
        public Packet(Object data) {
            this._data = data;
        }

        // Compute the packet size in bytes
        public int ComputePacketSize() {
            string jsonstr = System.Text.Json.JsonSerializer.Serialize(this._data);
            //Console.WriteLine(jsonstr);
            //Console.WriteLine(System.Text.Encoding.Unicode.GetBytes(jsonstr).Length);
            int packet_size = System.Text.Encoding.Unicode.GetBytes(jsonstr).Length;
            return packet_size;
        }

    }
}
