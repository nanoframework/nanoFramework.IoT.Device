// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.MulticastDNS.Enum;
using Iot.Device.MulticastDNS.Package;
using System.Collections;

namespace Iot.Device.MulticastDNS.Entities
{
    /// <summary>
    /// The class whom represents a Multicast DNS message.
    /// </summary>
    public class Message
    {
        private static readonly System.Random s_generator = new();

        private ushort _id;
        private ushort _flags = 0;

        /// <summary>
        /// The list of <see cref="Question">Questions</see> in the message.
        /// </summary>
        protected ArrayList questions = new();
        /// <summary>
        /// The list of <see cref="Resource">Answers</see> in the message.
        /// </summary>
        protected ArrayList answers = new();
        /// <summary>
        /// The list of <see cref="Resource">Servers</see> in the message.
        /// </summary>
        protected ArrayList servers = new();
        /// <summary>
        /// The list of <see cref="Resource">Additional Resources</see> in the message.
        /// </summary>
        protected ArrayList additionals = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Message" /> class.
        /// </summary>
        public Message() : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message" /> class.
        /// </summary>
        /// <param name="flags">The flags to be added.</param>
        public Message(ushort flags)
        {
            _id = (ushort)s_generator.Next(1 << 16);
            _flags |= flags;
        }

        /// <summary>
        /// Initializes an instance of Message.
        /// </summary>
        /// <param name="data">The byte[] containing a message.</param>
        public Message(byte[] data) => ParseData(data);

        /// <summary>
        /// Returns the <see cref="Question">Questions</see> in this message.
        /// </summary>
        /// <returns>An array of Questions.</returns>
        public Question[] GetQuestions() => (Question[])questions.ToArray(typeof(Question));

        /// <summary>
        /// Returns the <see cref="Resource">Resources</see> in this message.
        /// </summary>
        /// <returns>An array of Resources.</returns>
        public Resource[] GetResources()
        {
            ArrayList resources = new();
            resources.AddRange(answers);
            resources.AddRange(servers);
            resources.AddRange(additionals);
            return (Resource[])resources.ToArray(typeof(Resource));
        }

        /// <summary>
        /// Adds a Question to the message.
        /// </summary>
        /// <param name="question">The Question to add.</param>
        public void AddQuestion(Question question)
            => questions.Add(question);

        /// <summary>
        /// Returns a byte[] representation of this message.
        /// </summary>
        /// <returns>A byte[] representation of this message.</returns>
        public byte[] GetBytes()
        {
            PacketBuilder packet = new();
            packet.Add(_id);
            packet.Add(_flags);
            packet.Add((ushort)questions.Count);
            packet.Add((ushort)answers.Count);
            packet.Add((ushort)servers.Count);
            packet.Add((ushort)additionals.Count);

            foreach (Question query in questions)
            {
                packet.Add(query.GetBytes());
            }

            foreach (Resource resource in answers)
            {
                packet.Add(resource.GetBytes());
            }

            foreach (Resource resource in servers)
            {
                packet.Add(resource.GetBytes());
            }

            foreach (Resource resource in additionals)
            {
                packet.Add(resource.GetBytes());
            }

            return packet.GetBytes();
        }

        private void ParseData(byte[] data)
        {
            PacketParser packet = new(data);

            _id = packet.ReadUShort();
            _flags = packet.ReadUShort();
            ushort question_count = packet.ReadUShort();
            ushort answer_count = packet.ReadUShort();
            ushort server_count = packet.ReadUShort();
            ushort additional_count = packet.ReadUShort();

            for (int i = 0; i < question_count; ++i)
            {
                string domain = packet.ReadDomain();
                DnsResourceType rr_type = GetResourType(packet.ReadUShort());
                ushort rr_class = packet.ReadUShort();
                if (rr_type > 0) questions.Add(new Question(domain, rr_type, rr_class));
            }

            for (int i = 0; i < answer_count; ++i)
            {
                answers.Add(ParseResource(packet));
            }

            for (int i = 0; i < server_count; ++i)
            {
                servers.Add(ParseResource(packet));
            }

            for (int i = 0; i < additional_count; ++i)
            {
                additionals.Add(ParseResource(packet));
            }
        }

        private Resource ParseResource(PacketParser packet)
        {
            string domain = packet.ReadDomain();
            ushort rr_type = packet.ReadUShort();
            ushort rr_class = packet.ReadUShort();
            int ttl = packet.ReadInt();
            ushort length = packet.ReadUShort();

            switch (rr_type)
            {
                case 1: return new ARecord(packet, domain, ttl, length);
                case 5: return new CnameRecord(packet, domain, ttl);
                case 12: return new PtrRecord(packet, domain, ttl);
                case 16: return new TxtRecord(packet, domain, ttl);
                case 28: return new AaaaRecord(packet, domain, ttl, length);
                case 33: return new SrvRecord(packet, domain, ttl);
                default:
                    packet.ReadBytes(length);
                    return new Resource(domain, ttl);
            }
        }

        private DnsResourceType GetResourType(ushort rr_type) => rr_type switch
        {
            1 => DnsResourceType.A,
            5 => DnsResourceType.CNAME,
            12 => DnsResourceType.PTR,
            16 => DnsResourceType.TXT,
            28 => DnsResourceType.AAAA,
            33 => DnsResourceType.SRV,
            _ => 0,
        };
    }
}
