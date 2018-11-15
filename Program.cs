using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.Web.Script.Serialization;
using System.Net.Http;
using TeraPacketRetranslator.Extractor;
using TeraPacketRetranslator.Config;
using TeraPacketRetranslator.Messages;
using TeraPacketRetranslator.Parser;
using TeraPacketRetranslator.PacketLog;
using TeraPacketRetranslator.Processing;

namespace ShinraFork
{
    class Program
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static TeraPacketRetranslator.PacketLog.PacketLogWriter m_Writer;

        static void Main(string[] args)
        {

            ProcessingManager.Initialize();
            var thread = new Thread(MainLoop);
            thread.Start();
            
            Console.ReadKey();
        }


        static void MainLoop()
        {
            try
            {
                while (true)
                {
                    ProcessingManager.Update();
                    Thread.Sleep(1);
                }
            }
            catch (Exception e)
            {
                log.Debug(e.ToString());
            }

        }
    }
}

