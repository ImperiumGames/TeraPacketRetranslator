using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TeraPacketRetranslator.Config
{
    public class ProcessorConfig
    {
        public enum ProcessorSource
        {
            Undefined = 0,
            Network,
            Dump
        }

        public enum ProcessorDestination
        {
            Undefined = 0,
            Network,
            Dump,
            Log
        }

        public enum ProcessorType
        {
            Undefined = 0,
            InputRaw,
            RawTranslated,
            RawOutput,
            TranslatedTranslated,
            TranslatedOutput
        }

        public ProcessorType Type { get; private set; }
        public ProcessorSource Source { get; private set; }
        public ProcessorDestination Destination { get; private set; }
        public PacketFilterConfig Filter { get; private set; }
        public string Path { get; private set; }
        public int Version { get; private set; }

        public string Settings { get; private set; }

        public List<ProcessorConfig> Children;

        public ProcessorConfig(XElement elem)
        {
            var typeAttr = elem.Attribute("type");
            if (typeAttr != null)
            {
                string typeStr = typeAttr.Value.Replace("-","");
                Type = (ProcessorType)Enum.Parse(typeof(ProcessorType), typeStr, true);
            }

            var sourceAttr = elem.Attribute("source");
            if (sourceAttr != null)
                Source = (ProcessorSource)Enum.Parse(typeof(ProcessorSource), sourceAttr.Value, true);

            var destAttr = elem.Attribute("destination");
            if (destAttr != null)
                Destination = (ProcessorDestination)Enum.Parse(typeof(ProcessorDestination), destAttr.Value, true);

            var pathAttr = elem.Attribute("path");
            if (pathAttr != null)
                Path = pathAttr.Value;

            var versionAttr = elem.Attribute("version");
            if (versionAttr != null)
                Version = Int32.Parse(versionAttr.Value);

            var settingsAttr = elem.Attribute("settings");
            Settings = (settingsAttr != null) ? settingsAttr.Value : string.Empty;

            var filterElem = elem.Element("filter");
            if (filterElem != null)
            {
                Filter = new PacketFilterConfig(filterElem);
            }

            Children = elem.Elements("processor").Select(e => new ProcessorConfig(e)).ToList();

            //Validity check
            switch (Type)
            {
                case ProcessorType.Undefined:
                    throw new Exception("A processor node has no type attribute");
                    break;
                case ProcessorType.InputRaw:
                    if (Source == ProcessorSource.Undefined)
                        throw new Exception("A processor node with type input-raw has no source attribute");
                    if (Children.Any(c => c.Type != ProcessorType.RawTranslated && c.Type != ProcessorType.RawOutput))
                        throw new Exception("One ore more children of source-raw processor doesn't accept raw as a source");
                    break;
                case ProcessorType.RawTranslated:
                    if (Children.Any(c => c.Type != ProcessorType.TranslatedOutput && c.Type != ProcessorType.TranslatedTranslated))
                        throw new Exception("One ore more children of raw-translated processor doesn't accept translated as a source");
                    if (Version == 0)
                        throw new Exception("One ore more raw-translated processor doesn't have a version specified");
                    break;
                case ProcessorType.RawOutput:
                case ProcessorType.TranslatedOutput:
                    if (Destination == ProcessorDestination.Undefined)
                        throw new Exception("A processor node with type raw-output or translated-output has no destination attribute");
                    break;

            }

            if ((Source == ProcessorSource.Dump || Destination == ProcessorDestination.Dump || Destination == ProcessorDestination.Network) &&
                (string.IsNullOrEmpty(Path)))
                throw new Exception("A processor node with dump or network out doesn't contain path attribute");

        }

    }
}
