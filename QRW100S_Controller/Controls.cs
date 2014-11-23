using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Net.Sockets;

namespace QRW100S_Controller
{
    class Controls
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool deviceActive = false;
        private byte[] data = new byte[0];
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        /// <summary>
        /// 1-50 height (rodar speed)
        /// </summary>
        private int height = 1;
        public int Height
        {
            get { return height; }
            set { height = value; }
        }
        public enum Cmd
        {
            Start,
            Stop,
            Test,
        }

        public void Execute(Cmd cmd)
        {
            byte[] data = new byte[0];
            switch(cmd)
            {
                case Cmd.Start:
                    StartControls();
                    break;
                case Cmd.Stop:
                    StopControls();
                    break;
                case Cmd.Test:
                    int times = 0;
                    while (times < 300)
                    {
                        //byte h1 = BitConverter.GetBytes(43 + Height / 16)[0];
                        byte h1 = BitConverter.GetBytes(0)[0];
                        byte h2 = BitConverter.GetBytes(Height + 42)[0];
                        SendCommand(new byte[] { h1, h2, 0x04, 0x4a, 0x04, 0x4a, 0x04, 0x4a, 0x04, 0x4a, h1, h2, 0x02, 0xbc, 0x02, 0xbc });
                        times++;
                    }
                    break;
            }
        }
        /// <summary>
        /// convert command to data packet & sends it
        /// </summary>
        /// <param name="data">16 control bits</param>
        /// <param name="important">true: command will try to execute 5 times if failed</param>
        private void SendCommand(byte[] data, bool important = false)
        {
            // Prepare packet
            List<byte> command = new List<byte>();
            command.Add(97);                            // add initializer
            foreach (byte b in data)
                command.Add(b);
            byte[] noChecksum = command.ToArray();
            byte checksum = CalcCrc(noChecksum);
            command.Add(checksum);    // Add checksum to data
            data = command.ToArray();                   // convert back to array

            // Send packet
            bool success = false;
            int tries = 0;
            do
            {
                try
                {
                    tries++;
                    stream.Write(data, 0, data.Length);
                    success = true;
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("ArgumentNullException: {0}", e);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }
                //out of range?
                catch (System.IO.IOException e)
                {
                    Console.WriteLine("IOException: {0}", e);
                    Console.WriteLine("Device out of range?");
                }
            } while (important == true && success == false && tries <= 5);
        }

        public void StartControls()
        {
            Thread t = new Thread(() => _StartControls());
            t.Start();
        }

        private void _StartControls()
        {
            if (deviceActive) return;
            deviceActive = true;

            client = new TcpClient("192.168.10.1", 2001);
            stream = client.GetStream();

            // Activate rodars
            SendCommand(new byte[] { 0x05, 0x51, 0x04, 0x4a, 0x04, 0x4a, 0x04, 0x4a, 0x04, 0x4a, 0x05, 0x51, 0x02, 0xbc, 0x02, 0xbc });

            do
            {
                byte[] toSend = Data;
                Data = new byte[] { 0x02, 0xbf, 0x04, 0x4a, 0x04, 0x4a, 0x04, 0x4a, 0x04, 0x4a, 0x02, 0xbf, 0x02, 0xbc, 0x02, 0xbc }; // idle command
                SendCommand(toSend);
                Thread.Sleep(100); // Command interval
            } while (deviceActive);

            stream.Close();
            client.Close();
            //w data = new byte[] { 0x61, 0x02, 0xbf, 0x04, 0x4a, 0x04, 0x4a, 0x04, 0x4a, 0x04, 0x4a, 0x02, 0xbf, 0x02, 0xbc, 0x02, 0xbc, 0x97, 0x61, 0x02, 0xbf, 0x04, 0x4a, 0x04, 0x4a, 0x04, 0x4a, 0x04, 0x4a, 0x02, 0xbf, 0x02, 0xbc, 0x02, 0xbc, 0x97 };
        }


        public void StopControls()
        {
            deviceActive = false;
        }

        // Calculate checksum, byte 18 of command
        static byte CalcCrc(byte[] input)
        {
            byte[] Result = new byte[input.Length + 3];
            Array.Copy(input, Result, input.Length);
            int crc = 0;
            for (int i = 0; i < input.Length; i++)
                crc += input[i];
            crc &= 0xff;
            return BitConverter.GetBytes(crc)[0];
        }
    }
}
