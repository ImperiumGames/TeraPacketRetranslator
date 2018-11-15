using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeraPacketRetranslator.Processing.Processors;
using TeraPacketRetranslator.Config;
using MapEntry = System.Tuple<  TeraPacketRetranslator.Config.ProcessorConfig.ProcessorType, 
                                TeraPacketRetranslator.Config.ProcessorConfig.ProcessorSource, 
                                TeraPacketRetranslator.Config.ProcessorConfig.ProcessorDestination,
                                System.Type>;
using TeraPacketRetranslator.Messages;

namespace TeraPacketRetranslator.Processing
{
    public static class ProcessingManager
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static Tree<PacketProcessor> m_ProcessingTree = new Tree<PacketProcessor>();

        static List<MapEntry> m_Map = new List<MapEntry>()
            {
                new MapEntry(
                    ProcessorConfig.ProcessorType.InputRaw,
                    ProcessorConfig.ProcessorSource.Dump,
                    ProcessorConfig.ProcessorDestination.Undefined,
                    typeof(DumpReader)
                ),

                new MapEntry(
                    ProcessorConfig.ProcessorType.InputRaw,
                    ProcessorConfig.ProcessorSource.Network,
                    ProcessorConfig.ProcessorDestination.Undefined,
                    typeof(SocketReader)
                ),

                new MapEntry(
                    ProcessorConfig.ProcessorType.RawOutput,
                    ProcessorConfig.ProcessorSource.Undefined,
                    ProcessorConfig.ProcessorDestination.Dump,
                    typeof(DumpWriter)
                ),

                new MapEntry(
                    ProcessorConfig.ProcessorType.RawTranslated,
                    ProcessorConfig.ProcessorSource.Undefined,
                    ProcessorConfig.ProcessorDestination.Undefined,
                    typeof(PacketTranslator)
                ),

                new MapEntry(
                    ProcessorConfig.ProcessorType.TranslatedOutput,
                    ProcessorConfig.ProcessorSource.Undefined,
                    ProcessorConfig.ProcessorDestination.Log,
                    typeof(LogWriter)
                ),

                new MapEntry(
                    ProcessorConfig.ProcessorType.TranslatedOutput,
                    ProcessorConfig.ProcessorSource.Undefined,
                    ProcessorConfig.ProcessorDestination.Network,
                    typeof(NetWriter)
                )

            };

        public static void Initialize()
        {
            if (Config.Config.Processors != null)
            {
                m_ProcessingTree.Children = Config.Config.Processors.Select(pc => CreateProcessorRecursive(pc)).Where(pp => pp != null).ToList();
            }            
        }



        static Tree<PacketProcessor> CreateProcessorRecursive(ProcessorConfig config)
        {
            var found = m_Map.FirstOrDefault(me =>
                me.Item1 == config.Type &&
                (me.Item2 == config.Source || me.Item2 == ProcessorConfig.ProcessorSource.Undefined) &&
                (me.Item3 == config.Destination || me.Item3 == ProcessorConfig.ProcessorDestination.Undefined)
            );

            
            Tree<PacketProcessor> tree = null;
            if (found != default(MapEntry))
            {
                PacketProcessor currentProcessor = null;
                try
                {
                    currentProcessor = Activator.CreateInstance(found.Item4, config) as PacketProcessor;
                }
                catch(Exception e)
                {
                    log.Error(e.ToString());
                }
                
                if (currentProcessor != null)
                {
                    tree = new Tree<PacketProcessor>()
                    {
                        Value = currentProcessor,
                        Children = config.Children.Select(pc => CreateProcessorRecursive(pc)).Where(pp => pp != null).ToList()
                    };
                    
                }

            }
            return tree;
        }

        public static void Update()
        {
            if (m_ProcessingTree == null) return;
            try
            {
                m_ProcessingTree.Children.ForEach(node => ProcessRecursive(node, null));
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
        }

        static void ProcessRecursive(Tree<PacketProcessor> node, object input)
        {
            if (node!= null)
            {
                object result = ProcessSingle(node.Value, input);
                if (result != null)
                {
                    node.Children.ForEach((child) => ProcessRecursive(child, result));
                }
            }
        }

        static object ProcessSingle(PacketProcessor processor, object input)
        {
            if (processor is PPInputRaw)
            {
                return (processor as PPInputRaw).Process();
            }
            if (processor is PPRawTranslated)
            {
                return ((processor as PPRawTranslated).CheckFilter(input as RawMessage)) ? 
                        (processor as PPRawTranslated).Process(input as RawMessage) :
                        null;
            }
            if (processor is PPRawOutput)
            {
                if ((processor as PPRawOutput).CheckFilter(input as RawMessage))
                    (processor as PPRawOutput).Process(input as RawMessage);
                return null;
            }
            if (processor is PPTranslatedTranslated)
            {
                return ((processor as PPTranslatedTranslated).CheckFilter(input as ParsedMessage)) ?
                       (processor as PPTranslatedTranslated).Process(input as ParsedMessage) :
                       null;
            }
            if (processor is PPTranslatedOutput)
            {
                if ((processor as PPTranslatedOutput).CheckFilter(input as ParsedMessage))
                    (processor as PPTranslatedOutput).Process(input as ParsedMessage);
                return null;
            }
            return null;
        }


    }
}
